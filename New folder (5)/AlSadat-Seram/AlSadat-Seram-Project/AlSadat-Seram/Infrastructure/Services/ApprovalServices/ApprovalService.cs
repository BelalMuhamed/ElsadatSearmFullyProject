using Application.Common;
using Application.CommonPagination;
using Application.DTOs.Approvals;
using Application.Services.contract.Approvals;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Domain.Entities.Approvals;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.ApprovalServices;

/// <summary>
/// The approvals engine (F7, D4, D39–D42). Every decision runs in its own transaction: the request
/// is claimed first (its <c>RowVersion</c> makes a second decider fail with 409), then the type's
/// handler does its work, and both commit together or not at all.
/// </summary>
public sealed class ApprovalService : IApprovalService
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;
    private readonly IReadOnlyDictionary<ApprovalType, IApprovalHandler> _handlers;

    public ApprovalService(
        AppDbContext context,
        IEnumerable<IApprovalHandler> handlers,
        ICurrentUserService currentUser,
        ILocalizationService localization,
        TimeProvider clock)
    {
        _context = context;
        _currentUser = currentUser;
        _localization = localization;
        _clock = clock;

        var byType = new Dictionary<ApprovalType, IApprovalHandler>();
        foreach (var handler in handlers)
        {
            if (!byType.TryAdd(handler.Type, handler))
                throw new InvalidOperationException($"Two approval handlers are registered for {handler.Type}.");
        }
        _handlers = byType;
    }

    /// <inheritdoc />
    public async Task<Result<ApprovalDecisionDto>> SubmitAsync(ApprovalSubmission submission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        if (!_handlers.TryGetValue(submission.Type, out var handler))
            throw new InvalidOperationException($"No approval handler is registered for {submission.Type}.");
        if (submission.Payload?.Length > ApprovalRequest.PayloadMaxLength)
            throw new ArgumentException($"The payload is longer than {ApprovalRequest.PayloadMaxLength} characters.", nameof(submission));

        var reason = ApprovalRules.NormalizeReason(submission.Reason);
        if (reason is null)
            return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.ReasonRequired);

        var check = await handler.ValidateSubmitAsync(submission, cancellationToken);
        if (!check.IsSuccess)
            return FailureFrom(check);

        if (submission.SubjectId is { } subjectId && await _context.ApprovalRequests.AnyAsync(
                r => r.Type == submission.Type && r.SubjectId == subjectId && r.Status == ApprovalStatus.Pending,
                cancellationToken))
            return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.AlreadyPending, HttpStatusCode.Conflict);

        var request = new ApprovalRequest
        {
            Type = submission.Type,
            SubjectId = submission.SubjectId,
            Payload = submission.Payload,
            Reason = reason,
            Status = ApprovalStatus.Pending
        };
        _context.ApprovalRequests.Add(request);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Two requests for the same subject at the same moment: the index keeps one.
            _context.Entry(request).State = EntityState.Detached;
            return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.AlreadyPending, HttpStatusCode.Conflict);
        }

        return Result<ApprovalDecisionDto>.SuccessKey(
            ApprovalErrorCodes.Submitted, new ApprovalDecisionDto(request.Id, request.Status), HttpStatusCode.Created);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<ApprovalItemDto>>> ListPendingAsync(ApprovalListQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var types = AllowedTypes()
            .Where(t => query.Type is null || t == query.Type)
            .Where(t => query.Group is null || ApprovalTypes.GroupOf(t) == query.Group)
            .ToList();
        if (types.Count == 0)
            return Result<PagedResult<ApprovalItemDto>>.Success(new PagedResult<ApprovalItemDto>([], 0, query.Page, query.PageSize));

        var page = await PendingFor(types)
            .OrderBy(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Select(r => new RequestRow(
                r.Id, r.Type, r.SubjectId, r.Payload, r.Reason, r.CreatedBy,
                _context.Users.Where(u => u.Id == r.CreatedBy).Select(u => u.FullName).FirstOrDefault(),
                r.CreatedAt))
            .ToPagedResultAsync(query, cancellationToken);

        var cards = await DescribeAsync(page.Items, cancellationToken);
        var now = _clock.GetUtcNow().UtcDateTime;
        var items = page.Items
            .Select(r =>
            {
                var card = cards[r.Id];
                return new ApprovalItemDto(
                    r.Id, r.Type, ApprovalTypes.GroupOf(r.Type), r.SubjectId,
                    card.Title, card.Details, card.BlockedReason,
                    r.Reason, r.RequestedBy, r.RequestedByName, r.RequestedAt,
                    ApprovalRules.IsOverdue(r.RequestedAt, now));
            })
            .ToList();

        return Result<PagedResult<ApprovalItemDto>>.Success(
            new PagedResult<ApprovalItemDto>(items, page.TotalCount, page.Page, page.PageSize));
    }

    /// <inheritdoc />
    public async Task<Result<ApprovalSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var types = AllowedTypes();
        if (types.Count == 0)
            return Result<ApprovalSummaryDto>.Success(new ApprovalSummaryDto(0, 0, []));

        var overdueBefore = _clock.GetUtcNow().UtcDateTime - ApprovalRules.OverdueAfter;
        var counts = await PendingFor(types)
            .GroupBy(r => r.Type)
            .Select(g => new { Type = g.Key, Count = g.Count(), Overdue = g.Count(r => r.CreatedAt < overdueBefore) })
            .ToListAsync(cancellationToken);

        var byType = counts
            .OrderBy(c => c.Type)
            .Select(c => new ApprovalCountDto(c.Type, ApprovalTypes.GroupOf(c.Type), c.Count))
            .ToList();

        return Result<ApprovalSummaryDto>.Success(
            new ApprovalSummaryDto(counts.Sum(c => c.Count), counts.Sum(c => c.Overdue), byType));
    }

    /// <inheritdoc />
    public async Task<Result<ApprovalDetailsDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.RequireUserId();
        var row = await _context.ApprovalRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new
            {
                Request = new RequestRow(
                    r.Id, r.Type, r.SubjectId, r.Payload, r.Reason, r.CreatedBy,
                    _context.Users.Where(u => u.Id == r.CreatedBy).Select(u => u.FullName).FirstOrDefault(),
                    r.CreatedAt),
                r.Status,
                r.DecidedBy,
                DecidedByName = _context.Users.Where(u => u.Id == r.DecidedBy).Select(u => u.FullName).FirstOrDefault(),
                r.DecidedAt,
                r.DecisionReason
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null || (row.Request.RequestedBy != userId && !AllowedTypes().Contains(row.Request.Type)))
            return Result<ApprovalDetailsDto>.FailureKey(ApprovalErrorCodes.NotFound, HttpStatusCode.NotFound);

        var r = row.Request;
        var card = (await DescribeAsync([r], cancellationToken))[r.Id];
        return Result<ApprovalDetailsDto>.Success(new ApprovalDetailsDto(
            r.Id, r.Type, ApprovalTypes.GroupOf(r.Type), row.Status, r.SubjectId,
            card.Title, card.Details, card.BlockedReason,
            r.Reason, r.RequestedBy, r.RequestedByName, r.RequestedAt,
            row.DecidedBy, row.DecidedByName, row.DecidedAt, row.DecisionReason));
    }

    /// <inheritdoc />
    public Task<Result<ApprovalDecisionDto>> ApproveAsync(int id, CancellationToken cancellationToken = default)
        => DecideAsync(id, expectedType: null, ApprovalStatus.Approved, decisionReason: null, cancellationToken);

    /// <inheritdoc />
    public async Task<Result<ApprovalDecisionDto>> RejectAsync(int id, string reason, CancellationToken cancellationToken = default)
    {
        var normalized = ApprovalRules.NormalizeReason(reason);
        if (normalized is null)
            return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.ReasonRequired);

        return await DecideAsync(id, expectedType: null, ApprovalStatus.Rejected, normalized, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<ApprovalDecisionDto>> WithdrawAsync(int id, CancellationToken cancellationToken = default)
        => DecideAsync(id, expectedType: null, ApprovalStatus.Withdrawn, decisionReason: null, cancellationToken);

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<BulkApproveItemResult>>> ApproveBulkAsync(
        ApprovalType type, IReadOnlyList<int> ids, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        if (!AllowedTypes().Contains(type))
            return Result<IReadOnlyList<BulkApproveItemResult>>.FailureKey(ApprovalErrorCodes.NotAllowed, HttpStatusCode.Forbidden);

        var results = new List<BulkApproveItemResult>();
        foreach (var id in ids.Distinct())
        {
            // Each request on its own (D41): one that fails doesn't undo or stop the others.
            var result = await DecideAsync(id, type, ApprovalStatus.Approved, decisionReason: null, cancellationToken);
            results.Add(result.IsSuccess
                ? new BulkApproveItemResult(id, true, null, null)
                : new BulkApproveItemResult(id, false, result.Code, MessageOf(result)));
        }

        return Result<IReadOnlyList<BulkApproveItemResult>>.Success(results);
    }

    /// <summary>
    /// Approves, rejects or withdraws one request in its own transaction. The request is saved
    /// first, so a second decider fails on its <c>RowVersion</c> before any handler work runs twice.
    /// </summary>
    private async Task<Result<ApprovalDecisionDto>> DecideAsync(
        int id, ApprovalType? expectedType, ApprovalStatus outcome, string? decisionReason, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var request = await _context.ApprovalRequests.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);

        var refusal = Refusal(request, userId, expectedType, outcome);
        if (refusal is not null)
            return refusal;

        var handler = _handlers[request!.Type];
        var subject = SubjectOf(request);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            request.Status = outcome;
            request.DecidedBy = userId;
            request.DecidedAt = _clock.GetUtcNow().UtcDateTime;
            request.DecisionReason = decisionReason;
            await _context.SaveChangesAsync(cancellationToken);

            if (outcome == ApprovalStatus.Approved)
            {
                var work = await handler.ApproveAsync(subject, cancellationToken);
                if (!work.IsSuccess)
                {
                    await DiscardAsync(transaction);
                    return FailureFrom(work);
                }
            }
            else
            {
                await handler.ReleaseAsync(subject, outcome, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex) when (ex.Entries.Any(e => e.Entity is ApprovalRequest))
        {
            await DiscardAsync(transaction);
            return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.AlreadyDecided, HttpStatusCode.Conflict);
        }
        catch
        {
            await DiscardAsync(transaction);
            throw;
        }

        var messageKey = outcome switch
        {
            ApprovalStatus.Approved => ApprovalErrorCodes.Approved,
            ApprovalStatus.Rejected => ApprovalErrorCodes.Rejected,
            _ => ApprovalErrorCodes.Withdrawn
        };
        return Result<ApprovalDecisionDto>.SuccessKey(messageKey, new ApprovalDecisionDto(request.Id, request.Status));
    }

    /// <summary>Why the current user can't take this decision now, or null when they can.</summary>
    private Result<ApprovalDecisionDto>? Refusal(ApprovalRequest? request, string userId, ApprovalType? expectedType, ApprovalStatus outcome)
    {
        if (request is null)
            return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.NotFound, HttpStatusCode.NotFound);

        var canSeeType = AllowedTypes().Contains(request.Type);
        var isRequester = string.Equals(request.CreatedBy, userId, StringComparison.Ordinal);

        if (outcome == ApprovalStatus.Withdrawn)
        {
            if (!isRequester)
                return canSeeType
                    ? Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.NotRequester, HttpStatusCode.Forbidden)
                    : Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.NotFound, HttpStatusCode.NotFound);
        }
        else
        {
            if (!canSeeType)
                return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.NotFound, HttpStatusCode.NotFound);
            if (expectedType is { } type && request.Type != type)
                return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.TypeMismatch);
            if (!ApprovalRules.CanDecide(request.CreatedBy, userId, _currentUser.IsSuperAdmin))
                return Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.OwnRequest, HttpStatusCode.Forbidden);
        }

        return request.Status == ApprovalStatus.Pending
            ? null
            : Result<ApprovalDecisionDto>.FailureKey(ApprovalErrorCodes.AlreadyDecided, HttpStatusCode.Conflict);
    }

    /// <summary>Rolls back and forgets every tracked change, so the next request in a bulk starts clean.</summary>
    private async Task DiscardAsync(Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        await transaction.RollbackAsync(CancellationToken.None);
        _context.ChangeTracker.Clear();
    }

    /// <summary>
    /// The types the current user decides: those whose handler's permission is on their token, or
    /// all of them for <c>super_admin</c>. M1 moves the permission check to the cache (D3).
    /// </summary>
    private List<ApprovalType> AllowedTypes()
    {
        var principal = _currentUser.UserPrincipal;
        return _handlers.Values
            .Where(h => _currentUser.IsSuperAdmin
                        || principal?.HasClaim(AppClaimTypes.Permission, h.DecidePermission) == true)
            .Select(h => h.Type)
            .ToList();
    }

    /// <summary>Pending requests of these types the current user may decide: never their own (D40).</summary>
    private IQueryable<ApprovalRequest> PendingFor(List<ApprovalType> types)
    {
        var query = _context.ApprovalRequests
            .AsNoTracking()
            .Where(r => r.Status == ApprovalStatus.Pending && types.Contains(r.Type));

        if (_currentUser.IsSuperAdmin)
            return query;

        var userId = _currentUser.RequireUserId();
        return query.Where(r => r.CreatedBy == null || r.CreatedBy != userId);
    }

    /// <summary>Asks each type's handler for its cards, one call per type (D39).</summary>
    private async Task<Dictionary<int, ApprovalCard>> DescribeAsync(IReadOnlyList<RequestRow> rows, CancellationToken cancellationToken)
    {
        var cards = new Dictionary<int, ApprovalCard>();
        foreach (var group in rows.GroupBy(r => r.Type))
        {
            var subjects = group
                .Select(r => new ApprovalSubject(r.Id, r.SubjectId, r.Payload, r.Reason, r.RequestedBy))
                .ToList();
            var described = await _handlers[group.Key].DescribeAsync(subjects, cancellationToken);

            foreach (var subject in subjects)
            {
                if (!described.TryGetValue(subject.Id, out var card))
                    throw new InvalidOperationException($"The {group.Key} handler returned no card for request {subject.Id}.");
                cards[subject.Id] = card;
            }
        }
        return cards;
    }

    private string? MessageOf<T>(Result<T> result)
        => result.MessageKey is { } key ? _localization.Resolve(key) : result.Message;

    private static ApprovalSubject SubjectOf(ApprovalRequest request)
        => new(request.Id, request.SubjectId, request.Payload, request.Reason, request.CreatedBy);

    private static Result<ApprovalDecisionDto> FailureFrom(Result<bool> failure) => new()
    {
        IsSuccess = false,
        Message = failure.Message,
        MessageKey = failure.MessageKey,
        StatusCode = failure.StatusCode,
        Errors = failure.Errors
    };

    private static bool IsUniqueViolation(DbUpdateException exception)
        => exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation };

    /// <summary>The columns a list or a card needs.</summary>
    private sealed record RequestRow(
        int Id, ApprovalType Type, int? SubjectId, string? Payload, string Reason,
        string? RequestedBy, string? RequestedByName, DateTime RequestedAt);
}
