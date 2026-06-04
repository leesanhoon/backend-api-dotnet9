namespace backend_api_dotnet9.Services.Interfaces;

public interface ICloudinaryImageService
{
    Task<string> UploadImageAsync(IFormFile file, bool isAvatar, CancellationToken cancellationToken);
}
