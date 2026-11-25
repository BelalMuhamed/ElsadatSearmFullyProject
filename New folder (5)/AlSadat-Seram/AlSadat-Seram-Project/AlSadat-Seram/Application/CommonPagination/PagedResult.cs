namespace Application.CommonPagination;

/// <summary>
/// The only shape of a paged list (D23). Returned as the data of a <c>Result</c>.
/// </summary>
public sealed class PagedResult<T>(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = totalCount;
    public int TotalPages { get; } = (int)Math.Ceiling(totalCount / (double)pageSize);
}
