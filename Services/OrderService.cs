using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class OrderService(AppDbContext dbContext) : IOrderService
{
    public async Task<IReadOnlyList<OrderSummaryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new OrderSummaryResponse(x.Id, x.CustomerName, x.TotalAmount, x.Status, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<OrderDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .Include(x => x.Items)
            .ThenInclude(x => x.Material)
            .Include(x => x.Items)
            .ThenInclude(x => x.PrintType)
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
                x.MaterialId,
                x.Material != null ? x.Material.Name : null,
                x.PrintTypeId,
                x.PrintType != null ? x.PrintType.Name : null,
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

            if (item.MaterialId.HasValue)
            {
                var materialExists = await dbContext.Materials.AnyAsync(x => x.Id == item.MaterialId.Value, cancellationToken);
                if (!materialExists)
                {
                    return new CreateOrderResult { Error = $"MaterialId {item.MaterialId.Value} not found." };
                }
            }

            if (item.PrintTypeId.HasValue)
            {
                var printTypeExists = await dbContext.PrintTypes.AnyAsync(x => x.Id == item.PrintTypeId.Value, cancellationToken);
                if (!printTypeExists)
                {
                    return new CreateOrderResult { Error = $"PrintTypeId {item.PrintTypeId.Value} not found." };
                }
            }

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                MaterialId = item.MaterialId,
                PrintTypeId = item.PrintTypeId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        order.TotalAmount = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new CreateOrderResult { Data = order };
    }
}
