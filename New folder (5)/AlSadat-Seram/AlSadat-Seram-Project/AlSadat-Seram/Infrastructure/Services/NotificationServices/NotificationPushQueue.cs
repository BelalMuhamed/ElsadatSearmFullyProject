using System.Threading.Channels;

namespace Infrastructure.Services.NotificationServices;

/// <summary>A notification that was committed and still has to be pushed.</summary>
/// <param name="Id">The notification id.</param>
/// <param name="RecipientId">Who to push it to.</param>
public sealed record PendingPush(int Id, string RecipientId);

/// <summary>
/// Committed notifications waiting for <see cref="NotificationPushWorker"/> (singleton). Writing never
/// blocks the request that caused them; the push is best effort — the row is already saved, and the
/// client re-reads the list when it reconnects.
/// </summary>
public sealed class NotificationPushQueue
{
    private readonly Channel<IReadOnlyList<PendingPush>> _channel =
        Channel.CreateUnbounded<IReadOnlyList<PendingPush>>(new UnboundedChannelOptions { SingleReader = true });

    /// <summary>Queues a batch saved by one commit.</summary>
    public void Enqueue(IReadOnlyList<PendingPush> batch)
    {
        if (batch.Count > 0)
            _channel.Writer.TryWrite(batch);
    }

    /// <summary>The batches, in commit order.</summary>
    public IAsyncEnumerable<IReadOnlyList<PendingPush>> ReadAllAsync(CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    /// <summary>Takes a waiting batch without waiting (tests).</summary>
    public bool TryDequeue(out IReadOnlyList<PendingPush>? batch) => _channel.Reader.TryRead(out batch);
}
