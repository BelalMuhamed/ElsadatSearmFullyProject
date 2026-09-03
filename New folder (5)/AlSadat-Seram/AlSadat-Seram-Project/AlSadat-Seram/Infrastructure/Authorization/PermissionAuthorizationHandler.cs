using Domain.Common;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// Backs every [Authorize(Policy = ...)] permission check. Reads claims already
    /// on the validated JWT principal — no DB call here. super_admin bypasses
    /// everything; it's a claim, not a role-name check, so renaming the Admin role
    /// can never silently break this bypass.
    /// </summary>
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User.HasClaim(AppClaimTypes.SuperAdmin, "true"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User.HasClaim(AppClaimTypes.Permission, requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}