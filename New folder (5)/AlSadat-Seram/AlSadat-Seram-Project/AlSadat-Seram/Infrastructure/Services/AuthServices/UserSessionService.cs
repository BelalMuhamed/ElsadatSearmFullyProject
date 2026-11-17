using Application.Services.contract.AuthService;
using Domain.Common;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.AuthServices;

public class UserSessionService(AppDbContext _context, UserManager<ApplicationUser> _userManager) : IUserSessionService
{
    /// <inheritdoc />
    public async Task<Result<string>> AdminRevokeSessionsAsync(
        string targetUserId, string actorUserId, string ipAddress, CancellationToken ct = default)
    {
        var target = await _userManager.FindByIdAsync(targetUserId);
        if (target is null)
            return Result<string>.FailureKey("Auth.UserNotFound", HttpStatusCode.NotFound);

        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        var revokedCount = await _context.RefreshTokens
            .Where(t => t.UserId == targetUserId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                .SetProperty(t => t.RevokedByIp, ipAddress)
                .SetProperty(t => t.RevokedReason, RevokedReason.AdminRevoked), ct);

        // Bumping the stamp — not just revoking the rows above — is what actually
        // matters here: it also invalidates any refresh token this query missed
        // (e.g. one issued in the instant between the query and the update),
        // and it's what forces the SecurityStampSnapshot check to reject a refresh
        // attempt on the very next call, same mechanism as permission revocation.
        var stampResult = await _userManager.UpdateSecurityStampAsync(target);
        if (!stampResult.Succeeded)
        {
            await tx.RollbackAsync(ct);
            return Result<string>.FailureKey("Auth.RevokeSessionsFailed", HttpStatusCode.InternalServerError);
        }

        _context.AuthAuditLogs.Add(new AuthAuditLog
        {
            EventType = AuthEventType.AdminRevokedSessions,
            UserId = targetUserId,
            ActorUserId = actorUserId,
            Succeeded = true,
            IpAddress = ipAddress,
            Detail = $"{revokedCount} active session(s) revoked",
            OccurredAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);

        await tx.CommitAsync(ct);

        return Result<string>.SuccessKey("Permission.SessionsRevoked", targetUserId, HttpStatusCode.OK);
    }
}
