using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface ILidService
{
    Task<PagedResult<LidResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<LidResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrUpdateLidResult> CreateAsync(CreateLidCommand command, CancellationToken cancellationToken);
    Task<CreateOrUpdateLidResult> UpdateAsync(int id, CreateLidRequest request, CancellationToken cancellationToken);
    Task<DeleteLidResult> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<AddLidImagesResult> AddImagesAsync(int lidId, IFormFile? avatarImage, List<IFormFile>? galleryImages, CancellationToken cancellationToken);
    Task<DeleteLidImageResult> DeleteImageAsync(int lidId, int imageId, CancellationToken cancellationToken);
}

public sealed record CreateLidRequest(string Name, string? Description, int CategoryId, List<LidPriceItem> Prices);

public sealed record CreateLidCommand(string Name, string? Description, int CategoryId, IFormFile? AvatarImage, List<IFormFile>? GalleryImages, List<LidPriceItem> Prices);

public sealed record LidPriceItem(int DiameterMm, string? SizeName, decimal UnitPrice);

public sealed record LidResponse(int Id, string Name, string? Description, int CategoryId, string CategoryName, string? AvatarImageUrl, IReadOnlyList<LidImageResponse> GalleryImages, IReadOnlyList<LidPriceResponse> Prices);

public sealed record LidImageResponse(int Id, string ImageUrl, string ImageType, int DisplayOrder, DateTime CreatedAtUtc);

public sealed record LidPriceResponse(int Id, int DiameterMm, string? SizeName, decimal UnitPrice);

public sealed class CreateOrUpdateLidResult
{
    public LidResponse? LidResponse { get; init; }
    public bool CategoryNotFound { get; init; }
    public bool LidNotFound { get; init; }
    public string? ValidationError { get; init; }
    public string? ImageError { get; init; }
}

public sealed class DeleteLidResult
{
    public bool Deleted { get; init; }
    public bool NotFound { get; init; }
    public bool HasLinkedProducts { get; init; }
}

public sealed class AddLidImagesResult
{
    public LidResponse? LidResponse { get; init; }
    public bool LidNotFound { get; init; }
    public string? ImageError { get; init; }
}

public sealed class DeleteLidImageResult
{
    public bool Deleted { get; init; }
    public bool LidNotFound { get; init; }
    public bool ImageNotFound { get; init; }
}
