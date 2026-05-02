using Application.DTOs.Authentcation;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public AuthController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        // ========================= LOGIN ==============================
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var result = await serviceManager.AuthService.LoginAsync(request.email,request.password);

            if(!result.IsSuccess)
                return Unauthorized(result);

            return Ok(result);

        }
        // ========================= REFRESH TOKEN ======================
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto request)
        {
            var result = await serviceManager.AuthService.RefreshTokenAsync(request.token, request.ipAddress);
            if (!result.IsSuccess) return Unauthorized(result);
            return Ok(result);
        }
        // ========================= LOGOUT =============================
        [AllowAnonymous]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request)
        {
            var result = await serviceManager.AuthService.LogoutAsync(request);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        // ========================= ROLES ==============================
        // -------- Get All Roles --------
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await serviceManager.AuthService.GetAllRolesAsync();

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        // -------- Get Inactive Roles --------
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("roles/inactive")]
        public async Task<IActionResult> GetInactiveRoles()
        {
            var result = await serviceManager.AuthService.GetInactiveRolesAsync();

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        // -------- Get Soft Deleted Roles --------
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("roles/deleted")]
        public async Task<IActionResult> GetSoftDeletedRoles()
        {
            var result = await serviceManager.AuthService.GetSoftDeletedRolesAsync();

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        // -------- Get Role By ID --------
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("roles/{id}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var result = await serviceManager.AuthService.GetRoleByIdAsync(id);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // -------- Create Role --------
        [Authorize(Roles = "Admin,HR")]
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDTO dto)
        {
            var result = await serviceManager.AuthService.CreateRoleAsync(dto);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // -------- Update Role --------
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("roles/{id}")]
        public async Task<IActionResult> UpdateRole(string id,[FromBody] CreateRoleRequestDTO dto)
        {
            var result = await serviceManager.AuthService.UpdateRoleAsync(id,dto);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // -------- Soft Delete Role --------
        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("roles/{id}")]
        public async Task<IActionResult> SoftDeleteRole(string id)
        {
            var result = await serviceManager.AuthService.SoftDeleteRoleAsync(id);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // -------- Restore Role --------
        [Authorize(Roles = "Admin,HR")]
        [HttpPut("roles/restore/{id}")]
        public async Task<IActionResult> RestoreRole(string id)
        {
            var result = await serviceManager.AuthService.RestoreRoleAsync(id);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // -------- Hard Delete Role --------
        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("roles/hard/{id}")]
        public async Task<IActionResult> DeleteRolePermanently(string id)
        {
            var result = await serviceManager.AuthService.HardDeleteRoleAsync(id);

            if(!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        // ==================== LOGIN WITH GOOGLE =====================
        //[Authorize(Roles = "HR")]
        //[HttpPost("login/google")]
        //public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleSignInVM model)
        //{
        //    var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        //    var result = await serviceManager.AuthService.LoginWithGoogleAsync(model,ip);

        //    if(!result.IsSuccess)
        //        return Unauthorized(result);

        //    return Ok(result);
        //}

    }
}
