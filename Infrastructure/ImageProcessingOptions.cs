namespace backend_api_dotnet9.Infrastructure;

public sealed class ImageProcessingOptions
{
    public int AvatarMaxWidth { get; set; } = 1200;
    public int AvatarMaxHeight { get; set; } = 1200;
    public int GalleryMaxWidth { get; set; } = 1000;
    public int GalleryMaxHeight { get; set; } = 1000;
    public long JpegQuality { get; set; } = 82;
}
