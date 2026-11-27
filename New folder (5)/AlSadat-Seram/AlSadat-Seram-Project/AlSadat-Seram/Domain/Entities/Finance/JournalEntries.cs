using Domain.Common;
using Domain.Entities.Commonitems;
using Domain.Enums;

namespace Domain.Entities.Finance
{
    /// <summary>
    /// A journal entry (قيد يومية). System entries are written only by
    /// <c>IJournalPostingService</c> (D35); once posted, the entry and its lines never change —
    /// a mistake is corrected by a reversal entry (D36, D37).
    /// </summary>
    [Audited(AuditAreas.Accounting, ReferenceProperty = nameof(EntryNumber))]
    public class JournalEntries : BaseEntity, IAuditedEntity
    {
        public const int EntryNumberMaxLength = 20;
        public const int DescriptionMaxLength = 500;

        public DateTime EntryDate { get; set; }
        public ReferenceType? referenceType { get; set; }
        public string Desc { get; set; }
        public string? ReferenceNo { get; set; }
        public bool? IsPosted { get; set; }
        public DateTime? PostedDate { get; set; }

        /// <summary>
        /// <c>JE-0001</c>, from <c>IDocumentNumberService</c>. Null only on entries written by the
        /// old services until their module moves them to the engine (D35).
        /// </summary>
        public string? EntryNumber { get; set; }

        /// <summary>The kind of document the entry comes from; null on old-service entries (D35).</summary>
        public JournalSourceType? SourceType { get; set; }

        /// <summary>The id of that document (invoice, voucher, payroll run…); null for a manual entry.</summary>
        public int? SourceId { get; set; }

        /// <summary>True when the system wrote it: it can't be edited, deleted or reversed by hand.</summary>
        public bool IsSystemGenerated { get; set; }

        /// <summary>The entry this one reverses; an entry is reversed at most once.</summary>
        public int? ReversalOfEntryId { get; set; }

        public virtual JournalEntries? ReversalOf { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual List<JournalEntryDetails> Details { get; set; } = new List<JournalEntryDetails>();
    }
}
