using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface ICategoryService
{
    Task<PagedResult<Category>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryTreeNode>> GetTreeAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrUpdateCategoryResult> CreateAsync(string name, string? description, int? parentId, CancellationToken cancellationToken);
    Task<CreateOrUpdateCategoryResult> UpdateAsync(int id, string name, string? description, int? parentId, CancellationToken cancellationToken);
    Task<DeleteCategoryResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed record CategoryTreeNode(int Id, string Name, string? Description, int? ParentId, bool IsRoot, IReadOnlyList<CategoryTreeNode> Children);

public sealed class CreateOrUpdateCategoryResult
{
    public Category? Category { get; init; }
    public bool ParentNotFound { get; init; }
    public bool CategoryNotFound { get; init; }
    public bool IsRootProtected { get; init; }
    public bool WouldCreateCycle { get; init; }
}

public sealed class DeleteCategoryResult
{
    public bool Deleted { get; init; }
    public bool NotFound { get; init; }
    public bool IsRootProtected { get; init; }
    public bool HasLinkedProducts { get; init; }
    public bool HasChildren { get; init; }
    public bool HasLinkedLids { get; init; }
}
