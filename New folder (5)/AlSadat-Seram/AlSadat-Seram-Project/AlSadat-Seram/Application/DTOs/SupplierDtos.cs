using Domain.Entities;
using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SupplierDtos
    {
        public class SupplierDto
        {
            public int? id { get; set; }
            public string name { get; set; }
            public string phoneNumbers { get; set; }
            public string? address { get; set; }
            public int cityId { get; set; }
            public string cityName { get; set; }
            public bool isDeleted { get; set; }
            public List<ProductsForSupplierDto>? products { get; set; }
          
        }
        public class SupplierFilteration
        {
            public string? name { get; set; }
            public string? phoneNumbers { get; set; }
            public bool? isDeleted { get; set; }
            public int? page { get; set; }
            public int? pageSize { get; set; }
        }
        public class ProductsForSupplierDto
        {
            public int productId { get; set; }
            public string? productName { get; set; }
           
           
        }
    }
}
