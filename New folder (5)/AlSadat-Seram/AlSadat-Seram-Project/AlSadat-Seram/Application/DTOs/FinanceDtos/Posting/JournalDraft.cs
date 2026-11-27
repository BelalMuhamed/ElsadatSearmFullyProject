using Domain.Enums;

namespace Application.DTOs.FinanceDtos.Posting;

/// <summary>
/// A journal entry before it is posted (F6). Built by the service that owns the document and
/// handed to <c>IJournalPostingService.PostAsync</c>; <see cref="Common.JournalDraftRules"/>
/// checks it.
/// </summary>
/// <param name="EntryDate">The accounting date: the document's confirmation date, or the date the accountant chose.</param>
/// <param name="Description">The entry's statement (البيان), e.g. «تأكيد فاتورة INV-0040».</param>
/// <param name="SourceType">The kind of document the entry comes from.</param>
/// <param name="SourceId">The document's id; null only for <see cref="JournalSourceType.Manual"/>.</param>
/// <param name="Lines">At least two lines, debits equal to credits.</param>
public sealed record JournalDraft(
    DateTime EntryDate,
    string Description,
    JournalSourceType SourceType,
    int? SourceId,
    IReadOnlyList<JournalDraftLine> Lines);

/// <summary>
/// One line of a <see cref="JournalDraft"/>: a system account (by <see cref="SystemAccountCode"/>,
/// never by a code string or a magic id — D9) or a party account (customer, supplier, rep custody)
/// by its id, on one side only.
/// </summary>
public sealed record JournalDraftLine
{
    /// <summary>The system account, when the line hits one; otherwise null.</summary>
    public SystemAccountCode? SystemAccount { get; init; }

    /// <summary>The account id, when the line hits a party account; otherwise null.</summary>
    public int? AccountId { get; init; }

    /// <summary>Debit amount; zero on a credit line.</summary>
    public decimal Debit { get; init; }

    /// <summary>Credit amount; zero on a debit line.</summary>
    public decimal Credit { get; init; }

    /// <summary>Optional note for the line.</summary>
    public string? Description { get; init; }

    /// <summary>A debit line on a system account.</summary>
    public static JournalDraftLine DebitTo(SystemAccountCode account, decimal amount, string? description = null)
        => new() { SystemAccount = account, Debit = amount, Description = description };

    /// <summary>A credit line on a system account.</summary>
    public static JournalDraftLine CreditTo(SystemAccountCode account, decimal amount, string? description = null)
        => new() { SystemAccount = account, Credit = amount, Description = description };

    /// <summary>A debit line on an account by id (a customer, supplier or custody account).</summary>
    public static JournalDraftLine DebitTo(int accountId, decimal amount, string? description = null)
        => new() { AccountId = accountId, Debit = amount, Description = description };

    /// <summary>A credit line on an account by id (a customer, supplier or custody account).</summary>
    public static JournalDraftLine CreditTo(int accountId, decimal amount, string? description = null)
        => new() { AccountId = accountId, Credit = amount, Description = description };
}

/// <summary>The entry the engine wrote.</summary>
/// <param name="Id">The <c>JournalEntries</c> id.</param>
/// <param name="EntryNumber">The human number, e.g. <c>JE-0001</c>.</param>
public sealed record PostedJournalEntry(int Id, string EntryNumber);
