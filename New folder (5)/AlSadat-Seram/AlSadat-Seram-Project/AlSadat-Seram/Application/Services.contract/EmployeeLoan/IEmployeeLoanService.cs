using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeLoan;
using Application.DTOs.EmployeeLoanPayments;
using Domain.Common;
using Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.EmployeeLoan
{
    public interface IEmployeeLoanService
    {
        // العمليات الأساسية
        Task<Result<EmployeeLoanDto>> CreateLoanAsync(CreateEmployeeLoanDto dto);
        Task<Result<EmployeeLoanDto>> UpdateLoanAsync(int loanId,UpdateEmployeeLoanDto dto);
        Task<Result<string>> ApproveLoanAsync(ApproveLoanDto dto);
        Task<Result<string>> RejectLoanAsync(RejectLoanDto dto);
        Task<Result<EmployeeLoanDto>> GetLoanByIdAsync(int id);
        Task<Result<EmployeeLoanDto>> GetLoanByNumberAsync(string loanNumber);
        Task<PagedList<EmployeeLoanDto>> GetEmployeeLoansAsync(string employeeCode,PaginationParams pagination);
        Task<PagedList<EmployeeLoanDto>> GetAllLoansAsync(PaginationParams pagination,LoanFilterDto? filter = null);

        // المدفوعات
        Task<Result<string>> MakePaymentAsync(LoanPaymentsDTo dto);
        Task<Result<List<EmployeeLoanPayments>>> GetLoanPaymentsAsync(int loanId);

        // العمليات التلقائية
        Task<Result<decimal>> CalculateEmployeeMonthlyDeductionAsync(string employeeCode,DateTime month);
        Task<Result<EmployeeLoanSummaryDto>> GetEmployeeLoanSummaryAsync(string employeeCode);

        // الإدارة
        Task<Result<string>> SoftDeleteLoanAsync(int loanId);
        Task<Result<string>> RestoreLoanAsync(int loanId);
    }

}
