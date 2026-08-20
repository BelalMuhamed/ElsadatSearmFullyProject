using Application.Common;
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
