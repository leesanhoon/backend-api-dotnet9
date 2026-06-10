namespace backend_api_dotnet9.Models;

public class Lid
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<LidPrice> Prices { get; set; } = [];
    public List<ProductLid> ProductLids { get; set; } = [];
}
