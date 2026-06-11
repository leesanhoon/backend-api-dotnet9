using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PartnersController(IPartnerService partnerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PartnerResponse>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await partnerService.GetAllAsync(pagination.SafePage, pagination.SafePageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartnerResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var partner = await partnerService.GetByIdAsync(id, cancellationToken);
        return partner is null ? NotFound() : Ok(partner);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PartnerResponse>> Create([FromForm] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePartnerCommand(
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.Description,
            request.AvatarImage,
            request.GalleryImages);

        var result = await partnerService.CreateAsync(command, cancellationToken);

        if (result.ValidationError is not null) return BadRequest(result.ValidationError);
        if (result.ImageError is not null) return BadRequest(result.ImageError);

        return CreatedAtAction(nameof(GetById), new { id = result.PartnerResponse!.Id, version = "1" }, result.PartnerResponse);
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PartnerResponse>> Update(int id, [FromForm] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePartnerCommand(
            request.Name,
            request.Address,
            request.PhoneNumber,
            request.Description,
            request.AvatarImage,
            request.GalleryImages);

        var result = await partnerService.UpdateAsync(id, command, cancellationToken);

        if (result.PartnerNotFound) return NotFound();
        if (result.ValidationError is not null) return BadRequest(result.ValidationError);
        if (result.ImageError is not null) return BadRequest(result.ImageError);

        return Ok(result.PartnerResponse);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await partnerService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

public sealed record CreatePartnerRequest(
    string Name,
    string Address,
    string? PhoneNumber,
    string? Description,
    IFormFile? AvatarImage,
    List<IFormFile>? GalleryImages);
