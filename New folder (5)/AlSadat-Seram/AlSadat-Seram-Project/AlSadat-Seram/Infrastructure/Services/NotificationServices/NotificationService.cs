using Application.CommonPagination;
using Application.DTOs.Notifications;
using Application.Services.contract.Approvals;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Application.Services.contract.Notifications;
using Domain.Common;
using Domain.Entities.Notifications;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace Infrastructure.Services.NotificationServices;

/// <summary>
/// The current user's notifications (screens 65, 66) and preferences (screen 7). Text is rendered here,
/// in the request's language, from the stored key + arguments (D43).
/// </summary>
public sealed class NotificationService(
    AppDbContext context,
    ICurrentUserService currentUser,
    ILocalizationService localization,
    IApprovalService approvals,
    TimeProvider clock) : INotificationService
{
    /// <inheritdoc />
    public async Task<Result<PagedResult<NotificationDto>>> ListAsync(NotificationListQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var rows = Mine();

        if (query.Category is { } category)
        {
            var types = Enum.GetValues<NotificationType>().Where(t => NotificationTypes.CategoryOf(t) == category).ToList();
            rows = rows.Where(n => types.Contains(n.Type));
        }
        if (query.UnreadOnly)
            rows = rows.Where(n => n.ReadAt == null);
        if (query.From is { } from)
            rows = rows.Where(n => n.CreatedAt >= from);

        var page = await rows
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .ToPagedResultAsync(query, cancellationToken);

        var items = await RenderAsync(page.Items, cancellationToken);
        return Result<PagedResult<NotificationDto>>.Success(
            new PagedResult<NotificationDto>(items, page.TotalCount, page.Page, page.PageSize));
    }

    /// <inheritdoc />
    public async Task<Result<NotificationDto>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await Mine().SingleOrDefaultAsync(n => n.Id == id, cancellationToken);
        if (row is null)
            return Result<NotificationDto>.FailureKey(NotificationErrorCodes.NotFound, HttpStatusCode.NotFound);

        return Result<NotificationDto>.Success((await RenderAsync([row], cancellationToken))[0]);
    }

    /// <inheritdoc />
    public async Task<Result<UnreadCountDto>> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        => Result<UnreadCountDto>.Success(new UnreadCountDto(await UnreadAsync(cancellationToken)));

    /// <inheritdoc />
    public async Task<Result<UnreadCountDto>> MarkReadAsync(int id, CancellationToken cancellationToken = default)
    {
        var userId = currentUser.RequireUserId();
        var now = clock.GetUtcNow().UtcDateTime;
        var changed = await context.Notifications
            .Where(n => n.Id == id && n.RecipientId == userId && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, now), cancellationToken);

        if (changed == 0 && !await Mine().AnyAsync(n => n.Id == id, cancellationToken))
            return Result<UnreadCountDto>.FailureKey(NotificationErrorCodes.NotFound, HttpStatusCode.NotFound);

        return Result<UnreadCountDto>.Success(new UnreadCountDto(await UnreadAsync(cancellationToken)));
    }

    /// <inheritdoc />
    public async Task<Result<UnreadCountDto>> MarkAllReadAsync(CancellationToken cancellationToken = default)
    {
        var userId = currentUser.RequireUserId();
        var now = clock.GetUtcNow().UtcDateTime;
        await context.Notifications
            .Where(n => n.RecipientId == userId && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, now), cancellationToken);

        return Result<UnreadCountDto>.Success(new UnreadCountDto(0));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<NotificationPreferenceDto>>> GetPreferencesAsync(CancellationToken cancellationToken = default)
        => Result<IReadOnlyList<NotificationPreferenceDto>>.Success(await PreferencesAsync(cancellationToken));

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<NotificationPreferenceDto>>> UpdatePreferencesAsync(
        IReadOnlyList<NotificationPreferenceChange> changes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(changes);
        if (changes.Any(c => c.Type is not { } type || !Enum.IsDefined(type)))
            return Result<IReadOnlyList<NotificationPreferenceDto>>.FailureKey(ErrorCodes.ValidationFailed);
        if (changes.Any(c => !c.Enabled && !NotificationTypes.CanMute(c.Type!.Value)))
            return Result<IReadOnlyList<NotificationPreferenceDto>>.FailureKey(NotificationErrorCodes.CannotMute);

        var userId = currentUser.RequireUserId();
        var existing = await context.NotificationMutes.Where(m => m.UserId == userId).ToListAsync(cancellationToken);
        var now = clock.GetUtcNow().UtcDateTime;

        // The last change of a type wins when the client sends it twice.
        foreach (var change in changes.GroupBy(c => c.Type!.Value).Select(g => g.Last()))
        {
            var type = change.Type!.Value;
            var mute = existing.FirstOrDefault(m => m.Type == type);
            if (change.Enabled && mute is not null)
                context.NotificationMutes.Remove(mute);
            else if (!change.Enabled && mute is null)
                context.NotificationMutes.Add(new NotificationMute { UserId = userId, Type = type, MutedAt = now });
        }
        await context.SaveChangesAsync(cancellationToken);

        return Result<IReadOnlyList<NotificationPreferenceDto>>.Success(await PreferencesAsync(cancellationToken));
    }

    private IQueryable<Notification> Mine()
    {
        var userId = currentUser.RequireUserId();
        return context.Notifications.AsNoTracking().Where(n => n.RecipientId == userId);
    }

    private Task<int> UnreadAsync(CancellationToken cancellationToken)
        => Mine().CountAsync(n => n.ReadAt == null, cancellationToken);

    private async Task<IReadOnlyList<NotificationPreferenceDto>> PreferencesAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.RequireUserId();
        var muted = await context.NotificationMutes
            .Where(m => m.UserId == userId)
            .Select(m => m.Type)
            .ToListAsync(cancellationToken);

        return Enum.GetValues<NotificationType>()
            .Select(t => new NotificationPreferenceDto(t, NotificationTypes.CategoryOf(t), !muted.Contains(t), NotificationTypes.CanMute(t)))
            .ToList();
    }

    /// <summary>Renders the text and works out which approval buttons the user can still use.</summary>
    private async Task<List<NotificationDto>> RenderAsync(IReadOnlyList<Notification> rows, CancellationToken cancellationToken)
    {
        var requestIds = rows.Where(n => n.ApprovalRequestId is not null).Select(n => n.ApprovalRequestId!.Value).Distinct().ToList();
        IReadOnlySet<int> decidable = requestIds.Count == 0
            ? new HashSet<int>()
            : await approvals.DecidableAsync(requestIds, cancellationToken);

        return rows.Select(n => new NotificationDto(
                n.Id,
                n.Type,
                NotificationTypes.CategoryOf(n.Type),
                Text(n),
                n.LinkKind,
                n.LinkId,
                n.ApprovalRequestId,
                n.ApprovalRequestId is { } requestId && decidable.Contains(requestId),
                n.CreatedAt,
                n.ReadAt is not null))
            .ToList();
    }

    private string Text(Notification notification)
    {
        var args = string.IsNullOrEmpty(notification.Args)
            ? []
            : JsonSerializer.Deserialize<List<NotificationArg>>(notification.Args) ?? [];

        var values = args
            .Select(a => (object)(a.Key is { } key ? localization.Resolve(key) : a.Text ?? string.Empty))
            .ToArray();
        return localization.Resolve(notification.MessageKey, values);
    }
}
