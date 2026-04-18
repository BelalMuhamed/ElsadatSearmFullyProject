using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ProductsDtos
{
    public class ProductDto
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string productCode { get; set; }
        public decimal sellingPrice { get; set; }
        public int pointPerUnit { get; set; }
        public string? createBy { get; set; }
        public DateTime? createAt { get; set; } = DateTime.UtcNow;
        public string? updateBy { get; set; }
        public DateTime? updateAt { get; set; }
        public bool isDeleted { get; set; }
        public string? deleteBy { get; set; }
        public DateTime? deleteAt { get; set; }
        public int theSmallestPossibleQuantity { get; set; }
        public int theHighestPossibleQuantity { get; set; }
        public int categoryId { get; set; }
        public string? categoryName { get; set; }


    }
}
