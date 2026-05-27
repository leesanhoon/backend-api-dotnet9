using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class PrintTypeService(AppDbContext dbContext) : IPrintTypeService
{
    public async Task<IReadOnlyList<PrintType>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.PrintTypes
            .AsNoTracking()
            .OrderBy(x => x.ColorCount)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PrintType> CreateAsync(string name, int colorCount, string? description, CancellationToken cancellationToken)
    {
        var item = new PrintType
        {
            Name = name.Trim(),
            ColorCount = colorCount,
            Description = description?.Trim()
        };

        dbContext.PrintTypes.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }
}
