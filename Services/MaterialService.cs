using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class MaterialService(AppDbContext dbContext) : IMaterialService
{
    public async Task<IReadOnlyList<Material>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Materials
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
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
