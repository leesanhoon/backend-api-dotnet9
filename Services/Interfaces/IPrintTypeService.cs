using backend_api_dotnet9.Models;

namespace backend_api_dotnet9.Services.Interfaces;

public interface IPrintTypeService
{
    Task<IReadOnlyList<PrintType>> GetAllAsync(CancellationToken cancellationToken);
    Task<PrintType> CreateAsync(string name, int colorCount, string? description, CancellationToken cancellationToken);
}
