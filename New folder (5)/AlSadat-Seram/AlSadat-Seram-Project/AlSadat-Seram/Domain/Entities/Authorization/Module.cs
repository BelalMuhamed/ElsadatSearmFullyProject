using System.Collections.Generic;
using System.Security;

namespace Domain.Entities.Authorization
{
    public class Module
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}