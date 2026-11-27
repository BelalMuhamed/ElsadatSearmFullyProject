using Domain.Entities.Commonitems;
using Domain.Enums;

namespace Domain.Entities.Notifications;

/// <summary>
/// One notification for one user (F8). Written only by <c>INotificationPublisher</c>, in the same
/// transaction as what caused it, and pushed on SignalR after the commit (D5, D43). The text is a
/// message key + arguments rendered in the reader's language when it is read.
/// </summary>
public class Notification : BaseEntity
{
    public const int MessageKeyMaxLength = 100;
    public const int ArgsMaxLength = 2000;
    public const int LinkKindMaxLength = 40;
    public const int LinkIdMaxLength = 64;

    /// <summary><c>ApplicationUser.Id</c> of who receives it.</summary>
    public string RecipientId { get; set; } = string.Empty;

    /// <summary>What it is about; decides its tab and whether it can be muted.</summary>
    public NotificationType Type { get; set; }

    /// <summary>A key from <c>LocalizationResources</c>, e.g. <c>Notification.ApprovalRequested</c>.</summary>
    public string MessageKey { get; set; } = string.Empty;

    /// <summary>The key's arguments as JSON; an argument may itself be a key, translated when read.</summary>
    public string? Args { get; set; }

    /// <summary>What the notification opens (a constant from <c>NotificationLinks</c>); the client maps it to a screen.</summary>
    public string? LinkKind { get; set; }

    /// <summary>The id of what it opens.</summary>
    public string? LinkId { get; set; }

    /// <summary>The request its «موافقة / تأكيد» button acts on, through the approvals endpoints (F7).</summary>
    public int? ApprovalRequestId { get; set; }

    /// <summary>When it was sent (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the recipient read it (UTC); null while unread.</summary>
    public DateTime? ReadAt { get; set; }
}

/// <summary>A notification type one user turned off (D44): such notifications are not created for them.</summary>
public class NotificationMute
{
    /// <summary><c>ApplicationUser.Id</c>.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>The muted type; never one that <see cref="NotificationTypes.CanMute"/> refuses.</summary>
    public NotificationType Type { get; set; }

    /// <summary>When it was muted (UTC).</summary>
    public DateTime MutedAt { get; set; }
}
