namespace Application.DTOs.Profile
{
    /// <summary>
    /// Response payload for a successful profile update.
    /// <para>
    /// The <see cref="usernameChanged"/> flag tells the frontend whether the
    /// user's existing JWT now carries a stale username. When true, the
    /// frontend forces a re-login (decided in Turn 3 — simpler and more
    /// explicit than silent token refresh; keeps the auth surface narrow).
    /// </para>
    /// </summary>
    public class UpdateProfileResponse
    {
        /// <summary>
        /// True when the new userName differs from the old one. The frontend
        /// must log the user out and route to /login because the access token
        /// in localStorage was minted for the old username.
        /// </summary>
        public bool usernameChanged { get; set; }

        /// <summary>
        /// True when the new email differs from the old one. Informational —
        /// the frontend may want to refresh the displayed email in the topbar.
        /// EmailConfirmed is intentionally NOT reset (Turn 3 decision).
        /// </summary>
        public bool emailChanged { get; set; }

        /// <summary>
        /// True when the new phone number differs from the old one. Informational.
        /// </summary>
        public bool phoneChanged { get; set; }
    }
}
