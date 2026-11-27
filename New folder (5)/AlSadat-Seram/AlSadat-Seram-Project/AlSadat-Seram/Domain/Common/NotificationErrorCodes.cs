namespace Domain.Common;

/// <summary>
/// Message keys (and error <c>code</c>s, D22) of the notifications (F8), plus the message keys the
/// engine itself sends. Each one has its ar/en text in <c>LocalizationResources</c>.
/// </summary>
public static class NotificationErrorCodes
{
    /// <summary>No such notification for this user.</summary>
    public const string NotFound = "Notification.NotFound";

    /// <summary>Approvals and permission changes can't be turned off (D44).</summary>
    public const string CannotMute = "Notification.CannotMute";

    /// <summary>«{0} بعت طلب: {1}» — to whoever can decide it (F7).</summary>
    public const string ApprovalRequested = "Notification.ApprovalRequested";

    /// <summary>«{0} وافق على طلبك: {1}» — to the requester.</summary>
    public const string ApprovalApproved = "Notification.ApprovalApproved";

    /// <summary>«{0} رفض طلبك: {1} · السبب: {2}» — to the requester.</summary>
    public const string ApprovalRejected = "Notification.ApprovalRejected";

    /// <summary>Stands for a requester with no account (e.g. «نسيت كلمة السر»).</summary>
    public const string Someone = "Notification.Someone";

    /// <summary>The label of an approval type: <c>ApprovalType.{name}</c>.</summary>
    public static string ApprovalTypeKey(Enums.ApprovalType type) => $"ApprovalType.{type}";
}
