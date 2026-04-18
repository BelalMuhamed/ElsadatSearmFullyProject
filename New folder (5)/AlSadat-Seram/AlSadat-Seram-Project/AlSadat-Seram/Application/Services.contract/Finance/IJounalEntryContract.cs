using AlSadatSeram.Services.contract;
using Application.DTOs.FinanceDtos;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface IJounalEntryContract
    {
        Task<ApiResponse<List<JournalEntriesDto>>> GetAll(JournalEntryFilterationReq req);
        Task<Result<string>> AddNewJournalEntry(JournalEntriesDto journalEntry);
        Task<Result<string>> PostEntry(int id);
        Task<Result<string>> DeleteJournalEntry(int id);
        Task<Result<string>> UpdateJournalEntry(JournalEntriesDto journalEntry);
        Task<Result<JournalEntriesDto>>GetById(int id );
    }
}
