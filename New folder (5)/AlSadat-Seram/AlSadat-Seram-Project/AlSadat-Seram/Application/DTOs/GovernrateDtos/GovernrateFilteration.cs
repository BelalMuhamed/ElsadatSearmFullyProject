using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.GovernrateDtos
{
    public class GovernrateFilteration
    {
        public string? name { get; set; }
        public int? page { get; set; }
        public int? pageSize { get; set; }
    }
}
