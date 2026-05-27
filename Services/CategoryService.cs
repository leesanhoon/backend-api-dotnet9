using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class CategoryService(AppDbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
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

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
