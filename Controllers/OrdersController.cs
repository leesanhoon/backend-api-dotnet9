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
    [HttpGet]
    public async Task<ActionResult<PagedResult<OrderSummaryResponse>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await orderService.GetAllAsync(pagination.Page, pagination.PageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.CreateAsync(request, cancellationToken);
        if (!string.IsNullOrWhiteSpace(result.Error)) return BadRequest(result.Error);

        var order = result.Data!;
        return CreatedAtAction(nameof(GetById), new { id = order.Id, version = "1" }, order);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.UpdateStatusAsync(id, request.Status, cancellationToken);
        if (result.NotFound) return NotFound();
        if (!string.IsNullOrWhiteSpace(result.Error)) return BadRequest(result.Error);
        return Ok(new { status = result.NewStatus });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await orderService.DeleteAsync(id, cancellationToken);
        if (result.NotFound) return NotFound();
        if (!string.IsNullOrWhiteSpace(result.Error)) return BadRequest(result.Error);
        return NoContent();
    }
}

public sealed record UpdateOrderStatusRequest(string Status);
