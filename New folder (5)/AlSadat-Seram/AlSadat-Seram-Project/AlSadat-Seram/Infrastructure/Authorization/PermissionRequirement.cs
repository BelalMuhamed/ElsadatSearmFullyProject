using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization
{
    /// <summary>
    /// A policy contract, not an implementation detail — belongs in Application.
    /// Only the handler that evaluates it (Infrastructure.Authorization.PermissionAuthorizationHandler)
    /// stays in Infrastructure.
    /// </summary>
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}