using Application.CommonPagination;
using Application.DTOs.EmployeeLeaveBalance;
using Application.DTOs.EmployeeLeaveRequest;
using Application.Services.contract;
using Domain.Entities.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class EmployeeLeaveController:ControllerBase
    {
        private readonly IServiceManager _Servicmanger;

        public EmployeeLeaveController(IServiceManager servicmanger)
        {
            _Servicmanger = servicmanger;
        }
        [Authorize(Roles = "Admin,HR,Employee")]
        [HttpGet("GetMyLeaveRequests")]
        public async Task<IActionResult> GetMyLeaveRequests([FromQuery] PaginationParams paginationParams,string employeeEmail)
        {
            var result = await _Servicmanger.EmployeeLeaveService.GetEmployeeLeaveRequestsAsync(employeeEmail,paginationParams);
            return Ok(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetLeaveRequestById")]
        public async Task<IActionResult> GetLeaveRequest(int id)
        {
            var result = await _Servicmanger.EmployeeLeaveService.GetLeaveRequestByIdAsync(id);
            if(!result.IsSuccess)
                return NotFound(new { result.Message });

            return Ok(result.Data);
        }
        [Authorize(Roles = "Admin,HR,Employee")]
        [HttpPost("CreateLeaveRequest")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto requestDto)
        {

            var result = await _Servicmanger.EmployeeLeaveService.CreateLeaveRequestAsync(requestDto);
            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("CancelLeaveRequest")]
        public async Task<IActionResult> CancelLeaveRequest(int id,string employeeCode)
        {
            var result = await _Servicmanger.EmployeeLeaveService.CancelLeaveRequestAsync(id,employeeCode);
            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("SearchLeaveRequests")]
        public async Task<IActionResult> SearchLeaveRequests([FromQuery] LeaveRequestFilterDto filter)
        {
            var result = await _Servicmanger.EmployeeLeaveService.SearchLeaveRequestsAsync(filter);
            return Ok(result);
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("ApproveLeaveRequest")]
        public async Task<IActionResult> ApproveLeaveRequest([FromBody] ApproveRejectLeaveDto dto,string employeeCode)
        {
            var result = await _Servicmanger.EmployeeLeaveService.ApproveLeaveRequestAsync(dto.LeaveRequestId,dto.Reason);

            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("RejectLeaveRequest")]
        public async Task<IActionResult> RejectLeaveRequest([FromBody] ApproveRejectLeaveDto dto)
        {
            var result = await _Servicmanger.EmployeeLeaveService.RejectLeaveRequestAsync(dto.LeaveRequestId,dto.Reason ?? "");

            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetPendingRequests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var result = await _Servicmanger.EmployeeLeaveService.GetPendingLeaveRequestsAsync();
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("BulkApproveRequests")]
        public async Task<IActionResult> BulkApproveRequests([FromBody] List<int> requestIds)
        {
            var result = await _Servicmanger.EmployeeLeaveService.BulkApproveRequestsAsync(requestIds);

            return result.IsSuccess ?
                Ok(new { Message = result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR,Employee")]
        [HttpGet("GetEmplyeeLeaveBalance")]
        public async Task<IActionResult> GetEmplyeeLeaveBalance(string employeeName, [FromQuery] int? year = null)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var result = await _Servicmanger.EmployeeLeaveService.GetEmployeeLeaveBalanceAsync(employeeName,currentYear);
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR,Employee")]
        [HttpGet("GetMyLeaveBalance")]
        public async Task<IActionResult> GetMyLeaveBalance([FromQuery] int? year = null)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var result = await _Servicmanger.EmployeeLeaveService.GetLoginEmployeeLeaveBalanceAsync(currentYear);
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetLeaveBalanceByType")]
        public async Task<IActionResult> GetLeaveBalanceByType(string employeeCode,int leaveTypeId,[FromQuery] int? year = null)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var result = await _Servicmanger.EmployeeLeaveService.GetLeaveBalanceByTypeAsync(employeeCode,leaveTypeId,currentYear);
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("UpdateLeaveBalance")]
        public async Task<IActionResult> UpdateLeaveBalance([FromBody] EmployeeLeaveBalanceDto leaveBalance)
        {
            var result = await _Servicmanger.EmployeeLeaveService.UpdateLeaveBalanceAsync(leaveBalance);
            return result.IsSuccess ?
                Ok(new { result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("SetCustomLeaveBalance")]
        public async Task<IActionResult> SetCustomLeaveBalance([FromBody] SetCustomLeaveBalanceRequest request)
        {
            var result = await _Servicmanger.EmployeeLeaveService.SetCustomLeaveBalanceAsync(
                request.EmployeeCode,
                request.LeaveTypeId,
                request.OpeningBalance);
            return result.IsSuccess ?
                Ok(new { result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("InitializeLeaveBalance")]
        public async Task<IActionResult> InitializeLeaveBalance([FromBody] InitializeLeaveBalanceRequest request)
        {
            var result = await _Servicmanger.EmployeeLeaveService.InitializeLeaveBalanceAsync(
                request.EmployeeCode,
                request.Year);
            return result.IsSuccess ?
                Ok(new { result.Message }) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("CreateMultipleLeaveBalances")]
        public async Task<IActionResult> CreateMultipleLeaveBalances([FromBody] BulkLeaveBalanceRequestDto request)
        {
            var result = await _Servicmanger.EmployeeLeaveService.CreateMultipleLeaveBalancesAsync(request);
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("GetEmployeeLeaveTypesWithBalanceAsync")]
        public async Task<IActionResult> GetEmployeeLeaveTypesWithBalanceAsync(string employeeCode)
        {
            var result = await _Servicmanger.EmployeeLeaveService.GetEmployeeLeaveTypesWithBalanceAsync(employeeCode);
            return result.IsSuccess ?
                Ok(result.Data) :
                BadRequest(new { result.Message });
        }

    }

}
