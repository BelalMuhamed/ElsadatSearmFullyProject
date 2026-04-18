using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CityDtos
{
    public class CityFilteration
    {
        public int? page { get; set; }
        public int? pageSize { get; set; }
        public string? cityName { get; set; }
        public string? governrateName { get; set; }
    }
}
