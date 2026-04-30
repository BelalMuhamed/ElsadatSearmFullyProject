// =============================================================================
using Application.DTOs.FinanceDtos.Reports;
using Domain.Common;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface IFinancialReportsService
    {
        Task<Result<CashReportDto>> GetCashReportAsync(CashReportReq req);
        Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(DateRangeReq req);
        Task<Result<PartyBalancesReportDto>> GetSupplierBalancesAsync(DateRangeReq req);
        Task<Result<AgingReportDto>> GetReceivablesAgingAsync(AgingReportReq req);
        Task<Result<AgingReportDto>> GetPayablesAgingAsync(AgingReportReq req);
        Task<Result<InventoryMovementDto>> GetInventoryMovementAsync(InventoryMovementReq req);
        Task<Result<TrialBalanceDto>> GetTrialBalanceAsync(DateRangeReq req);
        Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(DateRangeReq req);
    }
}