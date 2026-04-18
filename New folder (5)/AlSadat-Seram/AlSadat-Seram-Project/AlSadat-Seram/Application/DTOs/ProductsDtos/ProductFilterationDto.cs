using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ProductsDtos
{
    public class ProductFilterationDto
    {
        public string? name { get; set; }
        public bool? isDeleted { get; set; }
     
        public int? pageSize { get; set; }
        public int? page { get; set; }
    }
    
    }
