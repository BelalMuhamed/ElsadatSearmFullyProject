using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Authorization
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        [ForeignKey(nameof(Module))]
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}