using Application.DTOs.FinanceDtos;
using Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface ITreeAccounts
    {
        Task<List<TreeAccountDto>> GetTree();

        Task<Result<List<AccountDto>>> GetAccounts(FilterationAccountsDto req);

        Task<Result<DisAndMerchAccountDto>> GetDisAndMerchAccountByUserId(string userId);

        /// <summary>
        /// Creates a new account. The account code is auto-generated from the parent's
        /// code (e.g. parent "1" → child "1.1", "1.2", ...). The client cannot supply
        /// or influence the code.
        /// </summary>
        Task<Result<string>> AddNewAccount(CreateAccountDto account);

        /// <summary>
        /// Edits an existing account. The account code is immutable post-creation
        /// and is ignored even if the client sends one.
        /// </summary>
        Task<Result<string>> EditAccount(AccountDto account);

        Task<Result<AccountDto>> GetByAccountId(int id);

        Task<Result<string>> DeleteAccount(int id);
    }
}