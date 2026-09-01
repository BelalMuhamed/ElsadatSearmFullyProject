using System.Collections.Generic;

namespace Domain.Entities.Authorization
{
    public class Module
    {
        public int Id { get; set; }

        /// <summary>Immutable identity, e.g. "HR", "Finance". Never rename after seeding.</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>Display name only — safe to rename; carries no meaning for tokens or policies.</summary>
        public string Name { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}