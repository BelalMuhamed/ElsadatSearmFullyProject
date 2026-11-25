namespace Application.CommonPagination;

/// <summary>
/// The page a list request asks for: <c>?page=1&amp;pageSize=20</c>.
/// Out-of-range values are clamped instead of failing (D23).
/// </summary>
public class PageRequest
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    /// <summary>Last page whose row offset still fits in an int.</summary>
    public const int MaxPage = int.MaxValue / MaxPageSize;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>1-based page number.</summary>
    public int Page
    {
        get => _page;
        set => _page = Math.Clamp(value, 1, MaxPage);
    }

    /// <summary>Rows per page, between 1 and <see cref="MaxPageSize"/>.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
    }
}
