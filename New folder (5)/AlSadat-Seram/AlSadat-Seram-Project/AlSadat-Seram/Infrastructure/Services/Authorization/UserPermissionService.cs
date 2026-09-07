using Application.DTOs.Authorization;
using Application.Services.contract.Authorization;
using Application.Services.contract.CurrentUserService;
using Domain.Common;
using Domain.Entities.Auth;
using Domain.Entities.Authorization;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;

namespace Infrastructure.Services.Authorization
{
    /// <summary>
    /// Talks to AppDbContext directly for the UserPermissions join table — same pattern
    /// AuthService already uses for RefreshTokens (simple join-row CRUD, no business rules
    /// that warrant routing through IUnitOfWork's generic repository).
    /// </summary>
    public class UserPermissionService : IUserPermissionService
    {
        private const string NoPermissions = "(none)";

        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;

        public UserPermissionService(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser)
        {
            _context = context;
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
        {
            return await _context.Set<UserPermission>()
                .AsNoTracking()
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.Code)
                .ToListAsync(ct);
        }

        public async Task<Result<UserPermissionsViewDto>> GetUserPermissionsViewAsync(string userId, CancellationToken ct = default)
        {
            var granted = (await GetUserPermissionsAsync(userId, ct)).ToHashSet();

            var modules = await _context.Set<Module>()
                .AsNoTracking()
                .Include(m => m.Permissions)
                .OrderBy(m => m.Name)
                .ToListAsync(ct);

            var dto = new UserPermissionsViewDto
            {
                userId = userId,
                modules = modules.Select(m => new ModulePermissionsDto
                {
                    moduleId = m.Id,
                    moduleName = m.Name,
                    permissions = m.Permissions.OrderBy(p => p.Name).Select(p => new PermissionDto
                    {
                        id = p.Id,
                        name = p.Name,
                        qualifiedName = p.Code,
                        description = p.Description,
                        isGranted = granted.Contains(p.Code)
                    }).ToList()
                }).ToList()
            };

            return Result<UserPermissionsViewDto>.Success(dto, HttpStatusCode.OK);
        }

        public async Task<Result<string>> AssignPermissionsAsync(
            AssignUserPermissionsRequest request, string grantedByUserId, CancellationToken ct = default)
        {
            var target = await _userManager.FindByIdAsync(request.userId);
            if (target is null)
                return Result<string>.FailureKey("Auth.UserNotFound", HttpStatusCode.NotFound);

            // Grants are Employee-only by design.
            if (!await _userManager.IsInRoleAsync(target, AppRoles.Employee))
                return Result<string>.FailureKey("Permission.TargetMustBeEmployee", HttpStatusCode.BadRequest);

            var requestedIds = request.permissionIds.Distinct().ToList();

            var requested = await _context.Set<Permission>()
                .AsNoTracking()
                .Where(p => requestedIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Code })
                .ToListAsync(ct);

            if (requested.Count != requestedIds.Count)
                return Result<string>.FailureKey("Permission.UnknownPermissionId", HttpStatusCode.BadRequest);

            // AssignPermissions is never delegable — only the seeded Admin may grant it,
            // and Admin bypasses this surface entirely via super_admin.
            if (requested.Any(p => p.Code == EmployeePermissions.AssignPermissions))
                return Result<string>.FailureKey("Permission.NotDelegable", HttpStatusCode.Forbidden);

            // MUST be read before ExecuteDeleteAsync — that runs straight against the DB
            // and never touches the change tracker.
            var oldCodes = (await _context.Set<UserPermission>()
                    .AsNoTracking()
                    .Where(up => up.UserId == request.userId)
                    .Select(up => up.Permission.Code)
                    .ToListAsync(ct))
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToList();

            var newCodes = requested
                .Select(p => p.Code)
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToList();

            // No change: skip the audit row and don't force a needless re-login.
            if (oldCodes.SequenceEqual(newCodes, StringComparer.Ordinal))
                return Result<string>.SuccessKey("Permission.AssignedSuccessfully", request.userId, HttpStatusCode.OK);

            await using var tx = await _context.Database.BeginTransactionAsync(ct);

            await _context.Set<UserPermission>()
                .Where(up => up.UserId == request.userId)
                .ExecuteDeleteAsync(ct);

            var now = DateTime.UtcNow;

            _context.Set<UserPermission>().AddRange(requested.Select(p => new UserPermission
            {
                UserId = request.userId,
                PermissionId = p.Id,
                GrantedAt = now,
                GrantedByUserId = grantedByUserId
            }));

            _context.AuthAuditLogs.Add(new AuthAuditLog
            {
                EventType = AuthEventType.PermissionsChanged,
                UserId = request.userId,
                ActorUserId = grantedByUserId,
                Succeeded = true,
                IpAddress = _currentUser.IpAddress ?? "unknown",
                Detail = BuildPermissionChangeDetail(oldCodes, newCodes),
                OccurredAt = now
            });

            await _context.SaveChangesAsync(ct);

            // Invalidates every refresh token on the next rotation (SecurityStampSnapshot
            // mismatch), so a revoked permission can't outlive the current access token.
            await _userManager.UpdateSecurityStampAsync(target);

            await tx.CommitAsync(ct);

            return Result<string>.SuccessKey("Permission.AssignedSuccessfully", request.userId, HttpStatusCode.OK);
        }

        /// <summary>
        /// Renders a before/after snapshot of a user's grants for the audit log.
        /// Codes only — ids are meaningless when this row is read months later.
        /// </summary>
        private static string BuildPermissionChangeDetail(
            IReadOnlyCollection<string> oldCodes, IReadOnlyCollection<string> newCodes)
        {
            var sb = new StringBuilder();

            sb.Append("Old:\n");
            sb.Append(oldCodes.Count == 0 ? NoPermissions : string.Join("\n", oldCodes));
            sb.Append("\nNew:\n");
            sb.Append(newCodes.Count == 0 ? NoPermissions : string.Join("\n", newCodes));

            return sb.ToString();
        }
    }
}