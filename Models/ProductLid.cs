namespace backend_api_dotnet9.Models;

public class ProductLid
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int LidId { get; set; }
    public Lid? Lid { get; set; }
}
