using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeLeaveBalance;
using Application.DTOs.EmployeeLeaveRequest;
using Domain.Common;

namespace Application.Services.contract.EmployeeLeave
{
    public interface IEmployeeLeaveService
    {

        // طلبات الإجازة
        Task<PagedList<EmployeeLeaveRequestDto>> GetEmployeeLeaveRequestsAsync(string employeeCode,PaginationParams paginationParams);
        Task<PagedList<EmployeeLeaveRequestDto>> SearchLeaveRequestsAsync(LeaveRequestFilterDto filter);
        Task<Result<EmployeeLeaveRequestDto>> GetLeaveRequestByIdAsync(int id);
        Task<Result<List<LeaveTypeBalanceDto>>> GetEmployeeLeaveTypesWithBalanceAsync(string employeeCode);
        Task<Result<string>> CreateLeaveRequestAsync(CreateLeaveRequestDto leaveRequest);
        //Task<Result<string>> UpdateLeaveRequestAsync(int id,EmployeeLeaveRequestDto leaveRequest);
        Task<Result<string>> ApproveLeaveRequestAsync(int leaveRequestId,string? reason = null);
        Task<Result<string>> RejectLeaveRequestAsync(int leaveRequestId,string reason);
        Task<Result<string>> CancelLeaveRequestAsync(int leaveRequestId,string cancelledBy);

        // رصيد الإجازات
        // رصيد الإجازات
        Task<Result<LeaveBalanceSummaryDto>> GetEmployeeLeaveBalanceAsync(string employeeName,int year);
        Task<Result<LeaveBalanceSummaryDto>> GetLoginEmployeeLeaveBalanceAsync(int year);
        Task<Result<EmployeeLeaveBalanceDto>> GetLeaveBalanceByTypeAsync(string employeeCode,int leaveTypeId,int year);
        Task<Result<string>> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto leaveBalance);
        Task<Result<string>> InitializeLeaveBalanceAsync(string employeeCode,int year);
        Task<Result<string>> SetCustomLeaveBalanceAsync(string employeeCode,int LeaveTypeId,int OpeningBalance);
        Task<Result<BulkLeaveBalanceResultDto>> CreateMultipleLeaveBalancesAsync(BulkLeaveBalanceRequestDto request);

        // دوال إضافية
        Task<Result<List<EmployeeLeaveRequestDto>>> GetPendingLeaveRequestsAsync();
        Task<Result<string>> BulkApproveRequestsAsync(List<int> requestIds);
    }

}
