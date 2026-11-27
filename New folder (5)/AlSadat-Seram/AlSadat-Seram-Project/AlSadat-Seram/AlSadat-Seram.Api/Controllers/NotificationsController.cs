using Application.DTOs.Notifications;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Application.Services.contract.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers;

/// <summary>
/// The current user's notifications (the bell, screens 65 and 66) and their preferences (screen 7).
/// Every signed-in user has these; each one only ever sees their own.
/// </summary>
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController(
    INotificationService notifications,
    ILocalizationService localization,
    ICurrentUserService currentUser) : BaseApiController(localization, currentUser)
{
    /// <summary>Newest first; filter by tab, unread, and «since».</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] NotificationListQuery query, CancellationToken cancellationToken)
        => HandleResult(await notifications.ListAsync(query, cancellationToken));

    /// <summary>The bell's number.</summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
        => HandleResult(await notifications.GetUnreadCountAsync(cancellationToken));

    /// <summary>One notification — what the client reads after a SignalR push (D43).</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
        => HandleResult(await notifications.GetAsync(id, cancellationToken));

    /// <summary>Marks one read; returns the new unread count.</summary>
    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
        => HandleResult(await notifications.MarkReadAsync(id, cancellationToken));

    /// <summary>«علّم الكل مقروء».</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
        => HandleResult(await notifications.MarkAllReadAsync(cancellationToken));

    /// <summary>Every type, on or off, and whether it can be turned off.</summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> Preferences(CancellationToken cancellationToken)
        => HandleResult(await notifications.GetPreferencesAsync(cancellationToken));

    /// <summary>Turns types on or off; approvals and permission changes can't be turned off (D44).</summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesRequest body, CancellationToken cancellationToken)
        => HandleResult(await notifications.UpdatePreferencesAsync(body.Items, cancellationToken));
}
