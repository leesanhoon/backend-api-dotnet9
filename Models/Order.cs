namespace backend_api_dotnet9.Models;

public enum OrderStatus
{
    PendingConfirmation,
    Confirmed,
    Shipping,
    Completed,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingConfirmation;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int? MaterialId { get; set; }
    public int? PrintTypeId { get; set; }
    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
