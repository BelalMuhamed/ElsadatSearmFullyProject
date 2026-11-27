using Application.CommonPagination;
using Domain.Entities.Approvals;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Approvals;

/// <summary>What a module sends to open a request (<c>IApprovalService.SubmitAsync</c>).</summary>
/// <param name="Type">What is asked for; it must have a registered handler.</param>
/// <param name="SubjectId">The module's row (invoice, leave…); null for types with no row.</param>
/// <param name="Reason">Why, as the requester typed it.</param>
/// <param name="Payload">Small JSON the type needs and has no table for.</param>
public sealed record ApprovalSubmission(ApprovalType Type, int? SubjectId, string Reason, string? Payload = null);

/// <summary>The request as a handler sees it.</summary>
/// <param name="Id">The request id.</param>
/// <param name="SubjectId">The module's row.</param>
/// <param name="Payload">The type's JSON, if any.</param>
/// <param name="Reason">The requester's reason.</param>
/// <param name="RequestedBy"><c>ApplicationUser.Id</c> of the requester; null when the system asked.</param>
public sealed record ApprovalSubject(int Id, int? SubjectId, string? Payload, string Reason, string? RequestedBy);

/// <summary>
/// One row of the approvals center, built by the type's handler when the list is read (D39), so
/// balances are current. Text is already in the request's language.
/// </summary>
/// <param name="Title">The bold line, e.g. «فاتورة بيع INV-1038 · مؤسسة النور · ٤٢٬٣٠٠ ج.م».</param>
/// <param name="Details">The grey line: what approving will do.</param>
/// <param name="BlockedReason">Why approve is disabled right now; null when it can be approved.</param>
public sealed record ApprovalCard(string Title, string? Details = null, string? BlockedReason = null);

/// <summary>Filters for the pending list; oldest first.</summary>
public sealed class ApprovalListQuery : PageRequest
{
    /// <summary>One type only (the bulk-approve view).</summary>
    public ApprovalType? Type { get; set; }

    /// <summary>One tab of the screen.</summary>
    public ApprovalGroup? Group { get; set; }
}

/// <summary>A pending request in the approvals center (screen 9).</summary>
public sealed record ApprovalItemDto(
    int Id,
    ApprovalType Type,
    ApprovalGroup Group,
    int? SubjectId,
    string Title,
    string? Details,
    string? BlockedReason,
    string Reason,
    string? RequestedById,
    string? RequestedByName,
    DateTime RequestedAt,
    bool IsOverdue);

/// <summary>One count on the screen's tabs.</summary>
public sealed record ApprovalCountDto(ApprovalType Type, ApprovalGroup Group, int Count);

/// <summary>The numbers of screen 9 and the top bar: what the user can decide.</summary>
/// <param name="Total">«مستني موافقتك».</param>
/// <param name="Overdue">Waiting longer than 24 hours.</param>
/// <param name="ByType">Per type; the client sums them per tab.</param>
public sealed record ApprovalSummaryDto(int Total, int Overdue, IReadOnlyList<ApprovalCountDto> ByType);

/// <summary>One request with its decision, for its own page or the requester.</summary>
public sealed record ApprovalDetailsDto(
    int Id,
    ApprovalType Type,
    ApprovalGroup Group,
    ApprovalStatus Status,
    int? SubjectId,
    string Title,
    string? Details,
    string? BlockedReason,
    string Reason,
    string? RequestedById,
    string? RequestedByName,
    DateTime RequestedAt,
    string? DecidedById,
    string? DecidedByName,
    DateTime? DecidedAt,
    string? DecisionReason);

/// <summary>Where a request stands after a decision.</summary>
public sealed record ApprovalDecisionDto(int Id, ApprovalStatus Status);

/// <summary>Body of <c>POST /api/approvals/{id}/reject</c>.</summary>
public sealed class RejectApprovalRequest
{
    /// <summary>Why it is rejected; it reaches the requester.</summary>
    [Required]
    [MaxLength(ApprovalRequest.ReasonMaxLength)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Body of <c>POST /api/approvals/approve-bulk</c>: several requests of one type (D41).</summary>
public sealed class BulkApproveRequest
{
    /// <summary>The type every id must have.</summary>
    [Required]
    public ApprovalType? Type { get; set; }

    /// <summary>The requests to approve, up to <see cref="Common.ApprovalRules.BulkMax"/>.</summary>
    [Required]
    [MinLength(1)]
    [MaxLength(Common.ApprovalRules.BulkMax)]
    public List<int> Ids { get; set; } = [];
}

/// <summary>What happened to one request of a bulk approve.</summary>
/// <param name="Id">The request id.</param>
/// <param name="Succeeded">Approved.</param>
/// <param name="Code">The error code when it wasn't (D22).</param>
/// <param name="Message">The localized reason when it wasn't.</param>
public sealed record BulkApproveItemResult(int Id, bool Succeeded, string? Code, string? Message);
