using System.Security.Claims;

namespace Application.Services.contract.CurrentUserService;

/// <summary>
/// Resolves the authenticated user EXCLUSIVELY from the request's claims
/// principal (the JWT). Nothing in this codebase should identify "who is
/// acting" any other way — not a DTO field, not a display name matched
/// against the database, not localStorage. Services depend on this
/// interface directly; BaseApiController exposes a thin accessor over it
/// for controller-level concerns.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>ApplicationUser.Id resolved from the token's NameIdentifier /
    /// sub claim. Null when the request is unauthenticated.</summary>
    string? UserId { get; }

    ClaimsPrincipal? UserPrincipal { get; }

    /// <summary>True when the current request carries an authenticated principal.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns <see cref="UserId"/> or throws <see cref="System.InvalidOperationException"/>
    /// when it is null. For call sites where proceeding without an authenticated
    /// user would be a bug, not a business-rule failure to report politely.
    /// </summary>
    string RequireUserId();
}
