using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Transactions
{
    public class TransactionProducts
    {
        [ForeignKey(nameof(Transaction))]
        public int TransactionId { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        public StoresTransaction Transaction { get; set; }
        public Products Product { get; set; }

        public decimal Quantity { get; set; }

        [Column(TypeName = "money")]
        public decimal CostPerUnit { get; set; } 
    }
}
