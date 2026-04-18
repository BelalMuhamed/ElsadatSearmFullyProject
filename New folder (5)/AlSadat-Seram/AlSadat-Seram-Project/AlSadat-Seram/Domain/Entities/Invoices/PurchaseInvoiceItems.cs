using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    public class PurchaseInvoiceItems : BaseEntity
    {
        public decimal Quantity { get; set; }
        [Column(TypeName = "money")]
        public decimal BuyingPricePerUnit { get; set; }

        //----------- Obj From Products and ForeignKey ProductId ---------------------------------
        [ForeignKey(nameof(Product))]
        public int? ProductId { get; set; }
        public virtual Products? Product { get; set; }
        //----------- Obj From PurchaseInvoice and ForeignKey PurchaseInvoiceId -------------------
        [ForeignKey(nameof(PurchaseInvoice))]
        public int? PurchaseInvoiceId { get; set; }
        public virtual PurchaseInvoice? PurchaseInvoice { get; set; }
        public decimal? PrecentageRival { get; set; }
        public decimal? RivalValue { get; set; }
        public decimal? TotalRivalValue { get; set; }
        public decimal? TotalGrowthAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
    }
}
