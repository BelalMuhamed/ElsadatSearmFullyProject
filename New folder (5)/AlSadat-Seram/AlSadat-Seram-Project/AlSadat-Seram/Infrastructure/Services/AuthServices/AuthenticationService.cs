using Application.DTOs.Authentcation;
using Application.Services.contract.Authorization;
using Application.Services.contract.AuthService;
using Application.Services.contract.JwtService;
using Domain.Common;
using Domain.Entities.Auth;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services.AuthServices;

public class AuthenticationService(
    UserManager<ApplicationUser> _userManager,
    SignInManager<ApplicationUser> _signInManager,
    IJwtService _jwtService,
    AppDbContext _context,
    IUserPermissionService _userPermissionService,
    IOptions<AuthSessionOptions> _sessionOptions) : IAuthenticationService
{
    // ── Identifier resolution: phone → username → email, first match wins (A-3) ──
    private async Task<ApplicationUser?> ResolveUserAsync(string identifier)
    {
        var byPhone = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == identifier);
        if (byPhone != null) return byPhone;

        var byUserName = await _userManager.FindByNameAsync(identifier);
        if (byUserName != null) return byUserName;

        return await _userManager.FindByEmailAsync(identifier);
    }

    private async Task LogAuthEventAsync(AuthEventType type, string? userId, string? attemptedIdentifier,
        bool succeeded, string ipAddress, string? detail = null)
    {
        _context.AuthAuditLogs.Add(new AuthAuditLog
        {
            EventType = type,
            UserId = userId,
            AttemptedIdentifier = attemptedIdentifier,
            Succeeded = succeeded,
            IpAddress = ipAddress,
            Detail = detail,
            OccurredAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    private async Task<Result<AuthResponse>> IssueTokensAsync(ApplicationUser user, string ipAddress)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty; // exactly one role per user (R-5)
        var isSuperAdmin = role == AppRoles.Admin;
        var permissions = isSuperAdmin
            ? new List<string>()
            : await _userPermissionService.GetUserPermissionsAsync(user.Id);

        var accessToken = _jwtService.GenerateToken(new TokenGenerationRequest(
            user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty,
            role, isSuperAdmin, permissions));

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)),
            FamilyId = Guid.NewGuid(),
            SecurityStampSnapshot = user.SecurityStamp ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            AbsoluteExpiresAt = DateTime.UtcNow.AddDays(_sessionOptions.Value.AbsoluteSessionDays)
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            userName = user.FullName,
            userMail = user.Email ?? string.Empty,
            accessToken = accessToken.Token,
            refreshToken = rawToken,
            role = role,
            permissions = permissions,
            accessTokenExpiresAt = accessToken.ExpiresAtUtc
        };

        return Result<AuthResponse>.Success(response, HttpStatusCode.OK);
    }

    public async Task<Result<AuthResponse>> LoginAsync(string identifier, string password, string ipAddress)
    {
        var user = await ResolveUserAsync(identifier);

        if (user == null)
        {
            await LogAuthEventAsync(AuthEventType.LoginFailed, null, identifier, false, ipAddress, "Unknown identifier");
            return Result<AuthResponse>.Failure("بيانات الدخول غير صحيحة", HttpStatusCode.Unauthorized);
        }

        if (user.IsDeleted)
        {
            // Same generic message as any other failure — do not reveal account state (H-3).
            await LogAuthEventAsync(AuthEventType.LoginFailed, user.Id, identifier, false, ipAddress, "Account deactivated");
            return Result<AuthResponse>.Failure("بيانات الدخول غير صحيحة", HttpStatusCode.Unauthorized);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            await LogAuthEventAsync(AuthEventType.AccountLockedOut, user.Id, identifier, false, ipAddress);
            return Result<AuthResponse>.Failure("تم قفل الحساب مؤقتًا بسبب محاولات دخول متكررة. حاول مرة أخرى لاحقًا", HttpStatusCode.Locked);
        }

        if (!signInResult.Succeeded)
        {
            await LogAuthEventAsync(AuthEventType.LoginFailed, user.Id, identifier, false, ipAddress, "Wrong password");
            return Result<AuthResponse>.Failure("بيانات الدخول غير صحيحة", HttpStatusCode.Unauthorized);
        }

        var result = await IssueTokensAsync(user, ipAddress);
        await LogAuthEventAsync(AuthEventType.LoginSucceeded, user.Id, identifier, true, ipAddress);
        return result;
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        var existing = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (existing == null)
            return Result<AuthResponse>.Failure("Invalid refresh token", HttpStatusCode.Unauthorized);

        // Reuse detection: presenting an already-revoked token means the token was
        // stolen and the legitimate flow already rotated past it — revoke the whole
        // family and treat this as a security event (S-2).
        if (existing.RevokedAt != null)
        {
            var family = await _context.RefreshTokens
                .Where(t => t.FamilyId == existing.FamilyId && t.RevokedAt == null)
                .ToListAsync();

            foreach (var t in family)
            {
                t.RevokedAt = DateTime.UtcNow;
                t.RevokedByIp = ipAddress;
                t.RevokedReason = Domain.Entities.Users.RevokedReason.ReuseDetected;
            }
            await _context.SaveChangesAsync();
            await LogAuthEventAsync(AuthEventType.RefreshTokenReuseDetected, existing.UserId, null, false, ipAddress);

            return Result<AuthResponse>.Failure("Refresh token is invalid or expired", HttpStatusCode.Unauthorized);
        }

        if (existing.IsExpired)
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired", HttpStatusCode.Unauthorized);

        var user = existing.User;
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid refresh token", HttpStatusCode.Unauthorized);

        if (user.IsDeleted)
        {
            existing.RevokedAt = DateTime.UtcNow;
            existing.RevokedByIp = ipAddress;
            existing.RevokedReason = Domain.Entities.Users.RevokedReason.UserDeactivated;
            await _context.SaveChangesAsync();
            return Result<AuthResponse>.Failure("تم ايقاف هذا الحساب من قبل الادمن", HttpStatusCode.Unauthorized);
        }

        if (existing.SecurityStampSnapshot != (user.SecurityStamp ?? string.Empty))
        {
            existing.RevokedAt = DateTime.UtcNow;
            existing.RevokedByIp = ipAddress;
            existing.RevokedReason = Domain.Entities.Users.RevokedReason.RoleChanged;
            await _context.SaveChangesAsync();
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired", HttpStatusCode.Unauthorized);
        }

        // Rotate: mint new tokens, revoke this one, keep the family.
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;
        var isSuperAdmin = role == AppRoles.Admin;
        var permissions = isSuperAdmin
            ? new List<string>()
            : await _userPermissionService.GetUserPermissionsAsync(user.Id);

        var accessToken = _jwtService.GenerateToken(new TokenGenerationRequest(
            user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty,
            role, isSuperAdmin, permissions));

        var rawNewToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(rawNewToken)),
            FamilyId = existing.FamilyId,
            SecurityStampSnapshot = user.SecurityStamp ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            AbsoluteExpiresAt = existing.AbsoluteExpiresAt // never extended (S-2/A-1)
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        existing.RevokedAt = DateTime.UtcNow;
        existing.RevokedByIp = ipAddress;
        existing.RevokedReason = Domain.Entities.Users.RevokedReason.Rotated;
        existing.ReplacedByTokenId = newRefreshToken.Id;
        await _context.SaveChangesAsync();

        var response = new AuthResponse
        {
            userName = user.FullName,
            userMail = user.Email ?? string.Empty,
            accessToken = accessToken.Token,
            refreshToken = rawNewToken,
            role = role,
            permissions = permissions,
            accessTokenExpiresAt = accessToken.ExpiresAtUtc
        };

        await LogAuthEventAsync(AuthEventType.TokenRefreshed, user.Id, null, true, ipAddress);
        return Result<AuthResponse>.Success(response, HttpStatusCode.OK);
    }

    public async Task<Result<string>> LogoutAsync(string refreshToken)
    {
        var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token == null)
            return Result<string>.Failure("Invalid refresh token", HttpStatusCode.BadRequest);

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = Domain.Entities.Users.RevokedReason.Logout;
        await _context.SaveChangesAsync();

        await LogAuthEventAsync(AuthEventType.LogoutSingle, token.UserId, null, true, "self");
        return Result<string>.Success("Logged out successfully", HttpStatusCode.OK);
    }

    public async Task<Result<string>> LogoutAllAsync(string userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync();

        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
            t.RevokedReason = Domain.Entities.Users.RevokedReason.LogoutAll;
        }
        await _context.SaveChangesAsync();

        await LogAuthEventAsync(AuthEventType.LogoutAll, userId, null, true, "self");
        return Result<string>.Success("Logged out from all devices", HttpStatusCode.OK);
    }
}