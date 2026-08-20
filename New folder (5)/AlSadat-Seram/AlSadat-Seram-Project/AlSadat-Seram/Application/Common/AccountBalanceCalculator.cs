using Domain.Enums;

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
