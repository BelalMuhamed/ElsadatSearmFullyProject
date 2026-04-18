using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SupplierProducts:BaseEntity
    {
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public  Products Product { get; set; }
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }
        public  Supplier Supplier { get; set; }
    }
}
