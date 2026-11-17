using System;
using System.Collections.Generic;

namespace Application.DTOs.Authentcation
{
    public class RefreshTokenDto
    {
        public string token { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string userName { get; set; } = string.Empty;
        public string userMail { get; set; } = string.Empty;
        public string accessToken { get; set; } = string.Empty;
        public string refreshToken { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public List<string> permissions { get; set; } = new();
        public DateTime accessTokenExpiresAt { get; set; }
        /// <summary>True while the user must change the initial password; the frontend
        /// sends the user to the change-password screen and every other endpoint returns 403.</summary>
        public bool mustChangePassword { get; set; }
    }
}