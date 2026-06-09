using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderSummaryResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<OrderDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<UpdateOrderStatusResult> UpdateStatusAsync(int id, string newStatus, CancellationToken cancellationToken);
    Task<DeleteOrderResult> DeleteAsync(int id, CancellationToken cancellationToken);
}

public sealed record CreateOrderRequest(string CustomerName, string? CustomerPhone, string? CustomerEmail, string? Note, List<CreateOrderItemRequest> Items);
public sealed record CreateOrderItemRequest(int ProductId, int Quantity, decimal UnitPrice);
public sealed record OrderSummaryResponse(int Id, string CustomerName, decimal TotalAmount, string Status, DateTime CreatedAtUtc);
public sealed record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
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

public sealed class UpdateOrderStatusResult
{
    public bool NotFound { get; init; }
    public string? Error { get; init; }
    public string? NewStatus { get; init; }
}

public sealed class DeleteOrderResult
{
    public bool NotFound { get; init; }
    public string? Error { get; init; }
    public bool Deleted { get; init; }
}
