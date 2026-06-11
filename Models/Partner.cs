namespace backend_api_dotnet9.Models;

public class Partner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
    public string? AvatarImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<PartnerImage> PartnerImages { get; set; } = [];
}
