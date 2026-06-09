using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IMaterialService
{
    Task<PagedResult<Material>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<Material> CreateAsync(string name, string? description, CancellationToken cancellationToken);
}
