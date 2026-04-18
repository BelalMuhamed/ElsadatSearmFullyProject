using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.CityDtos
{
    public class CityDto
    {
        public int? id { get; set; }
        public string? cityName { get; set; }
        public int? governrateId { get; set; }
        public string? governrateName { get; set; }
    }
}
