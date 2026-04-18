using Application.DTOs.Payroll;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PayrollController:ControllerBase
{
    private readonly IServiceManager _ServiceManager;

    public PayrollController(IServiceManager serviceManager )
    {
        _ServiceManager = serviceManager;
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("PreviewPayroll")]
    public async Task<IActionResult> PreviewPayrollAsync([FromQuery] GeneratePayrollRequestDto request)
    {
        var result = await _ServiceManager.EmployeePayrollService.PreviewPayrollAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);

    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("PreviewBulkPayroll")]
    public async Task<IActionResult> PreviewBulkPayrollAsync([FromQuery] GenerateBulkPayrollRequestDto request)
    {
        var result = await _ServiceManager.EmployeePayrollService.PreviewBulkPayrollAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpPost("GeneratePayroll")]
    //[Authorize(Roles = "HR,PayrollManager,Admin")]
    public async Task<IActionResult> GeneratePayroll([FromBody] GeneratePayrollRequestDto request)
    {
        var result = await _ServiceManager.EmployeePayrollService.GeneratePayrollAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpPost("GenerateBulkPayroll")]
    //[Authorize(Roles = "HR,PayrollManager,Admin")]
    public async Task<IActionResult> GenerateBulkPayroll([FromBody] GenerateBulkPayrollRequestDto request)
    {
        var result = await _ServiceManager.EmployeePayrollService.GenerateBulkPayrollAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpPost("PostToAccounting")]
    //[Authorize(Roles = "Accountant,Admin")]
    public async Task<IActionResult> PostToAccounting(int payrollId,bool confirmLoans)
    {
        var result = await _ServiceManager.EmployeePayrollService.PostPayrollToAccountingAsync(payrollId, confirmLoans);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    [Authorize(Roles = "StaffOnly")]
    [HttpPost("PostBulkToAccounting")]
    //[Authorize(Roles = "Accountant,Admin")]
    public async Task<IActionResult> PostBulkToAccounting([FromBody] List<int> payrollIds,bool confirmLoans)
    {
        var result = await _ServiceManager.EmployeePayrollService.PostBulkPayrollToAccountingAsync(payrollIds,confirmLoans);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpPut("MarkAsPaid")]
    //[Authorize(Roles = "HR,PayrollManager,Admin")]
    public async Task<IActionResult> MarkAsPaid(int payrollId,[FromQuery] string paymentMethod,[FromQuery] string? paymentReference = null)
    {
        var result = await _ServiceManager.EmployeePayrollService.MarkPayrollAsPaidAsync(payrollId,paymentMethod,paymentReference);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpPut("MarkBulkAsPaid")]
    //[Authorize(Roles = "HR,PayrollManager,Admin")]
    public async Task<IActionResult> MarkBulkAsPaid([FromBody] MarkPayrollPaidDto dto)
    {
        var result = await _ServiceManager.EmployeePayrollService.MarkBulkPayrollAsPaidAsync(dto.PayrollIds,dto.PaymentMethod,dto.PaymentReference);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("GetPayrolls")]
    //[Authorize(Roles = "HR,PayrollManager,Admin,Accountant")]
    public async Task<IActionResult> GetPayrolls([FromQuery] PayrollFilterDto filter)
    {
        var result = await _ServiceManager.EmployeePayrollService.GetPayrollsByFilterAsync(filter);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("ExportToExcel")]
    //[Authorize(Roles = "HR,PayrollManager,Admin,Accountant")]
    public async Task<IActionResult> ExportToExcel([FromQuery] PayrollFilterDto filter)
    {
        var result = await _ServiceManager.EmployeePayrollService.ExportPayrollsToExcelAsync(filter);
        if(!result.IsSuccess)
            return BadRequest(result.Message);

        return File(result.Data.FileContent,result.Data.ContentType,result.Data.FileName);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("GetSummary")]
    //[Authorize(Roles = "HR,PayrollManager,Admin,Accountant")]
    public async Task<IActionResult> GetSummary(int month,int year)
    {
        var result = await _ServiceManager.EmployeePayrollService.GetPayrollSummaryAsync(month,year);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Message);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("GetPayrollById")]
    //[Authorize(Roles = "HR,PayrollManager,Admin,Accountant,Employee")]
    public async Task<IActionResult> GetPayrollById(int id)
    {
        var result = await _ServiceManager.EmployeePayrollService.GetPayrollByIdAsync(id);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpGet("GetEmployeePayrolls")]
    //[Authorize(Roles = "HR,PayrollManager,Admin,Accountant,Employee")]
    public async Task<IActionResult> GetEmployeePayrolls(string employeeCode,[FromQuery] int? year = null)
    {
        // التحقق من صلاحية الموظف لعرض كشوف راتبه فقط
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role);
        var currentEmployeeCode = User.FindFirstValue("EmployeeCode");

        if(currentUserRole == "Employee" && currentEmployeeCode != employeeCode)
            return Forbid();

        var result = await _ServiceManager.EmployeePayrollService.GetEmployeePayrollsAsync(employeeCode,year);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
    //[Authorize(Roles = "StaffOnly")]
    [HttpDelete("DeletePayroll")]
    public async Task<IActionResult> DeletePayrollAsync(int PayrollID)
    {
        var result = await _ServiceManager.EmployeePayrollService.DeletePayrollAsync(PayrollID);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

