namespace Domain.Enums;

/// <summary>
/// What a notification is about (F8). Stored as <c>tinyint</c>: never renumber a member, only add.
/// Each module sends its own types through <c>INotificationPublisher</c>; the user can mute a type
/// unless <see cref="NotificationTypes.CanMute"/> says no (D44).
/// </summary>
public enum NotificationType : byte
{
    /// <summary>A request waits for the recipient's decision (F7).</summary>
    ApprovalRequested = 1,

    /// <summary>The recipient's own request was approved or rejected (F7).</summary>
    ApprovalDecided = 2,

    /// <summary>The recipient's permissions changed (M1).</summary>
    PermissionsChanged = 3,

    /// <summary>A product fell under its minimum in a warehouse (M4).</summary>
    StockLow = 4,

    /// <summary>A sales or purchase invoice was confirmed (M6, M7).</summary>
    InvoiceConfirmed = 5,

    /// <summary>A large export finished in the background (F10).</summary>
    ExportReady = 6,

    /// <summary>A new internal message (M13).</summary>
    Message = 7,

    /// <summary>A new offer from management (M13).</summary>
    Offer = 8,

    /// <summary>An inspection changed state, e.g. accepted with points waiting (M12).</summary>
    Inspection = 9
}

/// <summary>The tabs of «كل الإشعارات» (screen 66).</summary>
public enum NotificationCategory : byte
{
    /// <summary>موافقات.</summary>
    Approvals = 1,

    /// <summary>مخزون.</summary>
    Stock = 2,

    /// <summary>فواتير.</summary>
    Invoices = 3,

    /// <summary>رسايل.</summary>
    Messages = 4,

    /// <summary>تصدير.</summary>
    Exports = 5,

    /// <summary>Shown under «الكل» only.</summary>
    Other = 6
}

/// <summary>Fixed facts about each <see cref="NotificationType"/>.</summary>
public static class NotificationTypes
{
    /// <summary>The tab a type is shown under.</summary>
    public static NotificationCategory CategoryOf(NotificationType type) => type switch
    {
        NotificationType.ApprovalRequested or NotificationType.ApprovalDecided => NotificationCategory.Approvals,
        NotificationType.StockLow => NotificationCategory.Stock,
        NotificationType.InvoiceConfirmed => NotificationCategory.Invoices,
        NotificationType.Message => NotificationCategory.Messages,
        NotificationType.ExportReady => NotificationCategory.Exports,
        NotificationType.PermissionsChanged or NotificationType.Offer or NotificationType.Inspection => NotificationCategory.Other,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown notification type.")
    };

    /// <summary>Approvals and permission changes always reach the user (D44, screen 7).</summary>
    public static bool CanMute(NotificationType type)
        => type is not (NotificationType.ApprovalRequested or NotificationType.ApprovalDecided or NotificationType.PermissionsChanged);
}
