using Application.Services.contract.AuthService;
using Domain.Common;
using Domain.Entities.Auth;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.AuthServices;

public class UserSessionService(AppDbContext _context) : IUserSessionService
{
    public async Task<Result<string>> AdminRevokeSessionsAsync(string targetUserId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == targetUserId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = Domain.Entities.Users.RevokedReason.AdminRevoked;
        }
        await _context.SaveChangesAsync();

        _context.AuthAuditLogs.Add(new AuthAuditLog
        {
            EventType = AuthEventType.AdminRevokedSessions,
            UserId = targetUserId,
            Succeeded = true,
            IpAddress = "server",
            OccurredAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Result<string>.Success("Sessions revoked", HttpStatusCode.OK);
    }
}