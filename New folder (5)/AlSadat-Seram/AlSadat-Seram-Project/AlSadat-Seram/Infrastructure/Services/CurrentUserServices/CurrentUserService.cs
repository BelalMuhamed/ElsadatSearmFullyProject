using Application.Services.contract.CurrentUserService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.CurrentUserServices;
public class CurrentUserService:ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public string? UserId
    {
        get
        {
            try
            {
                // ✅ تحقق من وجود HttpContext
                var httpContext = _httpContextAccessor.HttpContext;
                if(httpContext == null)
                {
                    _logger.LogWarning("⚠️ HttpContext is NULL");
                    return null;
                }

                // ✅ تحقق من وجود User
                var user = httpContext.User;
                if(user == null)
                {
                    _logger.LogWarning("⚠️ User is NULL");
                    return null;
                }

                // ✅ تحقق من أن المستخدم معتمد
                if(user.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("⚠️ User is NOT authenticated");
                    return null;
                }

                _logger.LogInformation("✅ User is authenticated: {IsAuth}",
                    user.Identity.IsAuthenticated);

                // ✅ طريقة جديدة وأفضل لقراءة الـ Claims
                var claims = user.Claims.ToList();

                // ✅ اطبع جميع الـ Claims للـ Debug
                foreach(var claim in claims)
                {
                    _logger.LogDebug("🔍 Claim: {Type} = {Value}",claim.Type,claim.Value);
                }

                // ✅ ابحث عن UserId بطرق مختلفة (الأكثر شيوعاً أولاً)
                var userId = claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier)?.Value;

                if(string.IsNullOrEmpty(userId))
                {
                    userId = claims.FirstOrDefault(c =>
                        c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                }

                if(string.IsNullOrEmpty(userId))
                {
                    userId = claims.FirstOrDefault(c =>
                        c.Type == "UserId")?.Value;
                }

                if(string.IsNullOrEmpty(userId))
                {
                    userId = claims.FirstOrDefault(c =>
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                }

                if(!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("🎯 Found UserId: {UserId}",userId);
                }
                else
                {
                    _logger.LogWarning("❌ UserId NOT found in claims!");
                }

                return userId;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"💥 Error in CurrentUserService.UserId");
                return null;
            }
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            try
            {
                var isAuth = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
                _logger.LogInformation("🔐 IsAuthenticated: {IsAuth}",isAuth);
                return isAuth;
            }
            catch
            {
                return false;
            }
        }
        set { }
    }
    public ClaimsPrincipal? UserPrincipal
    {
        get
        {
            try
            {
                // أولاً: حاول من HttpContext
                var principal = _httpContextAccessor.HttpContext?.User;

                // ثانياً: إذا كان null، حاول من Thread.CurrentPrincipal
                if(principal == null)
                {
                    principal = Thread.CurrentPrincipal as ClaimsPrincipal;
                }

                // ثالثاً: إذا كان null، حاول من AsyncLocal أو CallContext
                if(principal == null)
                {
                    // قد تحتاج هذا في حالات Async متقدمة
                    // يمكنك استخدام AsyncLocal إذا كان لديك
                }

                return principal;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Error getting UserPrincipal");
                return null;
            }
        }
    }
}
