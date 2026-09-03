using Domain.Common;

namespace Application.Services.contract.AuthService;

public interface IUserSessionService
{
    /// <summary>Admin-only: revoke every active session for another user, e.g. on
    /// suspected compromise or immediately after deactivating them.</summary>
    Task<Result<string>> AdminRevokeSessionsAsync(string targetUserId);
}