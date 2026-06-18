using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class ProductService(AppDbContext dbContext, ICloudinaryImageService cloudinaryImageService) : IProductService
{
    public async Task<PagedResult<ProductResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
            .Include(x => x.Variants).ThenInclude(v => v.PriceTiers)
            .Include(x => x.ProductLids).ThenInclude(pl => pl.CompatibleProduct)
            .OrderByDescending(x => x.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<ProductResponse>(products.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
            .Include(x => x.Variants).ThenInclude(v => v.PriceTiers)
            .Include(x => x.ProductLids).ThenInclude(pl => pl.CompatibleProduct)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return product is null ? null : MapToResponse(product);
    }

    public async Task<CreateOrUpdateProductResult> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var validationError = ValidateVariants(command.Variants);
        if (validationError is not null)
            return new CreateOrUpdateProductResult { ValidationError = validationError };

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == command.CategoryId, cancellationToken);
        if (!categoryExists)
            return new CreateOrUpdateProductResult { CategoryNotFound = true };

        var product = new Product
        {
            Name = command.Name.Trim(),
            Description = command.Description?.Trim(),
            CategoryId = command.CategoryId
        };

        foreach (var v in command.Variants)
        {
            var variant = new ProductVariant
            {
                CapacityMl = v.CapacityMl,
                DiameterMm = v.DiameterMm
            };
            foreach (var pt in v.PriceTiers.OrderBy(t => t.MinQuantity))
            {
                variant.PriceTiers.Add(new VariantPriceTier
                {
                    MinQuantity = pt.MinQuantity,
                    UnitPrice = pt.UnitPrice
                });
            }
            product.Variants.Add(variant);
        }

        foreach (var lidId in command.CompatibleProductIds.Distinct())
        {
            product.ProductLids.Add(new ProductLid { CompatibleProductId = lidId });
        }

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await UploadImagesAsync(product, command.AvatarImage, command.GalleryImages, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOrUpdateProductResult { ImageError = ex.Message };
        }
        catch (InvalidOperationException ex)
        {
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync(cancellationToken);
            return new CreateOrUpdateProductResult { ImageError = ex.Message };
        }

        var response = await LoadProductResponseAsync(product.Id, cancellationToken);
        return new CreateOrUpdateProductResult { Product = product, ProductResponse = response };
    }

    public async Task<CreateOrUpdateProductResult> UpdateAsync(int id, CreateProductCommand command, CancellationToken cancellationToken)
    {
        var validationError = ValidateVariants(command.Variants);
        if (validationError is not null)
            return new CreateOrUpdateProductResult { ValidationError = validationError };

        var product = await dbContext.Products
            .Include(x => x.Variants).ThenInclude(v => v.PriceTiers)
            .Include(x => x.ProductLids)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (product is null)
            return new CreateOrUpdateProductResult { ProductNotFound = true };

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == command.CategoryId, cancellationToken);
        if (!categoryExists)
            return new CreateOrUpdateProductResult { CategoryNotFound = true };

        product.Name = command.Name.Trim();
        product.Description = command.Description?.Trim();
        product.CategoryId = command.CategoryId;

        dbContext.VariantPriceTiers.RemoveRange(product.Variants.SelectMany(v => v.PriceTiers));
        dbContext.ProductVariants.RemoveRange(product.Variants);
        dbContext.ProductLids.RemoveRange(product.ProductLids);

        foreach (var v in command.Variants)
        {
            var variant = new ProductVariant
            {
                ProductId = product.Id,
                CapacityMl = v.CapacityMl,
                DiameterMm = v.DiameterMm
            };
            foreach (var pt in v.PriceTiers.OrderBy(t => t.MinQuantity))
            {
                variant.PriceTiers.Add(new VariantPriceTier
                {
                    MinQuantity = pt.MinQuantity,
                    UnitPrice = pt.UnitPrice
                });
            }
            dbContext.ProductVariants.Add(variant);
        }

        foreach (var lidId in command.CompatibleProductIds.Distinct())
        {
            dbContext.ProductLids.Add(new ProductLid { ProductId = product.Id, CompatibleProductId = lidId });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await LoadProductResponseAsync(id, cancellationToken);
        return new CreateOrUpdateProductResult { Product = product, ProductResponse = response };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null)
            return false;

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetCompatibleLidsAsync(int productId, CancellationToken cancellationToken)
    {
        var diameters = await dbContext.ProductVariants
            .Where(v => v.ProductId == productId)
            .Select(v => v.DiameterMm)
            .Distinct()
            .ToListAsync(cancellationToken);

        var compatibleProducts = await dbContext.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
            .Include(x => x.Variants).ThenInclude(v => v.PriceTiers)
            .Include(x => x.ProductLids).ThenInclude(pl => pl.CompatibleProduct)
            .Where(p => p.Id != productId && p.Variants.Any(v => v.CapacityMl == 0 && diameters.Contains(v.DiameterMm)))
            .ToListAsync(cancellationToken);

        return compatibleProducts.Select(MapToResponse).ToList();
    }

    public async Task<AddImagesResult> AddImagesAsync(int productId, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

        if (product is null)
            return new AddImagesResult { ProductNotFound = true };

        try
        {
            await UploadImagesAsync(product, avatarImage, galleryImages, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return new AddImagesResult { ImageError = ex.Message };
        }
        catch (InvalidOperationException ex)
        {
            return new AddImagesResult { ImageError = ex.Message };
        }

        var response = await LoadProductResponseAsync(productId, cancellationToken);
        return new AddImagesResult { ProductResponse = response };
    }

    public async Task<DeleteImageResult> DeleteImageAsync(int productId, int imageId, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

        if (product is null)
            return new DeleteImageResult { ProductNotFound = true };

        var image = product.ProductImages.FirstOrDefault(x => x.Id == imageId);
        if (image is null)
            return new DeleteImageResult { ImageNotFound = true };

        dbContext.ProductImages.Remove(image);

        if (image.ImageType == ProductImageType.Avatar)
            product.AvatarImageUrl = null;

        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteImageResult { Deleted = true };
    }

    private async Task UploadImagesAsync(Product product, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken)
    {
        if (galleryImages is not null && galleryImages.Count > 10)
            throw new ArgumentException("Tối đa 10 ảnh gallery cho mỗi lần upload.");

        var productImages = new List<ProductImage>();
        var nextDisplayOrder = (product.ProductImages?.Where(x => x.ImageType == ProductImageType.Gallery).Select(x => x.DisplayOrder).DefaultIfEmpty(0).Max() ?? 0) + 1;

        if (avatarImage is not null)
        {
            var oldAvatar = product.ProductImages?.FirstOrDefault(x => x.ImageType == ProductImageType.Avatar);
            if (oldAvatar is not null)
                dbContext.ProductImages.Remove(oldAvatar);

            var avatarUrl = await cloudinaryImageService.UploadImageAsync(avatarImage, true, cancellationToken);
            product.AvatarImageUrl = avatarUrl;
            productImages.Add(new ProductImage
            {
                ProductId = product.Id,
                ImageType = ProductImageType.Avatar,
                ImageUrl = avatarUrl,
                DisplayOrder = 0
            });
        }

        if (galleryImages is not null)
        {
            foreach (var galleryImage in galleryImages)
            {
                var galleryUrl = await cloudinaryImageService.UploadImageAsync(galleryImage, false, cancellationToken);
                productImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageType = ProductImageType.Gallery,
                    ImageUrl = galleryUrl,
                    DisplayOrder = nextDisplayOrder++
                });
            }
        }

        if (productImages.Count > 0)
        {
            dbContext.ProductImages.AddRange(productImages);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static string? ValidateVariants(List<ProductVariantItem> variants)
    {
        if (variants.Count == 0)
            return "Ít nhất 1 variant dung tích.";

        var duplicateCapacities = variants.GroupBy(v => v.CapacityMl).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateCapacities.Count > 0)
            return $"Trùng dung tích: {string.Join(", ", duplicateCapacities)}ml.";

        foreach (var v in variants)
        {
            if (v.PriceTiers.Count == 0)
                return $"Variant {v.CapacityMl}ml cần ít nhất 1 mốc giá.";

            var duplicateQty = v.PriceTiers.GroupBy(t => t.MinQuantity).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateQty.Count > 0)
                return $"Variant {v.CapacityMl}ml có mốc số lượng trùng: {string.Join(", ", duplicateQty)}.";
        }

        return null;
    }

    private async Task<ProductResponse> LoadProductResponseAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
            .Include(x => x.Variants).ThenInclude(v => v.PriceTiers)
            .Include(x => x.ProductLids).ThenInclude(pl => pl.CompatibleProduct)
            .FirstAsync(x => x.Id == id, cancellationToken);

        return MapToResponse(product);
    }

    private static ProductResponse MapToResponse(Product product)
    {
        var galleryImages = product.ProductImages
            .Where(x => x.ImageType == ProductImageType.Gallery)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new ProductImageResponse(x.Id, x.ImageUrl, x.ImageType.ToString().ToLowerInvariant(), x.DisplayOrder, x.CreatedAtUtc))
            .ToList();

        var variants = product.Variants
            .OrderBy(v => v.CapacityMl)
            .Select(v => new ProductVariantResponse(
                v.Id,
                v.CapacityMl,
                v.DiameterMm,
                v.SizeName,
                v.PriceTiers.OrderBy(t => t.MinQuantity)
                    .Select(t => new PriceTierResponse(t.Id, t.MinQuantity, t.UnitPrice))
                    .ToList()))
            .ToList();

        var lids = product.ProductLids
            .Select(pl => new ProductLidResponse(pl.Id, pl.CompatibleProductId, pl.CompatibleProduct?.Name ?? string.Empty))
            .ToList();

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.CategoryId,
            product.Category?.Name ?? string.Empty,
            product.AvatarImageUrl,
            galleryImages,
            variants,
            lids);
    }
}
