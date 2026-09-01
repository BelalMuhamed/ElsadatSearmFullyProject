using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Users
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        {
            Id = Guid.CreateVersion7().ToString();
        }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreateBy { get; set; }
    }
}