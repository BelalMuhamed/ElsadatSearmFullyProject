using System;

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
