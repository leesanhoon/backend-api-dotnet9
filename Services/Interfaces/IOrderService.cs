using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDetailDto?> TrackAsync(int orderId, string phone, CancellationToken cancellationToken);
    Task<PagedResult<OrderSummaryDto>> GetAllAsync(int page, int pageSize, string? status, CancellationToken cancellationToken);
    Task<UpdateOrderStatusResult> UpdateStatusAsync(int id, string newStatus, CancellationToken cancellationToken);
}

public sealed record CreateOrderRequest(
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string? Note,
    List<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    int? MaterialId,
    int? PrintTypeId,
    int? LidId);

public sealed record UpdateOrderStatusRequest(string Status);

public sealed record OrderDetailDto(
    int Id,
    string CustomerName,
    string CustomerPhone,
    string? CustomerEmail,
    string? Note,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAtUtc,
    IReadOnlyList<OrderItemDto> Items);

public sealed record OrderItemDto(
    int ProductId,
    string ProductName,
    int? MaterialId,
    string? MaterialName,
    int? PrintTypeId,
    string? PrintTypeName,
    int? LidId,
    string? LidName,
    int Quantity,
    decimal UnitPrice);

public sealed record OrderSummaryDto(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAtUtc);

public sealed class UpdateOrderStatusResult
{
    public OrderDetailDto? Order { get; init; }
    public bool NotFound { get; init; }
    public string? ErrorMessage { get; init; }
}
