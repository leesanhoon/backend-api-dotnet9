using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrderDetailDto>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Items is not { Count: > 0 })
            return BadRequest(new { message = "Danh sach san pham khong duoc trong" });

        var result = await orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Track), new { orderId = result.Id, phone = result.CustomerPhone, version = "1" }, result);
    }

    [HttpGet("track")]
    public async Task<ActionResult<OrderDetailDto>> Track([FromQuery] int orderId, [FromQuery] string phone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return BadRequest(new { message = "So dien thoai khong duoc trong" });

        var order = await orderService.TrackAsync(orderId, phone.Trim(), cancellationToken);
        return order is null
            ? NotFound(new { message = "Khong tim thay don hang" })
            : Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAll(
        [FromQuery] PaginationRequest pagination,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetAllAsync(pagination.SafePage, pagination.SafePageSize, status, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OrderDetailDto>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.UpdateStatusAsync(id, request.Status, cancellationToken);

        if (result.NotFound) return NotFound();
        if (result.ErrorMessage is not null) return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Order);
    }
}
