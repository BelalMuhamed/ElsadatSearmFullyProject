using Application.CommonPagination;
using Application.DTOs.EmployeeSalary;
using Application.Helper;
using Application.Services.contract;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// new
namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmployeeController : BaseApiController
{
    private readonly IServiceManager _ServiceManager;

    public EmployeeController(IServiceManager serviceManager, ILocalizationService localization)
        : base(localization)
    {
        _ServiceManager = serviceManager;
    }
    [Authorize(Policy = EmployeePermissions.View)]
    [HttpGet("GetAllEmployees")]
    public async Task<IActionResult> GetAllEmployees([FromQuery] PaginationParams paginationParams)
    {
        
        var result = await _ServiceManager.EmployeeService.GetAllEmployeeAsync(paginationParams);      
        return Ok(result);
    }
    [Authorize(Policy = EmployeePermissions.View)]
    [HttpGet("GetAllActiveEmployee")]
    public async Task<IActionResult> GetAllActiveEmployee([FromQuery] PaginationParams paginationParams)
    {
        var result = await _ServiceManager.EmployeeService.GetAllActiveEmployeeAsync(paginationParams);
        return Ok(result);
    }
    [Authorize(Policy = EmployeePermissions.View)]
    [HttpGet("GetEmployeeByFilterAsync")]
    public async Task<IActionResult> GetEmployeeByFilterAsync([FromQuery] PaginationParams paginationParams ,[FromQuery] EmployeeHelper search)
    {
        var result = await _ServiceManager.EmployeeService.GetEmployeeByFilterAsync(paginationParams,search);
        return Ok(result);
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("GetEmployeeSalaryByYearAndMonth")]
    public async Task<IActionResult> GetEmployeeSalaryByYearAndMonth(string EmpCode, int? Month, int? Year)
    {
        var result = await _ServiceManager.EmployeeService.GetEmployeeSalaryByYearAndMonth(EmpCode, Month, Year);
        return Ok(result);
    }
    // new
    [Authorize(Policy = EmployeePermissions.Create)]
    [HttpPost("AddNewEmployee")]
    public async Task<IActionResult> AddNewEmployee([FromBody] EmployeeDTo DTo)
    {
        var result = await _ServiceManager.EmployeeService.AddEmployeeAsync(DTo);
        return HandleResult(result);
    }
    [Authorize(Policy = EmployeePermissions.Update)]
    [HttpPut("UpdateEmployee")]
    public async Task<IActionResult> UpdateEmployee ([FromBody] EmployeeDTo DTo)
    {
        var result = await _ServiceManager.EmployeeService.UpdateEmployeeAsync(DTo);
        return HandleResult(result);
    }
    [Authorize(Policy = EmployeePermissions.Delete)]
    [HttpPut("SoftDeleteEmployee")]
    public async Task<IActionResult> SoftDeleteEmployee([FromBody] EmployeeDTo DTo)
    {
        var result = await _ServiceManager.EmployeeService.SoftDeleteEmployeeAsync(DTo);
            return HandleResult(result);
    }
    [Authorize(Policy = EmployeePermissions.Delete)]
    [HttpPut("RestoreEmployee")]
    public async Task<IActionResult> RestoreEmployee([FromBody] EmployeeDTo DTo)
    {
        var result = await _ServiceManager.EmployeeService.RestoreEmployeeAsync(DTo);
        return HandleResult(result);

    }
    [Authorize(Roles = "Admin,HR,Accountant")]
    [HttpGet("GetMonthlySalarySummary")]
    public async Task<IActionResult> GetMonthlySalarySummary(string empCode, int? month, int? year)
    {
        var result = await _ServiceManager.EmployeeService.GetMonthlySalarySummaryAsync(empCode, month, year);
        return Ok(result);
    }
    [Authorize(Roles = "Admin,HR,Accountant")]
    [HttpGet("GetMonthlyStatistics")]
    public async Task<IActionResult> GetMonthlyStatistics(string empCode, int? month, int? year)
    {
        var result = await _ServiceManager.EmployeeService.GetMonthlyStatisticsAsync(empCode, month, year);
        return Ok(result);
    }
    [Authorize(Roles = "Admin,HR,Accountant")]
    [HttpGet("CompareMonthlySalaries")]
    public async Task<IActionResult> CompareMonthlySalaries(
        string empCode,
        [FromQuery] int baseMonth,
        [FromQuery] int baseYear,
        [FromQuery] int compareMonth,
        [FromQuery] int compareYear)
    {
        var result = await _ServiceManager.EmployeeService.CompareMonthlySalariesAsync(
            empCode, baseMonth, baseYear, compareMonth, compareYear);
        return Ok(result);
    }
    [Authorize(Roles = "Admin,HR,Accountant")]
    [HttpGet("GetSalaryHistory")]
    public async Task<IActionResult> GetSalaryHistory(string empCode, [FromQuery] int? year = null)
    {
        var result = await _ServiceManager.EmployeeService.GetSalaryHistoryAsync(empCode, year);
        return Ok(result);
    }


}
