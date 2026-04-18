using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Authentcation
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(3, ErrorMessage = "Password must be at least 3 characters long")]
        public string password { get; set; } = string.Empty;
    }
}
