namespace backend_api_dotnet9.Models;

public class PrintType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ColorCount { get; set; }
    public string? Description { get; set; }
    public List<ProductPrintOption> ProductPrintOptions { get; set; } = [];
    public List<OrderItem> OrderItems { get; set; } = [];
}
