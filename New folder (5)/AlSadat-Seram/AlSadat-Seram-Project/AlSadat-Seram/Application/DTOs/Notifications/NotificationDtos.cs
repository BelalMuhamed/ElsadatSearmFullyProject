using Application.CommonPagination;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs.Notifications;

/// <summary>
/// One argument of a notification's message: plain text (a name, a number) or a key translated in
/// the reader's language when it is read (e.g. <c>ApprovalType.Leave</c>).
/// </summary>
public sealed record NotificationArg(
    [property: JsonPropertyName("t"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Text,
    [property: JsonPropertyName("k"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Key)
{
    /// <summary>Text shown as is.</summary>
    public static NotificationArg Of(string text) => new(text, null);

    /// <summary>A localization key, translated when read.</summary>
    public static NotificationArg Localized(string key) => new(null, key);
}

/// <summary>What a module sends through <c>INotificationPublisher</c>.</summary>
/// <param name="Type">What it is about.</param>
/// <param name="MessageKey">A key from <c>LocalizationResources</c>.</param>
/// <param name="Args">The key's arguments, in order.</param>
/// <param name="LinkKind">What it opens (<c>NotificationLinks</c>).</param>
/// <param name="LinkId">The id of what it opens.</param>
/// <param name="ApprovalRequestId">The request its approve button acts on.</param>
public sealed record NotificationMessage(
    NotificationType Type,
    string MessageKey,
    IReadOnlyList<NotificationArg>? Args = null,
    string? LinkKind = null,
    string? LinkId = null,
    int? ApprovalRequestId = null);

/// <summary>One notification as the bell and screen 66 show it, in the request's language.</summary>
/// <param name="CanAct">Its approval is still pending and the user may decide it (the inline button).</param>
public sealed record NotificationDto(
    int Id,
    NotificationType Type,
    NotificationCategory Category,
    string Text,
    string? LinkKind,
    string? LinkId,
    int? ApprovalRequestId,
    bool CanAct,
    DateTime CreatedAt,
    bool IsRead);

/// <summary>Filters for screen 66; newest first.</summary>
public sealed class NotificationListQuery : PageRequest
{
    /// <summary>One tab.</summary>
    public NotificationCategory? Category { get; set; }

    /// <summary>Unread only.</summary>
    public bool UnreadOnly { get; set; }

    /// <summary>Sent at or after (UTC), e.g. «آخر 7 أيام».</summary>
    public DateTime? From { get; set; }
}

/// <summary>The bell's number.</summary>
public sealed record UnreadCountDto(int Count);

/// <summary>What the SignalR push carries (D43): the client reads the item with <c>GET /api/notifications/{id}</c>.</summary>
public sealed record NotificationPush(int Id, int UnreadCount);

/// <summary>One row of the notification preferences (screen 7).</summary>
/// <param name="Enabled">«جوه السيستم» on.</param>
/// <param name="CanMute">False for approvals and permission changes (D44).</param>
public sealed record NotificationPreferenceDto(NotificationType Type, NotificationCategory Category, bool Enabled, bool CanMute);

/// <summary>One change of <c>PUT /api/notifications/preferences</c>.</summary>
public sealed class NotificationPreferenceChange
{
    /// <summary>The type.</summary>
    [Required]
    public NotificationType? Type { get; set; }

    /// <summary>On or off.</summary>
    public bool Enabled { get; set; }
}

/// <summary>Body of <c>PUT /api/notifications/preferences</c>.</summary>
public sealed class UpdateNotificationPreferencesRequest
{
    /// <summary>The types to change; the others stay as they are.</summary>
    [Required]
    [MinLength(1)]
    public List<NotificationPreferenceChange> Items { get; set; } = [];
}
