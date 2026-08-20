using Application.DTOs.Authorization;
using Application.Services.contract.Authorization;
using Domain.Common;
using Domain.Entities.Authorization;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.Authorization
{
    /// <summary>
    /// Talks to AppDbContext directly for the UserPermissions join table — same pattern
    /// AuthService already uses for RefreshTokens (simple join-row CRUD, no business rules
    /// that warrant routing through IUnitOfWork's generic repository).
    /// </summary>
    public class UserPermissionService : IUserPermissionService
    {
        private readonly AppDbContext _context;

        public UserPermissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken ct = default)
        {
            return await _context.Set<UserPermission>()
                .Where(up => up.UserId == userId)
                .Select(up => up.Permission.Module.Name + "." + up.Permission.Name)
                .ToListAsync(ct);
        }

        public async Task<Result<UserPermissionsViewDto>> GetUserPermissionsViewAsync(string userId, CancellationToken ct = default)
        {
            var granted = (await GetUserPermissionsAsync(userId, ct)).ToHashSet();

            var modules = await _context.Set<Module>()
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
                        qualifiedName = $"{m.Name}.{p.Name}",
                        description = p.Description,
                        isGranted = granted.Contains($"{m.Name}.{p.Name}")
                    }).ToList()
                }).ToList()
            };

            return Result<UserPermissionsViewDto>.Success(dto, HttpStatusCode.OK);
        }

        public async Task<Result<string>> AssignPermissionsAsync(AssignUserPermissionsRequest request, string grantedByUserId, CancellationToken ct = default)
        {
            var existing = _context.Set<UserPermission>().Where(up => up.UserId == request.userId);
            _context.Set<UserPermission>().RemoveRange(existing);

            var now = DateTime.UtcNow;
            var toGrant = request.permissionIds.Distinct().Select(pid => new UserPermission
            {
                UserId = request.userId,
                PermissionId = pid,
                GrantedAt = now,
                GrantedByUserId = grantedByUserId
            });

            await _context.Set<UserPermission>().AddRangeAsync(toGrant, ct);
            await _context.SaveChangesAsync(ct);

            return new Result<string>
            {
                IsSuccess = true,
                MessageKey = "Permission.AssignedSuccessfully",
                StatusCode = HttpStatusCode.OK,
                Data = request.userId
            };
        }
    }
}