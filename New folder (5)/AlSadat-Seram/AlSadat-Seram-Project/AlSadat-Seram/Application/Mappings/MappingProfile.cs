using Application.DTOs.Notification;
using Application.DTOs.Profile;
using AutoMapper;
using Domain.Entities.Users;

namespace Application.Mappings
{
    /// <summary>
    /// AutoMapper profile.
    /// <para>
    /// IMPORTANT (Turn 3 change): the previous version had
    /// <c>CreateMap&lt;UpdateProfileRequest, ApplicationUser&gt;()</c> which let
    /// AutoMapper blindly copy properties from the DTO onto the entity. That was
    /// the root cause of a real data-loss bug: an empty-string default on
    /// FirstName / LastName / DefaultCurrency would overwrite the user's actual
    /// values whenever the DTO was deserialised without those fields.
    /// </para>
    /// <para>
    /// We deliberately keep ONLY the read mapping. The write path in
    /// <c>ProfileService.UpdateProfileAsync</c> now uses explicit per-field
    /// Identity APIs (SetUserNameAsync, SetPhoneNumberAsync, direct property
    /// assignment for Email + NormalizedEmail) — no AutoMapper magic on writes.
    /// </para>
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Profile (READ ONLY)
            CreateMap<ApplicationUser, ProfileDto>()
                // Map ApplicationUser.PhoneNumber to ProfileDto.phoneNumber (case + name)
                .ForMember(dst => dst.phoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                // FullName is not split into first/last on ApplicationUser — we mirror
                // the historical shape: leave FirstName/LastName empty for now and let
                // the controller fill what it can. Existing consumers stay unbroken.
                .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FullName ?? string.Empty))
                .ForMember(dst => dst.LastName,  opt => opt.MapFrom(src => string.Empty))
                .ForMember(dst => dst.DefaultCurrency, opt => opt.MapFrom(src => string.Empty));

            // INTENTIONALLY REMOVED:
            // CreateMap<UpdateProfileRequest, ApplicationUser>();
            // Reason: see class-level summary above.

            // Notification
            CreateMap<Notification, NotificationDto>();
        }
    }
}
