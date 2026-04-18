using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeAttendance;
using Application.Helper;
using Domain.Common;
using Domain.Entities.HR;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.EmployeeAttendanceService
{
    public interface IEmployeeAttendanceService
    {
        Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceForEmployeeByYearAndMonth(PaginationParams paginationParams , EmpAttendanceHelper Pramter);
        Task<PagedList<EmployeeAttendanceDTO>> GetTodayRecord(PaginationParams paginationParams);
        Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceByDateRange(PaginationParams paginationParams, EmpAttendanceHelper Pramter);
        Result<EmployeeAttendanceDTO> GetAttendanceForEmployeeByDate(EmpAttendanceHelper Pramter);
        Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceByEmployeeCode(PaginationParams paginationParams, EmpAttendanceHelper Pramter);
        Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceByEmployeeId(PaginationParams paginationParams, EmpAttendanceHelper Pramter);
        Task<PagedList<EmployeeAttendanceDTO>> GetAllAttendance(PaginationParams paginationParams);
        Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceWithFilter(PaginationParams paginationParams, EmpAttendanceHelper Pramter);
        Task<Result<string>> UpdateAttendanceStatus(EmployeeAttendanceDTO employeeAttendanceDTO , AttendanceStatus status);
        Task<Result<string>> CheckIn(EmpAttendanceHelper Pramter);
        Task<Result<string>> CheckOut(EmpAttendanceHelper Pramter);
        Task<Result<ExcelImportResultDTo>> ImportFromExcelAsync(Stream fileStream);

    }
}
