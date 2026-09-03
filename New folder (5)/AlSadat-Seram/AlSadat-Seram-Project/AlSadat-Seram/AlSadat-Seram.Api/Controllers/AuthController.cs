using Application.DTOs.Authentcation;
using Application.Services.contract.AuthService;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.LocalizationService;
using Domain.Common;
using Infrastructure.Services.AuthServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseApiController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(
            IAuthenticationService authenticationService,
            ILocalizationService localization,
            ICurrentUserService currentUser)
            : base(localization, currentUser)
        {
            _authenticationService = authenticationService;
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
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto request)
        {
            var result = await _authenticationService.LogoutAsync(request.token);
            return HandleResult(result);
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var result = await _authenticationService.LogoutAllAsync(RequireCurrentUserId());
            return HandleResult(result);
        }
    }
}