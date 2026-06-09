using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class MaterialService(AppDbContext dbContext) : IMaterialService
{
    public async Task<PagedResult<Material>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Materials.AsNoTracking().OrderBy(x => x.Name);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<Material>(items, totalCount, page, pageSize);
    }

    public async Task<Material> CreateAsync(string name, string? description, CancellationToken cancellationToken)
    {
        var item = new Material
        {
            Name = name.Trim(),
            Description = description?.Trim()
        };

        dbContext.Materials.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }
}
