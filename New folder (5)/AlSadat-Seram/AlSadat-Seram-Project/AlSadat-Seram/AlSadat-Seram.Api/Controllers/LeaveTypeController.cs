using Application.CommonPagination;
using Application.DTOs.LeaveType;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Roles = "HR,Admin")]
    public class LeaveTypeController:ControllerBase
    {
        private readonly IServiceManager _Servicmanger;

        public LeaveTypeController(IServiceManager servicmanger)
        {
            _Servicmanger = servicmanger;
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetAllLeaveTypes")]
        public async Task<IActionResult> GetAllLeaveTypes([FromQuery] PaginationParams paginationParams)
        {
            var result = await _Servicmanger.LeaveTypeService.GetAllLeaveTypesAsync(paginationParams);
            return Ok(result);
        }

        [Authorize(Roles = "Admin,HR,Employee")]
        [HttpGet("GetActiveLeaveTypes")]
        //[AllowAnonymous]
        public async Task<IActionResult> GetActiveLeaveTypes()
        {
            var result = await _Servicmanger.LeaveTypeService.GetActiveLeaveTypesAsync();
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }

        [Authorize(Roles = "Admin,HR,Employee")]
        [HttpGet("GetLeaveTypeById")]
        public async Task<IActionResult> GetLeaveTypeById(int id)
        {
            var result = await _Servicmanger.LeaveTypeService.GetLeaveTypeByIdAsync(id);
            return result.IsSuccess ?
                Ok(result.Data) :
                NotFound(new { result.Message });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPost("AddLeaveType")]
        public async Task<IActionResult> AddLeaveType([FromBody] LeaveTypeDto leaveTypeDto)
        {
            var result = await _Servicmanger.LeaveTypeService.AddLeaveTypeAsync(leaveTypeDto);
            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPut("UpdateLeaveType")]
        public async Task<IActionResult> UpdateLeaveType(int id,[FromBody] LeaveTypeDto leaveTypeDto)
        {
            if(id != leaveTypeDto.Id)
                return BadRequest(new { Message = "معرف النوع غير متطابق" });

            var result = await _Servicmanger.LeaveTypeService.UpdateLeaveTypeAsync(leaveTypeDto);
            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("SoftDeleteLeaveType")]
        public async Task<IActionResult> SoftDeleteLeaveType(int id)
        {
            var result = await _Servicmanger.LeaveTypeService.SoftDeleteLeaveTypeAsync(id);
            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPut("RestoreLeaveType")]
        public async Task<IActionResult> RestoreLeaveType(int id)
        {
            var result = await _Servicmanger.LeaveTypeService.RestoreLeaveTypeAsync(id);
            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("CheckLeaveTypeUsage")]
        public async Task<IActionResult> CheckLeaveTypeUsage(int id)
        {
            var result = await _Servicmanger.LeaveTypeService.CheckLeaveTypeUsageAsync(id);
            return result.IsSuccess ?
                Ok(new { IsUsed = result.Data }) :
                BadRequest(new { result.Message });
        }
    }

}
