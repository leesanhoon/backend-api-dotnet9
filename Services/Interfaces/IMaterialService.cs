using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IMaterialService
{
    Task<IReadOnlyList<Material>> GetAllAsync(CancellationToken cancellationToken);
    Task<Material> CreateAsync(string name, string? description, CancellationToken cancellationToken);
}
