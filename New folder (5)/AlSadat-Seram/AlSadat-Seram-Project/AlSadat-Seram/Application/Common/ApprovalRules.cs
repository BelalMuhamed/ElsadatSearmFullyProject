using Domain.Entities.Approvals;

namespace Application.Common;

/// <summary>The pure rules of the approvals engine (F7, D39–D42), kept apart so they're unit tested.</summary>
public static class ApprovalRules
{
    /// <summary>Most requests one bulk approve takes (D41).</summary>
    public const int BulkMax = 100;

    /// <summary>A request waiting longer than this is flagged on screen 9.</summary>
    public static readonly TimeSpan OverdueAfter = TimeSpan.FromHours(24);

    /// <summary>
    /// Nobody approves or rejects their own request, except <c>super_admin</c> (D40). A request the
    /// system opened (no requester) can be decided by anyone allowed.
    /// </summary>
    public static bool CanDecide(string? requestedBy, string userId, bool isSuperAdmin)
        => isSuperAdmin || !string.Equals(requestedBy, userId, StringComparison.Ordinal);

    /// <summary>True when the request has waited longer than <see cref="OverdueAfter"/>.</summary>
    public static bool IsOverdue(DateTime requestedAt, DateTime now) => now - requestedAt > OverdueAfter;

    /// <summary>A reason is required and fits its column; returns it trimmed, or null when it doesn't.</summary>
    public static string? NormalizeReason(string? reason)
    {
        var trimmed = reason?.Trim();
        return string.IsNullOrEmpty(trimmed) || trimmed.Length > ApprovalRequest.ReasonMaxLength ? null : trimmed;
    }
}
