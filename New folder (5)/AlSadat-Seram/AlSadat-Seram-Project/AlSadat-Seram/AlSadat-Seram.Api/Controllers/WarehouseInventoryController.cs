using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Stock;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Read-only inventory reporting endpoints — backs the مخزون page and the Excel export.
    /// Kept separate from <c>StockController</c> so the two responsibilities don't drift.
    /// </summary>
    [Route("api/warehouse-inventory")]
    [ApiController]
    [Authorize(Roles = "Admin,StockManager,Accountant")]
    public class WarehouseInventoryController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public WarehouseInventoryController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>Product-centric inventory matrix (rows = products, cols = warehouses).</summary>
        [HttpGet("matrix")]
        public async Task<IActionResult> GetMatrix(
            [FromQuery] WarehouseInventoryFilter filter,
            CancellationToken ct)
        {
            var result = await _serviceManager.warehouseInventoryReportService
                .GetInventoryMatrixAsync(filter, ct);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        /// <summary>Exports the inventory matrix as an .xlsx file.</summary>
        [HttpGet("matrix/export")]
        public async Task<IActionResult> ExportMatrix(
            [FromQuery] WarehouseInventoryFilter filter,
            CancellationToken ct)
        {
            var result = await _serviceManager.warehouseInventoryReportService
                .ExportInventoryMatrixToExcelAsync(filter, ct);

            if (!result.IsSuccess || result.Data is null)
                return StatusCode((int)result.StatusCode, result);

            return File(
                result.Data,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"warehouse-inventory-{System.DateTime.UtcNow:yyyyMMdd-HHmm}.xlsx");
        }
    }
}
