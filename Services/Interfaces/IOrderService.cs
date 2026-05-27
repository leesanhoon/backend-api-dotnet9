using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IOrderService
{
    Task<IReadOnlyList<OrderSummaryResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<OrderDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
}

public sealed record CreateOrderRequest(string CustomerName, string? CustomerPhone, string? CustomerEmail, string? Note, List<CreateOrderItemRequest> Items);
public sealed record CreateOrderItemRequest(int ProductId, int? MaterialId, int? PrintTypeId, int Quantity, decimal UnitPrice);
public sealed record OrderSummaryResponse(int Id, string CustomerName, decimal TotalAmount, string Status, DateTime CreatedAtUtc);
public sealed record OrderItemResponse(int ProductId, string ProductName, int? MaterialId, string? MaterialName, int? PrintTypeId, string? PrintTypeName, int Quantity, decimal UnitPrice);
public sealed record OrderDetailResponse(
    int Id,
    string CustomerName,
    string? CustomerPhone,
    string? CustomerEmail,
    string? Note,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemResponse> Items);

public sealed class CreateOrderResult
{
    public Order? Data { get; init; }
    public string? Error { get; init; }
}
