using Application.DTOs.Notifications;

namespace Application.Services.contract.Notifications;

/// <summary>
/// The one way to notify users (F8). The notification is saved with the scoped <c>AppDbContext</c>,
/// inside the caller's transaction when there is one, and pushed on SignalR only after it commits
/// (D5, D43). Recipients who muted the type get nothing (D44).
/// </summary>
public interface INotificationPublisher
{
    /// <summary>Notifies these users (duplicates and blanks are ignored).</summary>
    Task PublishAsync(NotificationMessage message, IReadOnlyCollection<string> recipientIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies every active user who holds this permission, and the Admins (super_admin), except
    /// <paramref name="exceptUserIds"/>. M1 changes how holders are found (D3, D16).
    /// </summary>
    Task PublishToPermissionAsync(
        NotificationMessage message,
        string permissionCode,
        IReadOnlyCollection<string>? exceptUserIds = null,
        CancellationToken cancellationToken = default);
}

/// <summary>Sends the push to a user's open connections; implemented by the API's SignalR hub.</summary>
public interface INotificationPushSender
{
    /// <summary>Pushes <paramref name="push"/> to every connection of <paramref name="userId"/>.</summary>
    Task SendAsync(string userId, NotificationPush push, CancellationToken cancellationToken);
}
