using Domain.Enums;

namespace Domain.Entities.Numbering;

/// <summary>
/// The running counter of one <see cref="DocumentType"/> (F4). One row per type, one counter
/// forever (D32); the next number is taken inside the document's own transaction so a rollback
/// gives it back (D31). Changed only through <c>IDocumentNumberService</c>.
/// </summary>
public class DocumentSequence
{
    public const int PrefixMaxLength = 8;

    /// <summary>Numbers start at 1 (D34).</summary>
    public const int InitialLastNumber = 0;

    /// <summary><c>INV-0001</c>: at least 4 digits, more once past 9999 (D34).</summary>
    public const byte DefaultMinDigits = 4;

    /// <summary>The seeded rows: every <see cref="DocumentType"/> must have one.</summary>
    public static readonly IReadOnlyList<DocumentSequence> Defaults =
    [
        New(DocumentType.SalesInvoice, "INV"),
        New(DocumentType.PurchaseInvoice, "PUR"),
        New(DocumentType.Receipt, "RC"),
        New(DocumentType.SalesReturn, "RET"),
        New(DocumentType.StoreTransfer, "TR"),
        New(DocumentType.StockCount, "CNT"),
        New(DocumentType.Inspection, "INS"),
        New(DocumentType.Warranty, "W"),
        New(DocumentType.JournalEntry, "JE")
    ];

    /// <summary>Primary key.</summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>Text before the dash, e.g. <c>INV</c>.</summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>The last number handed out; 0 before the first document.</summary>
    public int LastNumber { get; set; }

    /// <summary>Zero-padding of the number part.</summary>
    public byte MinDigits { get; set; }

    private static DocumentSequence New(DocumentType type, string prefix) => new()
    {
        DocumentType = type,
        Prefix = prefix,
        LastNumber = InitialLastNumber,
        MinDigits = DefaultMinDigits
    };
}
