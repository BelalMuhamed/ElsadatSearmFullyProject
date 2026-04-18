using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    public class SalesInvoiceItemStoresQuantities:BaseEntity
    {
        [ForeignKey(nameof(Store))]
        public int StoreID { get; set; }
        public Store Store { get; set; }
        [ForeignKey(nameof(SalesInvoice))]
        public int InvoiceId { get; set; }
        public SalesInvoices SalesInvoice { get; set; }
        public int Quantity { get; set; }
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Products Product { get; set; }
    }
}
