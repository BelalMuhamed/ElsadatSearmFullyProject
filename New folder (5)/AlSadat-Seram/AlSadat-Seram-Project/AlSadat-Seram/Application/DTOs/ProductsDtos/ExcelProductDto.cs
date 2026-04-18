using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.ProductsDtos
{
    public class ExcelProductDto
    {
        public string productName { get; set; }
        public string productCode { get; set; }
        public decimal sellingPrice { get; set; }
        public decimal pointsPerUnit { get; set; }
        public decimal minQuantity { get; set; }
    }
    public class ExcelReq
    {
        public IFormFile file { get; set; }
        public string  createdUser { get; set; }
    }
    public class ExcelUploadResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public List<ExcelReaderDtos.ExcelError> Errors { get; set; } = new();
    }
}
