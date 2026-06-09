using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class CategoryService(AppDbContext dbContext) : ICategoryService
{
    public async Task<PagedResult<Category>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Categories.AsNoTracking().OrderBy(x => x.Name);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<Category>(items, totalCount, page, pageSize);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Category> CreateAsync(string name, string? description, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = name.Trim(),
            Description = description?.Trim()
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<Category?> UpdateAsync(int id, string name, string? description, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return null;
        }

        category.Name = name.Trim();
        category.Description = description?.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task<DeleteCategoryResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return new DeleteCategoryResult { NotFound = true };
        }

        var hasLinkedProducts = await dbContext.Products.AnyAsync(x => x.CategoryId == id, cancellationToken);
        if (hasLinkedProducts)
        {
            return new DeleteCategoryResult { HasLinkedProducts = true };
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteCategoryResult { Deleted = true };
    }
}
