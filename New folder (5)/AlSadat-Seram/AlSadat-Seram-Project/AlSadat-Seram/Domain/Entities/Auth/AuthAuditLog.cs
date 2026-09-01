using System;

namespace Domain.Entities.Auth;

public enum AuthEventType
{
    LoginSucceeded = 1,
    LoginFailed = 2,
    AccountLockedOut = 3,
    TokenRefreshed = 4,
    RefreshTokenReuseDetected = 5,
    LogoutSingle = 6,
    LogoutAll = 7,
    AdminRevokedSessions = 8,
    PermissionsChanged = 9,
    UserCreated = 10,
    UserDeactivated = 11,
    UserReactivated = 12,
    PasswordChanged = 13
}

public class AuthAuditLog
{
    public long Id { get; set; }
    public AuthEventType EventType { get; set; }

    /// <summary>Null for a failed login against an unknown identifier.</summary>
    public string? UserId { get; set; }
    public Domain.Entities.Users.ApplicationUser? User { get; set; }

    /// <summary>The submitted identifier when UserId can't be resolved (e.g. unknown-user login failure).</summary>
    public string? AttemptedIdentifier { get; set; }

    /// <summary>Set when an admin acted on someone else's session/permissions — the acting user.</summary>
    public string? ActorUserId { get; set; }
    public Domain.Entities.Users.ApplicationUser? ActorUser { get; set; }

    public bool Succeeded { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? UserAgent { get; set; }

    /// <summary>Never the raw token, never a password.</summary>
    public string? Detail { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}