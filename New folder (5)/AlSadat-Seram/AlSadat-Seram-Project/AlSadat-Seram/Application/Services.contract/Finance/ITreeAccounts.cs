using Application.DTOs.FinanceDtos;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface ITreeAccounts
    {
        Task<List<TreeAccountDto>> GetTree();
        Task<Result<List<AccountDto>>> GetAccounts(FilterationAccountsDto req);
        Task<Result<DisAndMerchAccountDto>> GetDisAndMerchAccountByUserId(string userId);
        Task<Result<string>> AddNewAccount(AccountDto account);
        Task<Result<string>> EditAccount(AccountDto account);
        Task<Result<AccountDto>> GetByAccountId(int id);
        Task<Result<string>> DeleteAccount(int id);

    }
}
