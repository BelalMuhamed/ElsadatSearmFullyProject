using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static Application.DTOs.SupplierDtos;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Supplier management endpoints.
    /// Roles: Admin, Accountant (enforced at the controller level).
    /// All endpoints return a uniform <see cref="Result{T}"/> envelope.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplierController : ControllerBase
    {
        private const long MaxImportBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".xlsx", ".xls" };

        private readonly IServiceManager _serviceManager;

        public SupplierController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // -------------------------------------------------------------
        // 1) GET /api/Supplier  — paginated + filterable list
        //    (URL preserved for compatibility with the existing Angular service)
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin,Accountant")]
        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers([FromQuery] SupplierFilteration filters)
        {
            var result = await _serviceManager.supplierService.GetAllSuppliers(filters);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 2) GET /api/Supplier/Supplier/Details?id=5  — single by id
        //    (URL preserved for compatibility with the existing Angular service)
        // -------------------------------------------------------------
        [HttpGet("Supplier/Details")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetSupplierById([FromQuery] int id)
        {
            var result = await _serviceManager.supplierService.GetById(id);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 3) GET /api/Supplier/lookups  — light list for select-boxes
        // -------------------------------------------------------------
        [Authorize(Roles = "Admin,Accountant,StockManager")]
        [HttpGet("lookups")]
        public async Task<IActionResult> GetSupplierLookups([FromQuery] SupplierLookupFilter filter)
        {
            var result = await _serviceManager.supplierService.GetSupplierLookups(filter);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 4) POST /api/Supplier  — create
        //    (URL preserved; internal C# method renamed to AddNewSupplier)
        // -------------------------------------------------------------
        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddNewSupplier([FromBody] SupplierDto dto)
        {
            var result = await _serviceManager.supplierService.AddNewSupplier(dto);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 5) PUT /api/Supplier/Edit  — update
        //    (URL preserved for compatibility with the existing Angular service)
        // -------------------------------------------------------------
        [HttpPut("Edit")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> EditSupplier([FromBody] SupplierDto dto)
        {
            var result = await _serviceManager.supplierService.EditSupplier(dto);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 6) PUT /api/Supplier/{id}/toggle-status
        //    Dedicated endpoint; project convention uses PUT for state flips.
        // -------------------------------------------------------------
        [HttpPut("{id:int}/toggle-status")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> ToggleStatus([FromRoute] int id)
        {
            var result = await _serviceManager.supplierService.ToggleSupplierStatus(id);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 7) GET /api/Supplier/import/template  — download the .xlsx template
        // -------------------------------------------------------------
        [HttpGet("import/template")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DownloadImportTemplate(CancellationToken ct)
        {
            var result = await _serviceManager.supplierService.GenerateImportTemplateAsync(ct);
            if (!result.IsSuccess || result.Data is null)
                return StatusCode((int)result.StatusCode, result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Suppliers_Template.xlsx");
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
            var result = await _serviceManager.supplierService.ImportFromExcelAsync(stream, ct);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }
    }
}