using Application.Services.contract.Finance;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using System.Net;
using System.Threading.Tasks;

namespace Infrastructure.Services.FinanceService
{
    public sealed class SystemAccountGuard : ISystemAccountGuard
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string ProtectedAccountMessage =
            "هذا الحساب من حسابات النظام ولا يمكن تعديله أو حذفه";

        public SystemAccountGuard(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<Result<bool>> EnsureCanModifyAsync(int accountId)
        {
            var account = await _unitOfWork
                .GetRepository<ChartOfAccounts, int>()
                .GetByIdAsync(accountId);

            if (account is null)
                return Result<bool>.Failure("الحساب غير موجود", HttpStatusCode.NotFound);

            if (account.IsSystemAccount)
                return Result<bool>.Failure(ProtectedAccountMessage, HttpStatusCode.Forbidden);

            return Result<bool>.Success(true);
        }

        public Result<bool> EnsureEditIsAllowed(
            ChartOfAccounts current, string newCode, int? newParentId, int newType)
        {
            if (!current.IsSystemAccount) return Result<bool>.Success(true);

            // System accounts allow ZERO structural change. Even cosmetic edits are blocked.
            return Result<bool>.Failure(ProtectedAccountMessage, HttpStatusCode.Forbidden);
        }

        public async Task<ChartOfAccounts> GetBySystemCodeAsync(SystemAccountCode code)
        {
            var account = await _unitOfWork
                .GetRepository<ChartOfAccounts, int>()
                .FindAsync(a => a.SystemCode == code);

            return account
                ?? throw new System.InvalidOperationException(
                    $"System account '{code}' is missing. Run DbInitializer or seed migration.");
        }
    }
}
