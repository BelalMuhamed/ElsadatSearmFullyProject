using Application.DTOs.Profile;
using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Endpoints for the currently-authenticated user to manage their own profile.
    /// <para>
    /// No role restriction — every authenticated user has a profile. There is
    /// intentionally no admin-edit-other-users path here; that belongs to a
    /// separate UserAdminController if/when needed.
    /// </para>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ProfileController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // -------------------------------------------------------------
        // 1) GET /api/Profile  — current user snapshot
        // -------------------------------------------------------------
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _serviceManager.ProfileService.GetProfileAsync();
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 2) PUT /api/Profile  — update phone / email / username
        // -------------------------------------------------------------
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var result = await _serviceManager.ProfileService.UpdateProfileAsync(request);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        // -------------------------------------------------------------
        // 3) PUT /api/Profile/change-password  — change current user's password
        // -------------------------------------------------------------
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _serviceManager.ProfileService.ChangePasswordAsync(request);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }
    }
}
