using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.CommonPagination.Pagination
{
    public static class PaginationExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(
            this IQueryable<T> source,int pageNumber,int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<T>(items,count,pageNumber,pageSize);
        }
    }
}