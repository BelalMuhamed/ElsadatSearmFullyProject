using AlSadatSeram.Services.contract;
using Application.DTOs.FinanceDtos;
using Application.Services.contract.Finance;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.FinanceService
{
    public class TreeAccountsService : ITreeAccounts
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISystemAccountGuard _systemGuard;

        public TreeAccountsService(IUnitOfWork unitOfWork, ISystemAccountGuard systemGuard)
        {
            _unitOfWork = unitOfWork;
            this._systemGuard = systemGuard;
        }

        public async Task<List<TreeAccountDto>> GetTree()
        {
            try
            {
                // 1️⃣ كل الحسابات
                var accounts = await _unitOfWork
                    .GetRepository<ChartOfAccounts, int>()
                    .GetAllAsync();

                // 2️⃣ تفاصيل القيود المرحّلة فقط
                var details = await _unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable()
                    .Where(d => d.JournalEntry.IsPosted == true)
                    .AsNoTracking()
                    .ToListAsync();

                // 3️⃣ Grouping مرة واحدة
                var totalsLookup = details
                    .GroupBy(d => d.AccountId)
                    .ToDictionary(
                        g => g.Key,
                        g => new AccountTotals
                        {
                            Debit = g.Sum(x => x.Debit),
                            Credit = g.Sum(x => x.Credit)
                        });

                // 4️⃣ بناء الشجرة
                var tree = BuildTree(accounts, totalsLookup, null);

                return tree;
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return null;
            }
        }
        private (decimal debit, decimal credit) GetTotals(
    ChartOfAccounts account,
    Dictionary<int, AccountTotals> totalsLookup,
    IEnumerable<ChartOfAccounts> allAccounts)
        {
            decimal ownDebit = 0;
            decimal ownCredit = 0;

            // ناخد إجمالي الحساب نفسه من الـ Dictionary
            if (totalsLookup.TryGetValue(account.Id, out var totals))
            {
                ownDebit = totals.Debit;
                ownCredit = totals.Credit;
            }

            // نجيب الأبناء
            var children = allAccounts.Where(a => a.ParentAccountId == account.Id);

            foreach (var child in children)
            {
                var (childDebit, childCredit) =
                    GetTotals(child, totalsLookup, allAccounts);

                ownDebit += childDebit;
                ownCredit += childCredit;
            }

            return (ownDebit, ownCredit);
        }

        public async Task<Result<List<AccountDto>>> GetAccounts(FilterationAccountsDto req)
        {
            try
            {
                var query = _unitOfWork
                    .GetRepository<ChartOfAccounts, int>()
                    .GetQueryable();

                if (!string.IsNullOrWhiteSpace(req.accountCode))
                    query = query.Where(x => x.AccountCode.Contains(req.accountCode));

                if (!string.IsNullOrWhiteSpace(req.accountName))
                    query = query.Where(x => x.AccountName.Contains(req.accountName));

                if (req.type.HasValue)
                    query = query.Where(x => (int)x.Type == req.type.Value);

                if (req.parentAccountId.HasValue)
                    query = query.Where(x => x.ParentAccountId == req.parentAccountId);

                if (req.isLeaf.HasValue)
                    query = query.Where(x => x.IsLeaf == req.isLeaf.Value);

                if (req.isActive.HasValue)
                    query = query.Where(x => x.IsActive == req.isActive.Value);

                if (req.page.HasValue && req.pageSize.HasValue)
                {
                    int skip = (req.page.Value - 1) * req.pageSize.Value;
                    query = query.Skip(skip).Take(req.pageSize.Value);
                }

                var accounts = await query
                    .OrderBy(x => x.AccountCode)
                    .Select(x => new AccountDto
                    {
                        id = x.Id,
                        accountCode = x.AccountCode,
                        userId = x.UserId,
                        accountName = x.AccountName,
                        type = (int)x.Type,
                        parentAccountId = x.ParentAccountId,
                        isLeaf = x.IsLeaf,
                        isActive = x.IsActive
                    })
                    .ToListAsync();

                return Result<List<AccountDto>>.Success(accounts);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<List<AccountDto>>.Failure(
                    "خطأ في الاتصال بقاعدة البيانات: " + ex.Message
                );
            }
        }

        public async Task<Result<DisAndMerchAccountDto>> GetDisAndMerchAccountByUserId(string userId)
        {
            try
            {
                var accountsRepo = _unitOfWork.GetRepository<ChartOfAccounts, int>();
                var detailsRepo = _unitOfWork.GetRepository<JournalEntryDetails, int>();

                // 1️⃣ Get leaf account by userId
                var account = await accountsRepo
                    .GetQueryable()
                    .Where(x => x.UserId == userId && x.IsLeaf && x.IsActive)
                    .FirstOrDefaultAsync();

                if (account == null)
                    return Result<DisAndMerchAccountDto>.Failure("لم يتم العثور على حساب مطابق");

                // 2️⃣ Get journal entry details for this account
                var details = await detailsRepo
                    .GetQueryable()
                    .Include(d=>d.JournalEntry).Where(d => d.AccountId == account.Id && d.JournalEntry.IsPosted==true)
                    .ToListAsync();

                // 3️⃣ Calculate totals
                var debit = details.Sum(d => d.Debit);
                var credit = details.Sum(d => d.Credit);

                // 4️⃣ Map to DTO
                var dto = new DisAndMerchAccountDto
                {
                    accountCode = account.AccountCode,
                    userId = account.UserId,
                    accountName = account.AccountName,
                    type = (int)account.Type,
                    parentAccountId = account.ParentAccountId,
                    isLeaf = account.IsLeaf,
                    isActive = account.IsActive,
                    debit = debit,
                    credit = credit
                };

                return Result<DisAndMerchAccountDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<DisAndMerchAccountDto>.Failure(
                    "خطأ في الاتصال بقاعدة البيانات: " + ex.Message
                );
            }
        }


        //public async Task<Result<string>> AddNewAccount(AccountDto accountDto)
        //{
        //    try
        //    {
        //        var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();

        //        bool hasExternalTransaction = _unitOfWork.IsTransactionActive();

        //        if (!hasExternalTransaction)
        //        {
        //            await _unitOfWork.BeginTransactionAsync();
        //        }

        //        if (string.IsNullOrWhiteSpace(accountDto.accountName))
        //            return Result<string>.Failure("اسم الحساب حقل ضروري");

        //        if (!accountDto.parentAccountId.HasValue)
        //            return Result<string>.Failure("لا يمكن إضافة حساب بدون حساب أب");

        //        var parent = await repo.GetByIdAsync(accountDto.parentAccountId.Value);

        //        if (parent == null)
        //            return Result<string>.Failure("الحساب الأب غير موجود");

        //        if (parent.IsLeaf)
        //            return Result<string>.Failure("لا يمكن إضافة حساب تحت حساب نهائي");

        //        if ((int)parent.Type != accountDto.type)
        //            return Result<string>.Failure("نوع الحساب يجب أن يطابق نوع الأب");

        //        var children = await repo.GetAsync(x => x.ParentAccountId == parent.Id);

        //        string newCode;

        //        if (!children.Any())
        //        {
        //            newCode = parent.AccountCode + ".1";
        //        }
        //        else
        //        {
        //            var lastCode = children
        //                .Select(x => x.AccountCode)
        //                .Select(code => code.Split('.').Last())
        //                .Where(x => int.TryParse(x, out _))
        //                .Select(int.Parse)
        //                .Max();

        //            newCode = $"{parent.AccountCode}.{lastCode + 1}";
        //        }

        //        var entity = new ChartOfAccounts
        //        {
        //            AccountCode = newCode,
        //            AccountName = accountDto.accountName.Trim(),
        //            UserId = accountDto.userId,
        //            Type = parent.Type,
        //            ParentAccountId = parent.Id,
        //            IsLeaf = accountDto.isLeaf,
        //            IsActive = accountDto.isActive
        //        };

        //        await repo.AddAsync(entity);

        //        await _unitOfWork.SaveChangesAsync();

        //        if (!hasExternalTransaction)
        //        {
        //            await _unitOfWork.CommitAsync();
        //        }

        //        return Result<string>.Success(newCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!_unitOfWork.IsTransactionActive())
        //        {
        //            await _unitOfWork.RollbackAsync();
        //        }

        //        await _unitOfWork.LogError(ex);
        //        return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
        //    }
        //}
        /// <summary>
        /// Creates a new account under the given parent. Code is server-generated:
        /// parent has no children -> "{parent.Code}.1", otherwise "{parent.Code}.{maxChildSuffix + 1}".
        /// Type is inherited from the parent — clients cannot pick a type.
        /// </summary>
        public async Task<Result<string>> AddNewAccount(CreateAccountDto accountDto)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();

                bool hasExternalTransaction = _unitOfWork.IsTransactionActive();
                if (!hasExternalTransaction)
                {
                    await _unitOfWork.BeginTransactionAsync();
                }

                // ---- 1) Validate input ----
                if (string.IsNullOrWhiteSpace(accountDto.accountName))
                    return Result<string>.Failure("اسم الحساب حقل ضروري");

                if (!accountDto.parentAccountId.HasValue)
                    return Result<string>.Failure("لا يمكن إضافة حساب بدون حساب أب");

                // ---- 2) Resolve parent ----
                var parent = await repo.GetByIdAsync(accountDto.parentAccountId.Value);
                if (parent == null)
                    return Result<string>.Failure("الحساب الأب غير موجود");

                if (parent.IsLeaf)
                    return Result<string>.Failure("لا يمكن إضافة حساب تحت حساب نهائي");

                // ---- 3) Auto-generate code from parent + existing siblings ----
                var newCode = await GenerateNextChildCodeAsync(repo, parent);

                // ---- 4) Build entity (Type ALWAYS inherited from parent, Code ALWAYS server-generated) ----
                var entity = new ChartOfAccounts
                {
                    AccountCode = newCode,
                    AccountName = accountDto.accountName.Trim(),
                    UserId = accountDto.userId,
                    Type = parent.Type,
                    ParentAccountId = parent.Id,
                    IsLeaf = accountDto.isLeaf,
                    IsActive = accountDto.isActive,
                    IsSystemAccount = false,   // user-created accounts are NEVER system accounts
                    SystemCode = null
                };

                await repo.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                if (!hasExternalTransaction)
                {
                    await _unitOfWork.CommitAsync();
                }

                return Result<string>.Success(newCode);
            }
            catch (Exception ex)
            {
                if (!_unitOfWork.IsTransactionActive())
                {
                    await _unitOfWork.RollbackAsync();
                }
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
        }

        /// <summary>
        /// Generates the next child code under a parent following the pattern:
        /// "{parent.AccountCode}.{N}" where N is 1 for the first child or
        /// (max numeric suffix among siblings) + 1 otherwise.
        /// </summary>
        private async Task<string> GenerateNextChildCodeAsync(
            Domain.Repositories.contract.IGenericRepository<ChartOfAccounts, int> repo,
            ChartOfAccounts parent)
        {
            var siblings = await repo.GetAsync(x => x.ParentAccountId == parent.Id);

            if (!siblings.Any())
                return $"{parent.AccountCode}.1";

            var maxSuffix = siblings
                .Select(s => s.AccountCode.Split('.').Last())
                .Where(suffix => int.TryParse(suffix, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(0)
                .Max();

            return $"{parent.AccountCode}.{maxSuffix + 1}";
        }


        private bool IsDescendant(int accountId, int potentialParentId, IEnumerable<ChartOfAccounts> accounts)
        {
            var children = accounts.Where(x => x.ParentAccountId == accountId);

            foreach (var child in children)
            {
                if (child.Id == potentialParentId)
                    return true;

                if (IsDescendant(child.Id, potentialParentId, accounts))
                    return true;
            }

            return false;
        }
       
       

        public async Task<Result<AccountDto>> GetByAccountId(int id)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();

                if (id <= 0)
                    return Result<AccountDto>.Failure("المعرف غير صالح");

                                var dto = await repo
                     .GetQueryable()
                     .Where(x => x.Id == id)
                     .Select(x => new AccountDto
                     {
                         id = x.Id,
                         accountCode = x.AccountCode,
                         userId = x.UserId,
                         accountName = x.AccountName,
                         type = (int)x.Type,
                         parentAccountId = x.ParentAccountId,
                         isLeaf = x.IsLeaf,
                         isActive = x.IsActive
                     })
                     .FirstOrDefaultAsync();

                if (dto == null)
                    return Result<AccountDto>.Failure("الحساب غير موجود", HttpStatusCode.NotFound);

               

                return Result<AccountDto>.Success(dto);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<AccountDto>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
        }
        public async Task<Result<string>> EditAccount(AccountDto accountDto)
        {
            try
            {
                if (accountDto.id is null or 0)
                    return Result<string>.Failure("المعرف غير مرسل");

                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();
                var detailsRepo = _unitOfWork.GetRepository<JournalEntryDetails, int>();

                var account = await repo.GetByIdAsync(accountDto.id.Value);
                if (account is null)
                    return Result<string>.Failure("الحساب غير موجود", HttpStatusCode.NotFound);

                // 🔒 SYSTEM ACCOUNT GUARD — blocks any edit on seeded accounts
                if (account.IsSystemAccount)
                    return Result<string>.Failure(
                        "هذا الحساب من حسابات النظام ولا يمكن تعديله",
                        HttpStatusCode.Forbidden);

                if (string.IsNullOrWhiteSpace(accountDto.accountName))
                    return Result<string>.Failure("اسم الحساب حقل ضروري");

                if (!accountDto.parentAccountId.HasValue)
                    return Result<string>.Failure("لا يمكن إزالة الحساب الأب");

                if (accountDto.parentAccountId == accountDto.id)
                    return Result<string>.Failure("لا يمكن ربط الحساب بنفسه");

                var parent = await repo.GetByIdAsync(accountDto.parentAccountId.Value);
                if (parent is null) return Result<string>.Failure("الحساب الأب غير موجود");
                if (parent.IsLeaf) return Result<string>.Failure("لا يمكن وضع الحساب تحت حساب نهائي");
                if ((int)parent.Type != (int)account.Type)
                    return Result<string>.Failure("يجب أن يكون نوع الحساب مثل نوع الأب");

                var hasTransactions = await detailsRepo.AnyAsync(x => x.AccountId == account.Id);
                if (hasTransactions && account.Type != parent.Type)
                    return Result<string>.Failure("لا يمكن تغيير نوع الحساب لوجود قيود عليه");

                // Apply edit
                if (!string.IsNullOrWhiteSpace(accountDto.accountCode))
                    account.AccountCode = accountDto.accountCode.Trim();

                var allAccounts = await repo.GetAllAsync();
                if (IsDescendant(account.Id, parent.Id, allAccounts))
                    return Result<string>.Failure("لا يمكن نقل الحساب تحت أحد أبنائه");

                // Apply edit
                if (!string.IsNullOrWhiteSpace(accountDto.accountCode))
                    account.AccountCode = accountDto.accountCode.Trim();

                account.AccountName = accountDto.accountName.Trim();
                account.UserId = accountDto.userId;
                account.ParentAccountId = parent.Id;
                account.IsLeaf = accountDto.isLeaf;
                account.IsActive = accountDto.isActive;
                // IsSystemAccount and SystemCode are NEVER changed via this endpoint.

                await repo.UpdateAsync(account);
                return Result<string>.Success("تم تعديل الحساب بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
        }

        // -------------------- DELETE --------------------
        public async Task<Result<string>> DeleteAccount(int id)
        {
            try
            {
                if (id <= 0) return Result<string>.Failure("المعرف غير صالح");

                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();
                var account = await repo.GetByIdAsync(id);
                if (account is null)
                    return Result<string>.Failure("الحساب غير موجود", HttpStatusCode.NotFound);

                // 🔒 SYSTEM ACCOUNT GUARD
                if (account.IsSystemAccount)
                    return Result<string>.Failure(
                        "هذا الحساب من حسابات النظام ولا يمكن حذفه",
                        HttpStatusCode.Forbidden);

                if (await repo.AnyAsync(a => a.ParentAccountId == id))
                    return Result<string>.Failure("لا يمكن حذف حساب له حسابات فرعية", HttpStatusCode.Conflict);

                if (await _unitOfWork.GetRepository<JournalEntryDetails, int>()
                                     .AnyAsync(j => j.AccountId == id))
                    return Result<string>.Failure("لا يمكن حذف حساب مرتبط بقيود", HttpStatusCode.Conflict);

                // If account is bound to a User (supplier/customer sub-account),
                // block deletion and tell the user to delete the supplier/customer instead.
                if (!string.IsNullOrWhiteSpace(account.UserId))
                    return Result<string>.Failure(
                        "هذا الحساب مرتبط بمستخدم (عميل/مورد). يجب حذف العميل أو المورد أولاً.",
                        HttpStatusCode.Conflict);

                repo.DeleteWithoutSaveAsync(account);
                var saved = await _unitOfWork.SaveChangesAsync();
                return saved > 0
                    ? Result<string>.Success("تم حذف الحساب بنجاح")
                    : Result<string>.Failure("حدث خطأ أثناء الحذف", HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
        }

        // -------------------- TREE BUILD (unchanged, but pass IsSystemAccount through) --------------------
        private List<TreeAccountDto> BuildTree(
            IEnumerable<ChartOfAccounts> accounts,
            Dictionary<int, AccountTotals> totalsLookup,
            int? parentId)
        {
            var children = accounts
                .Where(a => a.ParentAccountId == parentId)
                .OrderBy(a => a.AccountCode);

            var tree = new List<TreeAccountDto>();
            foreach (var account in children)
            {
                var (debit, credit) = GetTotals(account, totalsLookup, accounts);
                tree.Add(new TreeAccountDto
                {
                    id = account.Id,
                    accountName = account.AccountName,
                    parentId = account.ParentAccountId,
                    accountCode = account.AccountCode,
                    debit = debit,
                    credit = credit,
                    isActive = account.IsActive,
                    isLeaf = account.IsLeaf,
                    isSystemAccount = account.IsSystemAccount,    // NEW
                    children = BuildTree(accounts, totalsLookup, account.Id)
                });
            }
            return tree;
        }

        // ... GetTotals, GetTree, GetAccounts, AddNewAccount, IsDescendant remain unchanged.
        // Just remember to also map isSystemAccount in GetByAccountId / GetAccounts projections.
    }


}

