using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Authorization
{
    public class Permission
    {
        public int Id { get; set; }

        /// <summary>Immutable identity, e.g. "HR.Payroll.Post". This — not Name/Module.Name — is what
        /// gets emitted as a JWT claim and referenced by [Authorize(Policy = ...)]. Never rename.</summary>
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Auto-granted to every new Employee at creation. Still an ordinary,
        /// individually revocable UserPermission row — this flag only drives the auto-grant.</summary>
        public bool IsBaseline { get; set; }

        [ForeignKey(nameof(Module))]
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; } = null!;

        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}