using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Category> CreateAsync(string name, string? description, CancellationToken cancellationToken);
    Task<Category?> UpdateAsync(int id, string name, string? description, CancellationToken cancellationToken);
    Task<DeleteCategoryResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed class DeleteCategoryResult
{
    public bool Deleted { get; init; }
    public bool NotFound { get; init; }
    public bool HasLinkedProducts { get; init; }
}
