using Domain.Entities.Commonitems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Finance
{
    public class JournalEntryDetails:BaseEntity
    {
        [ForeignKey(nameof(JournalEntry))]

        public int JournalEntryId { get; set; }
        [ForeignKey(nameof(Account))]
        public int AccountId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        /// <summary>The line's own note (البيان); optional.</summary>
        public string? Description { get; set; }

        public const int DescriptionMaxLength = 250;
        public virtual ChartOfAccounts Account { get; set; }
        public virtual JournalEntries JournalEntry { get; set; }
    }
}
