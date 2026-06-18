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
        var result = await productService.GetAllAsync(pagination.SafePage, pagination.SafePageSize, cancellationToken);
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
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.CategoryId,
            request.AvatarImage,
            request.GalleryImages,
            request.Variants,
            request.CompatibleProductIds ?? []);

        var result = await productService.CreateAsync(command, cancellationToken);

        if (result.CategoryNotFound) return BadRequest($"CategoryId {request.CategoryId} không tồn tại.");
        if (result.ValidationError is not null) return BadRequest(result.ValidationError);
        if (result.ImageError is not null) return BadRequest(result.ImageError);

        return CreatedAtAction(nameof(GetById), new { id = result.ProductResponse!.Id, version = "1" }, result.ProductResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.CategoryId,
            null,
            null,
            request.Variants,
            request.CompatibleProductIds ?? []);

        var result = await productService.UpdateAsync(id, command, cancellationToken);

        if (result.ProductNotFound) return NotFound();
        if (result.CategoryNotFound) return BadRequest($"CategoryId {request.CategoryId} không tồn tại.");
        if (result.ValidationError is not null) return BadRequest(result.ValidationError);

        return Ok(result.ProductResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await productService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:int}/compatible-lids")]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetCompatibleLids(int id, CancellationToken cancellationToken)
    {
        var lids = await productService.GetCompatibleLidsAsync(id, cancellationToken);
        return Ok(lids);
    }

    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ProductResponse>> AddImages(int id, [FromForm] AddProductImagesRequest request, CancellationToken cancellationToken)
    {
        var result = await productService.AddImagesAsync(id, request.AvatarImage, request.GalleryImages, cancellationToken);

        if (result.ProductNotFound) return NotFound();
        if (result.ImageError is not null) return BadRequest(result.ImageError);

        return Ok(result.ProductResponse);
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken cancellationToken)
    {
        var result = await productService.DeleteImageAsync(id, imageId, cancellationToken);

        if (result.ProductNotFound) return NotFound();
        if (result.ImageNotFound) return NotFound("Image không tồn tại.");

        return NoContent();
    }
}

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    int CategoryId,
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages,
    List<ProductVariantItem> Variants,
    List<int>? CompatibleProductIds);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    int CategoryId,
    List<ProductVariantItem> Variants,
    List<int>? CompatibleProductIds);

public sealed record AddProductImagesRequest(
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages);
