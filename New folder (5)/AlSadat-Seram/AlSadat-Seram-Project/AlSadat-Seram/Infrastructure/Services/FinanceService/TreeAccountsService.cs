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

        public TreeAccountsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public async Task<List<TreeAccountDto>> GetTree()
        //{
        //    try
        //    {

        //        var accounts = await _unitOfWork.GetRepository<ChartOfAccounts, int>()
        //                        .GetAsync(a => a.Id!=null);


        //        var details = await _unitOfWork
        //      .GetRepository<JournalEntryDetails, int>()
        //      .GetQueryable()
        //      .Where(d => d.JournalEntry.IsPosted == true)
        //      .AsNoTracking()
        //      .ToListAsync();


        //        var tree = BuildTree(accounts, details, null);

        //        return tree;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _unitOfWork.LogError(ex);
        //       return null;
        //    }
        //}


        //private (decimal debit, decimal credit) GetTotals(ChartOfAccounts account,
        //                                                  IEnumerable<JournalEntryDetails> details,
        //                                                  IEnumerable<ChartOfAccounts> allAccounts)
        //{

        //    var ownDebit = details.Where(d => d.AccountId == account.Id).Sum(d => d.debit);
        //    var ownCredit = details.Where(d => d.AccountId == account.Id).Sum(d => d.credit);

        //    var children = allAccounts.Where(a => a.ParentAccountId == account.Id);

        //    foreach (var child in children)
        //    {
        //        var (childDebit, childCredit) = GetTotals(child, details, allAccounts);
        //        ownDebit += childDebit;
        //        ownCredit += childCredit;
        //    }

        //    return (ownDebit, ownCredit);
        //}

        //private List<TreeAccountDto> BuildTree(IEnumerable<ChartOfAccounts> accounts,
        //                                       IEnumerable<JournalEntryDetails> details,
        //                                       int? parentId)
        //{
        //    var children = accounts.Where(a => a.ParentAccountId == parentId).OrderBy(a => a.AccountCode);

        //    var tree = new List<TreeAccountDto>();

        //    foreach (var account in children)
        //    {
        //        var (debit, credit) = GetTotals(account, details, accounts);

        //        var dto = new TreeAccountDto
        //        {
        //            id = account.Id,
        //            accountName = account.AccountName,
        //            parentId=account.ParentAccountId,
        //            accountCode=account.AccountCode,
        //            debit = debit,
        //            isActive = account.IsActive,
        //            isLeaf=account.IsLeaf,
        //            credit = credit,
        //            children = BuildTree(accounts, details, account.Id)
        //        };

        //        tree.Add(dto);
        //    }

        //    return tree;
        //}

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
                var (debit, credit) =
                    GetTotals(account, totalsLookup, accounts);

                var dto = new TreeAccountDto
                {
                    id = account.Id,
                    accountName = account.AccountName,
                    parentId = account.ParentAccountId,
                    accountCode = account.AccountCode,
                    debit = debit,
                    credit = credit,
                    isActive = account.IsActive,
                    isLeaf = account.IsLeaf,
                    children = BuildTree(accounts, totalsLookup, account.Id)
                };

                tree.Add(dto);
            }

            return tree;
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

        
        public async Task<Result<string>> AddNewAccount(AccountDto accountDto)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();

                if (string.IsNullOrWhiteSpace(accountDto.accountCode))
                    return Result<string>.Failure("كود الحساب حقل ضروري");

                if (string.IsNullOrWhiteSpace(accountDto.accountName))
                    return Result<string>.Failure("اسم الحساب حقل ضروري");

                // ✅ مهم جداً: لازم يكون فيه أب
                if (!accountDto.parentAccountId.HasValue)
                    return Result<string>.Failure("لا يمكن إضافة حساب بدون حساب أب");

                var parent = await repo.GetByIdAsync(accountDto.parentAccountId.Value);

                if (parent == null)
                    return Result<string>.Failure("الحساب الأب غير موجود");

                if (parent.IsLeaf)
                    return Result<string>.Failure("لا يمكن إضافة حساب تحت حساب نهائي");

                if ((int)parent.Type != accountDto.type)
                    return Result<string>.Failure("نوع الحساب يجب أن يطابق نوع الأب");

                var codeExists = await repo.AnyAsync(x => x.AccountCode == accountDto.accountCode);
                if (codeExists)
                    return Result<string>.Failure("كود الحساب مكرر");

                var nameExists = await repo.AnyAsync(x => x.AccountName == accountDto.accountName);
                if (nameExists)
                    return Result<string>.Failure("اسم الحساب مكرر");

                var entity = new ChartOfAccounts
                {
                    AccountCode = accountDto.accountCode.Trim(),
                    AccountName = accountDto.accountName.Trim(),
                    UserId = accountDto.userId,
                    Type = parent.Type, // ✅ نأخذ النوع من الأب إجبارياً
                    ParentAccountId = parent.Id,
                    IsLeaf = accountDto.isLeaf,
                    IsActive = accountDto.isActive
                };

                await repo.AddAsync(entity);

                return Result<string>.Success("تم إنشاء الحساب الفرعي بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
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
       
        public async Task<Result<string>> EditAccount(AccountDto accountDto)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();
                var detailsRepo = _unitOfWork.GetRepository<JournalEntryDetails, int>();

                if (accountDto.id == null)
                    return Result<string>.Failure("المعرف غير مرسل");

                var account = await repo.GetByIdAsync((int)accountDto.id);

                if (account == null)
                    return Result<string>.Failure("الحساب غير موجود");

                if (string.IsNullOrWhiteSpace(accountDto.accountCode))
                    return Result<string>.Failure("كود الحساب حقل ضروري");

                if (string.IsNullOrWhiteSpace(accountDto.accountName))
                    return Result<string>.Failure("اسم الحساب حقل ضروري");

                // ❌ ممنوع إزالة الأب
                if (!accountDto.parentAccountId.HasValue)
                    return Result<string>.Failure("لا يمكن إزالة الحساب الأب");

                if (accountDto.parentAccountId == accountDto.id)
                    return Result<string>.Failure("لا يمكن ربط الحساب بنفسه");

                var parent = await repo.GetByIdAsync(accountDto.parentAccountId.Value);

                if (parent == null)
                    return Result<string>.Failure("الحساب الأب غير موجود");

                if (parent.IsLeaf)
                    return Result<string>.Failure("لا يمكن وضع الحساب تحت حساب نهائي");

                if ((int)parent.Type != (int)account.Type)
                    return Result<string>.Failure("يجب أن يكون نوع الحساب مثل نوع الأب");

                var hasTransactions = await detailsRepo.AnyAsync(x => x.AccountId == account.Id);

                if (hasTransactions && account.Type != parent.Type)
                    return Result<string>.Failure("لا يمكن تغيير نوع الحساب لوجود قيود عليه");

                var codeExists = await repo.AnyAsync(x =>
                    x.AccountCode == accountDto.accountCode &&
                    x.Id != account.Id);

                if (codeExists)
                    return Result<string>.Failure("كود الحساب مكرر");

                var allAccounts = await repo.GetAllAsync();
                if (IsDescendant(account.Id, parent.Id, allAccounts))
                    return Result<string>.Failure("لا يمكن نقل الحساب تحت أحد أبنائه");

                account.AccountCode = accountDto.accountCode.Trim();
                account.AccountName = accountDto.accountName.Trim();
                account.UserId = accountDto.userId;
                account.ParentAccountId = parent.Id;
                account.IsLeaf = accountDto.isLeaf;
                account.IsActive = accountDto.isActive;

                await repo.UpdateAsync(account);

                return Result<string>.Success("تم تعديل الحساب بنجاح");
            }
            catch (Exception ex)
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
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

        public async Task<Result<string>> DeleteAccount(int id)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<ChartOfAccounts, int>();

                if (id <= 0)
                    return Result<string>.Failure("المعرف غير صالح");

                 var dto = await repo
                     .GetQueryable()
                     .Where(x => x.Id == id)
                     
                     .FirstOrDefaultAsync();
                if (dto == null)
                    return Result<string>.Failure("الحساب غير موجود", HttpStatusCode.NotFound);
                
                var isParent =await repo.AnyAsync(a=>a.ParentAccountId == id);
                if(isParent)
                    return Result<string>.Failure("لا يمكن حذف حساب له أبناء ", HttpStatusCode.NotFound);
                var haveJournalEntries=await _unitOfWork.GetRepository<JournalEntryDetails, int>().AnyAsync(j=>j.AccountId==id);
                if (haveJournalEntries)
                    return Result<string>.Failure("لا يمكن حذف  حساب مربوط بقيود  ", HttpStatusCode.NotFound);

                repo.DeleteWithoutSaveAsync(dto);
                var res =await _unitOfWork.SaveChangesAsync();
                if(res<=0)
                    return Result<string>.Failure("حدث خطأ أثناء الحذف ", HttpStatusCode.NotFound);

                return Result<string>.Success("تم حذف الحساب بنجاح ");
            }
            catch (Exception ex) 
            {
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات");
            }
        }
    }
}
