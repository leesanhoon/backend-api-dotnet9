using backend_api_dotnet9.Infrastructure;
using backend_api_dotnet9.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace backend_api_dotnet9.Services;

public sealed class CloudinaryImageService(Cloudinary cloudinary, IOptions<CloudinaryOptions> options, IImagePreparationService imagePreparationService) : ICloudinaryImageService
{
    private readonly string folder = string.IsNullOrWhiteSpace(options.Value.Folder) ? "products" : options.Value.Folder.Trim();

    public async Task<string> UploadImageAsync(IFormFile file, bool isAvatar, CancellationToken cancellationToken)
    {
        var prepared = await imagePreparationService.PrepareAsync(file, isAvatar, cancellationToken);
        await using var stream = prepared.Content;
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(prepared.FileName, stream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true,
            Overwrite = false,
            AssetFolder = folder
        };

        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        var secureUrl = uploadResult.SecureUrl?.ToString();
        if (!string.IsNullOrWhiteSpace(secureUrl)) return secureUrl;
        var fallbackUrl = uploadResult.Url?.ToString();
        if (!string.IsNullOrWhiteSpace(fallbackUrl)) return fallbackUrl;
        throw new InvalidOperationException("Cloudinary upload failed.");
    }
}
