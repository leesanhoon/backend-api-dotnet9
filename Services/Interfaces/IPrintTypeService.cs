using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IPrintTypeService
{
    Task<PagedResult<PrintType>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PrintType> CreateAsync(string name, int colorCount, string? description, CancellationToken cancellationToken);
}
