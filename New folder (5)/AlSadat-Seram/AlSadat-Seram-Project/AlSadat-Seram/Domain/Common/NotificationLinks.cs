namespace Domain.Common;

/// <summary>
/// What a notification opens (<c>Notification.LinkKind</c>); the client maps each value to a screen.
/// Each module adds its own when it starts sending notifications. Never rename a value: old rows keep it.
/// </summary>
public static class NotificationLinks
{
    /// <summary>An approval request (F7); <c>LinkId</c> is its id.</summary>
    public const string ApprovalRequest = "ApprovalRequest";
}
