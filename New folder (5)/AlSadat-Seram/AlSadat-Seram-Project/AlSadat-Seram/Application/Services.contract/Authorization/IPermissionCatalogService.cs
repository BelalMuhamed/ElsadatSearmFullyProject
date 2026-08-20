using Application.DTOs.Authorization;
using Domain.Common;

namespace Application.Services.contract.Authorization
{
    /// <summary>Read-only access to the fixed Module/Permission catalog (Decision 5).</summary>
    public interface IPermissionCatalogService
    {
        Task<Result<PermissionCatalogDto>> GetCatalogAsync(CancellationToken ct = default);
    }
}