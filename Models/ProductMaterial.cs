namespace backend_api_dotnet9.Models;

public class ProductMaterial
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int MaterialId { get; set; }
    public Material? Material { get; set; }
    public decimal ExtraPrice { get; set; }
}
