using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Distributors / Merchants / Agents (Clients module) endpoints.
    /// All endpoints return a uniform <see cref="Result{T}"/> / <see cref="ApiResponse{T}"/> envelope.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DistAndMerchController : ControllerBase
    {
        private const long MaxImportBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };

        private readonly IServiceManager _serviceManager;

        public DistAndMerchController(IServiceManager serviceManager)
            => _serviceManager = serviceManager;

        // -------------------------------------------------------------
        // 1) POST /api/DistAndMerch/add
        // -------------------------------------------------------------
        /// <summary>Create a new distributor / merchant / agent.</summary>
        [HttpPost("add")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddDistributorOrMerchant(
            [FromBody] DistributorsAndMerchantsAndAgentsDto dto)
        {
            var result = await _serviceManager.DistributorsAndMerchantsService
                                              .AddNewDistributorOrMerchant(dto);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 2) PUT /api/DistAndMerch/edit/{userId}
        // -------------------------------------------------------------
        /// <summary>Update an existing client. Address/City/Gender are optional.</summary>
        [HttpPut("edit/{userId}")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> EditDistributorOrMerchant(
            string userId,
            [FromBody] DistributorsAndMerchantsAndAgentsDto dto)
        {
            // Reject route/body mismatch — protect against ID tampering.
            if (!string.IsNullOrWhiteSpace(dto.userId) && dto.userId != userId)
                return BadRequest(Result<string>.Failure(
                    "تعارض في معرف المستخدم بين المسار والبيانات",
                    HttpStatusCode.BadRequest));

            dto.userId = userId;

            var result = await _serviceManager.DistributorsAndMerchantsService
                                              .EditDistributorOrMerchant(dto);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 3) GET /api/DistAndMerch/get/{userId}
        // -------------------------------------------------------------
        /// <summary>Get a single client by user id (used by the details popup).</summary>
        [HttpGet("get/{userId}")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetDistributorOrMerchantById(string userId)
        {
            var result = await _serviceManager.DistributorsAndMerchantsService
                                              .GetDistributorOrMerchantById(userId);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 4) GET /api/DistAndMerch/list
        // -------------------------------------------------------------
        /// <summary>Get a paginated, filterable list of clients.</summary>
        [HttpGet("list")]
        [Authorize(Roles = "Admin,Accountant,StockManager")]
        public async Task<IActionResult> GetAllDistributorsAndMerchants(
            [FromQuery] DistributorsAndMerchantsFilters filters)
        {
            // No try/catch here — global exception middleware preserves the Result envelope.
            var result = await _serviceManager.DistributorsAndMerchantsService
                                              .GetAllDistributorsAndMerchants(filters);
            return Ok(result);
        }

        // -------------------------------------------------------------
        // 5) GET /api/DistAndMerch/import/template
        // -------------------------------------------------------------
        /// <summary>Download the Excel import template.</summary>
        [HttpGet("import/template")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DownloadImportTemplate(CancellationToken ct)
        {
            var result = await _serviceManager.DistributorsAndMerchantsService
                                              .ExportTemplateAsync(ct);

            if (!result.IsSuccess || result.Data is null)
                return StatusCode((int)result.StatusCode, result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DistributorsAndMerchants_Template.xlsx");
        }

        // -------------------------------------------------------------
        // 6) POST /api/DistAndMerch/import
        // -------------------------------------------------------------
        /// <summary>Bulk import clients from an Excel file.</summary>
        [HttpPost("import")]
        [RequestSizeLimit(MaxImportBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImportBytes)]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ImportFromExcel(
            IFormFile file,
            CancellationToken ct)
        {
            var validation = ValidateImportFile(file);
            if (!validation.IsSuccess)
                return BadRequest(validation);

            await using var stream = file.OpenReadStream();
            var result = await _serviceManager.DistributorsAndMerchantsService
                                              .ImportFromExcelAsync(stream, ct);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // ===== private helpers =================================================

        private static Result<string> ValidateImportFile(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return Result<string>.Failure(
                    "الملف فارغ", HttpStatusCode.BadRequest);

            if (file.Length > MaxImportBytes)
                return Result<string>.Failure(
                    "حجم الملف أكبر من المسموح (5 ميجابايت)",
                    HttpStatusCode.BadRequest);

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(ext) || !AllowedExtensions.Contains(ext))
                return Result<string>.Failure(
                    "نوع الملف غير مدعوم — استخدم .xlsx أو .xls",
                    HttpStatusCode.BadRequest);

            return Result<string>.Success(string.Empty, HttpStatusCode.OK);
        }
    }
}