using Application.Services.contract;
using Application.Services.contract.CurrentUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AlSadat_Seram.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DebugAuthController:ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly ILogger<DebugAuthController> _logger;

    public DebugAuthController(
        IServiceManager serviceManager,
        ILogger<DebugAuthController> logger)
    {
        _serviceManager = serviceManager;
        _logger = logger;
    }

    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new
        {
            Message = "This is a public endpoint",
            Time = DateTime.UtcNow
        });
    }

    [HttpGet("protected")]
    [Authorize]
    public IActionResult Protected()
    {
        _logger.LogInformation("🛡️ Protected endpoint called");

        var user = HttpContext.User;
        var claims = user.Claims.Select(c => new { c.Type,c.Value }).ToList();

        var currentUserServiceResult = new
        {
            UserId = _serviceManager.CurrentUserService.UserId,
            IsAuthenticated = _serviceManager.CurrentUserService.IsAuthenticated
        };

        _logger.LogInformation("🔍 CurrentUserService Result: {@Result}",currentUserServiceResult);

        return Ok(new
        {
            Message = "This is a protected endpoint",
            HttpContextUser = new
            {
                IsAuthenticated = user.Identity?.IsAuthenticated,
                Name = user.Identity?.Name,
                AuthenticationType = user.Identity?.AuthenticationType
            },
            CurrentUserService = currentUserServiceResult,
            AllClaims = claims
        });
    }

    [HttpGet("roles-test")]
    [Authorize]
    public async Task<IActionResult> RolesTest()
    {
        _logger.LogInformation("🎯 Testing AuthService.GetAllRolesAsync");

        try
        {
            var result = await _serviceManager.AuthService.GetAllRolesAsync();

            _logger.LogInformation("✅ AuthService.GetAllRolesAsync result: {@Result}",
                new { result.IsSuccess,result.Message,DataCount = result.Data?.Count() });

            if(result.IsSuccess)
            {
                return Ok(new
                {
                    Message = "Successfully called GetAllRolesAsync",
                    Roles = result.Data,
                    CurrentUserServiceUserId = _serviceManager.CurrentUserService.UserId
                });
            }
            else
            {
                return StatusCode((int) result.StatusCode,new
                {
                    Error = result.Message,
                    CurrentUserServiceUserId = _serviceManager.CurrentUserService.UserId,
                    CurrentUserServiceIsAuthenticated = _serviceManager.CurrentUserService.IsAuthenticated
                });
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"💥 Error in RolesTest");
            return StatusCode(500,new
            {
                Error = ex.Message,
                CurrentUserServiceUserId = _serviceManager.CurrentUserService.UserId
            });
        }
    }

    [HttpGet("inspect-jwt")]
    [AllowAnonymous]
    public IActionResult InspectJwt()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();

        if(string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Ok(new
            {
                HasToken = false,
                Message = "No Bearer token provided"
            });
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var claims = jwtToken.Claims.Select(c => new
            {
                Type = c.Type,
                Value = c.Value,
                IsNameIdentifier = c.Type == ClaimTypes.NameIdentifier ||
                                  c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            }).ToList();

            return Ok(new
            {
                HasToken = true,
                TokenValid = true,
                HasNameIdentifier = claims.Any(c => c.IsNameIdentifier),
                NameIdentifierValue = claims.FirstOrDefault(c => c.IsNameIdentifier)?.Value,
                AllClaims = claims
            });
        }
        catch(Exception ex)
        {
            return Ok(new
            {
                HasToken = true,
                TokenValid = false,
                Error = ex.Message
            });
        }
    }
}
