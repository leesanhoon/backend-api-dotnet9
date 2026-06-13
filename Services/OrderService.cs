using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class OrderService(AppDbContext dbContext, ITelegramNotificationService telegramNotification) : IOrderService
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Draft] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Shipping],
        [OrderStatus.Shipping] = [OrderStatus.Completed],
        [OrderStatus.Completed] = [],
        [OrderStatus.Cancelled] = []
    };

    public async Task<OrderDetailDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone.Trim(),
            CustomerEmail = request.CustomerEmail?.Trim(),
            Note = request.Note?.Trim(),
            Status = OrderStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                MaterialId = item.MaterialId,
                PrintTypeId = item.PrintTypeId
            });
        }

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var detail = await LoadOrderDetailAsync(order.Id, cancellationToken);
        telegramNotification.SendOrderCreatedNotification(detail);
        return detail;
    }

    public async Task<OrderDetailDto?> TrackAsync(int orderId, string phone, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerPhone.Contains(phone), cancellationToken);

        return order is null ? null : MapToDetailDto(order);
    }

    public async Task<PagedResult<OrderSummaryDto>> GetAllAsync(int page, int pageSize, string? status, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(o => o.Status == parsedStatus);
        }

        query = query.OrderByDescending(o => o.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        var items = orders.Select(o => new OrderSummaryDto(
            o.Id,
            o.CustomerName,
            o.TotalAmount,
            o.Status.ToString().ToLowerInvariant(),
            o.CreatedAtUtc
        )).ToList();

        return new PagedResult<OrderSummaryDto>(items, totalCount, page, pageSize);
    }

    public async Task<UpdateOrderStatusResult> UpdateStatusAsync(int id, string newStatus, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var targetStatus))
        {
            return new UpdateOrderStatusResult { ErrorMessage = $"Trang thai '{newStatus}' khong hop le" };
        }

        var order = await dbContext.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order is null)
            return new UpdateOrderStatusResult { NotFound = true };

        if (!AllowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(targetStatus))
        {
            var from = order.Status.ToString().ToLowerInvariant();
            var to = targetStatus.ToString().ToLowerInvariant();
            return new UpdateOrderStatusResult { ErrorMessage = $"Khong the chuyen tu {from} sang {to}" };
        }

        order.Status = targetStatus;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateOrderStatusResult { Order = MapToDetailDto(order) };
    }

    private async Task<OrderDetailDto> LoadOrderDetailAsync(int id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.AsNoTracking()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == id, cancellationToken);

        return MapToDetailDto(order);
    }

    private static OrderDetailDto MapToDetailDto(Order order)
    {
        var items = order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.MaterialId,
            null,
            i.PrintTypeId,
            null,
            i.Quantity,
            i.UnitPrice
        )).ToList();

        return new OrderDetailDto(
            order.Id,
            order.CustomerName,
            order.CustomerPhone,
            order.CustomerEmail,
            order.Note,
            order.TotalAmount,
            order.Status.ToString().ToLowerInvariant(),
            order.CreatedAtUtc,
            items);
    }
}
