using Application.DTOs.Authorization;
using Application.Services.contract.Authorization;
using Domain.Common;
using Domain.Entities.Authorization;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.Authorization
{
    public class PermissionCatalogService : IPermissionCatalogService
    {
        // NOTE: adjust "AppDbContext" to its actual namespace in your project
        // (referenced elsewhere in Infrastructure simply as AppDbContext).
        private readonly AppDbContext _context;

        public PermissionCatalogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PermissionCatalogDto>> GetCatalogAsync(CancellationToken ct = default)
        {
            var modules = await _context.Set<Module>()
                .Include(m => m.Permissions)
                .OrderBy(m => m.Name)
                .ToListAsync(ct);

            var dto = new PermissionCatalogDto
            {
                modules = modules.Select(m => new ModulePermissionsDto
                {
                    moduleId = m.Id,
                    moduleName = m.Name,
                    permissions = m.Permissions
                        .OrderBy(p => p.Name)
                        .Select(p => new PermissionDto
                        {
                            id = p.Id,
                            name = p.Name,
                            qualifiedName = $"{m.Name}.{p.Name}",
                            description = p.Description
                        }).ToList()
                }).ToList()
            };

            return Result<PermissionCatalogDto>.Success(dto, HttpStatusCode.OK);
        }
    }
}