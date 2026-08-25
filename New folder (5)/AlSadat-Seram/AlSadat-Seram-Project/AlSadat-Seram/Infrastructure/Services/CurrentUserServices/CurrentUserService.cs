using Application.Services.contract.CurrentUserService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Infrastructure.Services.CurrentUserServices;

/// <summary>
/// Resolves the current user from <see cref="IHttpContextAccessor"/> only.
/// <para>
/// Resolution runs at most once per instance (this service is registered
/// per-request/scoped via the ServiceManager's Lazy&lt;T&gt; — see
/// ServiceManager.cs) and the result is cached in <see cref="_userId"/>,
/// so repeated reads within one request do not re-walk every claim or
/// re-log on every access.
/// </para>
/// <para>
/// Claim VALUES are never logged — only whether resolution succeeded —
/// because this is a hot path and claim values are exactly the kind of
/// data that should not end up in application logs.
/// </para>
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    private bool _resolved;
    private string? _userId;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? UserId
    {
        get
        {
            if (_resolved)
                return _userId;

            _resolved = true;

            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return _userId = null;

            var claims = user.Claims;

            _userId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                   ?? claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value
                   ?? claims.FirstOrDefault(c => c.Type == "UserId")?.Value
                   ?? claims.FirstOrDefault(c => c.Type ==
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (_userId is null)
                _logger.LogWarning("CurrentUserService: authenticated principal had no resolvable UserId claim.");

            return _userId;
        }
    }

    public bool IsAuthenticated
        => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal? UserPrincipal
        => _httpContextAccessor.HttpContext?.User;

    public string RequireUserId()
        => UserId ?? throw new InvalidOperationException(
            "CurrentUserService.RequireUserId() was called without an authenticated user. " +
            "This path must be reached only from an [Authorize]-protected action.");
}
