namespace backend_api_dotnet9.Models;

public enum LidImageType
{
    Avatar = 0,
    Gallery = 1
}

public class LidImage
{
    public int Id { get; set; }
    public int LidId { get; set; }
    public Lid? Lid { get; set; }
    public LidImageType ImageType { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
