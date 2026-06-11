using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IPartnerService
{
    Task<PagedResult<PartnerResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PartnerResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrUpdatePartnerResult> CreateAsync(CreatePartnerCommand command, CancellationToken cancellationToken);
    Task<CreateOrUpdatePartnerResult> UpdateAsync(int id, CreatePartnerCommand command, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed record CreatePartnerCommand(
    string Name,
    string Address,
    string? PhoneNumber,
    string? Description,
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages);

public sealed record PartnerResponse(
    int Id,
    string Name,
    string Address,
    string? PhoneNumber,
    string? Description,
    string? AvatarImageUrl,
    IReadOnlyList<PartnerImageResponse> GalleryImages,
    DateTime CreatedAtUtc);

public sealed record PartnerImageResponse(int Id, string ImageUrl, int DisplayOrder);

public sealed class CreateOrUpdatePartnerResult
{
    public PartnerResponse? PartnerResponse { get; init; }
    public bool PartnerNotFound { get; init; }
    public string? ValidationError { get; init; }
    public string? ImageError { get; init; }
}
