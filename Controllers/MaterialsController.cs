using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class MaterialsController(IMaterialService materialService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<Material>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await materialService.GetAllAsync(pagination.Page, pagination.PageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Material>> Create([FromBody] UpsertMaterialRequest request, CancellationToken cancellationToken)
    {
        var item = await materialService.CreateAsync(request.Name, request.Description, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { version = "1" }, item);
    }
}

public sealed record UpsertMaterialRequest(string Name, string? Description);
