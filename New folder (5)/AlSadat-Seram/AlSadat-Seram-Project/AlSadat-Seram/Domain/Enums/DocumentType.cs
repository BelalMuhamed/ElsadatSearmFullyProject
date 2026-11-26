namespace Domain.Enums;

/// <summary>
/// The documents that get a human number (F4, D31–D34). Stored as <c>tinyint</c> in
/// <c>DocumentSequences</c>: never renumber a member, only add new ones (and seed their row).
/// </summary>
public enum DocumentType : byte
{
    /// <summary>Sales invoice — <c>INV-1038</c>.</summary>
    SalesInvoice = 1,

    /// <summary>Purchase invoice — <c>PUR-0224</c>.</summary>
    PurchaseInvoice = 2,

    /// <summary>Receipt voucher (سند قبض) — <c>RC-0368</c>.</summary>
    Receipt = 3,

    /// <summary>Sales return — <c>RET-0044</c>.</summary>
    SalesReturn = 4,

    /// <summary>Transfer between warehouses — <c>TR-0094</c>.</summary>
    StoreTransfer = 5,

    /// <summary>Stock count (جرد) — <c>CNT-0013</c>.</summary>
    StockCount = 6,

    /// <summary>Plumber inspection — <c>INS-0412</c>.</summary>
    Inspection = 7,

    /// <summary>Warranty certificate — <c>W-2231</c>.</summary>
    Warranty = 8,

    /// <summary>Journal entry — <c>JE-4471</c>.</summary>
    JournalEntry = 9
}
