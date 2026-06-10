namespace backend_api_dotnet9.Models;

public class LidPrice
{
    public int Id { get; set; }
    public int LidId { get; set; }
    public Lid? Lid { get; set; }
    public int DiameterMm { get; set; }
    public string? SizeName { get; set; }
    public decimal UnitPrice { get; set; }
}
