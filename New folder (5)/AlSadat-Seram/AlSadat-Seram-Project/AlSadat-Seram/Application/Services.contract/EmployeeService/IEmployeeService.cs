using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeSalary;
using Application.Helper;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.EmployeeService;
public interface IEmployeeService
{
    Task<Result<string>> AddEmployeeAsync(EmployeeDTo DTo);
    Task<Result<string>> SoftDeleteEmployeeAsync(EmployeeDTo DTo);
    Task<Result<string>> RestoreEmployeeAsync(EmployeeDTo DTo);
    Task<Result<string>> UpdateEmployeeAsync(EmployeeDTo DTo);
    Task<PagedList<EmployeeDTo>> GetAllEmployeeAsync(PaginationParams paginationParams);
    Task<PagedList<EmployeeDTo>> GetAllActiveEmployeeAsync(PaginationParams paginationParams);
    Task<PagedList<EmployeeDTo>> GetEmployeeByFilterAsync(PaginationParams paginationParams,EmployeeHelper search);
    Task<Result<EmployeeSalaryDTo>> GetEmployeeSalaryByYearAndMonth(string EmpCode,int? Month,int? Year);
    Task<Result<MonthlySalarySummaryDto>> GetMonthlySalarySummaryAsync(string empCode, int? month, int? year);
    Task<Result<MonthlyStatisticsDto>> GetMonthlyStatisticsAsync(string empCode, int? month, int? year);
    Task<Result<SalaryComparisonDto>> CompareMonthlySalariesAsync(string empCode, int baseMonth, int baseYear, int compareMonth, int compareYear);
    Task<Result<SalaryHistoryDto>> GetSalaryHistoryAsync(string empCode, int? year);
}
