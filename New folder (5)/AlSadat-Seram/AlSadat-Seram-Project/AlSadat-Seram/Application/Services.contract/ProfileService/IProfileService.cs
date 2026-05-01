using Application.DTOs.Profile;
using Domain.Common;

namespace Application.Services.contract.ProfileService
{
    /// <summary>
    /// Application-layer contract for the current user's own profile.
    /// All methods operate on the authenticated user — there is no admin path
    /// here for editing other users.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>Returns the current user's profile snapshot.</summary>
        Task<Result<ProfileDto>> GetProfileAsync();

        /// <summary>
        /// Updates the current user's profile (phone, email, username).
        /// Returns flags describing which fields actually changed, so the
        /// frontend can decide whether to force re-login.
        /// </summary>
        Task<Result<UpdateProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request);

        /// <summary>Changes the current user's password (requires the old password).</summary>
        Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    }
}
