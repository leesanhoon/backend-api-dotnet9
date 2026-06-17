using backend_api_dotnet9.Infrastructure;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace backend_api_dotnet9.Services;

public sealed class ImagePreparationService(IOptions<ImageProcessingOptions> options) : IImagePreparationService
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private readonly ImageProcessingOptions processingOptions = options.Value;

    public async Task<PreparedImage> PrepareAsync(IFormFile file, bool isAvatar, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            throw new ArgumentException("Image file is required.");
        }

        if (file.Length <= 0)
        {
            throw new ArgumentException($"Image '{file.FileName}' is empty.");
        }

        if (file.Length > processingOptions.MaxFileSizeBytes)
        {
            var maxMb = processingOptions.MaxFileSizeBytes / (1024 * 1024);
            throw new ArgumentException($"Image '{file.FileName}' exceeds the maximum file size of {maxMb} MB.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Image '{file.FileName}' must be a supported image file (.jpg, .jpeg, .png, .webp, .gif).");
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) && !AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            throw new ArgumentException($"Image '{file.FileName}' has an unsupported content type '{file.ContentType}'.");
        }

        var sanitizedName = BuildSanitizedFileName(file.FileName, isAvatar ? "avatar" : "gallery");
        var width = isAvatar ? processingOptions.AvatarMaxWidth : processingOptions.GalleryMaxWidth;
        var height = isAvatar ? processingOptions.AvatarMaxHeight : processingOptions.GalleryMaxHeight;

        await using var input = file.OpenReadStream();
        using var image = await Image.LoadAsync(input, cancellationToken);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(width, height)
        }));

        var output = new MemoryStream();
        if (extension is ".png")
        {
            await image.SaveAsPngAsync(output, cancellationToken);
            output.Position = 0;
            return new PreparedImage(output, sanitizedName + ".png", "image/png", output.Length, ".png");
        }

        var encoder = new JpegEncoder { Quality = (int)processingOptions.JpegQuality };
        await image.SaveAsJpegAsync(output, encoder, cancellationToken);
        output.Position = 0;
        return new PreparedImage(output, sanitizedName + ".jpg", "image/jpeg", output.Length, ".jpg");
    }

    public void ValidateGalleryCount(List<IFormFile>? galleryImages)
    {
        if (galleryImages is not null && galleryImages.Count > processingOptions.MaxGalleryImages)
        {
            throw new ArgumentException($"Maximum {processingOptions.MaxGalleryImages} gallery images allowed.");
        }
    }

    private static string BuildSanitizedFileName(string originalFileName, string prefix)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var normalized = new string(baseName
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        normalized = normalized.Trim('-');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = prefix;
        }

        return $"{prefix}-{normalized}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }
}
