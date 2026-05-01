using Application.DTOs.Profile;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.ProfileService;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.ProfileServices
{
    /// <summary>
    /// Profile service for the currently-authenticated user only.
    /// <para>
    /// Refactored in Turn 3:
    /// <list type="bullet">
    ///   <item>Replaced AutoMapper-based update with explicit per-field Identity APIs.</item>
    ///   <item>Added duplicate-username and duplicate-email guards.</item>
    ///   <item>Returns <see cref="UpdateProfileResponse"/> so the frontend can detect
    ///         a username change and force re-login (Turn 3 decision).</item>
    ///   <item>Email + Phone confirmation flags are intentionally NOT reset
    ///         (Turn 3 decision — keep them as-is).</item>
    /// </list>
    /// </para>
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public ProfileService(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        // =======================================================================
        // GET PROFILE
        // =======================================================================
        public async Task<Result<ProfileDto>> GetProfileAsync()
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<ProfileDto>.Failure(
                    "المستخدم غير مسجل الدخول", HttpStatusCode.Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<ProfileDto>.Failure(
                    "المستخدم غير موجود", HttpStatusCode.NotFound);

            var dto = _mapper.Map<ProfileDto>(user);
            // Map writes some fields conservatively; ensure Id is populated explicitly.
            dto.id = user.Id;
            dto.UserName = user.UserName ?? string.Empty;
            dto.Email = user.Email ?? string.Empty;
            dto.phoneNumber = user.PhoneNumber;

            return Result<ProfileDto>.Success(dto);
        }

        // =======================================================================
        // UPDATE PROFILE
        // =======================================================================
        public async Task<Result<UpdateProfileResponse>> UpdateProfileAsync(UpdateProfileRequest request)
        {
            // ---- 1) Auth + lookup
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<UpdateProfileResponse>.Failure(
                    "المستخدم غير مسجل الدخول", HttpStatusCode.Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<UpdateProfileResponse>.Failure(
                    "المستخدم غير موجود", HttpStatusCode.NotFound);

            // ---- 2) Normalise inputs
            var newUserName = (request.userName ?? string.Empty).Trim();
            var newEmail = (request.email ?? string.Empty).Trim();
            var newPhone = string.IsNullOrWhiteSpace(request.phoneNumber)
                ? null
                : request.phoneNumber.Trim();

            // ---- 3) Detect what actually changed (case-insensitive comparisons)
            var response = new UpdateProfileResponse
            {
                usernameChanged = !string.Equals(user.UserName, newUserName, StringComparison.OrdinalIgnoreCase),
                emailChanged    = !string.Equals(user.Email,    newEmail,    StringComparison.OrdinalIgnoreCase),
                phoneChanged    = !string.Equals(user.PhoneNumber ?? string.Empty,
                                                 newPhone ?? string.Empty,
                                                 StringComparison.Ordinal)
            };

            // No-op short-circuit — saves DB round-trips for "save" clicks on
            // an unchanged form. Returns success with all flags false.
            if (!response.usernameChanged && !response.emailChanged && !response.phoneChanged)
                return Result<UpdateProfileResponse>.Success(response, "لا توجد تغييرات لحفظها");

            // ---- 4) Duplicate guards — query OTHER users only (not self)
            if (response.usernameChanged)
            {
                var clash = await _userManager.Users.AnyAsync(u =>
                    u.Id != user.Id && u.UserName == newUserName);
                if (clash)
                    return Result<UpdateProfileResponse>.Failure(
                        "اسم المستخدم محجوز — استخدم اسمًا آخر", HttpStatusCode.Conflict);
            }

            if (response.emailChanged)
            {
                var clash = await _userManager.Users.AnyAsync(u =>
                    u.Id != user.Id && u.Email == newEmail);
                if (clash)
                    return Result<UpdateProfileResponse>.Failure(
                        "البريد الإلكتروني مستخدم بالفعل لحساب آخر", HttpStatusCode.Conflict);
            }

            // ---- 5) Apply the changes through the right Identity API per field

            // 5a) Username — use SetUserNameAsync so NormalizedUserName is updated atomically.
            if (response.usernameChanged)
            {
                var setUser = await _userManager.SetUserNameAsync(user, newUserName);
                if (!setUser.Succeeded)
                    return Result<UpdateProfileResponse>.Failure(
                        FirstError(setUser, "تعذر تحديث اسم المستخدم"),
                        HttpStatusCode.BadRequest);
            }

            // 5b) Email — Identity has SetEmailAsync but it always resets EmailConfirmed=false.
            //     Per Turn 3 decision we keep EmailConfirmed as-is, so we update the
            //     properties manually and let UpdateAsync persist with concurrency-stamp.
            if (response.emailChanged)
            {
                user.Email = newEmail;
                user.NormalizedEmail = _userManager.NormalizeEmail(newEmail);
                // Intentionally NOT touching user.EmailConfirmed — see Turn 3 decision.
            }

            // 5c) Phone — same reasoning as Email. SetPhoneNumberAsync would reset
            //     PhoneNumberConfirmed; we keep it as-is by direct assignment.
            if (response.phoneChanged)
            {
                user.PhoneNumber = newPhone;
                // Intentionally NOT touching user.PhoneNumberConfirmed.
            }

            // 5d) Audit
            user.UpdateAt = DateTime.UtcNow;
            user.UpdateBy = user.UserName;

            // ---- 6) Persist
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return Result<UpdateProfileResponse>.Failure(
                    FirstError(updateResult, "تعذر حفظ التغييرات"),
                    HttpStatusCode.BadRequest);

            // ---- 7) Done
            var message = response.usernameChanged
                ? "تم تحديث الملف الشخصي. سيتم تسجيل الخروج لإعادة الدخول باسم المستخدم الجديد."
                : "تم تحديث الملف الشخصي بنجاح";

            return Result<UpdateProfileResponse>.Success(response, message);
        }

        // =======================================================================
        // CHANGE PASSWORD
        // =======================================================================
        public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrWhiteSpace(userId))
                return Result<bool>.Failure(
                    "المستخدم غير مسجل الدخول", HttpStatusCode.Unauthorized);

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Result<bool>.Failure(
                    "المستخدم غير موجود", HttpStatusCode.NotFound);

            // ChangePasswordAsync internally verifies the OldPassword — no need to
            // pre-check it ourselves.
            var result = await _userManager.ChangePasswordAsync(
                user, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
                return Result<bool>.Failure(
                    FirstError(result, "تعذر تغيير كلمة المرور"),
                    HttpStatusCode.BadRequest);

            return Result<bool>.Success(true, "تم تغيير كلمة المرور بنجاح");
        }

        // =======================================================================
        // Helpers
        // =======================================================================

        /// <summary>
        /// Picks the first Identity error description, or returns the fallback.
        /// Identity errors are usually self-explanatory and already localized
        /// when the project's password policy / username policy is configured —
        /// we surface them as-is.
        /// </summary>
        private static string FirstError(IdentityResult result, string fallback)
        {
            var first = result.Errors?.FirstOrDefault()?.Description;
            return string.IsNullOrWhiteSpace(first) ? fallback : first;
        }
    }
}
