using Domain.Entities.Commonitems;
using Domain.Entities.Finance;
using Domain.Entities.Users;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Invoices
{
    public class SalesInvoices : BaseInvoice
    {
        public float? FirstDiscount { get; set; }
        public float? SecondDiscount { get; set; }
        public float? ThirdDiscount { get; set; }
        public int TotalPoints { get; set; }
        public SalesInvoiceStatus SalesInvoiceStatus { get; set; } = SalesInvoiceStatus.New;

        [ForeignKey(nameof(Distributor))]
        public string DistributorID { get; set; } = string.Empty;
        public virtual ApplicationUser? Distributor { get; set; }
        //------------- ICollection From SalesInvoiceItems ------------------------------
        public virtual ICollection<SalesInvoiceItems> SalesInvoiceItems { get; set; } = [];
        public string InvoiceNumber { get; set; } = string.Empty;
      
        [ForeignKey(nameof(journalEntry))]
        public int? JournalEntryId { get; set; } 
        public JournalEntries journalEntry { get; set; }

        [ForeignKey(nameof(ReversejournalEntry))]
        public int? ReverseJournalEntryId { get; set; }
        public JournalEntries ReversejournalEntry { get; set; }
        public decimal? TaxPrecentage { get; set; }
        public decimal? TaxValue { get; set; }
      


    }

}
