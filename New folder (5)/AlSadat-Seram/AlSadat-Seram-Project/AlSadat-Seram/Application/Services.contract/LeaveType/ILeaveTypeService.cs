using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.LeaveType;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.LeaveType
{
    public interface ILeaveTypeService
    {
        Task<Result<string>> AddLeaveTypeAsync(LeaveTypeDto leaveTypeDto);
        Task<Result<string>> UpdateLeaveTypeAsync(LeaveTypeDto leaveTypeDto);
        Task<Result<string>> SoftDeleteLeaveTypeAsync(int id);
        Task<Result<string>> RestoreLeaveTypeAsync(int id);
        Task<Result<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id);
        Task<PagedList<LeaveTypeDto>> GetAllLeaveTypesAsync(PaginationParams paginationParams);
        Task<Result<List<LeaveTypeDto>>> GetActiveLeaveTypesAsync();
        Task<Result<bool>> CheckLeaveTypeUsageAsync(int leaveTypeId);
    }
}
