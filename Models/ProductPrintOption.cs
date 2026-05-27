namespace backend_api_dotnet9.Models;

public class ProductPrintOption
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int PrintTypeId { get; set; }
    public PrintType? PrintType { get; set; }
    public decimal ExtraPrice { get; set; }
}
