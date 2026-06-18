using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrUpdateProductResult> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken);
    Task<CreateOrUpdateProductResult> UpdateAsync(int id, CreateProductCommand command, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProductResponse>> GetCompatibleLidsAsync(int productId, CancellationToken cancellationToken);
    Task<AddImagesResult> AddImagesAsync(int productId, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken);
    Task<DeleteImageResult> DeleteImageAsync(int productId, int imageId, CancellationToken cancellationToken);
}

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    int CategoryId,
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages,
    List<ProductVariantItem> Variants,
    List<int> CompatibleProductIds);

public sealed record ProductVariantItem(int CapacityMl, int DiameterMm, List<PriceTierItem> PriceTiers);

public sealed record PriceTierItem(int MinQuantity, decimal UnitPrice);

public sealed record ProductResponse(
    int Id,
    string Name,
    string? Description,
    int CategoryId,
    string CategoryName,
    string? AvatarImageUrl,
    IReadOnlyList<ProductImageResponse> GalleryImages,
    IReadOnlyList<ProductVariantResponse> Variants,
    IReadOnlyList<ProductLidResponse> Lids);

public sealed record ProductImageResponse(int Id, string ImageUrl, string ImageType, int DisplayOrder, DateTime CreatedAtUtc);

public sealed record ProductVariantResponse(int Id, int CapacityMl, int DiameterMm, string? SizeName, IReadOnlyList<PriceTierResponse> PriceTiers);

public sealed record PriceTierResponse(int Id, int MinQuantity, decimal UnitPrice);

public sealed record ProductLidResponse(int Id, int CompatibleProductId, string CompatibleProductName);

public sealed class CreateOrUpdateProductResult
{
    public Product? Product { get; init; }
    public ProductResponse? ProductResponse { get; init; }
    public bool CategoryNotFound { get; init; }
    public bool ProductNotFound { get; init; }
    public string? ImageError { get; init; }
    public string? ValidationError { get; init; }
}

public sealed class AddImagesResult
{
    public ProductResponse? ProductResponse { get; init; }
    public bool ProductNotFound { get; init; }
    public string? ImageError { get; init; }
}

public sealed class DeleteImageResult
{
    public bool Deleted { get; init; }
    public bool ProductNotFound { get; init; }
    public bool ImageNotFound { get; init; }
}
