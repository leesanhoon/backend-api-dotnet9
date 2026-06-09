namespace backend_api_dotnet9.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string? AvatarImageUrl { get; set; }
    public Category? Category { get; set; }
    public List<ProductImage> ProductImages { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];
}
