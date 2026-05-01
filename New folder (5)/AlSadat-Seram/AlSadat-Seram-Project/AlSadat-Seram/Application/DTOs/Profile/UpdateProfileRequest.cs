using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Profile
{
    /// <summary>
    /// Profile update request.
    /// <para>
    /// REPLACED in Turn 3 — the previous shape allowed editing FirstName / LastName /
    /// DefaultCurrency / Address / CityID / Gender via blind AutoMapper copying. That
    /// approach had a real data-loss bug (empty strings overwrote stored values) and
    /// was not exposed in any UI. This shape is what the Profile UI page actually
    /// needs: the user's contact identity.
    /// </para>
    /// <para>
    /// Password is intentionally NOT here. Use <see cref="ChangePasswordRequest"/>
    /// against the dedicated endpoint — password changes have different security
    /// semantics and require the old password.
    /// </para>
    /// </summary>
    public class UpdateProfileRequest
    {
        /// <summary>
        /// Phone number. Optional in the entity (ApplicationUser.PhoneNumber is nullable),
        /// but if supplied here it must be non-empty after trim.
        /// Length matches the IdentityUser default column size.
        /// </summary>
        [MaxLength(50, ErrorMessage = "رقم الهاتف لا يمكن أن يتجاوز 50 حرف")]
        public string? phoneNumber { get; set; }

        /// <summary>
        /// Email. Required (login by email is supported app-wide).
        /// Validated as an email address; the service performs a duplicate-check.
        /// </summary>
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [MaxLength(256, ErrorMessage = "البريد الإلكتروني طويل جدًا")]
        public string email { get; set; } = string.Empty;

        /// <summary>
        /// Username. Required. Service enforces uniqueness.
        /// Min length 3 to match the conservative defaults; tighten in Identity options if needed.
        /// </summary>
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [MinLength(3, ErrorMessage = "اسم المستخدم يجب ألا يقل عن 3 أحرف")]
        [MaxLength(256, ErrorMessage = "اسم المستخدم طويل جدًا")]
        public string userName { get; set; } = string.Empty;
    }
}
