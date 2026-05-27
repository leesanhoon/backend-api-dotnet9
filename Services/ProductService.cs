using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace backend_api_dotnet9.Services;

public class ProductService(AppDbContext dbContext) : IProductService
{
    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderByDescending(x => x.Id)
            .Select(ToProductResponse())
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Id == id)
            .Select(ToProductResponse())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CreateOrUpdateProductResult> CreateAsync(string name, string? description, decimal price, int stockQuantity, int categoryId, CancellationToken cancellationToken)
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
        return new CreateOrUpdateProductResult { Product = product };
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

        var productResponse = await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.Id == id)
            .Select(ToProductResponse())
            .FirstAsync(cancellationToken);

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

    private static Expression<Func<Product, ProductResponse>> ToProductResponse()
    {
        return x => new ProductResponse(
            x.Id,
            x.Name,
            x.Description,
            x.Price,
            x.StockQuantity,
            x.CategoryId,
            x.Category != null ? x.Category.Name : string.Empty);
    }
}
