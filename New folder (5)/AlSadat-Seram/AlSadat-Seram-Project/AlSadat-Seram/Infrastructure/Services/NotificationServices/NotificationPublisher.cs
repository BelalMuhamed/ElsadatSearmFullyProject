using Application.DTOs.Notifications;
using Application.Services.contract.Notifications;
using Domain.Common;
using Domain.Entities.Notifications;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Infrastructure.Services.NotificationServices;

/// <summary>
/// Saves notifications with the scoped <see cref="AppDbContext"/> (so they share the caller's
/// transaction) and leaves the push to <c>NotificationPushInterceptor</c>, which sends it after the
/// commit (F8, D5, D43, D44).
/// </summary>
public sealed class NotificationPublisher(AppDbContext context, TimeProvider clock) : INotificationPublisher
{
    /// <summary>
    /// Arabic stays as is (escaped, each letter would take 6 characters). The column is read back by
    /// the API only and never written into HTML, so the relaxed encoder is safe here.
    /// </summary>
    private static readonly JsonSerializerOptions ArgsJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <inheritdoc />
    public async Task PublishAsync(NotificationMessage message, IReadOnlyCollection<string> recipientIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(recipientIds);
        ArgumentException.ThrowIfNullOrWhiteSpace(message.MessageKey);

        var recipients = recipientIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToList();
        if (recipients.Count == 0)
            return;

        if (NotificationTypes.CanMute(message.Type))
        {
            var muted = await context.NotificationMutes
                .Where(m => m.Type == message.Type && recipients.Contains(m.UserId))
                .Select(m => m.UserId)
                .ToListAsync(cancellationToken);
            recipients.RemoveAll(muted.Contains);
            if (recipients.Count == 0)
                return;
        }

        var args = SerializeArgs(message.Args);
        var now = clock.GetUtcNow().UtcDateTime;
        context.Notifications.AddRange(recipients.Select(recipient => new Notification
        {
            RecipientId = recipient,
            Type = message.Type,
            MessageKey = message.MessageKey,
            Args = args,
            LinkKind = message.LinkKind,
            LinkId = message.LinkId,
            ApprovalRequestId = message.ApprovalRequestId,
            CreatedAt = now
        }));
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task PublishToPermissionAsync(
        NotificationMessage message,
        string permissionCode,
        IReadOnlyCollection<string>? exceptUserIds = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);

        var holders = await context.Users
            .Where(u => !u.IsDeleted)
            .Where(u => context.UserPermissions.Any(p => p.UserId == u.Id && p.Permission.Code == permissionCode)
                        || context.UserRoles.Any(ur => ur.UserId == u.Id
                                                       && context.Roles.Any(r => r.Id == ur.RoleId && r.Name == AppRoles.Admin)))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (exceptUserIds is { Count: > 0 })
            holders.RemoveAll(exceptUserIds.Contains);

        await PublishAsync(message, holders, cancellationToken);
    }

    private static string? SerializeArgs(IReadOnlyList<NotificationArg>? args)
    {
        if (args is not { Count: > 0 })
            return null;

        var json = JsonSerializer.Serialize(args, ArgsJson);
        if (json.Length > Notification.ArgsMaxLength)
            throw new ArgumentException($"Notification arguments are longer than {Notification.ArgsMaxLength} characters.", nameof(args));
        return json;
    }
}
