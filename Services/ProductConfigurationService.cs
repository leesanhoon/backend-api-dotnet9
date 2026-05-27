using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class ProductConfigurationService(AppDbContext dbContext) : IProductConfigurationService
{
    public async Task<ProductConfigurationResult?> GetAsync(int productId, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Products.AnyAsync(x => x.Id == productId, cancellationToken);
        if (!exists) return null;

        var materials = await dbContext.ProductMaterials
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Include(x => x.Material)
            .Select(x => new ProductMaterialResponse(x.Id, x.MaterialId, x.Material != null ? x.Material.Name : string.Empty, x.ExtraPrice))
            .ToListAsync(cancellationToken);

        var printOptions = await dbContext.ProductPrintOptions
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Include(x => x.PrintType)
            .Select(x => new ProductPrintOptionResponse(x.Id, x.PrintTypeId, x.PrintType != null ? x.PrintType.Name : string.Empty, x.ExtraPrice))
            .ToListAsync(cancellationToken);

        return new ProductConfigurationResult(materials, printOptions);
    }

    public async Task<AddMaterialResult> AddMaterialAsync(int productId, int materialId, decimal extraPrice, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Products.AnyAsync(x => x.Id == productId, cancellationToken);
        if (!exists) return new AddMaterialResult { ProductNotFound = true };

        var materialExists = await dbContext.Materials.AnyAsync(x => x.Id == materialId, cancellationToken);
        if (!materialExists) return new AddMaterialResult { MaterialNotFound = true };

        var item = new ProductMaterial
        {
            ProductId = productId,
            MaterialId = materialId,
            ExtraPrice = extraPrice
        };

        dbContext.ProductMaterials.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AddMaterialResult { Data = item };
    }

    public async Task<AddPrintOptionResult> AddPrintOptionAsync(int productId, int printTypeId, decimal extraPrice, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Products.AnyAsync(x => x.Id == productId, cancellationToken);
        if (!exists) return new AddPrintOptionResult { ProductNotFound = true };

        var printTypeExists = await dbContext.PrintTypes.AnyAsync(x => x.Id == printTypeId, cancellationToken);
        if (!printTypeExists) return new AddPrintOptionResult { PrintTypeNotFound = true };

        var item = new ProductPrintOption
        {
            ProductId = productId,
            PrintTypeId = printTypeId,
            ExtraPrice = extraPrice
        };

        dbContext.ProductPrintOptions.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new AddPrintOptionResult { Data = item };
    }
}
