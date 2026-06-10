namespace backend_api_dotnet9.Models;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record PaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;

    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize < 1 ? 10 : PageSize > 100 ? 100 : PageSize;
}
