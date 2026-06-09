using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class ProductService(AppDbContext dbContext, ICloudinaryImageService cloudinaryImageService) : IProductService
{
    public async Task<PagedResult<ProductResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking().Include(x => x.Category).Include(x => x.ProductImages).OrderByDescending(x => x.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<ProductResponse>(products.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return product is null ? null : MapToResponse(product);
    }

    public async Task<CreateOrUpdateProductResult> CreateAsync(string name, string? description, decimal price, int stockQuantity, int categoryId, IFormFile? avatarImage, IReadOnlyList<IFormFile> galleryImages, CancellationToken cancellationToken)
    {
        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);
        if (!categoryExists)
        {
            return new CreateOrUpdateProductResult { CategoryNotFound = true };
        }

        var product = new Product
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            CategoryId = categoryId
        };

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var productImages = new List<ProductImage>();
            var nextDisplayOrder = 1;

            if (avatarImage is not null)
            {
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

            if (productImages.Count > 0)
            {
                dbContext.ProductImages.AddRange(productImages);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
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

        var productResponse = await LoadProductResponseAsync(product.Id, cancellationToken);
        return new CreateOrUpdateProductResult { Product = product, ProductResponse = productResponse };
    }

    public async Task<CreateOrUpdateProductResult> UpdateAsync(int id, string name, string? description, decimal price, int stockQuantity, int categoryId, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null)
        {
            return new CreateOrUpdateProductResult { ProductNotFound = true };
        }

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);
        if (!categoryExists)
        {
            return new CreateOrUpdateProductResult { CategoryNotFound = true };
        }

        product.Name = name.Trim();
        product.Description = description?.Trim();
        product.Price = price;
        product.StockQuantity = stockQuantity;
        product.CategoryId = categoryId;
        await dbContext.SaveChangesAsync(cancellationToken);

        var productResponse = await LoadProductResponseAsync(id, cancellationToken);
        return new CreateOrUpdateProductResult { Product = product, ProductResponse = productResponse };
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<ProductResponse> LoadProductResponseAsync(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.ProductImages)
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

        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CategoryId,
            product.Category != null ? product.Category.Name : string.Empty,
            product.AvatarImageUrl,
            galleryImages);
    }
}
