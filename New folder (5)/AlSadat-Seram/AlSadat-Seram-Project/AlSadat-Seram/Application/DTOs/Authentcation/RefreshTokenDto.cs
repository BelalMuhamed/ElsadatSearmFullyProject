using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authentcation
{
    public class RefreshTokenDto
    {
        public string token { get; set; } = string.Empty;
        public string ipAddress { get; set; }
    }
    public class AuthResponse
    {
        public string userName { get; set; }
        public string userMail { get; set; }
        public string accessToken { get; set; } = string.Empty;
        public string refreshToken { get; set; } = string.Empty;
        public List<string> roles { get; set; }
        public DateTime accessTokenExpiresAt { get; set; }
    }
}
