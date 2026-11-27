using Application.CommonPagination;
using Application.DTOs.Approvals;
using Domain.Common;
using Domain.Enums;

namespace Application.Services.contract.Approvals;

/// <summary>
/// The approvals engine (F7, D4, D39–D42): one table for every request, one handler per type. The
/// current user comes from the token; a user sees and decides only the types they have the
/// handler's permission for (<c>super_admin</c> sees all).
/// </summary>
public interface IApprovalService
{
    /// <summary>
    /// Opens a request for the current user. Called by a module's own endpoint («طلب إلغاء»…), never
    /// from the client directly. At most one pending request per (type, subject):
    /// a second one returns 409 <c>Approval.AlreadyPending</c>.
    /// </summary>
    Task<Result<ApprovalDecisionDto>> SubmitAsync(ApprovalSubmission submission, CancellationToken cancellationToken = default);

    /// <summary>The pending requests the current user can decide, oldest first, without their own (D40).</summary>
    Task<Result<PagedResult<ApprovalItemDto>>> ListPendingAsync(ApprovalListQuery query, CancellationToken cancellationToken = default);

    /// <summary>The numbers of screen 9 and the top bar for the current user.</summary>
    Task<Result<ApprovalSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>One request; visible to its requester and to whoever can decide its type.</summary>
    Task<Result<ApprovalDetailsDto>> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Approves a pending request: the handler's work and the status change in one transaction.</summary>
    Task<Result<ApprovalDecisionDto>> ApproveAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Rejects a pending request with a required reason that reaches the requester.</summary>
    Task<Result<ApprovalDecisionDto>> RejectAsync(int id, string reason, CancellationToken cancellationToken = default);

    /// <summary>The requester takes back their own pending request.</summary>
    Task<Result<ApprovalDecisionDto>> WithdrawAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves several requests of one type, each in its own transaction (D41); the result lists
    /// what happened to each one.
    /// </summary>
    Task<Result<IReadOnlyList<BulkApproveItemResult>>> ApproveBulkAsync(
        ApprovalType type, IReadOnlyList<int> ids, CancellationToken cancellationToken = default);
}
