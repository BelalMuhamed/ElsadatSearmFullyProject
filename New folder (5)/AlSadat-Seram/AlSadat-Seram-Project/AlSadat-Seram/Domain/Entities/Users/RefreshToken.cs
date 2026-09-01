using Domain.Entities.Commonitems;
using System;

namespace Domain.Entities.Users;

public enum RevokedReason
{
    Rotated = 1,
    Logout = 2,
    LogoutAll = 3,
    ReuseDetected = 4,
    UserDeactivated = 5,
    PasswordChanged = 6,
    RoleChanged = 7,
    AdminRevoked = 8
}

public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>SHA-256 of the raw token. The raw token is never persisted or logged.</summary>
    public byte[] TokenHash { get; set; } = Array.Empty<byte>();

    /// <summary>Groups a token with every token it was rotated into/from. Reuse of a revoked
    /// token revokes the whole family.</summary>
    public Guid FamilyId { get; set; }

    public int? ReplacedByTokenId { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }

    /// <summary>Snapshot of ApplicationUser.SecurityStamp at the moment this token was issued.
    /// Compared against the current stamp on refresh; mismatch revokes the family.</summary>
    public string SecurityStampSnapshot { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByIp { get; set; } = string.Empty;

    /// <summary>Sliding expiry, reset on each rotation.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Set once at login, never extended. 90-day cap (Auth:AbsoluteSessionDays).</summary>
    public DateTime AbsoluteExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public RevokedReason? RevokedReason { get; set; }

    /// <summary>Null today. Forward hook for mobile per-device sessions (Phase 2/S-6) —
    /// no schema change needed when that lands.</summary>
    public string? DeviceId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt || DateTime.UtcNow >= AbsoluteExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
}