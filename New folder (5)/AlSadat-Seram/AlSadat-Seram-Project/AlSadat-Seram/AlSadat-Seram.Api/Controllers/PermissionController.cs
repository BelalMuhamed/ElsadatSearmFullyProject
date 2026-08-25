using Application.DTOs.Authorization;
using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : BaseApiController
    {
        private readonly IServiceManager _serviceManager;

        public PermissionController(
            IServiceManager serviceManager,
            ILocalizationService localization,
            ICurrentUserService currentUser)
            : base(localization, currentUser)
        {
            _serviceManager = serviceManager;
        }

        [Authorize(Policy = EmployeePermissions.AssignPermissions)]
        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog(CancellationToken ct)
        {
            var result = await _serviceManager.PermissionCatalogService.GetCatalogAsync(ct);
            return HandleResult(result);
        }

        [Authorize(Policy = EmployeePermissions.AssignPermissions)]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPermissions(string userId, CancellationToken ct)
        {
            var result = await _serviceManager.UserPermissionService.GetUserPermissionsViewAsync(userId, ct);
            return HandleResult(result);
        }

        [Authorize(Policy = EmployeePermissions.AssignPermissions)]
        [HttpPut("user/{userId}")]
        public async Task<IActionResult> AssignUserPermissions(string userId, [FromBody] AssignUserPermissionsRequest request, CancellationToken ct)
        {
            if (userId != request.userId)
                return HandleResult(Result<string>.FailureKey("Common.BadRequest"));

            var grantedByUserId = _serviceManager.CurrentUserService.UserId ?? string.Empty;
            var result = await _serviceManager.UserPermissionService.AssignPermissionsAsync(request, grantedByUserId, ct);
            return HandleResult(result);
        }
    }
}