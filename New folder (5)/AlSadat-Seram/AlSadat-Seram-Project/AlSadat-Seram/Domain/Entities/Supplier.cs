using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Supplier:BaseEntity
    {
        public string Name { get; set; }
        public bool  IsDeleted { get; set; }
        public string phoneNumbers { get; set; }
        public string? address { get; set; }
        [ForeignKey(nameof(city))]
        public int cityId { get; set; }
        public City city { get; set; }

        public ICollection<SupplierProducts> SupplierProducts { get; set; } = new List<SupplierProducts>();

    }
}
