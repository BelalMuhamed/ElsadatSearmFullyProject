


using Application.DTOs.FinanceDtos.Reports;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant")]
    public sealed class FinancialReportsController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public FinancialReportsController(IServiceManager serviceManager)
            => _serviceManager = serviceManager;

        /// <summary>تقرير حركة الصندوق (الوارد/الصادر)</summary>
        [HttpGet("cash")]
        public async Task<IActionResult> GetCashReport([FromQuery] CashReportReq req)
        {
            var result = await _serviceManager.financialReports.GetCashReportAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>أرصدة العملاء (المدينون)</summary>
        [HttpGet("customers/balances")]
        public async Task<IActionResult> GetCustomerBalances([FromQuery] DateRangeReq req)
        {
            var result = await _serviceManager.financialReports.GetCustomerBalancesAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>أرصدة الموردين (الدائنون)</summary>
        [HttpGet("suppliers/balances")]
        public async Task<IActionResult> GetSupplierBalances([FromQuery] DateRangeReq req)
        {
            var result = await _serviceManager.financialReports.GetSupplierBalancesAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>تقرير أعمار الديون – العملاء</summary>
        [HttpGet("customers/aging")]
        public async Task<IActionResult> GetReceivablesAging([FromQuery] AgingReportReq req)
        {
            var result = await _serviceManager.financialReports.GetReceivablesAgingAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>تقرير أعمار الديون – الموردين</summary>
        [HttpGet("suppliers/aging")]
        public async Task<IActionResult> GetPayablesAging([FromQuery] AgingReportReq req)
        {
            var result = await _serviceManager.financialReports.GetPayablesAgingAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>حركة المخزون (وارد/صادر) محاسبياً</summary>
        [HttpGet("inventory/movement")]
        public async Task<IActionResult> GetInventoryMovement([FromQuery] InventoryMovementReq req)
        {
            var result = await _serviceManager.financialReports.GetInventoryMovementAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>ميزان المراجعة</summary>
        [HttpGet("trial-balance")]
        public async Task<IActionResult> GetTrialBalance([FromQuery] DateRangeReq req)
        {
            var result = await _serviceManager.financialReports.GetTrialBalanceAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>قائمة الدخل (الأرباح والخسائر)</summary>
        [HttpGet("income-statement")]
        public async Task<IActionResult> GetIncomeStatement([FromQuery] DateRangeReq req)
        {
            var result = await _serviceManager.financialReports.GetIncomeStatementAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}

