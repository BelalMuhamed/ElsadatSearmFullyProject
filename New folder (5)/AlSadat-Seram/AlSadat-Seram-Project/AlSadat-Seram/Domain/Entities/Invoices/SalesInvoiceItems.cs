using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    public class SalesInvoiceItems : BaseEntity
    {
        [Column(TypeName = "money")]
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
     
        [ForeignKey(nameof(Product))]
        public int? ProductID { get; set; }
        public virtual Products? Product { get; set; }

        //----------- Obj From SalesInvoices and ForeignKey SalesInvoiceID -------------------
        [ForeignKey(nameof(SalesInvoices))]
        public int? SalesInvoiceID { get; set; }
        public virtual SalesInvoices? SalesInvoices { get; set; }
        public decimal? PrecentageRival { get; set; }
        public decimal? RivalValue { get; set; }
        public decimal? TotalRivalValue { get; set; }
        public decimal? TotalGrowthAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        
    }
}
