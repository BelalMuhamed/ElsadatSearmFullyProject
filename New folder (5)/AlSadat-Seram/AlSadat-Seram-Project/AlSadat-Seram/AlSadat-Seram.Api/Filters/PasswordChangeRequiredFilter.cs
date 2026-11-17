using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace AlSadat_Seram.Api.Filters;

/// <summary>
/// Marks an action that a user may still call while they must change their initial password
/// (change password, logout).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class AllowWhilePasswordChangeRequiredAttribute : Attribute
{
}

/// <summary>
/// Blocks every authenticated request whose token carries the <c>must_change_password</c>
/// claim with 403 <c>Auth.PasswordChangeRequired</c>, except actions marked with
/// <see cref="AllowWhilePasswordChangeRequiredAttribute"/> or <c>[AllowAnonymous]</c>.
/// </summary>
public sealed class PasswordChangeRequiredFilter(ILocalizationService localization) : IAuthorizationFilter
{
    /// <inheritdoc />
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true ||
            !user.HasClaim(AppClaimTypes.MustChangePassword, "true"))
            return;

        var metadata = context.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<AllowWhilePasswordChangeRequiredAttribute>().Any() ||
            metadata.OfType<IAllowAnonymous>().Any())
            return;

        var result = Result<object>.FailureKey("Auth.PasswordChangeRequired", HttpStatusCode.Forbidden);
        result.Message = localization.Resolve("Auth.PasswordChangeRequired");
        context.Result = new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden };
    }
}
