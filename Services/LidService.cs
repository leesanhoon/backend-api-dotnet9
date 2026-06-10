using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class LidService(AppDbContext dbContext) : ILidService
{
    public async Task<PagedResult<LidResponse>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Lids.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Prices)
            .OrderByDescending(x => x.Id);

        var totalCount = await query.CountAsync(cancellationToken);
        var lids = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<LidResponse>(lids.Select(MapToResponse).ToList(), totalCount, page, pageSize);
    }

    public async Task<LidResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Prices)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return lid is null ? null : MapToResponse(lid);
    }

    public async Task<CreateOrUpdateLidResult> CreateAsync(CreateLidRequest request, CancellationToken cancellationToken)
    {
        if (request.Prices.Count == 0)
            return new CreateOrUpdateLidResult { ValidationError = "Ít nhất 1 dòng giá theo miệng ly." };

        var duplicateDiameters = request.Prices.GroupBy(p => p.DiameterMm).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateDiameters.Count > 0)
            return new CreateOrUpdateLidResult { ValidationError = $"Trùng phi miệng ly: {string.Join(", ", duplicateDiameters)}mm." };

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return new CreateOrUpdateLidResult { CategoryNotFound = true };

        var lid = new Lid
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId
        };

        foreach (var p in request.Prices)
        {
            lid.Prices.Add(new LidPrice
            {
                DiameterMm = p.DiameterMm,
                SizeName = p.SizeName?.Trim(),
                UnitPrice = p.UnitPrice
            });
        }

        dbContext.Lids.Add(lid);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await LoadLidResponseAsync(lid.Id, cancellationToken);
        return new CreateOrUpdateLidResult { LidResponse = response };
    }

    public async Task<CreateOrUpdateLidResult> UpdateAsync(int id, CreateLidRequest request, CancellationToken cancellationToken)
    {
        if (request.Prices.Count == 0)
            return new CreateOrUpdateLidResult { ValidationError = "Ít nhất 1 dòng giá theo miệng ly." };

        var duplicateDiameters = request.Prices.GroupBy(p => p.DiameterMm).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateDiameters.Count > 0)
            return new CreateOrUpdateLidResult { ValidationError = $"Trùng phi miệng ly: {string.Join(", ", duplicateDiameters)}mm." };

        var lid = await dbContext.Lids.Include(x => x.Prices).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lid is null)
            return new CreateOrUpdateLidResult { LidNotFound = true };

        var categoryExists = await dbContext.Categories.AnyAsync(x => x.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            return new CreateOrUpdateLidResult { CategoryNotFound = true };

        lid.Name = request.Name.Trim();
        lid.Description = request.Description?.Trim();
        lid.CategoryId = request.CategoryId;

        dbContext.LidPrices.RemoveRange(lid.Prices);
        foreach (var p in request.Prices)
        {
            lid.Prices.Add(new LidPrice
            {
                DiameterMm = p.DiameterMm,
                SizeName = p.SizeName?.Trim(),
                UnitPrice = p.UnitPrice
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await LoadLidResponseAsync(lid.Id, cancellationToken);
        return new CreateOrUpdateLidResult { LidResponse = response };
    }

    public async Task<DeleteLidResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (lid is null)
            return new DeleteLidResult { NotFound = true };

        var hasLinkedProducts = await dbContext.ProductLids.AnyAsync(x => x.LidId == id, cancellationToken);
        if (hasLinkedProducts)
            return new DeleteLidResult { HasLinkedProducts = true };

        dbContext.Lids.Remove(lid);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new DeleteLidResult { Deleted = true };
    }

    private async Task<LidResponse> LoadLidResponseAsync(int id, CancellationToken cancellationToken)
    {
        var lid = await dbContext.Lids.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Prices)
            .FirstAsync(x => x.Id == id, cancellationToken);
        return MapToResponse(lid);
    }

    private static LidResponse MapToResponse(Lid lid)
    {
        var prices = lid.Prices
            .OrderBy(p => p.DiameterMm)
            .Select(p => new LidPriceResponse(p.Id, p.DiameterMm, p.SizeName, p.UnitPrice))
            .ToList();

        return new LidResponse(
            lid.Id,
            lid.Name,
            lid.Description,
            lid.CategoryId,
            lid.Category?.Name ?? string.Empty,
            prices);
    }
}
