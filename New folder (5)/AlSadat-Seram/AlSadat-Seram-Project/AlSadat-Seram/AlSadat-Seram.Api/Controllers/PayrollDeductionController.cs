using Application.CommonPagination;
using Application.DTOs.PayrollDeductions;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollDeductionController : ControllerBase
    {
        private readonly IServiceManager _ServiceManager;

        public PayrollDeductionController(IServiceManager serviceManager)
        {
            _ServiceManager = serviceManager;
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost("AddPayrollDeduction")]
        public async Task<IActionResult> AddPayrollDeduction([FromBody] PayrollDeductionsDto dto)
        {
            var result = await _ServiceManager.PayrollDeductionService.AddPayrollDeductionAsync(dto);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetAllPayrollDeductions")]
        public async Task<IActionResult> GetAllPayrollDeductions([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.PayrollDeductionService.GetAllPayrollDeductionsAsync(paginationParams);
            return Ok(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetPayrollDeductionById")]
        public async Task<IActionResult> GetPayrollDeductionById(int id)
        {
            var result = await _ServiceManager.PayrollDeductionService.GetPayrollDeductionByIdAsync(id);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPut("UpdatePayrollDeduction")]
        public async Task<IActionResult> UpdatePayrollDeduction([FromBody] PayrollDeductionsDto dto)
        {
            var result = await _ServiceManager.PayrollDeductionService.UpdatePayrollDeductionAsync(dto);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("SoftDeletePayrollDeduction")]
        public async Task<IActionResult> SoftDeletePayrollDeduction(int id)
        {
            var result = await _ServiceManager.PayrollDeductionService.SoftDeletePayrollDeductionAsync(id);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("RestorePayrollDeduction")]
        public async Task<IActionResult> RestorePayrollDeduction(int id)
        {
            var result = await _ServiceManager.PayrollDeductionService.RestorePayrollDeductionAsync(id);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetEmployeeDeductionsWithSummary")]
        public async Task<IActionResult> GetEmployeeDeductionsWithSummary(string empCode,[FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var result = await _ServiceManager.PayrollDeductionService.GetEmployeeDeductionsWithSummaryAsync(empCode, month, year);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("SearchPayrollDeductions")]
        public async Task<IActionResult> SearchPayrollDeductions( [FromBody] PayrollDeductionSearchDto searchDto,[FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.PayrollDeductionService.SearchPayrollDeductionsAsync(searchDto, paginationParams);
            return Ok(result);
        }
    }
}

