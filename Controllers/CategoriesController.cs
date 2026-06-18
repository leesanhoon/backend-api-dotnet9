using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<Category>>> GetAll([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var result = await categoryService.GetAllAsync(pagination.SafePage, pagination.SafePageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeNode>>> GetTree(CancellationToken cancellationToken)
    {
        var tree = await categoryService.GetTreeAsync(cancellationToken);
        return Ok(tree);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetById(int id, CancellationToken cancellationToken)
    {
        var category = await categoryService.GetByIdAsync(id, cancellationToken);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create([FromBody] UpsertCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await categoryService.CreateAsync(request.Name, request.Description, request.ParentId, cancellationToken);

        if (result.ParentNotFound)
            return BadRequest("ParentId không tồn tại.");

        return CreatedAtAction(nameof(GetById), new { id = result.Category!.Id, version = "1" }, result.Category);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Category>> Update(int id, [FromBody] UpsertCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await categoryService.UpdateAsync(id, request.Name, request.Description, request.ParentId, cancellationToken);

        if (result.CategoryNotFound) return NotFound();
        if (result.IsRootProtected) return BadRequest("Không thể sửa danh mục gốc.");
        if (result.ParentNotFound) return BadRequest("ParentId không tồn tại.");
        if (result.WouldCreateCycle) return BadRequest("Không thể chọn danh mục con làm cha (tránh vòng lặp).");

        return Ok(result.Category);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);

        if (result.NotFound) return NotFound();
        if (result.IsRootProtected) return BadRequest("Không thể xoá danh mục gốc.");
        if (result.HasChildren) return BadRequest("Không thể xoá danh mục đang có danh mục con.");
        if (result.HasLinkedProducts) return BadRequest("Không thể xoá danh mục đang có sản phẩm.");

        return NoContent();
    }
}

public sealed record UpsertCategoryRequest(string Name, string? Description, int? ParentId);
