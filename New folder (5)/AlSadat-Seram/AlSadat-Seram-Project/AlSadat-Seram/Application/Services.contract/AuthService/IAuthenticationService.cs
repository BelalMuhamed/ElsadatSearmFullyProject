using Application.DTOs.Authentcation;
using Domain.Common;

namespace Application.Services.contract.AuthService;

public interface IAuthenticationService
{
    Task<Result<AuthResponse>> LoginAsync(string identifier, string password, string ipAddress);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, string ipAddress);
    Task<Result<string>> LogoutAsync(string refreshToken);
    /// <summary>Revokes every active refresh token for the given user. The caller
    /// (controller) is responsible for resolving which user — typically the
    /// authenticated caller's own id via ICurrentUserService.RequireUserId() —
    /// this method has no opinion on identity resolution.</summary>
    Task<Result<string>> LogoutAllAsync(string userId);
}