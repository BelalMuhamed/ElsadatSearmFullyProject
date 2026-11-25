using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CommonPagination
{
    public class PaginationParams
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = Math.Clamp(value, 1, PageRequest.MaxPage);
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value, 1, PageRequest.MaxPageSize);
        }
    }
}
