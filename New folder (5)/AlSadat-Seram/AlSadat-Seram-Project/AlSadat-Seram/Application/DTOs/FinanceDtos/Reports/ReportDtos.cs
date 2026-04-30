// All report request/response DTOs. Lower-case property names match the rest
// of your codebase (TreeAccountDto, AccountDto, etc.).

using System;
using System.Collections.Generic;

namespace Application.DTOs.FinanceDtos.Reports
{
    // ----- Common -----
    public class DateRangeReq
    {
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
    }

    // ----- 1. Cash Movement (Incoming / Outgoing) -----
    public sealed class CashReportReq : DateRangeReq
    {
        /// <summary>Filter: 0 = all, 1 = incoming only, 2 = outgoing only.</summary>
        public int direction { get; set; } = 0;
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }

    public sealed class CashMovementDto
    {
        public int journalEntryId { get; set; }
        public DateTime entryDate { get; set; }
        public string description { get; set; } = default!;
        public string? referenceType { get; set; }
        public string? referenceNo { get; set; }
        public decimal incoming { get; set; }   // Debit on cash account
        public decimal outgoing { get; set; }   // Credit on cash account
    }

    public sealed class CashReportDto
    {
        public decimal openingBalance { get; set; }
        public decimal totalIncoming { get; set; }
        public decimal totalOutgoing { get; set; }
        public decimal closingBalance { get; set; }
        public List<CashMovementDto> movements { get; set; } = new();
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }

    // ----- 2. Customer / Supplier Balances -----
    public sealed class PartyBalanceDto
    {
        public int accountId { get; set; }
        public string accountCode { get; set; } = default!;
        public string accountName { get; set; } = default!;
        public string? userId { get; set; }
        public decimal totalDebit { get; set; }
        public decimal totalCredit { get; set; }
        /// <summary>Positive = party owes us (receivable). Negative = we owe party (payable).</summary>
        public decimal balance { get; set; }
        public DateTime? lastTransactionDate { get; set; }
    }

    public sealed class PartyBalancesReportDto
    {
        public decimal totalReceivables { get; set; }   // For customers
        public decimal totalPayables { get; set; }      // For suppliers
        public List<PartyBalanceDto> parties { get; set; } = new();
    }

    // ----- 3. Aging Report (Receivable / Payable) -----
    public sealed class AgingReportReq
    {
        public DateTime? asOfDate { get; set; }
        public int bucket1Days { get; set; } = 30;
        public int bucket2Days { get; set; } = 60;
        public int bucket3Days { get; set; } = 90;
    }

    public sealed class AgingRowDto
    {
        public int accountId { get; set; }
        public string accountName { get; set; } = default!;
        public decimal current { get; set; }
        public decimal bucket1 { get; set; }   // 1-30 by default
        public decimal bucket2 { get; set; }   // 31-60
        public decimal bucket3 { get; set; }   // 61-90
        public decimal over { get; set; }      // 90+
        public decimal total { get; set; }
    }

    public sealed class AgingReportDto
    {
        public DateTime asOfDate { get; set; }
        public List<AgingRowDto> rows { get; set; } = new();
        public AgingRowDto totals { get; set; } = new();
    }

    // ----- 4. Inventory Movement -----
    public sealed class InventoryMovementReq : DateRangeReq
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
    }

    public sealed class InventoryMovementRowDto
    {
        public DateTime date { get; set; }
        public string referenceType { get; set; } = default!;
        public string referenceNo { get; set; } = default!;
        public string description { get; set; } = default!;
        public decimal stockIn { get; set; }    // Debit on inventory account (purchase)
        public decimal stockOut { get; set; }   // Credit on inventory account (sale → COGS)
    }

    public sealed class InventoryMovementDto
    {
        public decimal openingValue { get; set; }
        public decimal totalIn { get; set; }
        public decimal totalOut { get; set; }
        public decimal closingValue { get; set; }
        public List<InventoryMovementRowDto> rows { get; set; } = new();
        public int totalCount { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }

    // ----- 5. Trial Balance (recommended addition) -----
    public sealed class TrialBalanceRowDto
    {
        public string accountCode { get; set; } = default!;
        public string accountName { get; set; } = default!;
        public int type { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }

    public sealed class TrialBalanceDto
    {
        public DateTime asOfDate { get; set; }
        public List<TrialBalanceRowDto> rows { get; set; } = new();
        public decimal totalDebit { get; set; }
        public decimal totalCredit { get; set; }
        public bool isBalanced => totalDebit == totalCredit;
    }

    // ----- 6. Income Statement (P&L) (recommended addition) -----
    public sealed class IncomeStatementDto
    {
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public decimal totalRevenue { get; set; }
        public decimal totalCogs { get; set; }
        public decimal grossProfit { get; set; }
        public decimal totalExpenses { get; set; }
        public decimal netIncome { get; set; }
        public List<TrialBalanceRowDto> revenueLines { get; set; } = new();
        public List<TrialBalanceRowDto> expenseLines { get; set; } = new();
    }
}
