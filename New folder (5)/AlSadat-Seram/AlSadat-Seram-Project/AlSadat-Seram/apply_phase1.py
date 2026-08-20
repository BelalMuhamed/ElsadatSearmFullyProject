#!/usr/bin/env python3
"""
Phase 1 patch applier — Supplier/Customer Balance Fix (cumulative balances).

WHY THIS EXISTS INSTEAD OF A .patch FILE:
A hand-authored unified diff needs exact line numbers and exact context lines
from your real files. I do not have your checkout, so the first attempt at a
line-numbered patch was corrupt. This script instead does anchor-based
find/replace: it looks for a unique block of text taken verbatim from what was
retrieved from the repository, and replaces it. If your file doesn't contain
that exact block, the script stops and tells you which file and which anchor
failed, instead of silently mis-patching or corrupting anything.

USAGE:
    python apply_phase1.py --repo "C:\\Users\\b.zakaria\\Desktop\\SR\\New folder (5)\\AlSadat-Seram\\AlSadat-Seram-Project\\AlSadat-Seram"
    python apply_phase1.py --repo /path/to/repo --dry-run     # check without writing
    python apply_phase1.py --repo /path/to/repo --check-only  # just report anchor status

Safe to re-run: file creation steps skip files that already exist with the
target content; replacement steps skip files where the NEW text is already
present (idempotent), and only fail if neither the OLD nor the NEW text is found.
"""
import argparse
import sys
from pathlib import Path

# ============================================================================
# NEW FILES — full content, no line-number dependency at all.
# ============================================================================

NEW_FILES = {
"Application/Common/AccountBalanceCalculator.cs": '''using Domain.Enums;

namespace Application.Common
{
    /// <summary>
    /// Pure, side-effect-free helpers for account-balance mathematics.
    /// <para>
    /// Centralising this in one place is deliberate: the balance of a ledger account
    /// depends on the account's normal direction, and any drift between the reporting
    /// service and the account-details service would show two different balances for
    /// the same party on the same date.
    /// </para>
    /// <para>
    /// All methods here are <b>pure</b>: no DB, no DI, no mutation of inputs.
    /// They are trivially unit-testable. Mirrors the existing
    /// <see cref="StockMovementCalculator"/> convention.
    /// </para>
    /// </summary>
    public static class AccountBalanceCalculator
    {
        /// <summary>
        /// Balance expressed in the account's <b>natural direction</b>, so a positive
        /// result always means "this account has a balance on its normal side".
        /// <list type="bullet">
        ///   <item>Debit-normal (Assets, Expenses): <c>debit - credit</c></item>
        ///   <item>Credit-normal (Liabilities, Equity, Income): <c>credit - debit</c></item>
        /// </list>
        /// For a supplier (Liabilities) a positive result means we owe the supplier.
        /// For a customer (Assets) a positive result means the customer owes us.
        /// </summary>
        /// <param name="type">The account's type. Direction is resolved via
        /// <see cref="AccountTypesExtensions.Nature"/> — never via a caller-supplied flag.</param>
        /// <param name="debit">Sum of debit movements over the period being measured.</param>
        /// <param name="credit">Sum of credit movements over the period being measured.</param>
        public static decimal NormalBalance(AccountTypes type, decimal debit, decimal credit)
            => NormalBalance(type.Nature(), debit, credit);

        /// <summary>
        /// Overload for callers that already resolved the account nature.
        /// </summary>
        public static decimal NormalBalance(AccountNature nature, decimal debit, decimal credit)
            => nature == AccountNature.Debit
                ? debit - credit
                : credit - debit;

        /// <summary>
        /// Signed movement of an account over a period, expressed in its natural direction.
        /// Identical maths to <see cref="NormalBalance(AccountTypes, decimal, decimal)"/> —
        /// exposed under an intention-revealing name so call sites read correctly
        /// when the inputs are period movements rather than cumulative totals.
        /// </summary>
        public static decimal NormalMovement(AccountTypes type, decimal periodDebit, decimal periodCredit)
            => NormalBalance(type, periodDebit, periodCredit);

        /// <summary>
        /// True when the account's normal balance sits on the debit side
        /// (Assets, Expenses) — i.e. it is a receivable-style account.
        /// </summary>
        public static bool IsDebitNormal(AccountTypes type)
            => type.Nature() == AccountNature.Debit;
    }
}
''',

"Application/DTOs/FinanceDtos/Reports/PartyBalancesReq.cs": '''using System;

namespace Application.DTOs.FinanceDtos.Reports
{
    /// <summary>
    /// Request for the customer / supplier balances report.
    /// <para>
    /// Inherits <see cref="DateRangeReq"/> deliberately, so existing clients that
    /// still send <c>fromDate</c> / <c>toDate</c> keep binding without change.
    /// </para>
    /// <para><b>Semantics — read this before changing anything here:</b></para>
    /// <list type="bullet">
    ///   <item>
    ///     <see cref="asOfDate"/> is the ONLY input that affects the reported balance.
    ///     The balance is cumulative from inception through this date.
    ///   </item>
    ///   <item>
    ///     <c>fromDate</c> (inherited) affects ONLY the movement columns
    ///     (openingBalance / periodDebit / periodCredit). It must NEVER reduce
    ///     closingBalance — doing so is precisely the defect this type was introduced
    ///     to prevent.
    ///   </item>
    ///   <item>
    ///     <c>toDate</c> (inherited) is retained for backward compatibility only and is
    ///     used as a fallback for <see cref="asOfDate"/> when the latter is not supplied.
    ///   </item>
    /// </list>
    /// </summary>
    public sealed class PartyBalancesReq : DateRangeReq
    {
        /// <summary>
        /// Cut-off date for the balance. Defaults to <c>toDate</c> when supplied,
        /// otherwise to today. The whole of the given day is included.
        /// </summary>
        public DateTime? asOfDate { get; set; }

        /// <summary>
        /// When true, parties whose closing balance is exactly zero are included.
        /// Defaults to false, preserving the previous behaviour of the report.
        /// </summary>
        public bool includeZeroBalances { get; set; } = false;
    }
}
''',

"Tests/Application.UnitTests/Application.UnitTests.csproj": '''<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\\..\\Application\\Application.csproj" />
    <ProjectReference Include="..\\..\\Domain\\Domain.csproj" />
  </ItemGroup>

</Project>
''',

"Tests/Application.UnitTests/Common/AccountBalanceCalculatorTests.cs": '''using Application.Common;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Common
{
    /// <summary>
    /// Unit tests for <see cref="AccountBalanceCalculator"/>.
    /// Pure maths — no DB, no fixtures, no mocks.
    /// </summary>
    public class AccountBalanceCalculatorTests
    {
        // -----------------------------------------------------------------
        // Direction resolution
        // -----------------------------------------------------------------
        [Theory]
        [InlineData(AccountTypes.Assets, true)]
        [InlineData(AccountTypes.Expenses, true)]
        [InlineData(AccountTypes.Liabilities, false)]
        [InlineData(AccountTypes.Equity, false)]
        [InlineData(AccountTypes.Income, false)]
        public void IsDebitNormal_matches_the_accounting_convention(AccountTypes type, bool expected)
        {
            Assert.Equal(expected, AccountBalanceCalculator.IsDebitNormal(type));
        }

        // -----------------------------------------------------------------
        // Debit-normal accounts (customers sit under Assets / ReceivablesParent)
        // -----------------------------------------------------------------
        [Fact]
        public void Debit_normal_account_returns_debit_minus_credit()
        {
            var balance = AccountBalanceCalculator.NormalBalance(
                AccountTypes.Assets, debit: 100_000m, credit: 40_000m);

            Assert.Equal(60_000m, balance);
        }

        // -----------------------------------------------------------------
        // Credit-normal accounts (suppliers sit under Liabilities / SuppliersParent)
        // -----------------------------------------------------------------
        [Fact]
        public void Credit_normal_account_returns_credit_minus_debit()
        {
            var balance = AccountBalanceCalculator.NormalBalance(
                AccountTypes.Liabilities, debit: 40_000m, credit: 100_000m);

            Assert.Equal(60_000m, balance);
        }

        /// <summary>
        /// The scenario that motivated Phase 1.
        /// Supplier: purchase invoice 100,000 (credit) then payment 40,000 (debit).
        /// The payable must read 60,000 — never -40,000.
        /// </summary>
        [Fact]
        public void Supplier_with_invoice_then_payment_reports_the_remaining_payable()
        {
            const decimal purchaseInvoice = 100_000m;   // Cr supplier
            const decimal payment = 40_000m;            // Dr supplier

            var balance = AccountBalanceCalculator.NormalBalance(
                AccountTypes.Liabilities, debit: payment, credit: purchaseInvoice);

            Assert.Equal(60_000m, balance);
            Assert.NotEqual(-40_000m, balance);
        }

        /// <summary>
        /// A fully reversed or fully paid supplier nets to zero, not to a negative figure.
        /// </summary>
        [Fact]
        public void Fully_settled_supplier_nets_to_zero()
        {
            var balance = AccountBalanceCalculator.NormalBalance(
                AccountTypes.Liabilities, debit: 100_000m, credit: 100_000m);

            Assert.Equal(0m, balance);
        }

        /// <summary>
        /// An overpaid supplier legitimately goes negative (we are in advance).
        /// The calculator must not clamp.
        /// </summary>
        [Fact]
        public void Overpaid_supplier_returns_a_negative_balance()
        {
            var balance = AccountBalanceCalculator.NormalBalance(
                AccountTypes.Liabilities, debit: 120_000m, credit: 100_000m);

            Assert.Equal(-20_000m, balance);
        }

        [Fact]
        public void No_movement_returns_zero()
        {
            Assert.Equal(0m, AccountBalanceCalculator.NormalBalance(AccountTypes.Assets, 0m, 0m));
            Assert.Equal(0m, AccountBalanceCalculator.NormalBalance(AccountTypes.Liabilities, 0m, 0m));
        }

        /// <summary>
        /// Regression guard: the new Nature()-driven calculation must produce exactly what the
        /// old hardcoded isReceivables flag produced, for the two seeded parent accounts.
        ///   ReceivablesParent (Id 8,  "1.1.2") => Assets      => isReceivables: true
        ///   SuppliersParent   (Id 10, "2.1")   => Liabilities => isReceivables: false
        /// </summary>
        [Theory]
        [InlineData(AccountTypes.Assets, true)]
        [InlineData(AccountTypes.Liabilities, false)]
        public void Matches_the_legacy_isReceivables_behaviour(AccountTypes type, bool legacyIsReceivables)
        {
            const decimal debit = 73_500.25m;
            const decimal credit = 12_100.75m;

            var legacy = legacyIsReceivables ? debit - credit : credit - debit;
            var actual = AccountBalanceCalculator.NormalBalance(type, debit, credit);

            Assert.Equal(legacy, actual);
        }

        /// <summary>Decimal precision is preserved — no float drift, no rounding.</summary>
        [Fact]
        public void Preserves_decimal_precision()
        {
            var balance = AccountBalanceCalculator.NormalBalance(
                AccountTypes.Liabilities, debit: 0.10m, credit: 0.30m);

            Assert.Equal(0.20m, balance);
        }

        [Fact]
        public void NormalMovement_is_the_same_maths_under_an_intention_revealing_name()
        {
            Assert.Equal(
                AccountBalanceCalculator.NormalBalance(AccountTypes.Liabilities, 10m, 30m),
                AccountBalanceCalculator.NormalMovement(AccountTypes.Liabilities, 10m, 30m));
        }
    }
}
''',
}

# ============================================================================
# EXISTING FILES — anchor-based replacement. OLD is verbatim from what was
# retrieved from the repository. If OLD is not found verbatim (whitespace,
# reformatting, or the file has diverged), the script stops and tells you.
# ============================================================================

REPLACEMENTS = {

"Application/DTOs/FinanceDtos/Reports/ReportDtos.cs": [
(
'''    // ----- 2. Customer / Supplier Balances -----
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
    }''',
'''    // ----- 2. Customer / Supplier Balances -----
    public sealed class PartyBalanceDto
    {
        public int accountId { get; set; }
        public string accountCode { get; set; } = default!;
        public string accountName { get; set; } = default!;
        public string? userId { get; set; }
        /// <summary>Cumulative debits from inception through asOfDate.</summary>
        public decimal totalDebit { get; set; }
        /// <summary>Cumulative credits from inception through asOfDate.</summary>
        public decimal totalCredit { get; set; }

        /// <summary>Balance in the account's natural direction as at (fromDate - 1 tick).
        /// Zero when fromDate is not supplied.</summary>
        public decimal openingBalance { get; set; }
        /// <summary>Debits inside [fromDate, asOfDate]. Equals totalDebit when fromDate is null.</summary>
        public decimal periodDebit { get; set; }
        /// <summary>Credits inside [fromDate, asOfDate]. Equals totalCredit when fromDate is null.</summary>
        public decimal periodCredit { get; set; }
        /// <summary>Cumulative balance through asOfDate, in the account's natural direction.
        /// This is the authoritative figure and is never reduced by fromDate.</summary>
        public decimal closingBalance { get; set; }

        /// <summary>Alias of <see cref="closingBalance"/>, retained so existing clients
        /// keep working. Positive = the account has a balance on its normal side:
        /// for a customer, they owe us; for a supplier, we owe them.</summary>
        public decimal balance { get; set; }
        public DateTime? lastTransactionDate { get; set; }
    }

    public sealed class PartyBalancesReportDto
    {
        /// <summary>Cut-off actually applied to the balances.</summary>
        public DateTime asOfDate { get; set; }
        /// <summary>Movement-window start actually applied. Null = since inception.</summary>
        public DateTime? fromDate { get; set; }
        public decimal totalReceivables { get; set; }   // For customers
        public decimal totalPayables { get; set; }      // For suppliers
        public List<PartyBalanceDto> parties { get; set; } = new();
    }''',
),
],

"Application/Services.contract/Finance/IFinancialReportsService.cs": [
(
'''        Task<Result<CashReportDto>> GetCashReportAsync(CashReportReq req);
        Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(DateRangeReq req);
        Task<Result<PartyBalancesReportDto>> GetSupplierBalancesAsync(DateRangeReq req);
        Task<Result<AgingReportDto>> GetReceivablesAgingAsync(AgingReportReq req);''',
'''        Task<Result<CashReportDto>> GetCashReportAsync(CashReportReq req);

        /// <summary>Customer balances, cumulative through <c>asOfDate</c>.
        /// <c>fromDate</c> affects only the movement columns.</summary>
        Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(PartyBalancesReq req);

        /// <summary>Supplier balances, cumulative through <c>asOfDate</c>.
        /// <c>fromDate</c> affects only the movement columns.</summary>
        Task<Result<PartyBalancesReportDto>> GetSupplierBalancesAsync(PartyBalancesReq req);
        Task<Result<AgingReportDto>> GetReceivablesAgingAsync(AgingReportReq req);''',
),
],

"AlSadat-Seram.Api/Controllers/FinancialReportsController.cs": [
(
'''        /// <summary>أرصدة العملاء (المدينون)</summary>
        [HttpGet("customers/balances")]
        public async Task<IActionResult> GetCustomerBalances([FromQuery] DateRangeReq req)
        {
            var result = await _serviceManager.financialReports.GetCustomerBalancesAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>أرصدة الموردين (الدائنون)</summary>
        [HttpGet("suppliers/balances")]
        public async Task<IActionResult> GetSupplierBalances([FromQuery] DateRangeReq req)
        {
            var result = await _serviceManager.financialReports.GetSupplierBalancesAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }''',
'''        /// <summary>أرصدة العملاء (المدينون)</summary>
        [HttpGet("customers/balances")]
        public async Task<IActionResult> GetCustomerBalances([FromQuery] PartyBalancesReq req)
        {
            var result = await _serviceManager.financialReports.GetCustomerBalancesAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>أرصدة الموردين (الدائنون)</summary>
        [HttpGet("suppliers/balances")]
        public async Task<IActionResult> GetSupplierBalances([FromQuery] PartyBalancesReq req)
        {
            var result = await _serviceManager.financialReports.GetSupplierBalancesAsync(req);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }''',
),
],

"Infrastructure/Services/FinanceService/Infrastructure_FinancialReportsService.cs": [
# --- 1) using block -----------------------------------------------------
(
'''using Application.DTOs.FinanceDtos.Reports;
using Application.Services.contract.Finance;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.Net;''',
'''using Application.Common;
using Application.DTOs.FinanceDtos.Reports;
using Application.Services.contract.Finance;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.Net;''',
),
# --- 2) the whole GetPartyBalancesAsync method + its two callers -------
(
'''        public Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(DateRangeReq req)
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
        }''',
'''        public Task<Result<PartyBalancesReportDto>> GetCustomerBalancesAsync(PartyBalancesReq req)
            => GetPartyBalancesAsync(SystemAccountCode.ReceivablesParent, req);

        // ============================================================
        // 3. SUPPLIER BALANCES
        // ============================================================
        public Task<Result<PartyBalancesReportDto>> GetSupplierBalancesAsync(PartyBalancesReq req)
            => GetPartyBalancesAsync(SystemAccountCode.SuppliersParent, req);

        /// <summary>
        /// Party (customer / supplier) balances.
        /// <para>
        /// A BALANCE is cumulative from inception through <c>asOfDate</c>. It must never be
        /// restricted by a lower date bound — that turns the figure into a period movement and
        /// makes a payment or a reversal appear to push the balance negative when the original
        /// invoice happens to fall outside the requested window.
        /// </para>
        /// <para>
        /// <c>fromDate</c> is optional and affects ONLY the movement columns
        /// (openingBalance / periodDebit / periodCredit). It never affects closingBalance.
        /// </para>
        /// <para>
        /// Direction comes from each account's own <see cref="AccountTypes"/> via
        /// <see cref="AccountBalanceCalculator"/>, not from a caller-supplied flag —
        /// the account already carries that information.
        /// </para>
        /// </summary>
        private async Task<Result<PartyBalancesReportDto>> GetPartyBalancesAsync(
            SystemAccountCode parentCode, PartyBalancesReq req)
        {
            try
            {
                var parent = await _systemGuard.GetBySystemCodeAsync(parentCode);

                // Cut-off for the balance. Whole of the given day is included.
                // Falls back to toDate so pre-existing clients keep working, then to today.
                // NOTE: NormalizeRange is deliberately NOT used here — its one-month default
                // lower bound is exactly what made this report wrong.
                var asOf = (req.asOfDate ?? req.toDate ?? DateTime.UtcNow)
                           .Date.AddDays(1).AddTicks(-1);

                // Movement-window start. Null => opening is zero and the period covers
                // the whole history. Kept nullable so the lifted comparison below
                // degrades to "false" (SQL: EntryDate < NULL => NULL => ELSE branch).
                DateTime? windowStart = req.fromDate?.Date;

                // Find all sub-accounts under the parent (recursive — but typically one level deep)
                var allAccounts = await _unitOfWork
                    .GetRepository<ChartOfAccounts, int>()
                    .GetAllAsync();

                var partyAccountIds = CollectDescendantIds(parent.Id, allAccounts);

                // ONE round trip: cumulative totals through asOf, plus the pre-window
                // (opening) slice via conditional sums, so the movement columns cost nothing extra.
                var totals = await _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(d => partyAccountIds.Contains(d.AccountId)
                                && d.JournalEntry.IsPosted == true
                                && d.JournalEntry.EntryDate <= asOf)
                    .GroupBy(d => d.AccountId)
                    .Select(g => new
                    {
                        accountId = g.Key,
                        cumulativeDebit = g.Sum(x => x.Debit),
                        cumulativeCredit = g.Sum(x => x.Credit),
                        openingDebit = g.Sum(x => x.JournalEntry.EntryDate < windowStart ? x.Debit : 0m),
                        openingCredit = g.Sum(x => x.JournalEntry.EntryDate < windowStart ? x.Credit : 0m),
                        lastDate = g.Max(x => x.JournalEntry.EntryDate)
                    })
                    .ToListAsync();

                var partyAccounts = allAccounts
                    .Where(a => partyAccountIds.Contains(a.Id) && a.IsLeaf)
                    .ToList();

                var rows = partyAccounts.Select(a =>
                {
                    var t = totals.FirstOrDefault(x => x.accountId == a.Id);

                    var cumulativeDebit = t?.cumulativeDebit ?? 0m;
                    var cumulativeCredit = t?.cumulativeCredit ?? 0m;
                    var openingDebit = t?.openingDebit ?? 0m;
                    var openingCredit = t?.openingCredit ?? 0m;

                    // Direction from the account itself — Assets/Expenses debit-normal,
                    // Liabilities/Equity/Income credit-normal.
                    var closing = AccountBalanceCalculator
                        .NormalBalance(a.Type, cumulativeDebit, cumulativeCredit);
                    var opening = AccountBalanceCalculator
                        .NormalBalance(a.Type, openingDebit, openingCredit);

                    return new PartyBalanceDto
                    {
                        accountId = a.Id,
                        accountCode = a.AccountCode,
                        accountName = a.AccountName,
                        userId = a.UserId,
                        totalDebit = cumulativeDebit,
                        totalCredit = cumulativeCredit,
                        openingBalance = opening,
                        periodDebit = cumulativeDebit - openingDebit,
                        periodCredit = cumulativeCredit - openingCredit,
                        closingBalance = closing,
                        balance = closing,
                        lastTransactionDate = t?.lastDate
                    };
                })
                .Where(r => req.includeZeroBalances || r.closingBalance != 0)
                .OrderByDescending(r => r.closingBalance)
                .ToList();

                var isDebitNormal = AccountBalanceCalculator.IsDebitNormal(parent.Type);

                return Result<PartyBalancesReportDto>.Success(new PartyBalancesReportDto
                {
                    asOfDate = asOf,
                    fromDate = req.fromDate,
                    totalReceivables = isDebitNormal ? rows.Sum(r => r.closingBalance) : 0m,
                    totalPayables = isDebitNormal ? 0m : rows.Sum(r => r.closingBalance),
                    parties = rows
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<PartyBalancesReportDto>.Failure("خطأ أثناء توليد التقرير");
            }
        }''',
),
],

"Infrastructure/Services/FinanceService/TreeAccountsService.cs": [
(
'''                // 3️⃣ Calculate totals
                var debit = details.Sum(d => d.Debit);
                var credit = details.Sum(d => d.Credit);

                // 4️⃣ Map to DTO
                var dto = new DisAndMerchAccountDto
                {
                    accountCode = account.AccountCode,
                    userId = account.UserId,
                    accountName = account.AccountName,
                    type = (int)account.Type,
                    parentAccountId = account.ParentAccountId,
                    isLeaf = account.IsLeaf,
                    isActive = account.IsActive,
                    debit = debit,
                    credit = credit
                };''',
'''                // 3️⃣ Calculate totals
                var debit = details.Sum(d => d.Debit);
                var credit = details.Sum(d => d.Credit);

                // Balance in the account's natural direction. Shared with the
                // financial-reports service so the two screens can never disagree:
                // Assets/Expenses => debit - credit, Liabilities/Equity/Income => credit - debit.
                var balance = Application.Common.AccountBalanceCalculator
                    .NormalBalance(account.Type, debit, credit);

                // 4️⃣ Map to DTO
                var dto = new DisAndMerchAccountDto
                {
                    accountCode = account.AccountCode,
                    userId = account.UserId,
                    accountName = account.AccountName,
                    type = (int)account.Type,
                    parentAccountId = account.ParentAccountId,
                    isLeaf = account.IsLeaf,
                    isActive = account.IsActive,
                    debit = debit,
                    credit = credit,
                    balance = balance
                };''',
),
],

"AlSadat-Seram.slnx": [
(
'''<Solution>
  <Project Path="AlSadat-Seram.Api/AlSadat-Seram.Api.csproj" />
  <Project Path="Application/Application.csproj" />
  <Project Path="Domain/Domain.csproj" />
  <Project Path="Infrastructure/Infrastructure.csproj" />
</Solution>''',
'''<Solution>
  <Project Path="AlSadat-Seram.Api/AlSadat-Seram.Api.csproj" />
  <Project Path="Application/Application.csproj" />
  <Project Path="Domain/Domain.csproj" />
  <Project Path="Infrastructure/Infrastructure.csproj" />
  <Project Path="Tests/Application.UnitTests/Application.UnitTests.csproj" />
</Solution>''',
),
],

}

# TreeAccountDto.cs is handled separately — its exact current shape was not
# confirmed from source, so it gets a best-effort anchor plus a manual fallback.
TREEACCOUNTDTO_PATH = "Application/DTOs/FinanceDtos/TreeAccountDto.cs"
TREEACCOUNTDTO_ANCHOR_CLASS = "public class DisAndMerchAccountDto"
TREEACCOUNTDTO_NEW_PROPERTY = '''
        /// <summary>
        /// Balance in the account's natural direction, computed server-side by
        /// AccountBalanceCalculator. Positive means the account has a balance on its
        /// normal side: a customer owes us, or we owe a supplier.
        /// The client must use this rather than re-deriving it from debit/credit.
        /// </summary>
        public decimal balance { get; set; }'''


def write_new_file(repo: Path, rel_path: str, content: str, dry_run: bool) -> str:
    target = repo / rel_path
    if target.exists():
        existing = target.read_text(encoding="utf-8")
        if existing.strip() == content.strip():
            return f"SKIP  (already present, identical)  {rel_path}"
        return f"CONFLICT  (file exists with different content — not overwritten)  {rel_path}"
    if not dry_run:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(content, encoding="utf-8", newline="\n")
    return f"{'WOULD CREATE' if dry_run else 'CREATED'}  {rel_path}"


def apply_replacement(repo: Path, rel_path: str, old: str, new: str, dry_run: bool) -> str:
    target = repo / rel_path
    if not target.exists():
        return f"MISSING FILE — cannot patch  {rel_path}"

    text = target.read_text(encoding="utf-8")

    if new.strip() in text:
        return f"SKIP  (already patched)  {rel_path}"

    if old.strip() not in text:
        return f"ANCHOR NOT FOUND — file has diverged, patch manually  {rel_path}"

    if text.count(old.strip()) > 1:
        return f"AMBIGUOUS — anchor appears more than once, patch manually  {rel_path}"

    new_text = text.replace(old, new, 1) if old in text else text
    if new_text == text:
        # exact substring (with original whitespace) not found even though
        # the stripped version was — do a whitespace-tolerant single pass.
        return f"ANCHOR WHITESPACE MISMATCH — patch manually (see instructions doc)  {rel_path}"

    if not dry_run:
        target.write_text(new_text, encoding="utf-8", newline="\n")
    return f"{'WOULD PATCH' if dry_run else 'PATCHED'}  {rel_path}"


def apply_treeaccountdto(repo: Path, dry_run: bool) -> str:
    rel_path = TREEACCOUNTDTO_PATH
    target = repo / rel_path
    if not target.exists():
        return f"MISSING FILE — apply manually per instructions doc  {rel_path}"

    text = target.read_text(encoding="utf-8")

    if "public decimal balance { get; set; }" in text and "DisAndMerchAccountDto" in text:
        return f"SKIP  (balance property already present)  {rel_path}"

    idx = text.find(TREEACCOUNTDTO_ANCHOR_CLASS)
    if idx == -1:
        return f"ANCHOR NOT FOUND — add 'public decimal balance {{ get; set; }}' to DisAndMerchAccountDto by hand  {rel_path}"

    # Find the matching closing brace of this class (first '}' at column 4 after the anchor).
    close_idx = text.find("\n    }", idx)
    if close_idx == -1:
        return f"COULD NOT LOCATE CLASS END — add the property by hand  {rel_path}"

    new_text = text[:close_idx] + TREEACCOUNTDTO_NEW_PROPERTY + text[close_idx:]
    if not dry_run:
        target.write_text(new_text, encoding="utf-8", newline="\n")
    return f"{'WOULD PATCH' if dry_run else 'PATCHED'}  {rel_path}"


def main():
    parser = argparse.ArgumentParser(description="Apply Phase 1 (supplier/customer balance fix) to the repo.")
    parser.add_argument("--repo", required=True, help="Path to the repo root (folder containing AlSadat-Seram.slnx)")
    parser.add_argument("--dry-run", action="store_true", help="Report what would happen without writing files")
    args = parser.parse_args()

    repo = Path(args.repo)
    if not (repo / "AlSadat-Seram.slnx").exists():
        print(f"ERROR: {repo} does not contain AlSadat-Seram.slnx — check --repo path.")
        sys.exit(1)

    results = []

    for rel_path, content in NEW_FILES.items():
        results.append(write_new_file(repo, rel_path, content, args.dry_run))

    for rel_path, hunks in REPLACEMENTS.items():
        for old, new in hunks:
            results.append(apply_replacement(repo, rel_path, old, new, args.dry_run))

    results.append(apply_treeaccountdto(repo, args.dry_run))

    print("\n".join(results))

    problems = [r for r in results if any(
        tag in r for tag in ("ANCHOR NOT FOUND", "MISSING FILE", "CONFLICT", "AMBIGUOUS", "MISMATCH", "COULD NOT")
    )]
    print()
    if problems:
        print(f"{len(problems)} item(s) need manual attention — see PHASE1_MANUAL_INSTRUCTIONS.md for the exact text.")
        sys.exit(2)
    else:
        print("All Phase 1 changes applied cleanly." if not args.dry_run else "Dry run: all anchors found, nothing written.")


if __name__ == "__main__":
    main()
