using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeAttendance;
using Application.DTOs.RepresentativeAttendanceDtos;
using Application.Helper;
using Domain.Common;
using Domain.Enums;

namespace Application.Services.contract.RepresentativeAttendanceService;
    public interface IRepresentativeAttendanceService
    {     
        Task<PagedList<RepresentativeAttendanceDto>> GetRepresentativeAttendanceWithFilter(PaginationParams paginationParams,RepresentativeAttendanceHelper Pramter);
        Task<Result<string>> UpdateRepresentativeAttendanceStatus(RepresentativeAttendanceDto representativeAttendanceDto,AttendanceStatus status);
        Task<Result<string>> RepresentativeCheckIn(RepresentativeAttendanceHelper Pramter);
    }
