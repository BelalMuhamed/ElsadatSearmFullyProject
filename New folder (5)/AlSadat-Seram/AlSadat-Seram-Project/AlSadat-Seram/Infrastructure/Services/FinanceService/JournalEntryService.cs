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
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.FinanceService
{
    public class JournalEntryService : IJounalEntryContract
    {
        private readonly IUnitOfWork unitOfWork;

        public JournalEntryService(IUnitOfWork _unitOfWork)
        {
            unitOfWork = _unitOfWork;
        }


      

        public async Task<ApiResponse<List<JournalEntriesDto>>> GetAll(JournalEntryFilterationReq req)
        {
           
                var repo = unitOfWork.GetRepository<JournalEntries, int>();
                var query = repo.GetQueryable();
                if (req.entryDate != default)
                    query = query.Where(x => x.EntryDate.Date == req.entryDate.Date);
                if (req.referenceType.HasValue)
                    query = query.Where(x => (int)x.referenceType == req.referenceType);
                if (!string.IsNullOrWhiteSpace(req.referenceNo))
                    query = query.Where(x => x.ReferenceNo.Contains(req.referenceNo));
                if (req.isPosted.HasValue)
                    query = query.Where(x => x.IsPosted == req.isPosted);
                if (req.postedDate.HasValue)
                    query = query.Where(x => x.PostedDate.Value.Date == req.postedDate.Value.Date);
                query = query.OrderByDescending(x => x.EntryDate);
                int totalCount = await query.CountAsync();
                int page = req.page <= 0 ? 1 : req.page;
                int pageSize = req.pageSize <= 0 ? 10 : req.pageSize;
                var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new JournalEntriesDto
                    {
                        id = x.Id,
                        entryDate = x.EntryDate,
                        referenceType = (int?)x.referenceType,
                        desc = x.Desc,
                        referenceNo = x.ReferenceNo,
                        isPosted = x.IsPosted,
                        postedDate = x.PostedDate

                    })
                    .ToListAsync();
                return new ApiResponse<List<JournalEntriesDto>>
                {
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    data = data
                };
           
          
        }
        public async Task<Result<string>> AddNewJournalEntry(JournalEntriesDto dto)
        {
            if (dto == null)
                return Result<string>.Failure("Invalid data");

            if (dto.entryDetails == null || !dto.entryDetails.Any())
                return Result<string>.Failure("لا يمكن إنشاء قيد بدون تفاصيل");

            if (dto.entryDetails.Any(d => d.debit < 0 || d.credit < 0))
                return Result<string>.Failure("لا يسمح بقيم سالبة");


            if (dto.entryDetails.Any(d => d.debit > 0 && d.credit > 0))
                return Result<string>.Failure("لا يمكن أن يحتوي السطر على debit و credit معاً");


            if (dto.entryDetails.Any(d => d.debit == 0 && d.credit == 0))
                return Result<string>.Failure("لا يمكن إضافة سطر بدون قيمة");

            var totalDebit = dto.entryDetails.Sum(x => x.debit);
            var totalCredit = dto.entryDetails.Sum(x => x.credit);

            if (totalDebit == 0 || totalCredit == 0)
                return Result<string>.Failure("لا يمكن أن يكون إجمالي القيد صفر");

            if (totalDebit != totalCredit)
                return Result<string>.Failure("القيد غير متوازن");

            var accountRepo = unitOfWork.GetRepository<ChartOfAccounts, int>();

            var accountIds = dto.entryDetails.Select(d => d.accountId).Distinct().ToList();

            var accounts = await accountRepo
                .GetQueryable()
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync();

            if (accounts.Count != accountIds.Count)
                return Result<string>.Failure("يوجد حساب غير موجود");

            if (accounts.Any(a => !a.IsLeaf))
                return Result<string>.Failure("لا يمكن إضافة حركة على حساب أب");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var entry = new JournalEntries
                {
                    EntryDate = DateTime.UtcNow,
                    referenceType = (ReferenceType?)dto.referenceType,
                    Desc = dto.desc,
                    ReferenceNo = dto.referenceNo,
                    IsPosted = false,
                    PostedDate = null
                };

                var repo = unitOfWork.GetRepository<JournalEntries, int>();
                await repo.AddWithoutSaveAsync(entry);
                await unitOfWork.SaveChangesAsync();

                var detailsRepo = unitOfWork.GetRepository<JournalEntryDetails, int>();

                var details = dto.entryDetails.Select(d => new JournalEntryDetails
                {
                    JournalEntryId = entry.Id,
                    AccountId = (int)d.accountId,
                    Debit = d.debit,
                    Credit = d.credit
                }).ToList();

                await detailsRepo.AddRangeAsyncWithoutSave(details);

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم إنشاء القيد بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);
                return Result<string>.Failure("حدث خطأ أثناء إنشاء القيد");
            }
        }

        public async Task<Result<string>> UpdateJournalEntry(JournalEntriesDto dto)
        {
            if (dto == null || dto.id == 0)
                return Result<string>.Failure("بيانات غير صحيحة");

            var repo = unitOfWork.GetRepository<JournalEntries, int>();

            var entry = await repo
                .GetQueryable()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == dto.id);

            if (entry == null)
                return Result<string>.Failure("القيد غير موجود");

            if (entry.IsPosted == true)
                return Result<string>.Failure("لا يمكن تعديل قيد مرحل");

            if (dto.entryDetails == null || !dto.entryDetails.Any())
                return Result<string>.Failure("لا يمكن حفظ قيد بدون تفاصيل");

            // ==== Validation نفس اللي عندك ====

            if (dto.entryDetails.Any(d => d.debit < 0 || d.credit < 0))
                return Result<string>.Failure("لا يسمح بقيم سالبة");

            if (dto.entryDetails.Any(d => d.debit > 0 && d.credit > 0))
                return Result<string>.Failure("لا يمكن أن يحتوي السطر على debit و credit معاً");

            if (dto.entryDetails.Any(d => d.debit == 0 && d.credit == 0))
                return Result<string>.Failure("لا يمكن إضافة سطر بدون قيمة");

            var totalDebit = dto.entryDetails.Sum(x => x.debit);
            var totalCredit = dto.entryDetails.Sum(x => x.credit);

            if (totalDebit == 0 || totalCredit == 0)
                return Result<string>.Failure("إجمالي القيد لا يمكن أن يكون صفر");

            if (totalDebit != totalCredit)
                return Result<string>.Failure("القيد غير متوازن");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                // ===== Update Header =====
                entry.EntryDate = dto.entryDate;
                entry.referenceType = (ReferenceType?)dto.referenceType;
                entry.Desc = dto.desc;
                entry.ReferenceNo = dto.referenceNo;

                var existingDetails = entry.Details.ToList();

                // ============================
                // 1️⃣ Update & Add
                // ============================
                foreach (var dtoDetail in dto.entryDetails)
                {
                    var existing = existingDetails
                        .FirstOrDefault(x => x.Id == dtoDetail.id && dtoDetail.id != 0);

                    if (existing != null)
                    {
                        // Update
                        existing.AccountId = (int)dtoDetail.accountId;
                        existing.Debit = dtoDetail.debit;
                        existing.Credit = dtoDetail.credit;
                    }
                    else
                    {
                        // Add new
                        entry.Details.Add(new JournalEntryDetails
                        {
                            AccountId = (int)dtoDetail.accountId,
                            Debit = dtoDetail.debit,
                            Credit = dtoDetail.credit
                        });
                    }
                }

                // ============================
                // 2️⃣ Delete removed rows
                // ============================
                var dtoIds = dto.entryDetails
                    .Where(x => x.id != 0)
                    .Select(x => x.id)
                    .ToList();

                var toRemove = existingDetails
                    .Where(x => !dtoIds.Contains(x.Id))
                    .ToList();

                foreach (var remove in toRemove)
                    entry.Details.Remove(remove);

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم تعديل القيد بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);
                return Result<string>.Failure("حدث خطأ أثناء التعديل");
            }
        }
        public async Task<Result<string>> PostEntry(int id)
        {
            var repo = unitOfWork.GetRepository<JournalEntries, int>();

            var entry = await repo
                .GetQueryable()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entry == null)
                return Result<string>.Failure("القيد غير موجود");

            if (entry.IsPosted == true)
                return Result<string>.Failure("القيد مرحل بالفعل");

            if (!entry.Details.Any())
                return Result<string>.Failure("لا يمكن ترحيل قيد بدون تفاصيل");

            var totalDebit = entry.Details.Sum(x => x.Debit);
            var totalCredit = entry.Details.Sum(x => x.Credit);

            if (totalDebit != totalCredit)
                return Result<string>.Failure("القيد غير متوازن");

            entry.IsPosted = true;
            entry.PostedDate = DateTime.Now;

            await unitOfWork.SaveChangesAsync();

            return Result<string>.Success("تم ترحيل القيد بنجاح");
        }

        public async Task<Result<string>> DeleteJournalEntry(int id)
        {
            var repo = unitOfWork.GetRepository<JournalEntries, int>();

            var entry = await repo
                .GetQueryable()
                .Include(x => x.Details)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entry == null)
                return Result<string>.Failure("القيد غير موجود");

            if (entry.IsPosted == true)
                return Result<string>.Failure("لا يمكن حذف قيد مرحل");

            await unitOfWork.BeginTransactionAsync();

            try
            {
                var detailsRepo = unitOfWork.GetRepository<JournalEntryDetails, int>();

                detailsRepo.DeleteRangeWithoutSaveAsync(entry.Details);

                repo.DeleteWithoutSaveAsync(entry);

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم حذف القيد بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);
                return Result<string>.Failure("حدث خطأ أثناء الحذف");
            }
        }

        public async Task<Result<JournalEntriesDto>> GetById(int id)
        {
            if (id <= 0)
                return Result<JournalEntriesDto>.Failure("رقم غير صالح");

            var repo = unitOfWork.GetRepository<JournalEntries, int>();

            var dto = await repo
                .GetQueryable()
                .Where(x => x.Id == id)
                .Select(entry => new JournalEntriesDto
                {
                    id = entry.Id,
                    entryDate = entry.EntryDate,
                    referenceType = (int?)entry.referenceType,
                    desc = entry.Desc,
                    referenceNo = entry.ReferenceNo,
                    isPosted = entry.IsPosted,
                    postedDate = entry.PostedDate,

                    entryDetails = entry.Details.Select(d => new JournalEntryDetailsDto
                    {
                        id = d.Id,
                        accountId = d.AccountId,
                        accountCode = d.Account.AccountCode,
                        accountName = d.Account.AccountName,
                        accountType = (int)d.Account.Type,
                        isLeaf = d.Account.IsLeaf,
                        debit = d.Debit,
                        credit = d.Credit
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (dto == null)
                return Result<JournalEntriesDto>.Failure("القيد غير موجود");

            return Result<JournalEntriesDto>.Success(dto);
        }
    }
}
