using Application.DTOs.Authentcation;
using Application.Services.contract.GoogleAuthService;
using Domain.Common;
using Domain.Entities.Users;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.GoogleAuthServices;
public class GoogleAuthService:IGoogleAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GoogleAuthService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<ApplicationUser>> GoogleSignInAsync(GoogleSignInVM model)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken);

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if(user == null)
            {
                user = new ApplicationUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    FullName = payload.GivenName ?? "",

                    
                };

                var result = await _userManager.CreateAsync(user);
                if(!result.Succeeded)
                    return Result<ApplicationUser>.Failure(string.Join(", ",result.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user,"User");
            }

            return Result<ApplicationUser>.Success(user);
        }
        catch(Exception ex)
        {
            return Result<ApplicationUser>.Failure("Invalid Google token: " + ex.Message);
        }
    }
}