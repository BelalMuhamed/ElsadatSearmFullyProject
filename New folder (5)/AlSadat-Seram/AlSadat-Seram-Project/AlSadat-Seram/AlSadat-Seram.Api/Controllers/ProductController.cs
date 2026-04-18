using Application.DTOs.ProductsDtos;
using Application.Services.contract;
using Domain.Common;
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
        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(ExcelReq req)
        {
            if (req.file == null || req.file.Length == 0)
                return BadRequest(Result<string>.Failure("الملف فارغ"));

            try
            {
                using var stream = req.file.OpenReadStream();

                var res = await serviceManager.ProductService
                    .BulkAddFromExcel(stream, req.createdUser);

                var response = new ExcelUploadResponse<ExcelProductDto>
                {
                    Data = res.data,
                    Errors = res.errors
                };

                // حالة: كل الصفوف فشلت
                if (res.errors.Any() &&  res.data.Count==0)
                {
                    return Ok(Result<ExcelUploadResponse<ExcelProductDto>>.Failure(
                        $"فشل إضافة جميع المنتجات ({res.errors.Count} صفوف)"
                    ));
                }

                // حالة: بعض الصفوف فشل
                if (res.errors.Any())
                {
                    return Ok(Result<ExcelUploadResponse<ExcelProductDto>>.Success(
                        response,
                        $"تمت إضافة بعض المنتجات مع وجود أخطاء ({res.errors.Count} صفوف فشلت)"
                    ));
                }

                // نجاح كامل
                return Ok(Result<ExcelUploadResponse<ExcelProductDto>>.Success(
                    response,
                    "تمت إضافة جميع المنتجات بنجاح"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    Result<string>.Failure(ex.Message, HttpStatusCode.InternalServerError));
            }
        }

    }
}
