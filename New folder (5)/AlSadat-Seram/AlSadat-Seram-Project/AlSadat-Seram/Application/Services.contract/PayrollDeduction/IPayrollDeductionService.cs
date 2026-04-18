using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.PayrollDeductions;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.PayrollDeduction
{
    public interface IPayrollDeductionService
    {
        Task<Result<string>> AddPayrollDeductionAsync(PayrollDeductionsDto dto);
        Task<Result<string>> UpdatePayrollDeductionAsync(PayrollDeductionsDto dto);
        Task<Result<string>> SoftDeletePayrollDeductionAsync(int id);
        Task<Result<string>> RestorePayrollDeductionAsync(int id); 
        Task<Result<DeductionDetailDto>> GetPayrollDeductionByIdAsync(int id);
        Task<PagedList<DeductionDetailDto>> GetAllPayrollDeductionsAsync(PaginationParams paginationParams);
        Task<Result<EmployeeDeductionsSummaryDto>> GetEmployeeDeductionsWithSummaryAsync(string empCode,int? selectedMonth = null,int? selectedYear = null);
        Task<PagedList<DeductionDetailDto>> SearchPayrollDeductionsAsync(PayrollDeductionSearchDto searchDto,PaginationParams paginationParams);
    }
}
