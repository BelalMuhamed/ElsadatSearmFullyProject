using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class City : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        [ForeignKey(nameof(Governrate))]
        public int GovernrateId { get; set; }
        public virtual Governrate Governrate { get; set; }
        //------------- ICollection From User ------------------------------
        public virtual ICollection<ApplicationUser> Users { get; set; } = [];
        //------------- ICollection From SpecialRepresentiveCity ------------------------------
        public virtual ICollection<SpecialRepresentiveCity> SpecialRepresentiveCity { get; set; } = [];
        //------------- ICollection From Previews ------------------------------
        public virtual ICollection<Previews> Previews { get; set; } = [];
    }
}
