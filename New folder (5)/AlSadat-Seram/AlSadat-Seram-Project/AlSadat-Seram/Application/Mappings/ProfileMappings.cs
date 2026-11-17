using Application.DTOs.Profile;
using Domain.Entities.Users;

namespace Application.Mappings
{
    /// <summary>
    /// Hand-written read mapping for the profile screen (replaces the AutoMapper
    /// profile, removed in F1-S1 / decision D19). Writes never go through a mapper:
    /// <c>ProfileService.UpdateProfileAsync</c> sets each field explicitly.
    /// </summary>
    public static class ProfileMappings
    {
        /// <summary>
        /// Builds the <see cref="ProfileDto"/> returned by <c>GET /api/Profile</c>.
        /// <see cref="ApplicationUser"/> has no first/last name or currency, so
        /// <c>FirstName</c> carries the full name and the other two stay empty —
        /// the same shape the API returned before.
        /// </summary>
        /// <param name="user">The signed-in user.</param>
        /// <returns>The profile snapshot.</returns>
        public static ProfileDto ToProfileDto(this ApplicationUser user) => new()
        {
            id = user.Id,
            FirstName = user.FullName ?? string.Empty,
            LastName = string.Empty,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            phoneNumber = user.PhoneNumber,
            DefaultCurrency = string.Empty
        };
    }
}
