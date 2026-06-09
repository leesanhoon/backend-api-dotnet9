using backend_api_dotnet9.Data;
using backend_api_dotnet9.Models;
using backend_api_dotnet9.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend_api_dotnet9.Services;

public class PrintTypeService(AppDbContext dbContext) : IPrintTypeService
{
    public async Task<PagedResult<PrintType>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.PrintTypes.AsNoTracking().OrderBy(x => x.ColorCount).ThenBy(x => x.Name);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<PrintType>(items, totalCount, page, pageSize);
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
