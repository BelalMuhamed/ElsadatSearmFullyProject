using Domain.Entities.Commonitems;
using Domain.Entities.Finance;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    //فاتورة شراء 
    public class PurchaseInvoice : BaseInvoice
    {
        
        public string InvoiceNumber { get; set; } = string.Empty;
        [ForeignKey(nameof(supplier))]
        public int SupplierId { get; set; }
        public Supplier supplier { get; set; }
        public PurchaseInvoivceStoresStatus SettledStatus { get; set; }   //to do
        [ForeignKey(nameof(journalEntry))]
        public int? JournalEntryId { get; set; } //to do
        public JournalEntries journalEntry { get; set; }
        public decimal? PrecentageRival { get; set; }
        public decimal?  RivalValue { get; set; }
        public decimal? TotalRivalValue { get; set; }
        public decimal? TaxPrecentage { get; set; }
        public decimal? TaxValue { get; set; } //ضريبة

        [ForeignKey(nameof(Store))]
        public int? SetteledStoreId { get; set; }
        public Store? Store { get; set; }

        //------------- ICollection From PurchaseInvoiceItems ------------------------------
        public virtual ICollection<PurchaseInvoiceItems> PurchaseInvoiceItems { get; set; } = [];
    }
}
