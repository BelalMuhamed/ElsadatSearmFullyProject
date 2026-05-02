using Application.CommonPagination;
using Application.DTOs.EmployeeAttendance;
using Application.Helper;
using Application.Services.contract;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class EmployeeAttendanceController:ControllerBase
{
    private readonly IServiceManager _ServiceManager;

    public EmployeeAttendanceController(IServiceManager serviceManager)
    {
        _ServiceManager = serviceManager;
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("GetAllEmployeeAttendance")]
    public async Task<IActionResult> GetAllEmployeeAttendance([FromQuery] PaginationParams paginationParams)
    {
        var result = await _ServiceManager.EmployeeAttendanceService.GetAllAttendance(paginationParams);     
        return Ok(result);
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("GetEmployeeAttendanceByFilter")]
    public async Task<IActionResult> GetEmployeeAttendanceByFilter([FromQuery] PaginationParams paginationParams, [FromQuery] EmpAttendanceHelper pramter)
    {
        
        var result = await _ServiceManager.EmployeeAttendanceService.GetAttendanceWithFilter(paginationParams, pramter);
        return Ok(result);
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("GetTodayRecord")]
    public IActionResult GetTodayRecord([FromQuery] PaginationParams paginationParams)
    {
        var result = _ServiceManager.EmployeeAttendanceService
            .GetTodayRecord(paginationParams);
        return Ok(result);
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("CheckIn")]
    public async Task<IActionResult> CheckIn([FromBody] EmpAttendanceHelper pramter)
    {
        var result = await _ServiceManager.EmployeeAttendanceService.CheckIn(pramter);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result);
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("CheckOut")]
    public async Task<IActionResult> CheckOut([FromBody] EmpAttendanceHelper pramter)
    {
        var result = await _ServiceManager.EmployeeAttendanceService.CheckOut(pramter);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result);
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("ImportFromExcel")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var result = await _ServiceManager.EmployeeAttendanceService.ImportFromExcelAsync(memoryStream);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result);
    }
    //-------------------------------------------------------------
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("UpdateAttendanceStatus")]
    public async Task<IActionResult> UpdateAttendanceStatus([FromBody] EmployeeAttendanceDTO employeeAttendanceDTO, [FromQuery] AttendanceStatus status)
    {
        var result = await _ServiceManager.EmployeeAttendanceService.UpdateAttendanceStatus(employeeAttendanceDTO, status);
        if (result.IsSuccess)
            return Ok(result);
        return BadRequest(result);
    }

}
