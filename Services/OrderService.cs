using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class OrderService(AppDbContext dbContext) : IOrderService
{
    public async Task<PagedResult<OrderSummaryResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking().OrderByDescending(x => x.Id);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new OrderSummaryResponse(x.Id, x.CustomerName, x.TotalAmount, x.Status, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return new PagedResult<OrderSummaryResponse>(items, totalCount, page, pageSize);
    }

    public async Task<OrderDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (order is null) return null;

        return new OrderDetailResponse(
            order.Id,
            order.CustomerName,
            order.CustomerPhone,
            order.CustomerEmail,
            order.Note,
            order.TotalAmount,
            order.Status,
            order.CreatedAtUtc,
            order.Items.Select(x => new OrderItemResponse(
                x.ProductId,
                x.Product != null ? x.Product.Name : string.Empty,
                x.Quantity,
                x.UnitPrice)).ToList());
    }

    public async Task<CreateOrderResult> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            return new CreateOrderResult { Error = "Order must contain at least one item." };
        }

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            CustomerEmail = request.CustomerEmail?.Trim(),
            Note = request.Note?.Trim(),
            Status = "draft"
        };

        foreach (var item in request.Items)
        {
            var product = await dbContext.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.ProductId, cancellationToken);
            if (product is null)
            {
                return new CreateOrderResult { Error = $"ProductId {item.ProductId} not found." };
            }

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateOrderResult { Data = order };
    }

    private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
    {
        ["draft"] = ["confirmed", "cancelled"],
        ["confirmed"] = ["shipping"],
        ["shipping"] = ["completed"],
    };

    public async Task<UpdateOrderStatusResult> UpdateStatusAsync(int id, string newStatus, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null) return new UpdateOrderStatusResult { NotFound = true };

        if (!ValidTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(newStatus))
        {
            return new UpdateOrderStatusResult { Error = $"Cannot transition from '{order.Status}' to '{newStatus}'." };
        }

        order.Status = newStatus;
        await dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateOrderStatusResult { NewStatus = newStatus };
    }

    public async Task<DeleteOrderResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (order is null) return new DeleteOrderResult { NotFound = true };

        if (order.Status != "draft")
        {
            return new DeleteOrderResult { Error = "Only draft orders can be deleted." };
        }

        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteOrderResult { Deleted = true };
    }
}
