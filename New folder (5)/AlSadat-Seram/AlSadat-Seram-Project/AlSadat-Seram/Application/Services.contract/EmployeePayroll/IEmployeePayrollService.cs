using Application.DTOs.Payroll;
using Domain.Common;

namespace Application.Services.contract.EmployeePayroll
{
    public interface IEmployeePayrollService
    {
        // العمليات الفردية
        Task<Result<PayrollPreviewDto>> PreviewPayrollAsync(GeneratePayrollRequestDto request);  
        Task<Result<PayrollResponseDto>> GeneratePayrollAsync(GeneratePayrollRequestDto request);
        Task<Result<string>> PostPayrollToAccountingAsync(int payrollId,bool confirmLoans);
        Task<Result<string>> MarkPayrollAsPaidAsync(int payrollId, string paymentMethod, string? paymentReference = null);

        // العمليات الجماعية
        Task<Result<PreviewBulkPayrollDto>> PreviewBulkPayrollAsync(GenerateBulkPayrollRequestDto request);
        Task<Result<BulkPayrollResultDto>> GenerateBulkPayrollAsync(GenerateBulkPayrollRequestDto request);
        Task<Result<string>> PostBulkPayrollToAccountingAsync(List<int> payrollIds,bool confirmLoans);
        Task<Result<string>> MarkBulkPayrollAsPaidAsync(List<int> payrollIds, string paymentMethod, string? paymentReference = null);

        // التقارير والاستعلامات
        Task<Result<List<PayrollResponseDto>>> GetPayrollsByFilterAsync(PayrollFilterDto filter);
        Task<Result<PayrollExportDto>> ExportPayrollsToExcelAsync(PayrollFilterDto filter);
        Task<Result<PayrollSummaryDto>> GetPayrollSummaryAsync(int month, int year);
        Task<Result<PayrollResponseDto>> GetPayrollByIdAsync(int id);
        Task<Result<List<PayrollResponseDto>>> GetEmployeePayrollsAsync(string employeeCode, int? year = null);

        Task<Result<string>> DeletePayrollAsync(int PayrollID);



    }
}
