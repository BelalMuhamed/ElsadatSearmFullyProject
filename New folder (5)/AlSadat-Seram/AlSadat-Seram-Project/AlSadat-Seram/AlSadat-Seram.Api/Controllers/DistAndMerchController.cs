using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DistAndMerchController : ControllerBase
    {
        private readonly IServiceManager serviceManager;
        private const long MaxImportBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly string[] AllowedExtensions =
        {
    ".xlsx",
    ".xls"
};
        public DistAndMerchController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        // -----------------------------------------------------------
        // 1) Add New Distributor or Merchant
        // -----------------------------------------------------------
        [HttpPost("add")]

        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddDistributorOrMerchant([FromBody] DistributorsAndMerchantsAndAgentsDto dto)
        {
            var result = await serviceManager.DistributorsAndMerchantsService.AddNewDistributorOrMerchant(dto);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        // -----------------------------------------------------------
        // 2) Edit Distributor or Merchant
        // -----------------------------------------------------------
        [Authorize(Roles = "Admin,Accountant")]
        [HttpPut("edit/{userId}")]
        public async Task<IActionResult> EditDistributorOrMerchant(string userId, [FromBody] DistributorsAndMerchantsAndAgentsDto dto)
        {
            dto.userId = userId; // inject from route

            var result = await serviceManager.DistributorsAndMerchantsService.EditDistributorOrMerchant(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }
        [Authorize(Roles = "Admin,Accountant")]
        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetDistributorOrMerchantById(string userId)
        {
            var result = await serviceManager.DistributorsAndMerchantsService.GetDistributorOrMerchantById(userId);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        // -----------------------------------------------------------
        // 4) Get All With Filters + Pagination
        // -----------------------------------------------------------
        [HttpGet("list")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetAllDistributorsAndMerchants([FromQuery] DistributorsAndMerchantsFilters filters)
       {
            try
            {
                var result = await serviceManager.DistributorsAndMerchantsService.GetAllDistributorsAndMerchants(filters);
                return Ok(result);
            }
            catch (Exception ex) 
            {
                return BadRequest(new {message= ex.Message });
            }
        }


        // -------------------------------------------------------------
        // 1) GET /api/DistributorsAndMerchants/import/template
        // download excel template
        // -------------------------------------------------------------
        [HttpGet("import/template")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DownloadImportTemplate(
            CancellationToken ct)
        {
            var result =
                await serviceManager
                    .DistributorsAndMerchantsService
                    .ExportTemplateAsync(ct);

            if (!result.IsSuccess || result.Data is null)
                return StatusCode(
                    (int)result.StatusCode,
                    result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DistributorsAndMerchants_Template.xlsx");
        }


        // -------------------------------------------------------------
        // 2) POST /api/DistributorsAndMerchants/import
        // upload excel file and import
        // -------------------------------------------------------------
        [HttpPost("import")]
        [RequestSizeLimit(MaxImportBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImportBytes)]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ImportFromExcel(
            IFormFile file,
          
            CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest(
                    Result<string>.Failure(
                        "الملف فارغ",
                        HttpStatusCode.BadRequest));

            if (file.Length > MaxImportBytes)
                return BadRequest(
                    Result<string>.Failure(
                        "حجم الملف أكبر من المسموح (5 ميجابايت)",
                        HttpStatusCode.BadRequest));

            var ext = Path.GetExtension(file.FileName)?
                .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(ext) ||
                !AllowedExtensions.Contains(ext))
            {
                return BadRequest(
                    Result<string>.Failure(
                        "نوع الملف غير مدعوم — استخدم .xlsx أو .xls",
                        HttpStatusCode.BadRequest));
            }

        

            await using var stream = file.OpenReadStream();

            var result =
                await serviceManager
                    .DistributorsAndMerchantsService
                    .ImportFromExcelAsync(
                        stream,
                       
                        ct);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode(
                    (int)result.StatusCode,
                    result);
        }
    }
}
