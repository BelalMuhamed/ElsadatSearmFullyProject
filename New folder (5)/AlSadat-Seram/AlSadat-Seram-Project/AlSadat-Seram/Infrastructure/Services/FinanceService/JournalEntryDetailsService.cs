using AlSadatSeram.Services.contract;
using Application.DTOs.FinanceDtos;
using Application.Services.contract.Finance;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.FinanceService
{
    public class JournalEntryDetailsService : IjournalEntryDetails
    {
        private readonly IUnitOfWork unitOfWork;

        public JournalEntryDetailsService(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }
        public async Task<Result<AccountDetailsDto>> GetAccountDetails(AccountDetailsDtoReq req)
        {
            try
            {
                if (req.page <= 0) req.page = 1;
                if (req.pageSize <= 0) req.pageSize = 10;

                var account = await unitOfWork
                    .GetRepository<ChartOfAccounts, int>()
                    .FindAsync(a => a.Id == req.accountId);

                if (account == null)
                    return Result<AccountDetailsDto>.Failure("الحساب غير موجود");

                var baseQuery = unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable()
                    .Include(d => d.JournalEntry)
                    .Where(d => d.AccountId == req.accountId && d.JournalEntry.IsPosted == true )
                    .AsQueryable();

                if (req.entryId.HasValue)
                    baseQuery = baseQuery.Where(d => d.JournalEntryId == req.entryId.Value);

                if (req.entryDate.HasValue)
                    baseQuery = baseQuery.Where(d =>
                        d.JournalEntry.EntryDate.Date == req.entryDate.Value.Date);

                // إجمالي عدد الحركات
                var totalCount = await baseQuery.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / req.pageSize);

                // حساب الرصيد الافتتاحي (كل اللي قبل الصفحة الحالية)
                var skipCount = (req.page - 1) * req.pageSize;

                var openingBalance = await baseQuery
                    .OrderBy(d => d.JournalEntry.EntryDate)
                    .ThenBy(d => d.Id)
                    .Take(skipCount)
                    .SumAsync(x => (decimal?)x.Debit - x.Credit) ?? 0;

                // نجيب بيانات الصفحة الحالية (مرتبة تنازلي)
                var pageEntries = await baseQuery
                    .OrderByDescending(d => d.JournalEntry.EntryDate)
                    .ThenByDescending(d => d.Id)
                    .Skip(skipCount)
                    .Take(req.pageSize)
                    .ToListAsync();

                // علشان نحسب running صح لازم نحسبهم تصاعدي الأول
                var orderedForRunning = pageEntries
                    .OrderBy(d => d.JournalEntry.EntryDate)
                    .ThenBy(d => d.Id)
                    .ToList();

                decimal runningBalance = openingBalance;

                var calculatedMovements = orderedForRunning.Select(d =>
                {
                    runningBalance += (d.Debit - d.Credit);

                    return new AccountMovementDto
                    {
                        entryId = d.JournalEntryId,
                        entryDate = d.JournalEntry.EntryDate,
                        description = d.JournalEntry.Desc,
                        debit = d.Debit,
                        credit = d.Credit,
                        runningBalance = runningBalance
                    };
                }).ToList();

                // نرجعهم DESC زي ما طلبت
                var movementsDesc = calculatedMovements
                    .OrderByDescending(x => x.entryDate)
                    .ThenByDescending(x => x.entryId)
                    .ToList();

                var currentBalance = await baseQuery
                    .SumAsync(x => (decimal?)x.Debit - x.Credit) ?? 0;

                var response = new AccountDetailsDto
                {
                    accountId = account.Id,
                    accountName = account.AccountName,
                    accountCode = account.AccountCode,
                    type = (int)account.Type,
                    isActive = account.IsActive,
                    currentBalance = currentBalance,
                    movements = new ApiResponse<List<AccountMovementDto>>
                    {
                        totalCount = totalCount,
                        page = req.page,
                        pageSize = req.pageSize,
                        totalPages = totalPages,
                        data = movementsDesc
                    }
                };

                return Result<AccountDetailsDto>.Success(response);
            }
            catch
            {
                return Result<AccountDetailsDto>.Failure("خطأ أثناء الاتصال بقاعدة البيانات");
            }
        }
    }
}
