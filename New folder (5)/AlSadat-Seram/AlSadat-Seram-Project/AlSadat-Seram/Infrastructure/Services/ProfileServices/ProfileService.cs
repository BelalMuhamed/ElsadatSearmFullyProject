using Application.DTOs.Profile;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.ProfileService;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.ProfileServices;
public class ProfileService(
    IMapper _mapper,
    UserManager<ApplicationUser> _userManager,
    ICurrentUserService _currentUserService
    ):IProfileService
{
    public async Task<Result<ProfileDto>> GetProfileAsync()
    {
        var userId = _currentUserService.UserId;
        if(userId is null)
            return Result<ProfileDto>.Failure("User not authenticated.",HttpStatusCode.Unauthorized);

        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
            return Result<ProfileDto>.Failure("User not found.",HttpStatusCode.NotFound);

        var profileDto = _mapper.Map<ProfileDto>(user);
        return Result<ProfileDto>.Success(profileDto);
    }

    public async Task<Result<bool>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if(userId is null)
            return Result<bool>.Failure("User not authenticated.",HttpStatusCode.Unauthorized);

        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
            return Result<bool>.Failure("User not found.",HttpStatusCode.NotFound);

        _mapper.Map(request,user);

        var result = await _userManager.UpdateAsync(user);
        if(!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(e => e.Description));
            return Result<bool>.Failure(errors,HttpStatusCode.BadRequest);
        }

        return Result<bool>.Success(true,"Profile updated successfully.");
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var userId = _currentUserService.UserId;
        if(userId is null)
            return Result<bool>.Failure("User not authenticated.",HttpStatusCode.Unauthorized);

        var user = await _userManager.FindByIdAsync(userId);
        if(user is null)
            return Result<bool>.Failure("User not found.",HttpStatusCode.NotFound);

        var result = await _userManager.ChangePasswordAsync(user,request.OldPassword,request.NewPassword);
        if(!result.Succeeded)
        {
            var errors = string.Join(", ",result.Errors.Select(e => e.Description));
            return Result<bool>.Failure(errors,HttpStatusCode.BadRequest);
        }

        return Result<bool>.Success(true,"Password changed successfully.");
    }
}
