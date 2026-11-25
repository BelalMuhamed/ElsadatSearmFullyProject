using Microsoft.EntityFrameworkCore;

namespace Application.CommonPagination;

/// <summary>Runs a query as one page of results.</summary>
public static class PagedResultExtensions
{
    /// <summary>
    /// Counts the query and reads the requested page. The query must be ordered and
    /// already projected to its DTO.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, PageRequest request, CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, request.Page, request.PageSize);
    }
}
