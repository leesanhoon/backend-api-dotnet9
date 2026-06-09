using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductResponse>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await productService.GetAllAsync(pagination.Page, pagination.PageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductResponse>> Create([FromForm] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.CreateAsync(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            request.AvatarImage,
            request.GalleryImages ?? [],
            cancellationToken);

        if (result.CategoryNotFound)
        {
            return BadRequest($"CategoryId {request.CategoryId} does not exist.");
        }

        if (!string.IsNullOrWhiteSpace(result.ImageError))
        {
            return BadRequest(result.ImageError);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.ProductResponse!.Id, version = "1" }, result.ProductResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpsertProductRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.UpdateAsync(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity,
            request.CategoryId,
            cancellationToken);

        if (result.ProductNotFound)
        {
            return NotFound();
        }

        if (result.CategoryNotFound)
        {
            return BadRequest($"CategoryId {request.CategoryId} does not exist.");
        }

        return Ok(result.ProductResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }
        return NoContent();
    }
}

public sealed record CreateProductRequest(string Name, string? Description, decimal Price, int StockQuantity, int CategoryId, IFormFile? AvatarImage, List<IFormFile>? GalleryImages);
public sealed record UpsertProductRequest(string Name, string? Description, decimal Price, int StockQuantity, int CategoryId);
