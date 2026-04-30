// Single source of truth for system-account protection. Used by EditAccount,
// DeleteAccount, and anywhere else that mutates ChartOfAccounts.

using Domain.Common;
using Domain.Entities.Finance;
using System.Threading.Tasks;

namespace Application.Services.contract.Finance
{
    public interface ISystemAccountGuard
    {
        /// <summary>
        /// Returns failure if the account is a system account (cannot be edited/deleted),
        /// or if the proposed change would orphan a system account's parent linkage.
        /// </summary>
        Task<Result<bool>> EnsureCanModifyAsync(int accountId);

        /// <summary>
        /// Validates that an edit operation is not changing protected fields
        /// (Code/Type/Parent/IsSystem) on a system account.
        /// </summary>
        Result<bool> EnsureEditIsAllowed(ChartOfAccounts current, string newCode, int? newParentId, int newType);

        /// <summary>
        /// Resolves an account by SystemAccountCode. Throws InvalidOperationException
        /// if not found — system accounts must always exist.
        /// </summary>
        Task<ChartOfAccounts> GetBySystemCodeAsync(Domain.Enums.SystemAccountCode code);
    }
}
