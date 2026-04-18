using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeSalary;
using Application.DTOs.RepresentativeDtos;
using Application.Helper;
using Domain.Common;

namespace Application.Services.contract.RepresentativeService;
public interface IRepresentativeService
{
    Task<Result<string>> AddRepresentativeAsync(RepresentativeDTo DTo);
    Task<Result<string>> SoftDeleteRepresentativeAsync(RepresentativeDTo DTo);
    Task<Result<string>> RestoreRepresentativeAsync(RepresentativeDTo DTo);
    Task<Result<string>> UpdateRepresentativeAsync(RepresentativeDTo DTo);
    Task<PagedList<RepresentativeDTo>> GetRepresentativeByFilterAsync(PaginationParams paginationParams,RepresentativeHelper search);


    Task<Result<RepresentativeSalaryDTo>> GetRepresentativeSalaryByYearAndMonth(string EmpCode,int? Month,int? Year);
    Task<Result<MonthlySalarySummaryDto>> GetRepresentativeMonthlySalarySummaryAsync(string empCode,int? month,int? year);
    Task<Result<MonthlyStatisticsDto>> GetRepresentativeMonthlyStatisticsAsync(string empCode,int? month,int? year);
    Task<Result<SalaryComparisonDto>> CompareRepresentativeMonthlySalariesAsync(string empCode,int baseMonth,int baseYear,int compareMonth,int compareYear);
    Task<Result<SalaryHistoryDto>> GetRepresentativeSalaryHistoryAsync(string empCode,int? year);
}
