using Domain.Common;
using Domain.Entities.Commonitems;
using Domain.Enums;

namespace Domain.Entities.Approvals;

/// <summary>
/// One request waiting for (or past) a decision in the approvals center (F7, D4). The module's own
/// row (the invoice, the leave…) is the <see cref="SubjectId"/>; its type's <c>IApprovalHandler</c>
/// describes it and does the work on approval. Changed only through <c>IApprovalService</c>.
/// <see cref="IAuditedEntity.CreatedBy"/> is the requester.
/// </summary>
[Audited(AuditAreas.Approvals)]
public class ApprovalRequest : BaseEntity, IAuditedEntity
{
    public const int ReasonMaxLength = 500;
    public const int PayloadMaxLength = 2000;

    /// <summary>What is asked for.</summary>
    public ApprovalType Type { get; set; }

    /// <summary>
    /// Id of the module's row the request is about. At most one pending request per
    /// (<see cref="Type"/>, <see cref="SubjectId"/>). Null for types with no row of their own.
    /// </summary>
    public int? SubjectId { get; set; }

    /// <summary>Small JSON the type needs and has no table for (e.g. the permission asked for).</summary>
    public string? Payload { get; set; }

    /// <summary>Why the requester asks; always given.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Where the request stands; only <see cref="ApprovalStatus.Pending"/> changes.</summary>
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    /// <summary><c>ApplicationUser.Id</c> of who approved, rejected or withdrew it.</summary>
    public string? DecidedBy { get; set; }

    /// <summary>When it was decided (UTC).</summary>
    public DateTime? DecidedAt { get; set; }

    /// <summary>The rejection reason (required on reject); it reaches the requester.</summary>
    public string? DecisionReason { get; set; }

    /// <inheritdoc />
    public string? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
    public string? UpdatedBy { get; set; }

    /// <inheritdoc />
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Two decisions at the same moment: the second save fails with 409 (D33).</summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[] RowVersion { get; set; } = [];
}
