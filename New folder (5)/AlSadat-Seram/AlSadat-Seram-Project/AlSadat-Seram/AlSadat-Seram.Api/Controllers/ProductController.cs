using Application.DTOs.ProductsDtos;
using Application.Services.contract;
using Domain.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant,StockManager")]

    public class ProductController : ControllerBase
    {
        private const long MaxImportBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };
        private readonly IServiceManager serviceManager;

        public ProductController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllProducts([FromQuery] ProductFilterationDto filters)
        {
            try
            {
                var res = await serviceManager.ProductService.GetAllProducts(filters);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

       
        [HttpPost]
        public async Task<ActionResult> AddProduct(ProductDto dto)
        {
            try
            {
                // check name
                var existedByName = await serviceManager.ProductService.GetByName(dto.name);
                if (existedByName != null)
                    return BadRequest(new { message = "يوجد منتج بنفس الاسم" });

                // check productCode
                var codeExists = await serviceManager.ProductService
                    .IsProductCodeExists(dto.productCode);

                if (codeExists)
                    return BadRequest(new { message = "كود الصنف مستخدم من قبل" });

                await serviceManager.ProductService.AddNewProduct(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<ActionResult> EditProduct([FromBody] ProductDto dto)
        {
            try
            {
                // check productCode but exclude current product
                var codeExists = await serviceManager.ProductService
                    .IsProductCodeExists(dto.productCode, dto.id);

                if (codeExists)
                    return BadRequest(new { message = "كود الصنف مستخدم في صنف آخر" });

                await serviceManager.ProductService.EditProduct(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Route("Products/Details")]
        public async Task<ActionResult> GetByName(string productName)
        {
            try
            {
                var res = await serviceManager.ProductService.GetByName(productName);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut]
        [Route("toggle-status")]
        public async Task<ActionResult> ToggleStatus([FromBody] ProductDto dto)
        {
            try
            {

                await serviceManager.ProductService.EditProduct(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("import/template")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DownloadImportTemplate(CancellationToken ct)
        {
            var result = await serviceManager.ProductService.ExportProductsTemplateAsync(ct);
            if (!result.IsSuccess || result.Data is null)
                return StatusCode((int)result.StatusCode, result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "productstemplate.xlsx");
        }

        // -------------------------------------------------------------
        // 8) POST /api/Supplier/import  — upload + import an .xlsx
        //    - 5 MB request-size cap (S-6)
        //    - whitelist extensions
        //    - CancellationToken forwarded to the service (B-21)
        // -------------------------------------------------------------
        [HttpPost("import")]
        [RequestSizeLimit(MaxImportBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImportBytes)]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest(Result<string>.Failure("الملف فارغ", HttpStatusCode.BadRequest));

            if (file.Length > MaxImportBytes)
                return BadRequest(Result<string>.Failure(
                    "حجم الملف أكبر من المسموح (5 ميجابايت)", HttpStatusCode.BadRequest));

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
                return BadRequest(Result<string>.Failure(
                    "نوع الملف غير مدعوم — استخدم .xlsx أو .xls", HttpStatusCode.BadRequest));

            await using var stream = file.OpenReadStream();
            var result = await serviceManager.ProductService.ImportProductsFromExcelAsync(stream, ct);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

    }
}
