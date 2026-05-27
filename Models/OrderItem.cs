namespace backend_api_dotnet9.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? MaterialId { get; set; }
    public Material? Material { get; set; }
    public int? PrintTypeId { get; set; }
    public PrintType? PrintType { get; set; }
    public int? DesignFileId { get; set; }
    public DesignFile? DesignFile { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
