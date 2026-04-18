using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Stock
    {

        [ForeignKey(nameof(Store))]
        public int StoreId { get; set; }
        public Store Store { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Products Product { get; set; }

        public decimal Quantity { get; set; }

        [Column(TypeName = "money")]
        public decimal AvgCost { get; set; }
    }
}
