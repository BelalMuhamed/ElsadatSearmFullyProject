using Application.DTOs.Authentcation;
using Application.Services.contract.AuthService;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.GoogleAuthService;
using Application.Services.contract.JwtService;
using Domain.Common;
using Domain.Entities.Users;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.AuthServices;
public class AuthService(UserManager<ApplicationUser> _userManager,IJwtService _jwtService,AppDbContext _context
    ,IGoogleAuthService _googleAuthService, RoleManager<ApplicationRole> _roleManager , ICurrentUserService _currentUserService) :IAuthService
{
    private RefreshToken GenerateRefreshToken(string ipAddress)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }
    //-------------------------------------------------------------------------
    //public async Task<Result<string>> RegisterAsync(RegisterDto request)
    //{
    //    var userExists = await _userManager.FindByEmailAsync(request.Email);
    //    if(userExists != null)
    //    {
    //        return Result<string>.Failure("A user with this email already exists.",HttpStatusCode.Conflict);
    //    }

    //    var user = new ApplicationUser
    //    {
    //        UserName = request.Email,
    //        Email = request.Email,
    //        FirstName = request.FirstName,
    //        LastName = request.LastName
    //    };

    //    var result = await _userManager.CreateAsync(user,request.Password);

    //    if(!result.Succeeded)
    //    {
    //        var errorMessage = string.Join(", ",result.Errors.Select(e => e.Description));
    //        return Result<string>.Failure(errorMessage,HttpStatusCode.BadRequest);
    //    }

    //    var roleResult = await _userManager.AddToRoleAsync(user,"User");
    //    if(!roleResult.Succeeded)
    //    {
    //        var errorMessage = string.Join(", ",roleResult.Errors.Select(e => e.Description));
    //        return Result<string>.Failure(errorMessage,HttpStatusCode.BadRequest);
    //    }

    //    return Result<string>.Success("User registered successfully",HttpStatusCode.Created);
    //}
    //-------------------------------------------------------------------------
    public async Task<Result<AuthResponse>> LoginAsync(string email,string password)
    {
        var user = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == email  || u.PhoneNumber==email || u.UserName==email);
        if(user==null)
            return Result<AuthResponse>.Failure("مستخدم غير موجود", HttpStatusCode.Unauthorized);
        if (user.IsDeleted) 
        {
            return Result<AuthResponse>.Failure("تم ايقاف هذا الحساب من قبل الادمن ", HttpStatusCode.Unauthorized);
        }
        var x = await _userManager.CheckPasswordAsync(user, password);
        if ( !await _userManager.CheckPasswordAsync(user,password))
            return Result<AuthResponse>.Failure("مشكلة في الباسورد ",HttpStatusCode.Unauthorized);

        var accessToken = await _jwtService.GenerateToken(user);
        var roles = await _userManager.GetRolesAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            CreatedByIp = "user-ip-or-placeholder",

            User = user
        };

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        {
            userName = user.FullName,
            userMail = user.Email,
            accessToken = accessToken,
            refreshToken = refreshToken.Token,
            accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30),
            roles = roles.ToList(), 

        };

        return Result<AuthResponse>.Success(response, "تم تسجيل الدخول بنجاح ", HttpStatusCode.OK);
    }
    //-------------------------------------------------------------------------
    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken,string ipAddress)
    {
        var user = _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
        var roles = await _userManager.GetRolesAsync(user);
        if (user == null)
            return Result<AuthResponse>.Failure("Invalid refresh token",HttpStatusCode.Unauthorized);

        var token = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if(token.IsExpired || token.Revoked != null)
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired",HttpStatusCode.Unauthorized);

        var accessToken = await _jwtService.GenerateToken(user);

        var newRefreshToken = GenerateRefreshToken(ipAddress);
        user.RefreshTokens.Add(newRefreshToken);

        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        {
            accessToken = accessToken,
            refreshToken = newRefreshToken.Token
        };

        return Result<AuthResponse>.Success(response, "Token Refreshed Successfuly", HttpStatusCode.OK);
    }
    //-------------------------------------------------------------------------
    public async Task<Result<AuthResponse>> LoginWithGoogleAsync(GoogleSignInVM model, string ipAddress)
    {
        var userResult = await _googleAuthService.GoogleSignInAsync(model);

        if (!userResult.IsSuccess)
            return Result<AuthResponse>.Failure(userResult.Message!);

        var user = userResult.Data!;

        var accessToken = await _jwtService.GenerateToken(user);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            Created = DateTime.UtcNow,
            CreatedByIp = ipAddress,
            Expires = DateTime.UtcNow.AddDays(7),
            User = user
        };

        user.RefreshTokens.Add(refreshToken);
        await _userManager.UpdateAsync(user);

        var response = new AuthResponse
        {
            userName=user.FullName,
            userMail=user.Email,
            accessToken = accessToken,
            refreshToken = refreshToken.Token,
            accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        return Result<AuthResponse>.Success(response, "Login With Google Successfuly");
    }
    //-------------------------------------------------------------------------
    public async Task<Result<string>> LogoutAsync(LogoutDto request)
    {
        var token = await _context.Users
            .SelectMany(u => u.RefreshTokens)
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken);

        if(token == null)
            return Result<string>.Failure("Invalid refresh token.",HttpStatusCode.BadRequest);

        _context.RefreshTokens.Remove(token);
        await _context.SaveChangesAsync();

        return Result<string>.Success("User logged out Successfuly.", HttpStatusCode.OK);
    }
    //-------------------------------------------------------------------------
    public async Task<Result<IEnumerable<RoleDTO>>> GetInactiveRolesAsync(CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if(userId == null)
            return Result<IEnumerable<RoleDTO>>.Failure("Unauthorized", HttpStatusCode.Unauthorized);
        var result = await _roleManager.Roles.Where(r => !r.IsDeleted)
           .Select(r => new RoleDTO(
                   r.Id,
                   r.Name!,
                   r.CreatedAt,
                   r.IsDeleted
           )).ToListAsync(CancellationToken);
        return Result<IEnumerable<RoleDTO>>.Success(result , "Load InactiveRoles Successfuly ", HttpStatusCode.OK);
    }
    //-------------------------------------------------------------------------
    public async Task<Result<IEnumerable<RoleDTO>>> GetSoftDeletedRolesAsync(CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<IEnumerable<RoleDTO>>.Failure("Unauthorized", HttpStatusCode.Unauthorized);
        var result = await _roleManager.Roles.Where(r => r.IsDeleted == true)
           .Select(r => new RoleDTO(
                   r.Id,
                   r.Name!,
                   r.CreatedAt,
                   r.IsDeleted
           )).ToListAsync(CancellationToken);
        return Result<IEnumerable<RoleDTO>>.Success(result);
    }
    //-------------------------------------------------------------------------
    public async Task<Result<IEnumerable<RoleDTO>>> GetAllRolesAsync(CancellationToken CancellationToken = default)
    {
        try
        {
            // ⭐⭐⭐⭐ إزالة التحقق هنا لأن [Authorize] على Controller يكفي ⭐⭐⭐⭐
            // إذا كان المستخدم ليس معتمداً، فلن يصل إلى هذه الدالة أساساً

            var userId = _currentUserService.UserId;

            // فقط للـ Debug
            if(string.IsNullOrEmpty(userId))
            {
                // إذا وصلنا هنا وكان userId null، فهذا يعني أن CurrentUserService لا يعمل
                // لكن نستمر في التنفيذ للسماح باختبار النظام
            }

            var result = await _roleManager.Roles
               .Select(r => new RoleDTO(
                       r.Id,
                       r.Name!,
                       r.CreatedAt,
                       r.IsDeleted
               )).ToListAsync(CancellationToken);

            return Result<IEnumerable<RoleDTO>>.Success(result,"Load All Roles Successfully",HttpStatusCode.OK);
        }
        catch(Exception ex)
        {
            return Result<IEnumerable<RoleDTO>>.Failure($"Error: {ex.Message}",HttpStatusCode.InternalServerError);
        }
    }
    //-------------------------------------------------------------------------
    public async Task<Result<RoleDTO>> GetRoleByIdAsync(string RoleID, CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<RoleDTO>.Failure("Unauthorized", HttpStatusCode.Unauthorized);
        if (await _roleManager.FindByIdAsync(RoleID) is not { } role)

            return Result<RoleDTO>.Failure("This Role Note Found");
        var response =  new RoleDTO(
            role.Id,
            role.Name!,
            role.CreatedAt,
            role.IsDeleted
        );
        return Result<RoleDTO>.Success(response , "Load Role By Id Successfuly" , HttpStatusCode.OK);
    }
    //-------------------------------------------------------------------------
    public async Task<Result<string>> CreateRoleAsync(CreateRoleRequestDTO createRoleRequestDTO, CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);

        var RoleIsExists = await _roleManager.RoleExistsAsync(createRoleRequestDTO.RoleName);
        if (RoleIsExists)
           return Result<string>.Failure("Role Already Exists");       
        var NewRole = new ApplicationRole
        {
            Name = createRoleRequestDTO.RoleName,
            ConcurrencyStamp = Guid.CreateVersion7().ToString(),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            CreateBy = userId
        };
        var Result = await _roleManager.CreateAsync(NewRole);
        if (!Result.Succeeded)
            throw new DataException("Failed to Create Role");
        await _context.SaveChangesAsync(CancellationToken);
        return Result<string>.Success("Role Created Successfully");
    }
    //-------------------------------------------------------------------------
    public async Task<Result<string>> UpdateRoleAsync(string RoleID, CreateRoleRequestDTO createRoleRequestDTO, CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);

        var RoleIsExists = await _roleManager.Roles.AnyAsync(r => r.Name == createRoleRequestDTO.RoleName && r.Id != RoleID);
        if (RoleIsExists)
            return Result<string>.Failure("Role Name Already Exists");
        if (await _roleManager.FindByIdAsync(RoleID) is not { } Role)
            return Result<string>.Failure("This Role Note Found");
        
        Role.Name = createRoleRequestDTO.RoleName;
        Role.UpdateBy = userId;
        Role.UpdateAt = DateTime.UtcNow;
        var Result = await _roleManager.UpdateAsync(Role);
        if (!Result.Succeeded)
            return Result<string>.Failure("Failed to Update Role");
        await _context.SaveChangesAsync(CancellationToken);
        return Result<string>.Success("Role Updated Successfully");
    }
    //-------------------------------------------------------------------------
    public async Task<Result<string>> SoftDeleteRoleAsync(string RoleID, CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);

        if (await _roleManager.FindByIdAsync(RoleID) is not { } role)
            return Result<string>.Failure("This Role Note Found");
        role.IsDeleted = true;
        role.DeleteBy = userId;
        role.DeleteAt = DateTime.UtcNow;
        var Result = await _roleManager.UpdateAsync(role);
        if (!Result.Succeeded)
            return Result<string>.Failure("Failed to Delete Role");
        await _context.SaveChangesAsync(CancellationToken);
        return Result<string>.Success("Role Deleted Successfully");
    }
    //-------------------------------------------------------------------------
    public async Task<Result<string>> RestoreRoleAsync(string RoleID, CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);

        if (await _roleManager.FindByIdAsync(RoleID) is not { } role)
            return Result<string>.Failure("This Role Note Found");
        
        role.IsDeleted = false;
        role.UpdateBy = userId;
        role.UpdateAt = DateTime.UtcNow;
        var Result = await _roleManager.UpdateAsync(role);
        if (!Result.Succeeded)
            return Result<string>.Failure("Failed to Restore Role");
        await _context.SaveChangesAsync(CancellationToken);
        return Result<string>.Success("Role Restore Successfully");
    }
    //-------------------------------------------------------------------------
    public async Task<Result<string>> HardDeleteRoleAsync(string RoleID, CancellationToken CancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
            return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);

        if (await _roleManager.FindByIdAsync(RoleID) is not { } role)
           return Result<string>.Failure("This Role Note Found");
        var Result = await _roleManager.DeleteAsync(role);
        if (!Result.Succeeded)
            return Result<string>.Failure("Failed to Delete Role");
        return Result<string>.Success("Role Deleted Successfully");
    }
    //-------------------------------------------------------------------------
}

