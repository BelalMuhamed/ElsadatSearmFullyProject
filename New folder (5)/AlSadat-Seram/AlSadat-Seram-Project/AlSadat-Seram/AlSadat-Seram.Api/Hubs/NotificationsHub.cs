using Application.DTOs.Notifications;
using Application.Services.contract.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AlSadat_Seram.Api.Hubs;

/// <summary>
/// Live notifications (F8, D5). The client connects to <see cref="Path"/> with its access token
/// (<c>accessTokenFactory</c>, sent as <c>access_token</c>) and receives <see cref="PushMethod"/> with
/// <c>{ id, unreadCount }</c> (D43); it then reads the item from <c>GET /api/notifications/{id}</c>.
/// The hub has no client-to-server methods.
/// </summary>
[Authorize]
public sealed class NotificationsHub : Hub
{
    /// <summary>Where the hub is mapped.</summary>
    public const string Path = "/hubs/notifications";

    /// <summary>The client method that receives a <see cref="NotificationPush"/>.</summary>
    public const string PushMethod = "notification";
}

/// <summary>Sends pushes to a user's connections through <see cref="NotificationsHub"/>.</summary>
public sealed class SignalRNotificationPushSender(IHubContext<NotificationsHub> hub) : INotificationPushSender
{
    /// <inheritdoc />
    public Task SendAsync(string userId, NotificationPush push, CancellationToken cancellationToken)
        => hub.Clients.User(userId).SendAsync(NotificationsHub.PushMethod, push, cancellationToken);
}

/// <summary>A connection belongs to the token's user id — the same claim <c>CurrentUserService</c> reads.</summary>
public sealed class TokenUserIdProvider : IUserIdProvider
{
    /// <inheritdoc />
    public string? GetUserId(HubConnectionContext connection)
        => connection.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? connection.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}
