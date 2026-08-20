using Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Backs every [Authorize(Policy = EmployeePermissions.X)] check.
    /// Reads the "permission" claims already on the validated JWT principal — no DB
    /// call here, because permissions are baked into the access token at login/refresh
    /// time (see JwtService). Admin bypasses everything (Decision 3).
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.IsInRole(AppRoles.Admin))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User.HasClaim("permission", requirement.Permission))
            {
                context.Succeed(requirement);
            }

            // No explicit Fail() — ASP.NET Core fails closed by default when no
            // handler succeeds, and an explicit Fail() would short-circuit any other
            // handler registered against the same policy in the future.
            return Task.CompletedTask;
        }
    }
}