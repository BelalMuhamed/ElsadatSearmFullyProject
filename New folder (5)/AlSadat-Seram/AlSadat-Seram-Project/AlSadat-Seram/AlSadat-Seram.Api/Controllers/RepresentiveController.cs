using Application.CommonPagination;
using Application.DTOs.RepresentativeAttendanceDtos;
using Application.DTOs.RepresentativeDtos;
using Application.Helper;
using Application.Services.contract;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RepresentiveController:ControllerBase
{
    private readonly IServiceManager _ServiceManager;
    public RepresentiveController(IServiceManager ServiceManager)
    {
        _ServiceManager = ServiceManager;
    }

    #region Representives
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("GetAllRepresentives")]
    public async Task<ActionResult> GetAllRepresentives([FromQuery] PaginationParams paginationParams,[FromQuery] RepresentativeHelper search)
    {
        try
        {
            var res = await _ServiceManager._RepresentativeService.GetRepresentativeByFilterAsync(paginationParams,search);
            return Ok(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("AddNewRepresentive")]
    public async Task<ActionResult> AddNewRepresentive([FromBody] RepresentativeDTo DTo)
    {
        try
        {
            var res = await _ServiceManager._RepresentativeService.AddRepresentativeAsync(DTo);
            if(res.IsSuccess)
                return Ok("Add Representive Successful");
            return BadRequest(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("UpdateRepresentive")]
    public async Task<ActionResult> UpdateRepresentive([FromBody] RepresentativeDTo DTo)
    {
        try
        {
            var res = await _ServiceManager._RepresentativeService.UpdateRepresentativeAsync(DTo);
            if(res.IsSuccess)
                return Ok("Update Representive Successful");
            return BadRequest(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("SoftDeleteRepresentive")]
    public async Task<ActionResult> SoftDeleteRepresentive([FromBody]RepresentativeDTo DTo)
    {
        try
        {
            var res = await _ServiceManager._RepresentativeService.SoftDeleteRepresentativeAsync(DTo);
            if(res.IsSuccess)
                return Ok("Soft Delete Representive Successful");
            return BadRequest(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("RestoreRepresentive")]
    public async Task<ActionResult> RestoreRepresentive([FromBody] RepresentativeDTo DTo)
    {
        try
        {
            var res = await _ServiceManager._RepresentativeService.RestoreRepresentativeAsync(DTo);
            if(res.IsSuccess)
                return Ok("Activate Representive Successful");
            return BadRequest(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion


    #region RepresentiveAttendance
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("GetAllRepresentiveAttendance")]
    public async Task<IActionResult> GetAllRepresentiveAttendance([FromQuery] PaginationParams paginationParams, [FromQuery] RepresentativeAttendanceHelper search)
    {
        try
        {
            var res = await _ServiceManager.RepresentativeAttendanceService.GetRepresentativeAttendanceWithFilter(paginationParams, search);
            return Ok(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("UpdateRepresentiveAttendanceStatus")]
    public async Task<IActionResult> UpdateRepresentiveAttendanceStatus([FromBody] RepresentativeAttendanceDto DTo ,[FromQuery] AttendanceStatus status)
    {
        try
        {
            var res = await _ServiceManager.RepresentativeAttendanceService.UpdateRepresentativeAttendanceStatus(DTo,status);
            if(res.IsSuccess)
                return Ok("Update Representive Profile Successful");
            return BadRequest(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [Authorize(Roles = "Admin,HR,Representative")]
    [HttpPost()]
    public async Task<IActionResult> CheckIn ([FromBody] RepresentativeAttendanceHelper pramter)
    {
        try
        {
            var res = await _ServiceManager.RepresentativeAttendanceService.RepresentativeCheckIn(pramter);
            if(res.IsSuccess)
                return Ok(res);
            return BadRequest(res);
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    #endregion






}
