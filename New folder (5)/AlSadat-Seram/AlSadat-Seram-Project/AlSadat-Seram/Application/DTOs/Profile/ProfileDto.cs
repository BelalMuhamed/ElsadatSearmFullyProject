namespace Application.DTOs.Profile
{
    /// <summary>
    /// Read-only profile snapshot returned by GET /api/Profile.
    /// <para>
    /// Extended in Turn 3 to include <see cref="phoneNumber"/> and <see cref="id"/>
    /// — the Profile page needs these to pre-populate the edit form. The historical
    /// fields (FirstName, LastName, DefaultCurrency) remain on the DTO so the
    /// AutoMapper read mapping continues to work; they are display-only and not
    /// editable through this controller.
    /// </para>
    /// </summary>
    public class ProfileDto
    {
        /// <summary>The user's identifier — useful for change-detection on the client.</summary>
        public string id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? phoneNumber { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
    }
}
