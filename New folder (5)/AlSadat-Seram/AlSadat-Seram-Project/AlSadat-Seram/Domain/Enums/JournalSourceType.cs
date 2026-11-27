namespace Domain.Enums;

/// <summary>
/// The document a journal entry comes from (F6, D35). Stored as <c>tinyint</c> on
/// <c>JournalEntries.SourceType</c> next to <c>SourceId</c>: never renumber a member, only add.
/// Every source except <see cref="Manual"/> is written by the system and opens its document.
/// </summary>
public enum JournalSourceType : byte
{
    /// <summary>A manual entry typed by the accountant (screen 38).</summary>
    Manual = 1,

    /// <summary>Sales invoice confirmation or cancellation.</summary>
    SalesInvoice = 2,

    /// <summary>Sales return.</summary>
    SalesReturn = 3,

    /// <summary>Purchase invoice receipt (استلام).</summary>
    PurchaseInvoice = 4,

    /// <summary>Return to a supplier.</summary>
    PurchaseReturn = 5,

    /// <summary>Receipt voucher (سند قبض).</summary>
    Receipt = 6,

    /// <summary>Payment voucher (سند صرف).</summary>
    Payment = 7,

    /// <summary>Company expense.</summary>
    Expense = 8,

    /// <summary>Monthly payroll approval or payment.</summary>
    Payroll = 9,

    /// <summary>Employee loan or its repayment.</summary>
    EmployeeLoan = 10,

    /// <summary>Sales-rep custody movement.</summary>
    Custody = 11,

    /// <summary>Stock count adjustment.</summary>
    StockCount = 12,

    /// <summary>Gifts purchase or hand-out.</summary>
    Gifts = 13
}
