using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IProductConfigurationService
{
    Task<ProductConfigurationResult?> GetAsync(int productId, CancellationToken cancellationToken);
    Task<AddMaterialResult> AddMaterialAsync(int productId, int materialId, decimal extraPrice, CancellationToken cancellationToken);
    Task<AddPrintOptionResult> AddPrintOptionAsync(int productId, int printTypeId, decimal extraPrice, CancellationToken cancellationToken);
}

public sealed record ProductMaterialResponse(int Id, int MaterialId, string MaterialName, decimal ExtraPrice);
public sealed record ProductPrintOptionResponse(int Id, int PrintTypeId, string PrintTypeName, decimal ExtraPrice);
public sealed record ProductConfigurationResult(IReadOnlyList<ProductMaterialResponse> Materials, IReadOnlyList<ProductPrintOptionResponse> PrintOptions);

public sealed class AddMaterialResult
{
    public ProductMaterial? Data { get; init; }
    public bool ProductNotFound { get; init; }
    public bool MaterialNotFound { get; init; }
}

public sealed class AddPrintOptionResult
{
    public ProductPrintOption? Data { get; init; }
    public bool ProductNotFound { get; init; }
    public bool PrintTypeNotFound { get; init; }
}
