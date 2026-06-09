using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class PrintTypesController(IPrintTypeService printTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PrintType>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await printTypeService.GetAllAsync(pagination.Page, pagination.PageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PrintType>> Create([FromBody] UpsertPrintTypeRequest request, CancellationToken cancellationToken)
    {
        var item = await printTypeService.CreateAsync(request.Name, request.ColorCount, request.Description, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { version = "1" }, item);
    }
}

public sealed record UpsertPrintTypeRequest(string Name, int ColorCount, string? Description);
