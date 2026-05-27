using Asp.Versioning;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_api_dotnet9.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/products/{productId:int}/configurations")]
[ApiVersion("1.0")]
public class ProductConfigurationsController(IProductConfigurationService productConfigurationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProductConfigurationResult>> Get(int productId, CancellationToken cancellationToken)
    {
        var result = await productConfigurationService.GetAsync(productId, cancellationToken);
        return result is null ? NotFound("Product not found.") : Ok(result);
    }

    [HttpPost("materials")]
    public async Task<ActionResult<ProductMaterial>> AddMaterial(int productId, [FromBody] AddProductMaterialRequest request, CancellationToken cancellationToken)
    {
        var result = await productConfigurationService.AddMaterialAsync(productId, request.MaterialId, request.ExtraPrice, cancellationToken);
        if (result.ProductNotFound) return NotFound("Product not found.");
        if (result.MaterialNotFound) return BadRequest("Material not found.");
        return Ok(result.Data);
    }

    [HttpPost("print-options")]
    public async Task<ActionResult<ProductPrintOption>> AddPrintOption(int productId, [FromBody] AddProductPrintOptionRequest request, CancellationToken cancellationToken)
    {
        var result = await productConfigurationService.AddPrintOptionAsync(productId, request.PrintTypeId, request.ExtraPrice, cancellationToken);
        if (result.ProductNotFound) return NotFound("Product not found.");
        if (result.PrintTypeNotFound) return BadRequest("Print type not found.");
        return Ok(result.Data);
    }
}

public sealed record AddProductMaterialRequest(int MaterialId, decimal ExtraPrice);
public sealed record AddProductPrintOptionRequest(int PrintTypeId, decimal ExtraPrice);
