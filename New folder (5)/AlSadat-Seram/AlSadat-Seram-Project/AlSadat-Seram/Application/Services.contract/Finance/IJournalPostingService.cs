using Application.DTOs.FinanceDtos.Posting;
using Domain.Common;

namespace Application.Services.contract.Finance;

/// <summary>
/// The only writer of journal entries (F6, D35–D38). Every call runs inside the caller's
/// transaction — the one that also saves the document, moves the stock or the money — so the
/// entry, its <c>JE-</c> number and the document commit or roll back together.
/// </summary>
public interface IJournalPostingService
{
    /// <summary>
    /// Posts <paramref name="draft"/> as a new entry with the next <c>JE-</c> number.
    /// </summary>
    /// <param name="draft">The entry; system accounts by <c>SystemAccountCode</c>, party accounts by id.</param>
    /// <param name="cancellationToken">Cancels the database calls.</param>
    /// <returns>
    /// The posted entry, or <see cref="JournalErrorCodes.AccountNotPostable"/> when a line hits an
    /// inactive or parent account.
    /// </returns>
    /// <exception cref="ArgumentException">The draft breaks <c>JournalDraftRules</c> (unbalanced, bad amounts…).</exception>
    /// <exception cref="InvalidOperationException">No transaction is open, or an account is missing.</exception>
    Task<Result<PostedJournalEntry>> PostAsync(JournalDraft draft, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts the reversal of a posted entry: the same lines with debit and credit swapped, linked to
    /// the original, same source document, dated <paramref name="reversalDate"/> (D37).
    /// </summary>
    /// <param name="entryId">The entry to reverse.</param>
    /// <param name="reversalDate">The reversal's accounting date, usually today; not before the original's.</param>
    /// <param name="reason">Why it is reversed; goes into the reversal's statement.</param>
    /// <param name="cancellationToken">Cancels the database calls.</param>
    /// <returns>
    /// The reversal entry, or <see cref="JournalErrorCodes.EntryNotFound"/>,
    /// <see cref="JournalErrorCodes.NotPosted"/>, <see cref="JournalErrorCodes.CannotReverseReversal"/>,
    /// <see cref="JournalErrorCodes.AlreadyReversed"/>.
    /// </returns>
    /// <exception cref="ArgumentException">The reason is empty or the date is before the original entry.</exception>
    /// <exception cref="InvalidOperationException">No transaction is open.</exception>
    Task<Result<PostedJournalEntry>> ReverseAsync(int entryId, DateTime reversalDate, string reason, CancellationToken cancellationToken = default);
}
