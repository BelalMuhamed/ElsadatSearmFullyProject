using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.FinanceDtos
{
    public class JournalEntriesDto
    {
        public int id { get; set; }
        public DateTime entryDate { get; set; } = DateTime.UtcNow;
        public int? referenceType { get; set; }
        public string desc { get; set; }
        public string? referenceNo { get; set; }
        public bool? isPosted { get; set; }
        public DateTime? postedDate { get; set; }
        public List<JournalEntryDetailsDto> entryDetails { get; set; } = null;
    }
    public class JournalEntryDetailsDto
    {
        public int? id { get; set; }
        public int accountId { get; set; }
        public string? accountCode { get; set; }
        public string? accountName { get; set; }
        public int? accountType { get; set; }
        public bool isLeaf { get; set; }
        public decimal debit { get; set; }
        public decimal credit { get; set; }
    }
    public class JournalEntryFilterationReq
    {
        public DateTime entryDate { get; set; }
        public int? referenceType { get; set; }
        public string? referenceNo { get; set; }
        public bool? isPosted { get; set; }
        public DateTime? postedDate { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
    }
}
