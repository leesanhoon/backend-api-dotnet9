using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class LidService(AppDbContext dbContext, ICloudinaryImageService cloudinaryImageService) : ILidService
{
    public async Task<PagedResult<LidResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Lids.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Prices)
            .Include(x => x.LidImages)
            .OrderByDescending(x => x.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var lids = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<LidResponse>(lids.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    public async Task<LidResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Prices)
            .Include(x => x.LidImages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return lid is null ? null : MapToResponse(lid);
    }

    public async Task<CreateOrUpdateLidResult> CreateAsync(CreateLidCommand command, CancellationToken cancellationToken)
    {
        if (command.Prices.Count == 0)
            return new CreateOrUpdateLidResult { ValidationError = "Ít nhất 1 dòng giá theo miệng ly." };

        var duplicateDiameters = command.Prices.GroupBy(p => p.DiameterMm).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateDiameters.Count > 0)
            return new CreateOrUpdateLidResult { ValidationError = $"Trùng phi miệng ly: {string.Join(", ", duplicateDiameters)}mm." };

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == command.CategoryId, cancellationToken);
        if (!categoryExists)
            return new CreateOrUpdateLidResult { CategoryNotFound = true };

        var lid = new Lid
        {
            Name = command.Name.Trim(),
            Description = command.Description?.Trim(),
            CategoryId = command.CategoryId
        };

        foreach (var p in command.Prices)
        {
            lid.Prices.Add(new LidPrice
            {
                DiameterMm = p.DiameterMm,
                SizeName = p.SizeName?.Trim(),
                UnitPrice = p.UnitPrice
            });
        }

        dbContext.Lids.Add(lid);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await UploadImagesAsync(lid, command.AvatarImage, command.GalleryImages, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            dbContext.Lids.Remove(lid);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOrUpdateLidResult { ImageError = ex.Message };
        }
        catch (InvalidOperationException ex)
        {
            dbContext.Lids.Remove(lid);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOrUpdateLidResult { ImageError = ex.Message };
        }

        var response = await LoadLidResponseAsync(lid.Id, cancellationToken);
        return new CreateOrUpdateLidResult { LidResponse = response };
    }

    public async Task<CreateOrUpdateLidResult> UpdateAsync(int id, CreateLidRequest request, CancellationToken cancellationToken)
    {
        if (request.Prices.Count == 0)
            return new CreateOrUpdateLidResult { ValidationError = "Ít nhất 1 dòng giá theo miệng ly." };

        var duplicateDiameters = request.Prices.GroupBy(p => p.DiameterMm).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateDiameters.Count > 0)
            return new CreateOrUpdateLidResult { ValidationError = $"Trùng phi miệng ly: {string.Join(", ", duplicateDiameters)}mm." };

        var lid = await dbContext.Lids.Include(x => x.Prices).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lid is null)
            return new CreateOrUpdateLidResult { LidNotFound = true };

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return new CreateOrUpdateLidResult { CategoryNotFound = true };

        lid.Name = request.Name.Trim();
        lid.Description = request.Description?.Trim();
        lid.CategoryId = request.CategoryId;

        dbContext.LidPrices.RemoveRange(lid.Prices);
        foreach (var p in request.Prices)
        {
            lid.Prices.Add(new LidPrice
            {
                DiameterMm = p.DiameterMm,
                SizeName = p.SizeName?.Trim(),
                UnitPrice = p.UnitPrice
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await LoadLidResponseAsync(lid.Id, cancellationToken);
        return new CreateOrUpdateLidResult { LidResponse = response };
    }

    public async Task<AddLidImagesResult> AddImagesAsync(int lidId, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids
            .Include(x => x.LidImages)
            .FirstOrDefaultAsync(x => x.Id == lidId, cancellationToken);

        if (lid is null)
            return new AddLidImagesResult { LidNotFound = true };

        try
        {
            await UploadImagesAsync(lid, avatarImage, galleryImages, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return new AddLidImagesResult { ImageError = ex.Message };
        }
        catch (InvalidOperationException ex)
        {
            return new AddLidImagesResult { ImageError = ex.Message };
        }

        var response = await LoadLidResponseAsync(lidId, cancellationToken);
        return new AddLidImagesResult { LidResponse = response };
    }

    public async Task<DeleteLidImageResult> DeleteImageAsync(int lidId, int imageId, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids
            .Include(x => x.LidImages)
            .FirstOrDefaultAsync(x => x.Id == lidId, cancellationToken);

        if (lid is null)
            return new DeleteLidImageResult { LidNotFound = true };

        var image = lid.LidImages.FirstOrDefault(x => x.Id == imageId);
        if (image is null)
            return new DeleteLidImageResult { ImageNotFound = true };

        dbContext.LidImages.Remove(image);

        if (image.ImageType == LidImageType.Avatar)
            lid.AvatarImageUrl = null;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteLidImageResult { Deleted = true };
    }

    public async Task<DeleteLidResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lid is null)
            return new DeleteLidResult { NotFound = true };

        var hasLinkedProducts = await dbContext.ProductLids.AnyAsync(x => x.LidId == id, cancellationToken);
        if (hasLinkedProducts)
            return new DeleteLidResult { HasLinkedProducts = true };

        dbContext.Lids.Remove(lid);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteLidResult { Deleted = true };
    }

    private async Task UploadImagesAsync(Lid lid, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken)
    {
        var lidImages = new List<LidImage>();
        var nextDisplayOrder = (lid.LidImages?.Where(x => x.ImageType == LidImageType.Gallery).Select(x => x.DisplayOrder).DefaultIfEmpty(0).Max() ?? 0) + 1;

        if (avatarImage is not null)
        {
            var oldAvatar = lid.LidImages?.FirstOrDefault(x => x.ImageType == LidImageType.Avatar);
            if (oldAvatar is not null)
                dbContext.LidImages.Remove(oldAvatar);

            var avatarUrl = await cloudinaryImageService.UploadImageAsync(avatarImage, true, cancellationToken, "lids");
            lid.AvatarImageUrl = avatarUrl;
            lidImages.Add(new LidImage
            {
                LidId = lid.Id,
                ImageType = LidImageType.Avatar,
                ImageUrl = avatarUrl,
                DisplayOrder = 0
            });
        }

        if (galleryImages is not null)
        {
            foreach (var galleryImage in galleryImages)
            {
                var galleryUrl = await cloudinaryImageService.UploadImageAsync(galleryImage, false, cancellationToken, "lids");
                lidImages.Add(new LidImage
                {
                    LidId = lid.Id,
                    ImageType = LidImageType.Gallery,
                    ImageUrl = galleryUrl,
                    DisplayOrder = nextDisplayOrder++
                });
            }
        }

        if (lidImages.Count > 0)
        {
            dbContext.LidImages.AddRange(lidImages);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<LidResponse> LoadLidResponseAsync(int id, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Prices)
            .Include(x => x.LidImages)
            .FirstAsync(x => x.Id == id, cancellationToken);
        return MapToResponse(lid);
    }

    private static LidResponse MapToResponse(Lid lid)
    {
        var prices = lid.Prices
            .OrderBy(p => p.DiameterMm)
            .Select(p => new LidPriceResponse(p.Id, p.DiameterMm, p.SizeName, p.UnitPrice))
            .ToList();

        var galleryImages = lid.LidImages
            .Where(x => x.ImageType == LidImageType.Gallery)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new LidImageResponse(x.Id, x.ImageUrl, x.ImageType.ToString().ToLowerInvariant(), x.DisplayOrder, x.CreatedAtUtc))
            .ToList();

        return new LidResponse(
            lid.Id,
            lid.Name,
            lid.Description,
            lid.CategoryId,
            lid.Category?.Name ?? string.Empty,
            lid.AvatarImageUrl,
            galleryImages,
            prices);
    }
}
