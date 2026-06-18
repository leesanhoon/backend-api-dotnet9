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

    public async Task<IReadOnlyList<CategoryTreeNode>> GetTreeAsync(CancellationToken cancellationToken)
    {
        var allCategories = await dbContext.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var lookup = allCategories.ToLookup(c => c.ParentId);

        IReadOnlyList<CategoryTreeNode> BuildChildren(int? parentId)
        {
            return lookup[parentId]
                .Select(c => new CategoryTreeNode(c.Id, c.Name, c.Description, c.ParentId, c.IsRoot, BuildChildren(c.Id)))
                .ToList();
        }

        return BuildChildren(null);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CreateOrUpdateCategoryResult> CreateAsync(string name, string? description, int? parentId, CancellationToken cancellationToken)
    {
        if (parentId.HasValue)
        {
            var parentExists = await dbContext.Categories.AnyAsync(x => x.Id == parentId.Value, cancellationToken);
            if (!parentExists)
                return new CreateOrUpdateCategoryResult { ParentNotFound = true };
        }

        var category = new Category
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentId = parentId
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateOrUpdateCategoryResult { Category = category };
    }

    public async Task<CreateOrUpdateCategoryResult> UpdateAsync(int id, string name, string? description, int? parentId, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
            return new CreateOrUpdateCategoryResult { CategoryNotFound = true };

        if (category.IsRoot)
            return new CreateOrUpdateCategoryResult { IsRootProtected = true };

        if (parentId.HasValue)
        {
            if (parentId.Value == id)
                return new CreateOrUpdateCategoryResult { WouldCreateCycle = true };

            var parentExists = await dbContext.Categories.AnyAsync(x => x.Id == parentId.Value, cancellationToken);
            if (!parentExists)
                return new CreateOrUpdateCategoryResult { ParentNotFound = true };

            if (await IsDescendantAsync(parentId.Value, id, cancellationToken))
                return new CreateOrUpdateCategoryResult { WouldCreateCycle = true };
        }

        category.Name = name.Trim();
        category.Description = description?.Trim();
        category.ParentId = parentId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateOrUpdateCategoryResult { Category = category };
    }

    public async Task<DeleteCategoryResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
            return new DeleteCategoryResult { NotFound = true };

        if (category.IsRoot)
            return new DeleteCategoryResult { IsRootProtected = true };

        var hasChildren = await dbContext.Categories.AnyAsync(x => x.ParentId == id, cancellationToken);
        if (hasChildren)
            return new DeleteCategoryResult { HasChildren = true };

        var hasProducts = await dbContext.Products.AnyAsync(x => x.CategoryId == id, cancellationToken);
        if (hasProducts)
            return new DeleteCategoryResult { HasLinkedProducts = true };

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteCategoryResult { Deleted = true };
    }

    private async Task<bool> IsDescendantAsync(int candidateId, int ancestorId, CancellationToken cancellationToken)
    {
        var allCategories = await dbContext.Categories.AsNoTracking()
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var childrenLookup = allCategories.ToLookup(c => c.ParentId);
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(ancestorId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in childrenLookup[current])
            {
                if (child.Id == candidateId) return true;
                if (visited.Add(child.Id))
                    queue.Enqueue(child.Id);
            }
        }

        return false;
    }
}
