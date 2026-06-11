namespace backend_api_dotnet9.Models;

public enum PartnerImageType
{
    Avatar = 0,
    Gallery = 1
}

public class PartnerImage
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public Partner? Partner { get; set; }
    public PartnerImageType ImageType { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
