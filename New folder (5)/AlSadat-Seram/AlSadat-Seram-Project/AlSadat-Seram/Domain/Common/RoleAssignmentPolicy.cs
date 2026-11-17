namespace Domain.Common;

/// <summary>
/// Who may give which role while roles still gate endpoints (until M1 replaces
/// roles with permission categories). Only a system administrator may create
/// another administrator or change an administrator's role.
/// </summary>
public static class RoleAssignmentPolicy
{
    /// <summary>
    /// True when the caller may give the requested roles: everyone except the
    /// <see cref="AppRoles.Admin"/> role, which only a super admin may give.
    /// </summary>
    public static bool CanAssign(IEnumerable<string?>? requestedRoles, bool callerIsSuperAdmin)
        => callerIsSuperAdmin
           || requestedRoles is null
           || !requestedRoles.Any(IsAdminRole);

    /// <summary>True when <paramref name="role"/> is the administrator role (case-insensitive).</summary>
    public static bool IsAdminRole(string? role)
        => string.Equals(role?.Trim(), AppRoles.Admin, StringComparison.OrdinalIgnoreCase);
}
