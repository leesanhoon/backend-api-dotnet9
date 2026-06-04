using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IImagePreparationService
{
    Task<PreparedImage> PrepareAsync(IFormFile file, bool isAvatar, CancellationToken cancellationToken);
}

public sealed record PreparedImage(Stream Content, string FileName, string ContentType, long Length, string Extension);
