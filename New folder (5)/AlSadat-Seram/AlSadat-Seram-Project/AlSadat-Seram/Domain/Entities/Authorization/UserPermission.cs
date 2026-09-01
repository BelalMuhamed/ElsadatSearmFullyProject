using System;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Entities.Users;

namespace Domain.Entities.Authorization
{
    /// <summary>
    /// Direct grant: ApplicationUser -> Permission. No Role->Permission table (Decision 2/6).
    /// Composite PK (UserId, PermissionId) — configured in AppDbContext.OnModelCreating.
    /// </summary>
    public class UserPermission
    {
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(Permission))]
        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = null!;

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Audit trail — who granted this. Nullable because seed-time grants have no actor.
        /// NO ACTION on delete — deleting an admin must not delete the grants they issued.</summary>
        public string? GrantedByUserId { get; set; }
        public virtual ApplicationUser? GrantedByUser { get; set; }
    }
}