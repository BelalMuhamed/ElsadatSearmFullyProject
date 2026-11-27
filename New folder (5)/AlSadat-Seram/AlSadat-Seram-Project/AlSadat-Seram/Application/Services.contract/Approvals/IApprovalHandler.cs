using Application.DTOs.Approvals;
using Domain.Common;
using Domain.Enums;

namespace Application.Services.contract.Approvals;

/// <summary>
/// Everything the approvals engine needs to know about one <see cref="ApprovalType"/> (D4). A module
/// registers one handler per type it owns (D42), scoped, as <c>IApprovalHandler</c>.
/// </summary>
public interface IApprovalHandler
{
    /// <summary>The type this handler serves; one handler per type.</summary>
    ApprovalType Type { get; }

    /// <summary>The permission code that lets a user see and decide this type.</summary>
    string DecidePermission { get; }

    /// <summary>
    /// Checks a new request before it is saved (the subject exists and can be asked about).
    /// A failure is returned to the requester as is.
    /// </summary>
    Task<Result<bool>> ValidateSubmitAsync(ApprovalSubmission submission, CancellationToken cancellationToken);

    /// <summary>
    /// Builds the cards of several pending requests in one go — one query for the batch, never one
    /// per request (D39). Returns a card for every request id given.
    /// </summary>
    Task<IReadOnlyDictionary<int, ApprovalCard>> DescribeAsync(
        IReadOnlyList<ApprovalSubject> requests, CancellationToken cancellationToken);

    /// <summary>
    /// Does what approving means (reverse the invoice, move the points…) inside the engine's
    /// transaction; the request is marked approved in the same save. A failure (e.g. the merchant's
    /// balance is now too low) rolls everything back and is returned to the approver.
    /// </summary>
    Task<Result<bool>> ApproveAsync(ApprovalSubject request, CancellationToken cancellationToken);

    /// <summary>
    /// The request ended without approval (rejected or withdrawn): undo anything held for it.
    /// Runs inside the engine's transaction.
    /// </summary>
    Task ReleaseAsync(ApprovalSubject request, ApprovalStatus outcome, CancellationToken cancellationToken);
}
