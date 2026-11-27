namespace Domain.Enums;

/// <summary>
/// What an approval request asks for (F7, D4). Stored as <c>tinyint</c> on
/// <c>ApprovalRequests.Type</c>: never renumber a member, only add. Each type gets its
/// <c>IApprovalHandler</c> with its module (D42); a type without a handler can't be submitted.
/// </summary>
public enum ApprovalType : byte
{
    /// <summary>Cancel a sales invoice (M6): a draft is deleted, a confirmed one reversed.</summary>
    SalesInvoiceCancel = 1,

    /// <summary>Cancel a purchase invoice (M7).</summary>
    PurchaseInvoiceCancel = 2,

    /// <summary>Move a merchant's points to a plumber after an accepted inspection (M5).</summary>
    PointsToPlumber = 3,

    /// <summary>Confirm cash a sales rep paid out (M11).</summary>
    TreasuryConfirmation = 4,

    /// <summary>An employee's leave (M9).</summary>
    Leave = 5,

    /// <summary>An employee's loan (M9).</summary>
    Loan = 6,

    /// <summary>A user asks for a permission from the «no access» page (M1).</summary>
    PermissionRequest = 7,

    /// <summary>A forgotten password (M1).</summary>
    PasswordReset = 8
}

/// <summary>The tabs of the approvals center (screen 9); bulk approve works inside one type.</summary>
public enum ApprovalGroup : byte
{
    /// <summary>إلغاء فواتير.</summary>
    InvoiceCancellations = 1,

    /// <summary>تحويل نقاط.</summary>
    PointsTransfers = 2,

    /// <summary>تأكيدات الخزنة.</summary>
    TreasuryConfirmations = 3,

    /// <summary>إجازات وقروض.</summary>
    LeavesAndLoans = 4,

    /// <summary>صلاحيات وكلمات مرور.</summary>
    Access = 5
}

/// <summary>Where a request stands. Only <see cref="Pending"/> can change.</summary>
public enum ApprovalStatus : byte
{
    /// <summary>Waiting for a decision.</summary>
    Pending = 1,

    /// <summary>Approved; the handler's work is done in the same transaction.</summary>
    Approved = 2,

    /// <summary>Rejected with a reason.</summary>
    Rejected = 3,

    /// <summary>The requester took it back before a decision.</summary>
    Withdrawn = 4
}

/// <summary>Fixed facts about each <see cref="ApprovalType"/>.</summary>
public static class ApprovalTypes
{
    /// <summary>The tab a type is shown under.</summary>
    public static ApprovalGroup GroupOf(ApprovalType type) => type switch
    {
        ApprovalType.SalesInvoiceCancel or ApprovalType.PurchaseInvoiceCancel => ApprovalGroup.InvoiceCancellations,
        ApprovalType.PointsToPlumber => ApprovalGroup.PointsTransfers,
        ApprovalType.TreasuryConfirmation => ApprovalGroup.TreasuryConfirmations,
        ApprovalType.Leave or ApprovalType.Loan => ApprovalGroup.LeavesAndLoans,
        ApprovalType.PermissionRequest or ApprovalType.PasswordReset => ApprovalGroup.Access,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown approval type.")
    };
}
