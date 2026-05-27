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
    public async Task<ActionResult<IReadOnlyList<Material>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await materialService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<Material>> Create([FromBody] UpsertMaterialRequest request, CancellationToken cancellationToken)
    {
        var item = await materialService.CreateAsync(request.Name, request.Description, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { version = "1" }, item);
    }
}

public sealed record UpsertMaterialRequest(string Name, string? Description);
