using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class PartnerService(AppDbContext dbContext, ICloudinaryImageService cloudinaryImageService) : IPartnerService
{
    public async Task<PagedResult<PartnerResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Partners.AsNoTracking()
            .Include(x => x.PartnerImages)
            .OrderByDescending(x => x.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var partners = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<PartnerResponse>(partners.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    public async Task<PartnerResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var partner = await dbContext.Partners.AsNoTracking()
            .Include(x => x.PartnerImages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return partner is null ? null : MapToResponse(partner);
    }

    public async Task<CreateOrUpdatePartnerResult> CreateAsync(CreatePartnerCommand command, CancellationToken cancellationToken)
    {
        var trimmedName = command.Name.Trim();
        var nameExists = await dbContext.Partners.AnyAsync(x => x.Name == trimmedName, cancellationToken);
        if (nameExists)
            return new CreateOrUpdatePartnerResult { ValidationError = $"Tên đối tác '{trimmedName}' đã tồn tại." };

        var partner = new Partner
        {
            Name = trimmedName,
            Address = command.Address.Trim(),
            PhoneNumber = command.PhoneNumber?.Trim(),
            Description = command.Description?.Trim()
        };

        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await UploadImagesAsync(partner, command.AvatarImage, command.GalleryImages, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            dbContext.Partners.Remove(partner);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOrUpdatePartnerResult { ImageError = ex.Message };
        }
        catch (InvalidOperationException ex)
        {
            dbContext.Partners.Remove(partner);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOrUpdatePartnerResult { ImageError = ex.Message };
        }

        var response = await LoadPartnerResponseAsync(partner.Id, cancellationToken);
        return new CreateOrUpdatePartnerResult { PartnerResponse = response };
    }

    public async Task<CreateOrUpdatePartnerResult> UpdateAsync(int id, CreatePartnerCommand command, CancellationToken cancellationToken)
    {
        var partner = await dbContext.Partners
            .Include(x => x.PartnerImages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (partner is null)
            return new CreateOrUpdatePartnerResult { PartnerNotFound = true };

        var trimmedName = command.Name.Trim();
        var nameExists = await dbContext.Partners.AnyAsync(x => x.Name == trimmedName && x.Id != id, cancellationToken);
        if (nameExists)
            return new CreateOrUpdatePartnerResult { ValidationError = $"Tên đối tác '{trimmedName}' đã tồn tại." };

        partner.Name = trimmedName;
        partner.Address = command.Address.Trim();
        partner.PhoneNumber = command.PhoneNumber?.Trim();
        partner.Description = command.Description?.Trim();

        if (command.AvatarImage is not null)
        {
            var oldAvatarImages = partner.PartnerImages.Where(x => x.ImageType == PartnerImageType.Avatar).ToList();
            dbContext.PartnerImages.RemoveRange(oldAvatarImages);

            var avatarUrl = await cloudinaryImageService.UploadImageAsync(command.AvatarImage, true, cancellationToken);
            partner.AvatarImageUrl = avatarUrl;
            dbContext.PartnerImages.Add(new PartnerImage
            {
                PartnerId = partner.Id,
                ImageType = PartnerImageType.Avatar,
                ImageUrl = avatarUrl,
                DisplayOrder = 0
            });
        }

        if (command.GalleryImages is not null)
        {
            var oldGalleryImages = partner.PartnerImages.Where(x => x.ImageType == PartnerImageType.Gallery).ToList();
            dbContext.PartnerImages.RemoveRange(oldGalleryImages);

            var nextDisplayOrder = 1;
            foreach (var galleryImage in command.GalleryImages)
            {
                var galleryUrl = await cloudinaryImageService.UploadImageAsync(galleryImage, false, cancellationToken);
                dbContext.PartnerImages.Add(new PartnerImage
                {
                    PartnerId = partner.Id,
                    ImageType = PartnerImageType.Gallery,
                    ImageUrl = galleryUrl,
                    DisplayOrder = nextDisplayOrder++
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await LoadPartnerResponseAsync(id, cancellationToken);
        return new CreateOrUpdatePartnerResult { PartnerResponse = response };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var partner = await dbContext.Partners.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (partner is null)
            return false;

        dbContext.Partners.Remove(partner);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task UploadImagesAsync(Partner partner, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken)
    {
        if (galleryImages is not null && galleryImages.Count > 10)
            throw new ArgumentException("Tối đa 10 ảnh gallery cho mỗi lần upload.");

        var partnerImages = new List<PartnerImage>();
        var nextDisplayOrder = 1;

        if (avatarImage is not null)
        {
            var avatarUrl = await cloudinaryImageService.UploadImageAsync(avatarImage, true, cancellationToken, "partners");
            partner.AvatarImageUrl = avatarUrl;
            partnerImages.Add(new PartnerImage
            {
                PartnerId = partner.Id,
                ImageType = PartnerImageType.Avatar,
                ImageUrl = avatarUrl,
                DisplayOrder = 0
            });
        }

        if (galleryImages is not null)
        {
            foreach (var galleryImage in galleryImages)
            {
                var galleryUrl = await cloudinaryImageService.UploadImageAsync(galleryImage, false, cancellationToken, "partners");
                partnerImages.Add(new PartnerImage
                {
                    PartnerId = partner.Id,
                    ImageType = PartnerImageType.Gallery,
                    ImageUrl = galleryUrl,
                    DisplayOrder = nextDisplayOrder++
                });
            }
        }

        if (partnerImages.Count > 0)
        {
            dbContext.PartnerImages.AddRange(partnerImages);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<PartnerResponse> LoadPartnerResponseAsync(int id, CancellationToken cancellationToken)
    {
        var partner = await dbContext.Partners.AsNoTracking()
            .Include(x => x.PartnerImages)
            .FirstAsync(x => x.Id == id, cancellationToken);

        return MapToResponse(partner);
    }

    private static PartnerResponse MapToResponse(Partner partner)
    {
        var galleryImages = partner.PartnerImages
            .Where(x => x.ImageType == PartnerImageType.Gallery)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new PartnerImageResponse(x.Id, x.ImageUrl, x.DisplayOrder))
            .ToList();

        return new PartnerResponse(
            partner.Id,
            partner.Name,
            partner.Address,
            partner.PhoneNumber,
            partner.Description,
            partner.AvatarImageUrl,
            galleryImages,
            partner.CreatedAtUtc);
    }
}
