using Domain.Common;

namespace Application.Services.contract.AuthService;

public interface IUserSessionService
{
    /// <summary>Revoke every active refresh-token session for a user and bump their
    /// security stamp, so any refresh attempt after this point fails on the
    /// SecurityStampSnapshot check regardless of which tokens were caught by the
    /// revoke query. Callable by super_admin (bypass) or a holder of
    /// EmployeePermissions.RevokeSessions — including against their own account.
    /// </summary>
    Task<Result<string>> AdminRevokeSessionsAsync(
        string targetUserId, string actorUserId, string ipAddress, CancellationToken ct = default);
}