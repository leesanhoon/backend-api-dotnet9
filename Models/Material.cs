namespace backend_api_dotnet9.Models;

public class Material
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<ProductMaterial> ProductMaterials { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];
}
