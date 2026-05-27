using Asp.Versioning;
using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class TodoItemsController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TodoItem>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await dbContext.TodoItems
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
}
