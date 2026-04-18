using Application.DTOs.Profile;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.ProfileService;
    public interface IProfileService
    {
        Task<Result<ProfileDto>> GetProfileAsync();
        Task<Result<bool>> UpdateProfileAsync(UpdateProfileRequest request);
        Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    }
