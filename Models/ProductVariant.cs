namespace backend_api_dotnet9.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int CapacityMl { get; set; }
    public int DiameterMm { get; set; }
    public string? SizeName { get; set; }
    public List<VariantPriceTier> PriceTiers { get; set; } = [];
}
