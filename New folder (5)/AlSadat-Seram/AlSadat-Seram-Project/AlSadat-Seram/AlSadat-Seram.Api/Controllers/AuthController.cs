using Application.DTOs.Authentcation;
using Application.Services.contract.AuthService;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserSessionService _userSessionService;

        public AuthController(
            IAuthenticationService authenticationService,
            IUserSessionService userSessionService,
            ILocalizationService localization,
            ICurrentUserService currentUser)
            : base(localization, currentUser)
        {
            _authenticationService = authenticationService;
            _userSessionService = userSessionService;
        }

        /// <summary>
        /// Ends every active session of a user (all devices) and changes their security stamp.
        /// </summary>
        [Authorize(Policy = EmployeePermissions.RevokeSessions)]
        [HttpPost("revoke-sessions/{targetUserId}")]
        public async Task<IActionResult> RevokeSessions(string targetUserId, CancellationToken ct)
        {
            // Behind a proxy (Azure), RemoteIpAddress is the proxy's IP until
            // ForwardedHeaders is configured — tracked for the hosting step.
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _userSessionService.AdminRevokeSessionsAsync(
                targetUserId, RequireCurrentUserId(), ipAddress, ct);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authenticationService.LoginAsync(request.email, request.password, ipAddress);
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authenticationService.RefreshTokenAsync(request.token, ipAddress);
            return HandleResult(result);
        }

        [Authorize]
        [AlSadat_Seram.Api.Filters.AllowWhilePasswordChangeRequired]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto request)
        {
            var result = await _authenticationService.LogoutAsync(request.token);
            return HandleResult(result);
        }

        [Authorize]
        [AlSadat_Seram.Api.Filters.AllowWhilePasswordChangeRequired]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var result = await _authenticationService.LogoutAllAsync(RequireCurrentUserId());
            return HandleResult(result);
        }
    }
}