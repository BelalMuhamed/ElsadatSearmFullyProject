using Domain.Entities.Notifications;
using Infrastructure.Services.NotificationServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// Queues the push of saved notifications only once they are really committed (F8, D5): right after
/// the save when there is no transaction, at the commit when there is one, never after a rollback.
/// Scoped: one instance per request, like its <see cref="AppDbContext"/>.
/// </summary>
public sealed class NotificationPushInterceptor(NotificationPushQueue queue) : SaveChangesInterceptor, IDbTransactionInterceptor
{
    private readonly List<Notification> _saving = [];
    private readonly List<PendingPush> _pending = [];

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Collect(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Collect(eventData.Context);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Saved(eventData.Context);
        return result;
    }

    /// <inheritdoc />
    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Saved(eventData.Context);
        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => _saving.Clear();

    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _saving.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) => Flush();

    /// <inheritdoc />
    public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Flush();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) => _pending.Clear();

    /// <inheritdoc />
    public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData) => _pending.Clear();

    /// <inheritdoc />
    public Task TransactionFailedAsync(DbTransaction transaction, TransactionErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return Task.CompletedTask;
    }

    private void Collect(DbContext? context)
    {
        _saving.Clear();
        if (context is null)
            return;

        foreach (var entry in context.ChangeTracker.Entries<Notification>())
        {
            if (entry.State == EntityState.Added)
                _saving.Add(entry.Entity);
        }
    }

    private void Saved(DbContext? context)
    {
        _pending.AddRange(_saving.Select(n => new PendingPush(n.Id, n.RecipientId)));
        _saving.Clear();

        if (context?.Database.CurrentTransaction is null)
            Flush();
    }

    private void Flush()
    {
        if (_pending.Count == 0)
            return;

        queue.Enqueue(_pending.ToList());
        _pending.Clear();
    }
}
