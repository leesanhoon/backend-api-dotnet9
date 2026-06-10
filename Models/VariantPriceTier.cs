namespace backend_api_dotnet9.Models;

public class VariantPriceTier
{
    public int Id { get; set; }
    public int ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public int MinQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}
