using Application.Common;
using Application.DTOs.FinanceDtos.Posting;
using Application.Services.contract.Finance;
using Application.Services.contract.Numbering;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.FinanceService;

/// <summary>
/// Posts and reverses journal entries (F6, D35–D38). Works on the scoped <see cref="AppDbContext"/>,
/// so the caller's transaction (<c>IUnitOfWork.BeginTransactionAsync</c> or
/// <c>Database.BeginTransactionAsync</c>) covers the entry, its number and the document.
/// </summary>
public sealed class JournalPostingService(
    AppDbContext context,
    IDocumentNumberService documentNumbers,
    TimeProvider timeProvider) : IJournalPostingService
{
    private const string ReversalDescriptionFormat = "عكس قيد {0}: {1}";

    /// <inheritdoc />
    public async Task<Result<PostedJournalEntry>> PostAsync(JournalDraft draft, CancellationToken cancellationToken = default)
    {
        JournalDraftRules.EnsureValid(draft);
        EnsureTransaction();

        var accountIds = await ResolveAccountsAsync(draft.Lines, cancellationToken);
        if (accountIds is null)
            return Result<PostedJournalEntry>.FailureKey(JournalErrorCodes.AccountNotPostable);

        return Result<PostedJournalEntry>.Success(
            await SaveAsync(draft, accountIds, reversalOfEntryId: null, cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Result<PostedJournalEntry>> ReverseAsync(
        int entryId, DateTime reversalDate, string reason, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        EnsureTransaction();

        var original = await context.journalEntries
            .AsNoTracking()
            .Where(e => e.Id == entryId)
            .Select(e => new
            {
                e.Id,
                e.EntryNumber,
                e.EntryDate,
                e.IsPosted,
                e.ReversalOfEntryId,
                e.SourceType,
                e.SourceId,
                Lines = e.Details.Select(d => new JournalDraftLine
                {
                    AccountId = d.AccountId,
                    Debit = d.Debit,
                    Credit = d.Credit,
                    Description = d.Description
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (original is null)
            return Result<PostedJournalEntry>.FailureKey(JournalErrorCodes.EntryNotFound, HttpStatusCode.NotFound);
        if (original.IsPosted != true)
            return Result<PostedJournalEntry>.FailureKey(JournalErrorCodes.NotPosted, HttpStatusCode.Conflict);
        if (original.ReversalOfEntryId is not null)
            return Result<PostedJournalEntry>.FailureKey(JournalErrorCodes.CannotReverseReversal, HttpStatusCode.Conflict);
        // The filtered unique index on ReversalOfEntryId still stops a race that passes this check.
        if (await context.journalEntries.AnyAsync(e => e.ReversalOfEntryId == entryId, cancellationToken))
            return Result<PostedJournalEntry>.FailureKey(JournalErrorCodes.AlreadyReversed, HttpStatusCode.Conflict);

        if (reversalDate.Date < original.EntryDate.Date)
            throw new ArgumentException(
                $"A reversal can't be dated before the entry it reverses ({original.EntryDate:yyyy-MM-dd}).", nameof(reversalDate));

        // Old-service entries (D35) have no source: their reversal is recorded as a manual entry,
        // still linked to the original through ReversalOfEntryId.
        var sourceType = original.SourceType ?? JournalSourceType.Manual;
        var draft = new JournalDraft(
            reversalDate,
            ReversalDescription(original.EntryNumber ?? $"#{original.Id}", reason),
            sourceType,
            sourceType == JournalSourceType.Manual ? null : original.SourceId,
            JournalDraftRules.Reversed(original.Lines));
        JournalDraftRules.EnsureValid(draft);

        // No leaf/active check: undoing an entry must work even after its account was deactivated.
        var accountIds = draft.Lines.Select(l => l.AccountId!.Value).ToList();
        return Result<PostedJournalEntry>.Success(
            await SaveAsync(draft, accountIds, original.Id, cancellationToken));
    }

    private void EnsureTransaction()
    {
        if (context.Database.CurrentTransaction is null)
            throw new InvalidOperationException(
                "IJournalPostingService must run inside the transaction that saves the document (F6, D31).");
    }

    /// <summary>
    /// The account id of every line, in order, in one query; null when one of them can't take
    /// entries (inactive or a parent).
    /// </summary>
    private async Task<IReadOnlyList<int>?> ResolveAccountsAsync(
        IReadOnlyList<JournalDraftLine> lines, CancellationToken cancellationToken)
    {
        var codes = lines.Where(l => l.SystemAccount is not null).Select(l => l.SystemAccount!.Value).Distinct().ToList();
        var ids = lines.Where(l => l.AccountId is not null).Select(l => l.AccountId!.Value).Distinct().ToList();

        var accounts = await context.Accounts
            .AsNoTracking()
            .Where(a => ids.Contains(a.Id) || (a.SystemCode != null && codes.Contains(a.SystemCode.Value)))
            .Select(a => new { a.Id, a.SystemCode, a.IsLeaf, a.IsActive })
            .ToListAsync(cancellationToken);

        var bySystemCode = accounts.Where(a => a.SystemCode is not null).ToDictionary(a => a.SystemCode!.Value);
        var byId = accounts.ToDictionary(a => a.Id);

        var missingCodes = codes.Where(c => !bySystemCode.ContainsKey(c)).ToList();
        if (missingCodes.Count > 0)
            throw new InvalidOperationException(
                $"System account(s) {string.Join(", ", missingCodes)} are missing: seed them with their SystemAccountCode.");
        var missingIds = ids.Where(id => !byId.ContainsKey(id)).ToList();
        if (missingIds.Count > 0)
            throw new InvalidOperationException($"Account id(s) {string.Join(", ", missingIds)} don't exist.");

        var resolved = lines
            .Select(l => l.SystemAccount is { } code ? bySystemCode[code] : byId[l.AccountId!.Value])
            .ToList();
        return resolved.All(a => a.IsLeaf && a.IsActive) ? resolved.Select(a => a.Id).ToList() : null;
    }

    private async Task<PostedJournalEntry> SaveAsync(
        JournalDraft draft, IReadOnlyList<int> accountIds, int? reversalOfEntryId, CancellationToken cancellationToken)
    {
        var number = await documentNumbers.NextAsync(DocumentType.JournalEntry, cancellationToken);
        var entry = new JournalEntries
        {
            EntryNumber = number,
            EntryDate = draft.EntryDate,
            Desc = draft.Description,
            SourceType = draft.SourceType,
            SourceId = draft.SourceId,
            IsSystemGenerated = draft.SourceType != JournalSourceType.Manual,
            ReversalOfEntryId = reversalOfEntryId,
            IsPosted = true,
            PostedDate = timeProvider.GetUtcNow().UtcDateTime,
            Details = draft.Lines.Select((line, i) => new JournalEntryDetails
            {
                AccountId = accountIds[i],
                Debit = line.Debit,
                Credit = line.Credit,
                Description = line.Description
            }).ToList()
        };

        context.journalEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);
        return new PostedJournalEntry(entry.Id, number);
    }

    private static string ReversalDescription(string entryNumber, string reason)
    {
        var text = string.Format(ReversalDescriptionFormat, entryNumber, reason.Trim());
        return text.Length <= JournalEntries.DescriptionMaxLength ? text : text[..JournalEntries.DescriptionMaxLength];
    }
}
