using Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// A posted journal entry and its lines are never changed or deleted (D36): a mistake is
/// corrected with a reversal (<c>IJournalPostingService.ReverseAsync</c>). Any save that modifies
/// or deletes such a row — from any service — throws, so the request ends in a 500 and the
/// caller's transaction rolls back.
/// <para>
/// The posted state is read from the database, not from the tracked entity, so an entity attached
/// with <c>State = Modified</c> or removed by id can't slip through, and posting a draft
/// (<c>IsPosted</c> false → true) is still allowed. Adding a line is not checked yet: the old
/// services save the entry first and its lines in a second save (D35); M3 closes that once they
/// are gone. <c>ExecuteUpdate</c>/<c>ExecuteDelete</c> bypass interceptors.
/// </para>
/// </summary>
public sealed class PostedJournalGuardInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context)
            EnsureNoPostedEntryChangesAsync(context, useAsync: false, CancellationToken.None).GetAwaiter().GetResult();
        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
            await EnsureNoPostedEntryChangesAsync(context, useAsync: true, cancellationToken);
        return result;
    }

    private static async Task EnsureNoPostedEntryChangesAsync(DbContext context, bool useAsync, CancellationToken cancellationToken)
    {
        var entryIds = new HashSet<int>();

        foreach (var entry in context.ChangeTracker.Entries<JournalEntries>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                entryIds.Add(entry.Entity.Id);
        }

        foreach (var line in context.ChangeTracker.Entries<JournalEntryDetails>())
        {
            if (line.State is not (EntityState.Modified or EntityState.Deleted))
                continue;

            // Both sides, so a line can't be moved out of (or into) a posted entry.
            entryIds.Add(line.Property(l => l.JournalEntryId).OriginalValue);
            entryIds.Add(line.Entity.JournalEntryId);
        }

        if (entryIds.Count == 0)
            return;

        var query = context.Set<JournalEntries>()
            .AsNoTracking()
            .Where(e => entryIds.Contains(e.Id) && e.IsPosted == true)
            .Select(e => e.Id);
        var posted = useAsync ? await query.ToListAsync(cancellationToken) : query.ToList();

        if (posted.Count > 0)
            throw new InvalidOperationException(
                $"Posted journal entries can't be changed or deleted (D36): id {string.Join(", ", posted)}. Post a reversal instead.");
    }
}
