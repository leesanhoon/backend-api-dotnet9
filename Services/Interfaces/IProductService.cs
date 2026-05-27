using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrUpdateProductResult> CreateAsync(string name, string? description, decimal price, int stockQuantity, int categoryId, CancellationToken cancellationToken);
    Task<CreateOrUpdateProductResult> UpdateAsync(int id, string name, string? description, decimal price, int stockQuantity, int categoryId, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed record ProductResponse(int Id, string Name, string? Description, decimal Price, int StockQuantity, int CategoryId, string CategoryName);

public sealed class CreateOrUpdateProductResult
{
    public Product? Product { get; init; }
    public ProductResponse? ProductResponse { get; init; }
    public bool CategoryNotFound { get; init; }
    public bool ProductNotFound { get; init; }
}
