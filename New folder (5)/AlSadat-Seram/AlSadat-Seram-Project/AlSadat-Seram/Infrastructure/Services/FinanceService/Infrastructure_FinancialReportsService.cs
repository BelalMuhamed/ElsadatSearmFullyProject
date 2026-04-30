// =============================================================================
// File: Infrastructure/Services/FinanceService/FinancialReportsService.cs (NEW)
// =============================================================================
// Resolves accounts via SystemAccountCode (no magic strings).
// Uses projection (.Select) and AsNoTracking for performance.
// All queries filter by JournalEntry.IsPosted == true (mirrors your existing convention).

using Application.DTOs.FinanceDtos.Reports;
using Application.Services.contract.Finance;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.FinanceService
{
    public sealed class FinancialReportsService : IFinancialReportsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISystemAccountGuard _systemGuard;

        public FinancialReportsService(IUnitOfWork unitOfWork, ISystemAccountGuard guard)
        {
            _unitOfWork = unitOfWork;
            _systemGuard = guard;
        }

        // ============================================================
        // 1. CASH REPORT (Incoming / Outgoing)
        // ============================================================
        public async Task<Result<CashReportDto>> GetCashReportAsync(CashReportReq req)
        {
            try
            {
                var cash = await _systemGuard.GetBySystemCodeAsync(SystemAccountCode.Cash);
                var (from, to) = NormalizeRange(req.fromDate, req.toDate);

                var details = _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(d => d.JournalEntry)
                    .Where(d => d.AccountId == cash.Id && d.JournalEntry.IsPosted == true);

                // Opening balance = sum of (debit - credit) BEFORE fromDate
                var openingBalance = await details
                    .Where(d => d.JournalEntry.EntryDate < from)
                    .SumAsync(d => (decimal?)(d.Debit - d.Credit)) ?? 0m;

                var inRange = details
                    .Where(d => d.JournalEntry.EntryDate >= from && d.JournalEntry.EntryDate <= to);

                if (req.direction == 1) inRange = inRange.Where(d => d.Debit > 0);
                else if (req.direction == 2) inRange = inRange.Where(d => d.Credit > 0);

                var totalCount = await inRange.CountAsync();
                var totalIn = await inRange.SumAsync(d => (decimal?)d.Debit) ?? 0m;
                var totalOut = await inRange.SumAsync(d => (decimal?)d.Credit) ?? 0m;

                var movements = await inRange
                    .OrderByDescending(d => d.JournalEntry.EntryDate)
                    .ThenByDescending(d => d.Id)
                    .Skip((req.page - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .Select(d => new CashMovementDto
                    {
                        journalEntryId = d.JournalEntryId,
                        entryDate = d.JournalEntry.EntryDate,
                        description = d.JournalEntry.Desc,
                        referenceType = d.JournalEntry.referenceType.ToString(),
                        referenceNo = d.JournalEntry.ReferenceNo,
                        incoming = d.Debit,
                        outgoing = d.Credit
                    })
                    .ToListAsync();

                return Result<CashReportDto>.Success(new CashReportDto
                {
                    openingBalance = openingBalance,
                    totalIncoming = totalIn,
                    totalOutgoing = totalOut,
                    closingBalance = openingBalance + totalIn - totalOut,
                    movements = movements,
                    totalCount = totalCount,
                    page = req.page,
                    pageSize = req.pageSize
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<CashReportDto>.Failure("خطأ أثناء توليد تقرير الصندوق");
            }
        }

        // ============================================================
        // 2. CUSTOMER BALANCES
        // ============================================================
        public Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(DateRangeReq req)
            => GetPartyBalancesAsync(SystemAccountCode.ReceivablesParent, isReceivables: true, req);

        // ============================================================
        // 3. SUPPLIER BALANCES
        // ============================================================
        public Task<Result<PartyBalancesReportDto>> GetSupplierBalancesAsync(DateRangeReq req)
            => GetPartyBalancesAsync(SystemAccountCode.SuppliersParent, isReceivables: false, req);

        private async Task<Result<PartyBalancesReportDto>> GetPartyBalancesAsync(
            SystemAccountCode parentCode, bool isReceivables, DateRangeReq req)
        {
            try
            {
                var parent = await _systemGuard.GetBySystemCodeAsync(parentCode);
                var (from, to) = NormalizeRange(req.fromDate, req.toDate);

                // Find all sub-accounts under the parent (recursive — but typically one level deep)
                var allAccounts = await _unitOfWork
                    .GetRepository<ChartOfAccounts, int>()
                    .GetAllAsync();

                var partyAccountIds = CollectDescendantIds(parent.Id, allAccounts);

                var totals = await _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(d => partyAccountIds.Contains(d.AccountId)
                                && d.JournalEntry.IsPosted == true
                                && d.JournalEntry.EntryDate >= from
                                && d.JournalEntry.EntryDate <= to)
                    .GroupBy(d => d.AccountId)
                    .Select(g => new
                    {
                        accountId = g.Key,
                        debit = g.Sum(x => x.Debit),
                        credit = g.Sum(x => x.Credit),
                        lastDate = g.Max(x => x.JournalEntry.EntryDate)
                    })
                    .ToListAsync();

                var partyAccounts = allAccounts
                    .Where(a => partyAccountIds.Contains(a.Id) && a.IsLeaf)
                    .ToList();

                var rows = partyAccounts.Select(a =>
                {
                    var t = totals.FirstOrDefault(x => x.accountId == a.Id);
                    var debit = t?.debit ?? 0m;
                    var credit = t?.credit ?? 0m;
                    // Receivables: balance = debit - credit (positive means customer owes us)
                    // Payables:    balance = credit - debit (positive means we owe supplier)
                    var balance = isReceivables ? debit - credit : credit - debit;
                    return new PartyBalanceDto
                    {
                        accountId = a.Id,
                        accountCode = a.AccountCode,
                        accountName = a.AccountName,
                        userId = a.UserId,
                        totalDebit = debit,
                        totalCredit = credit,
                        balance = balance,
                        lastTransactionDate = t?.lastDate
                    };
                })
                .Where(r => r.balance != 0)
                .OrderByDescending(r => r.balance)
                .ToList();

                return Result<PartyBalancesReportDto>.Success(new PartyBalancesReportDto
                {
                    totalReceivables = isReceivables ? rows.Sum(r => r.balance) : 0m,
                    totalPayables = isReceivables ? 0m : rows.Sum(r => r.balance),
                    parties = rows
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<PartyBalancesReportDto>.Failure("خطأ أثناء توليد التقرير");
            }
        }

        // ============================================================
        // 4 & 5. AGING REPORTS (Receivables / Payables)
        // ============================================================
        public Task<Result<AgingReportDto>> GetReceivablesAgingAsync(AgingReportReq req)
            => GetAgingAsync(SystemAccountCode.ReceivablesParent, isReceivables: true, req);

        public Task<Result<AgingReportDto>> GetPayablesAgingAsync(AgingReportReq req)
            => GetAgingAsync(SystemAccountCode.SuppliersParent, isReceivables: false, req);

        private async Task<Result<AgingReportDto>> GetAgingAsync(
            SystemAccountCode parentCode, bool isReceivables, AgingReportReq req)
        {
            try
            {
                var asOf = (req.asOfDate ?? DateTime.UtcNow).Date;
                var parent = await _systemGuard.GetBySystemCodeAsync(parentCode);

                var allAccounts = await _unitOfWork
                    .GetRepository<ChartOfAccounts, int>().GetAllAsync();

                var partyIds = CollectDescendantIds(parent.Id, allAccounts);

                // Each unsettled detail is "aged" by days between its entry date and asOf.
                // Simple approach: bucket by entry date age (open invoice approximation).
                var details = await _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable().AsNoTracking()
                    .Where(d => partyIds.Contains(d.AccountId)
                                && d.JournalEntry.IsPosted == true
                                && d.JournalEntry.EntryDate <= asOf)
                    .Select(d => new
                    {
                        d.AccountId,
                        d.JournalEntry.EntryDate,
                        amount = isReceivables ? d.Debit - d.Credit : d.Credit - d.Debit
                    })
                    .ToListAsync();

                var partyAccounts = allAccounts
                    .Where(a => partyIds.Contains(a.Id) && a.IsLeaf)
                    .ToList();

                var rows = partyAccounts.Select(a =>
                {
                    var lines = details.Where(d => d.AccountId == a.Id).ToList();
                    var row = new AgingRowDto { accountId = a.Id, accountName = a.AccountName };
                    foreach (var l in lines)
                    {
                        var ageDays = (asOf - l.EntryDate.Date).Days;
                        if (ageDays <= 0) row.current += l.amount;
                        else if (ageDays <= req.bucket1Days) row.bucket1 += l.amount;
                        else if (ageDays <= req.bucket2Days) row.bucket2 += l.amount;
                        else if (ageDays <= req.bucket3Days) row.bucket3 += l.amount;
                        else row.over += l.amount;
                    }
                    row.total = row.current + row.bucket1 + row.bucket2 + row.bucket3 + row.over;
                    return row;
                })
                .Where(r => r.total != 0)
                .OrderByDescending(r => r.total)
                .ToList();

                var totals = new AgingRowDto
                {
                    accountName = "الإجمالي",
                    current = rows.Sum(r => r.current),
                    bucket1 = rows.Sum(r => r.bucket1),
                    bucket2 = rows.Sum(r => r.bucket2),
                    bucket3 = rows.Sum(r => r.bucket3),
                    over = rows.Sum(r => r.over),
                    total = rows.Sum(r => r.total)
                };

                return Result<AgingReportDto>.Success(new AgingReportDto
                {
                    asOfDate = asOf,
                    rows = rows,
                    totals = totals
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<AgingReportDto>.Failure("خطأ أثناء توليد تقرير الأعمار");
            }
        }

        // ============================================================
        // 6. INVENTORY MOVEMENT
        // ============================================================
        public async Task<Result<InventoryMovementDto>> GetInventoryMovementAsync(InventoryMovementReq req)
        {
            try
            {
                var inventory = await _systemGuard.GetBySystemCodeAsync(SystemAccountCode.Inventory);
                var (from, to) = NormalizeRange(req.fromDate, req.toDate);

                var details = _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable().AsNoTracking()
                    .Include(d => d.JournalEntry)
                    .Where(d => d.AccountId == inventory.Id && d.JournalEntry.IsPosted == true);

                var openingValue = await details
                    .Where(d => d.JournalEntry.EntryDate < from)
                    .SumAsync(d => (decimal?)(d.Debit - d.Credit)) ?? 0m;

                var inRange = details.Where(d => d.JournalEntry.EntryDate >= from
                                                  && d.JournalEntry.EntryDate <= to);

                var totalCount = await inRange.CountAsync();
                var totalIn = await inRange.SumAsync(d => (decimal?)d.Debit) ?? 0m;
                var totalOut = await inRange.SumAsync(d => (decimal?)d.Credit) ?? 0m;

                var rows = await inRange
                    .OrderByDescending(d => d.JournalEntry.EntryDate)
                    .Skip((req.page - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .Select(d => new InventoryMovementRowDto
                    {
                        date = d.JournalEntry.EntryDate,
                        referenceType = d.JournalEntry.referenceType.ToString(),
                        referenceNo = d.JournalEntry.ReferenceNo ?? string.Empty,
                        description = d.JournalEntry.Desc,
                        stockIn = d.Debit,
                        stockOut = d.Credit
                    })
                    .ToListAsync();

                return Result<InventoryMovementDto>.Success(new InventoryMovementDto
                {
                    openingValue = openingValue,
                    totalIn = totalIn,
                    totalOut = totalOut,
                    closingValue = openingValue + totalIn - totalOut,
                    rows = rows,
                    totalCount = totalCount,
                    page = req.page,
                    pageSize = req.pageSize
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<InventoryMovementDto>.Failure("خطأ أثناء توليد تقرير المخزون");
            }
        }

        // ============================================================
        // 7. TRIAL BALANCE
        // ============================================================
        public async Task<Result<TrialBalanceDto>> GetTrialBalanceAsync(DateRangeReq req)
        {
            try
            {
                var (from, to) = NormalizeRange(req.fromDate, req.toDate);
                var accounts = await _unitOfWork
                    .GetRepository<ChartOfAccounts, int>().GetAllAsync();

                var totals = await _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable().AsNoTracking()
                    .Where(d => d.JournalEntry.IsPosted == true
                                && d.JournalEntry.EntryDate >= from
                                && d.JournalEntry.EntryDate <= to)
                    .GroupBy(d => d.AccountId)
                    .Select(g => new
                    {
                        accountId = g.Key,
                        debit = g.Sum(x => x.Debit),
                        credit = g.Sum(x => x.Credit)
                    })
                    .ToListAsync();

                var rows = accounts
                    .Where(a => a.IsLeaf)
                    .Select(a =>
                    {
                        var t = totals.FirstOrDefault(x => x.accountId == a.Id);
                        return new TrialBalanceRowDto
                        {
                            accountCode = a.AccountCode,
                            accountName = a.AccountName,
                            type = (int)a.Type,
                            debit = t?.debit ?? 0m,
                            credit = t?.credit ?? 0m
                        };
                    })
                    .Where(r => r.debit != 0 || r.credit != 0)
                    .OrderBy(r => r.accountCode)
                    .ToList();

                return Result<TrialBalanceDto>.Success(new TrialBalanceDto
                {
                    asOfDate = to,
                    rows = rows,
                    totalDebit = rows.Sum(r => r.debit),
                    totalCredit = rows.Sum(r => r.credit)
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<TrialBalanceDto>.Failure("خطأ أثناء توليد ميزان المراجعة");
            }
        }

        // ============================================================
        // 8. INCOME STATEMENT (P&L)
        // ============================================================
        public async Task<Result<IncomeStatementDto>> GetIncomeStatementAsync(DateRangeReq req)
        {
            var trial = await GetTrialBalanceAsync(req);
            if (!trial.IsSuccess) return Result<IncomeStatementDto>.Failure(trial.Message);

            var (from, to) = NormalizeRange(req.fromDate, req.toDate);
            var revenue = trial.Data.rows.Where(r => r.type == (int)AccountTypes.Income).ToList();
            var expenses = trial.Data.rows.Where(r => r.type == (int)AccountTypes.Expenses).ToList();

            // For revenue (credit-normal): balance = credit - debit
            var totalRevenue = revenue.Sum(r => r.credit - r.debit);
            // For expenses (debit-normal): balance = debit - credit
            var totalExpenses = expenses.Sum(r => r.debit - r.credit);

            // COGS is an expense leaf marked with SystemAccountCode.CostOfGoodsSold
            var cogsAccount = await _systemGuard.GetBySystemCodeAsync(SystemAccountCode.CostOfGoodsSold);
            var cogsRow = expenses.FirstOrDefault(e => e.accountCode == cogsAccount.AccountCode);
            var totalCogs = cogsRow is null ? 0m : (cogsRow.debit - cogsRow.credit);

            return Result<IncomeStatementDto>.Success(new IncomeStatementDto
            {
                fromDate = from,
                toDate = to,
                totalRevenue = totalRevenue,
                totalCogs = totalCogs,
                grossProfit = totalRevenue - totalCogs,
                totalExpenses = totalExpenses,
                netIncome = totalRevenue - totalExpenses,
                revenueLines = revenue,
                expenseLines = expenses
            });
        }

        // ============================================================
        // Helpers
        // ============================================================
        private static (DateTime from, DateTime to) NormalizeRange(DateTime? from, DateTime? to)
        {
            var f = (from ?? DateTime.UtcNow.AddMonths(-1)).Date;
            var t = (to ?? DateTime.UtcNow).Date.AddDays(1).AddTicks(-1);
            return (f, t);
        }

        private static HashSet<int> CollectDescendantIds(int parentId, IEnumerable<ChartOfAccounts> all)
        {
            var ids = new HashSet<int> { parentId };
            void Walk(int id)
            {
                foreach (var c in all.Where(a => a.ParentAccountId == id))
                {
                    if (ids.Add(c.Id)) Walk(c.Id);
                }
            }
            Walk(parentId);
            return ids;
        }
    }
}
