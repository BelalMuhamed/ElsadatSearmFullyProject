namespace Domain.Common;

/// <summary>
/// Message keys (and error <c>code</c>s, D22) of the approvals engine (F7). Each one has its ar/en
/// text in <c>LocalizationResources</c>.
/// </summary>
public static class ApprovalErrorCodes
{
    /// <summary>No such request, or the user can't see it.</summary>
    public const string NotFound = "Approval.NotFound";

    /// <summary>Someone already approved, rejected or withdrew it.</summary>
    public const string AlreadyDecided = "Approval.AlreadyDecided";

    /// <summary>Nobody decides their own request, except <c>super_admin</c> (D40).</summary>
    public const string OwnRequest = "Approval.OwnRequest";

    /// <summary>The user hasn't the permission that decides this type.</summary>
    public const string NotAllowed = "Approval.NotAllowed";

    /// <summary>The reason is missing or too long.</summary>
    public const string ReasonRequired = "Approval.ReasonRequired";

    /// <summary>A request for the same subject is still waiting.</summary>
    public const string AlreadyPending = "Approval.AlreadyPending";

    /// <summary>Only the requester withdraws a request.</summary>
    public const string NotRequester = "Approval.NotRequester";

    /// <summary>A bulk approve got a request of another type.</summary>
    public const string TypeMismatch = "Approval.TypeMismatch";

    /// <summary>Success: the request was sent to the approvals center.</summary>
    public const string Submitted = "Approval.Submitted";

    /// <summary>Success: approved.</summary>
    public const string Approved = "Approval.Approved";

    /// <summary>Success: rejected.</summary>
    public const string Rejected = "Approval.Rejected";

    /// <summary>Success: withdrawn.</summary>
    public const string Withdrawn = "Approval.Withdrawn";
}
