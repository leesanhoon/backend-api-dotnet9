using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class LidsController(ILidService lidService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<LidResponse>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await lidService.GetAllAsync(pagination.SafePage, pagination.SafePageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LidResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var lid = await lidService.GetByIdAsync(id, cancellationToken);
        return lid is null ? NotFound() : Ok(lid);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<LidResponse>> Create([FromForm] CreateLidFormRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateLidCommand(
            request.Name,
            request.Description,
            request.CategoryId,
            request.AvatarImage,
            request.GalleryImages,
            request.Prices);

        var result = await lidService.CreateAsync(command, cancellationToken);

        if (result.CategoryNotFound) return BadRequest("CategoryId không tồn tại.");
        if (result.ValidationError is not null) return BadRequest(result.ValidationError);
        if (result.ImageError is not null) return BadRequest(result.ImageError);

        return CreatedAtAction(nameof(GetById), new { id = result.LidResponse!.Id, version = "1" }, result.LidResponse);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LidResponse>> Update(int id, [FromBody] CreateLidRequest request, CancellationToken cancellationToken)
    {
        var result = await lidService.UpdateAsync(id, request, cancellationToken);

        if (result.LidNotFound) return NotFound();
        if (result.CategoryNotFound) return BadRequest("CategoryId không tồn tại.");
        if (result.ValidationError is not null) return BadRequest(result.ValidationError);

        return Ok(result.LidResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await lidService.DeleteAsync(id, cancellationToken);

        if (result.NotFound) return NotFound();
        if (result.HasLinkedProducts) return BadRequest("Không thể xoá nắp ly đang được sản phẩm sử dụng.");

        return NoContent();
    }

    [HttpPost("{id:int}/images")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<LidResponse>> AddImages(int id, [FromForm] AddLidImagesRequest request, CancellationToken cancellationToken)
    {
        var result = await lidService.AddImagesAsync(id, request.AvatarImage, request.GalleryImages, cancellationToken);

        if (result.LidNotFound) return NotFound();
        if (result.ImageError is not null) return BadRequest(result.ImageError);

        return Ok(result.LidResponse);
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId, CancellationToken cancellationToken)
    {
        var result = await lidService.DeleteImageAsync(id, imageId, cancellationToken);

        if (result.LidNotFound) return NotFound();
        if (result.ImageNotFound) return NotFound("Image không tồn tại.");

        return NoContent();
    }
}

public sealed record CreateLidFormRequest(
    string Name,
    string? Description,
    int CategoryId,
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages,
    List<LidPriceItem> Prices);

public sealed record AddLidImagesRequest(
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages);
