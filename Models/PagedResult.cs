namespace backend_api_dotnet9.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record PaginationRequest(int Page = 1, int PageSize = 10);
