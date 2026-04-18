using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class StoreDto
    {
        public int? id { get; set; }
        public string storeName { get; set; }
        public bool? isDeleted { get; set; }

    }
    public class StoreDeleteDto
    {
        public int? id { get; set; }
        public string storeName { get; set; }
        public int transferedToStoreDto { get; set; }
        public string? makeActionUser { get; set; }

    }
    public class StoreFilteration
    {
        public string? storeName { get; set; }
        public bool? isDeleted { get; set; }
        public int? page { get; set; }
        public int? pageSize { get; set; }

    }
}
