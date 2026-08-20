using Application.DTOs.Authorization;
using Domain.Common;

namespace Application.Services.contract.Authorization
{
    public interface IUserPermissionService
    {
        /// <summary>Qualified "Module.Action" strings for the user — consumed by JwtService when minting/refreshing tokens.</summary>
        Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default);

        /// <summary>Catalog + isGranted flags for the given user — powers the assignment checklist UI.</summary>
        Task<Result<UserPermissionsViewDto>> GetUserPermissionsViewAsync(string userId, CancellationToken ct = default);

        /// <summary>Full-replace grant. grantedByUserId is the acting Admin/HR user's id, for the audit column.</summary>
        Task<Result<string>> AssignPermissionsAsync(AssignUserPermissionsRequest request, string grantedByUserId, CancellationToken ct = default);
    }
}