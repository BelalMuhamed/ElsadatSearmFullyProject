using Application.CommonPagination;
using Application.DTOs.Notifications;
using Domain.Common;

namespace Application.Services.contract.Notifications;

/// <summary>The current user's notifications and preferences (screens 65, 66 and 7).</summary>
public interface INotificationService
{
    /// <summary>The user's notifications, newest first, rendered in the request's language.</summary>
    Task<Result<PagedResult<NotificationDto>>> ListAsync(NotificationListQuery query, CancellationToken cancellationToken = default);

    /// <summary>One of the user's notifications (what the client reads after a push, D43).</summary>
    Task<Result<NotificationDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>The bell's number.</summary>
    Task<Result<UnreadCountDto>> GetUnreadCountAsync(CancellationToken cancellationToken = default);

    /// <summary>Marks one notification read; returns the new unread count.</summary>
    Task<Result<UnreadCountDto>> MarkReadAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Marks every notification of the user read.</summary>
    Task<Result<UnreadCountDto>> MarkAllReadAsync(CancellationToken cancellationToken = default);

    /// <summary>Every type with whether it is on for the user and whether it can be turned off.</summary>
    Task<Result<IReadOnlyList<NotificationPreferenceDto>>> GetPreferencesAsync(CancellationToken cancellationToken = default);

    /// <summary>Turns types on or off; a type that can't be muted returns 400 <c>Notification.CannotMute</c>.</summary>
    Task<Result<IReadOnlyList<NotificationPreferenceDto>>> UpdatePreferencesAsync(
        IReadOnlyList<NotificationPreferenceChange> changes, CancellationToken cancellationToken = default);
}
