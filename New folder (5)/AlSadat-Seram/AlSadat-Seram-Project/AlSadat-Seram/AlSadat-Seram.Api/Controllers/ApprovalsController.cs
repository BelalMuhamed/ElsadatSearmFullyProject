using Application.DTOs.Approvals;
using Application.Services.contract.Approvals;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers;

/// <summary>
/// The approvals center (screen 9, F7). Any signed-in user may call it: the service shows and lets
/// them decide only the request types whose permission they have, plus withdrawing their own.
/// Requests are opened by each module's own endpoint, not here.
/// </summary>
[Route("api/approvals")]
[Authorize]
public sealed class ApprovalsController(
    IApprovalService approvals,
    ILocalizationService localization,
    ICurrentUserService currentUser) : BaseApiController(localization, currentUser)
{
    /// <summary>Pending requests the user can decide, oldest first; filter by tab or by one type.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] ApprovalListQuery query, CancellationToken cancellationToken)
        => HandleResult(await approvals.ListPendingAsync(query, cancellationToken));

    /// <summary>The counts of the tabs, the top-bar badge and the «over 24 hours» notice.</summary>
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        => HandleResult(await approvals.GetSummaryAsync(cancellationToken));

    /// <summary>One request with its decision.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => HandleResult(await approvals.GetAsync(id, cancellationToken));

    /// <summary>Approves a request; the type's work happens in the same transaction.</summary>
    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        => HandleResult(await approvals.ApproveAsync(id, cancellationToken));

    /// <summary>Rejects a request with a reason that reaches the requester.</summary>
    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectApprovalRequest body, CancellationToken cancellationToken)
        => HandleResult(await approvals.RejectAsync(id, body.Reason, cancellationToken));

    /// <summary>The requester takes back their own pending request.</summary>
    [HttpPost("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
        => HandleResult(await approvals.WithdrawAsync(id, cancellationToken));

    /// <summary>Approves several requests of one type; each succeeds or fails on its own (D41).</summary>
    [HttpPost("approve-bulk")]
    public async Task<IActionResult> ApproveBulk([FromBody] BulkApproveRequest body, CancellationToken cancellationToken)
        => HandleResult(await approvals.ApproveBulkAsync(body.Type!.Value, body.Ids, cancellationToken));
}
