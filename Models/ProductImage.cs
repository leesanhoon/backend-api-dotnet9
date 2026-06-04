namespace backend_api_dotnet9.Models;

public enum ProductImageType
{
    Avatar = 0,
    Gallery = 1
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public ProductImageType ImageType { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
