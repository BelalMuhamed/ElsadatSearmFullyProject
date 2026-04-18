using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Governrate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<City> Cities { get; set; } = new List<City>();
    }
}
