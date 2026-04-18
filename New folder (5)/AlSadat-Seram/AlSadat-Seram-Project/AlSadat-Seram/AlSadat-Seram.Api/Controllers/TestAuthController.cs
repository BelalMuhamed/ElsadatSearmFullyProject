using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AlSadat_Seram.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TestAuthController:ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new { Message = "This is public",Time = DateTime.UtcNow });
    }

    [HttpGet("private")]
    [Authorize]
    public IActionResult Private()
    {
        var user = HttpContext.User;
        return Ok(new
        {
            Message = "This is private",
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = user.FindFirstValue(ClaimTypes.Email),
            IsAuthenticated = user.Identity?.IsAuthenticated,
            Claims = user.Claims.Select(c => new { c.Type,c.Value }).ToList()
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new { Message = "Welcome Admin!" });
    }
}
