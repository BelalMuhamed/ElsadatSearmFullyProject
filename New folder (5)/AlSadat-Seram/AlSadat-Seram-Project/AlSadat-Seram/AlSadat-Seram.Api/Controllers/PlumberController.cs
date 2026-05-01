using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Application.DTOs.PlumberDtos;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Plumber management endpoints.
    /// Roles: Admin, Accountant (mirrors Supplier's role policy).
    /// All endpoints return a uniform <see cref="Result{T}"/> envelope.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlumberController : ControllerBase
    {
        private const long MaxImportBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };

        private readonly IServiceManager _serviceManager;

        public PlumberController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // -------------------------------------------------------------
        // 1) GET /api/Plumber  — paginated + filterable list
        // -------------------------------------------------------------
        [HttpGet]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetAllPlumbers([FromQuery] PlumberFilteration filters)
        {
            var result = await _serviceManager.plumberService.GetAllPlumbers(filters);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 2) GET /api/Plumber/Plumber/Details?id=5  — single by id
        //    URL pattern follows Supplier for consistency.
        // -------------------------------------------------------------
        [HttpGet("Plumber/Details")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetPlumberById([FromQuery] int id)
        {
            var result = await _serviceManager.plumberService.GetById(id);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 3) GET /api/Plumber/lookups  — light list for select-boxes
        // -------------------------------------------------------------
        [HttpGet("lookups")]
        [Authorize(Roles = "Admin,Accountant,StockManager")]
        public async Task<IActionResult> GetPlumberLookups([FromQuery] PlumberLookupFilter filter)
        {
            var result = await _serviceManager.plumberService.GetPlumberLookups(filter);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 4) POST /api/Plumber  — create
        // -------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddNewPlumber([FromBody] PlumberDto dto)
        {
            var result = await _serviceManager.plumberService.AddNewPlumber(dto);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 5) PUT /api/Plumber/Edit  — update
        // -------------------------------------------------------------
        [HttpPut("Edit")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> EditPlumber([FromBody] PlumberDto dto)
        {
            var result = await _serviceManager.plumberService.EditPlumber(dto);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 6) PUT /api/Plumber/{id}/toggle-status
        // -------------------------------------------------------------
        [HttpPut("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            var result = await _serviceManager.plumberService.TogglePlumberStatus(id);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 7) GET /api/Plumber/import/template
        // -------------------------------------------------------------
        [HttpGet("import/template")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DownloadImportTemplate(CancellationToken ct)
        {
            var result = await _serviceManager.plumberService.GenerateImportTemplateAsync(ct);

            if (!result.IsSuccess || result.Data is null)
                return StatusCode((int)result.StatusCode, result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Plumbers_Template.xlsx");
        }

        // -------------------------------------------------------------
        // 8) POST /api/Plumber/import
        // -------------------------------------------------------------
        [HttpPost("import")]
        [RequestSizeLimit(MaxImportBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxImportBytes)]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ImportFromExcel(IFormFile file, CancellationToken ct)
        {
            var validation = ValidateImportFile(file);
            if (!validation.IsSuccess)
                return BadRequest(validation);

            await using var stream = file.OpenReadStream();
            var result = await _serviceManager.plumberService.ImportFromExcelAsync(stream, ct);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 9) GET /api/Plumber/export  — download current filtered list
        // -------------------------------------------------------------
        [HttpGet("export")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ExportToExcel(
            [FromQuery] PlumberFilteration filter,
            CancellationToken ct)
        {
            var result = await _serviceManager.plumberService.ExportToExcelAsync(filter, ct);

            if (!result.IsSuccess || result.Data is null)
                return StatusCode((int)result.StatusCode, result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Plumbers_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
        }

        // ===== private helpers =================================================

        private static Result<string> ValidateImportFile(IFormFile? file)
        {
            if (file is null || file.Length == 0)
                return Result<string>.Failure("الرجاء اختيار ملف", HttpStatusCode.BadRequest);

            if (file.Length > MaxImportBytes)
                return Result<string>.Failure(
                    "حجم الملف يتجاوز الحد المسموح به (5 ميجابايت)", HttpStatusCode.BadRequest);

            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
            if (!AllowedExtensions.Contains(ext))
                return Result<string>.Failure(
                    "الصيغة غير مدعومة — استخدم ملف Excel بصيغة .xlsx أو .xls",
                    HttpStatusCode.BadRequest);

            return Result<string>.Success(string.Empty);
        }
    }
}
