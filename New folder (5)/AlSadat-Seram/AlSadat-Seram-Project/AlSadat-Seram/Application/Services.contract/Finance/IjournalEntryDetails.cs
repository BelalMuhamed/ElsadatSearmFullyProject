using Application.DTOs.FinanceDtos;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface IjournalEntryDetails
    {
        Task<Result<AccountDetailsDto>> GetAccountDetails(AccountDetailsDtoReq req);
    }
}
