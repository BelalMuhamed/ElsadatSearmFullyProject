// =============================================================================
using Application.DTOs.FinanceDtos.Reports;
using Domain.Common;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface IFinancialReportsService
    {
        Task<Result<CashReportDto>> GetCashReportAsync(CashReportReq req);

        /// <summary>Customer balances, cumulative through <c>asOfDate</c>.
        /// <c>fromDate</c> affects only the movement columns.</summary>
        Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(PartyBalancesReq req);

        /// <summary>Supplier balances, cumulative through <c>asOfDate</c>.
        /// <c>fromDate</c> affects only the movement columns.</summary>
        Task<Result<PartyBalancesReportDto>> GetSupplierBalancesAsync(PartyBalancesReq req);
        Task<Result<AgingReportDto>> GetReceivablesAgingAsync(AgingReportReq req);
        Task<Result<AgingReportDto>> GetPayablesAgingAsync(AgingReportReq req);
        Task<Result<InventoryMovementDto>> GetInventoryMovementAsync(InventoryMovementReq req);
        Task<Result<TrialBalanceDto>> GetTrialBalanceAsync(DateRangeReq req);
        Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(DateRangeReq req);
    }
}