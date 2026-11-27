using Application.DTOs.Notifications;
using Application.Services.contract.Notifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.NotificationServices;

/// <summary>
/// Pushes committed notifications to their recipients' SignalR connections (D5): one
/// <c>{ id, unreadCount }</c> per notification (D43). Runs outside the request, so a slow or failed push
/// never delays or breaks the operation that caused it.
/// </summary>
public sealed class NotificationPushWorker(
    NotificationPushQueue queue,
    IServiceScopeFactory scopes,
    INotificationPushSender sender,
    ILogger<NotificationPushWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var batch in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await PushAsync(batch, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Pushing {Count} notifications failed; they stay unread in the list.", batch.Count);
            }
        }
    }

    /// <summary>Counts each recipient's unread notifications once and sends one push per notification.</summary>
    public async Task PushAsync(IReadOnlyList<PendingPush> batch, CancellationToken cancellationToken)
    {
        var recipients = batch.Select(p => p.RecipientId).Distinct().ToList();

        await using var scope = scopes.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var unread = await context.Notifications
            .AsNoTracking()
            .Where(n => recipients.Contains(n.RecipientId) && n.ReadAt == null)
            .GroupBy(n => n.RecipientId)
            .Select(g => new { RecipientId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RecipientId, x => x.Count, cancellationToken);

        foreach (var push in batch)
            await sender.SendAsync(push.RecipientId, new NotificationPush(push.Id, unread.GetValueOrDefault(push.RecipientId)), cancellationToken);
    }
}
