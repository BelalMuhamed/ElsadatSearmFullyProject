namespace Domain.Common;

/// <summary>
/// Central claim-name constants shared by JwtService (emitter) and
/// PermissionAuthorizationHandler (reader), so the two never drift
/// on a hand-typed magic string.
/// </summary>
public static class AppClaimTypes
{
    public const string Permission = "permission";
    public const string SuperAdmin = "super_admin";

    /// <summary>Present (value "true") while the user must change the initial password.
    /// Stored as a user claim until M1 adds a proper column.</summary>
    public const string MustChangePassword = "must_change_password";
}