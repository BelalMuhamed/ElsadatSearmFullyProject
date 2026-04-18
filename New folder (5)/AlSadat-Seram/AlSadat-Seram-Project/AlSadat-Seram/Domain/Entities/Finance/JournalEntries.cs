using Domain.Entities.Commonitems;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Domain.Entities.Finance
{
    public class JournalEntries:BaseEntity
    {
        public DateTime EntryDate { get; set; }
        public ReferenceType? referenceType { get; set; }
        public string Desc { get; set; }
        public string? ReferenceNo { get; set; }
        public bool? IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }
        public virtual List<JournalEntryDetails> Details { get; set; }= new List<JournalEntryDetails>();
    }
}
