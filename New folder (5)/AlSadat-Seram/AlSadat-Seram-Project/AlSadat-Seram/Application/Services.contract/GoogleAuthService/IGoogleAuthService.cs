using Application.DTOs.Authentcation;
using Domain.Common;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.GoogleAuthService;
public interface IGoogleAuthService
{
    Task<Result<ApplicationUser>> GoogleSignInAsync(GoogleSignInVM model);
}

