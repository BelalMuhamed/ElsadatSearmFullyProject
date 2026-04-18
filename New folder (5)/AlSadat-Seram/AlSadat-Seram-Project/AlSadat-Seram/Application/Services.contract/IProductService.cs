using AlSadatSeram.Services.contract;
using Application.DTOs.ProductsDtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.ExcelReaderDtos;

namespace Application.Services.contract
{
    public interface IProductService
    {
        Task<ApiResponse<List<ProductDto>>> GetAllProducts(ProductFilterationDto req);
        Task<ProductDto?> GetByName(string productName);
        Task EditProduct(ProductDto product);
        Task AddNewProduct(ProductDto product);
        Task<List<Products>> GetAsync(string name);
        Task<ProductDto> GetByProductCode(string productCode);

        Task<bool> IsProductCodeExists(string productCode, int? excludeProductId = null);
        Task<ExcelReadResult<ExcelProductDto>> BulkAddFromExcel(Stream fileStream,string createdUser);

    }
}
