using Application.CommonPagination;
using Application.DTOs.RepresentativeAttendanceDtos;
using Application.Helper;
using Application.Services.contract;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RepresentativeAttendanceController:ControllerBase
{
    private readonly IServiceManager _ServiceManager;

    public RepresentativeAttendanceController(IServiceManager serviceManager)
    {
        _ServiceManager = serviceManager;
    }
    [HttpGet("GetAllRepresentativeAttendance")]
    public async Task<IActionResult> GetAllRepresentativeAttendance([FromQuery] PaginationParams paginationParams,[FromQuery] RepresentativeAttendanceHelper Pramter)
    {
        try
        {
            var result = await _ServiceManager.RepresentativeAttendanceService.
            GetRepresentativeAttendanceWithFilter(paginationParams,Pramter);
            return Ok(result);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }
    //--------------------------------------------------------------
    [HttpPost("RepresentativeCheckIn")]
    public async Task<IActionResult> RepresentativeCheckIn([FromBody] RepresentativeAttendanceHelper pramter)
    {
        try
        {
            var result = await _ServiceManager.RepresentativeAttendanceService.RepresentativeCheckIn(pramter);
            if(result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    //--------------------------------------------------------------
    [HttpPut("UpdateRepresentativeAttendanceStatus")]
    public async Task<IActionResult> UpdateRepresentativeAttendanceStatus
        ([FromBody]RepresentativeAttendanceDto representativeAttendanceDto,[FromQuery]AttendanceStatus status)
    {
        try
        {
            var result = await _ServiceManager.RepresentativeAttendanceService.UpdateRepresentativeAttendanceStatus(representativeAttendanceDto,status);
            if(result.IsSuccess)
                return Ok(result);
            return BadRequest(result);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
