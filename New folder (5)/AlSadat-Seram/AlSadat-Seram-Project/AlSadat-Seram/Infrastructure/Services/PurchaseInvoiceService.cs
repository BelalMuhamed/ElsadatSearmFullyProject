using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.ProductsDtos;
using Application.Helper;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Finance;
using Domain.Entities.Invoices;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceContract
    {
        private readonly IUnitOfWork unitOfWork;
        public PurchaseInvoiceService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        //public async Task<Result<string>> AddNewPurchaseInvoice(PurchaseInvoiceDtos dto)
        //{
        //    await unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        if (dto.supplierId == null)
        //            return Result<string>.Failure("المورد مطلوب", HttpStatusCode.BadRequest);

        //        bool exists = await unitOfWork.GetRepository<PurchaseInvoice, int>()
        //            .GetQueryable()
        //            .AnyAsync(s => s.InvoiceNumber == dto.invoiceNumber);

        //        if (exists)
        //            return Result<string>.Failure("رقم الفاتورة موجود بالفعل", HttpStatusCode.Conflict);

        //        if (dto.items == null || dto.items.Count <= 0)
        //            return Result<string>.Failure("يجب إضافة منتجات الفاتورة", HttpStatusCode.BadRequest);

        //        var AddedInvoive = new PurchaseInvoice()
        //        {
        //            InvoiceNumber = dto.invoiceNumber,
        //            SupplierId = (int)dto.supplierId,
        //            CreateAt = DateTime.Now,
        //            CreateBy = dto.createdBy,
        //            PurchaseInvoiceItems = new List<PurchaseInvoiceItems>()
        //        };

        //        #region Calculate Items (before invoice discount & tax)
        //        foreach (var item in dto.items)
        //        {
        //            if (item.quantity <= 0)
        //                return Result<string>.Failure("لا يمكن إضافة منتج بدون كمية", HttpStatusCode.BadRequest);

        //            if (item.buyingPricePerUnit < 0)
        //                return Result<string>.Failure("لا يمكن إضافة منتج سعره أقل من 0", HttpStatusCode.BadRequest);

        //            var addedItem = new PurchaseInvoiceItems()
        //            {
        //                Quantity = item.quantity,
        //                BuyingPricePerUnit = (decimal)item.buyingPricePerUnit,
        //                ProductId = item.productId
        //            };

        //            // Gross
        //            addedItem.TotalGrowthAmount = item.quantity * item.buyingPricePerUnit;

        //            // Item Discount
        //            if (item.precentageRival != null && item.precentageRival > 0)
        //            {
        //                addedItem.PrecentageRival = item.precentageRival;
        //                addedItem.TotalRivalValue = addedItem.TotalGrowthAmount * (item.precentageRival / 100);
        //            }
        //            else if (item.rivalValue != null && item.rivalValue > 0)
        //            {
        //                addedItem.RivalValue = item.rivalValue;
        //                addedItem.TotalRivalValue = item.rivalValue;
        //            }

        //            else
        //                addedItem.TotalRivalValue = 0;

        //            // Net after item discount (before invoice discount & tax)
        //            addedItem.TotalNetAmount =
        //                (decimal)addedItem.TotalGrowthAmount - (decimal)addedItem.TotalRivalValue;

        //            AddedInvoive.PurchaseInvoiceItems.Add(addedItem);
        //        }
        //        #endregion

        //        #region Invoice total before invoice discount
        //        AddedInvoive.TotalGrowthAmount =
        //            AddedInvoive.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);

        //        AddedInvoive.TotalNetAmount = AddedInvoive.TotalGrowthAmount;
        //        #endregion

        //        #region Invoice Discount
        //        if (dto.precentageRival != null && dto.precentageRival > 0)
        //        {
        //            AddedInvoive.PrecentageRival = dto.precentageRival;
        //            AddedInvoive.TotalRivalValue =
        //                AddedInvoive.TotalNetAmount * (dto.precentageRival / 100);
        //            AddedInvoive.TotalNetAmount -= AddedInvoive.TotalRivalValue;
        //        }
        //        else if (dto.rivalValue != null && dto.rivalValue > 0)
        //        {
        //            AddedInvoive.TotalRivalValue = dto.rivalValue;
        //            AddedInvoive.TotalNetAmount -= AddedInvoive.TotalRivalValue;
        //        }
        //        #endregion

        //        #region Distribute Invoice Discount across items
        //        var itemsTotalBeforeInvoiceDiscount =
        //            AddedInvoive.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);

        //        var invoiceNetAfterInvoiceDiscount = AddedInvoive.TotalNetAmount;

        //        var factor = invoiceNetAfterInvoiceDiscount / itemsTotalBeforeInvoiceDiscount;

        //        foreach (var item in AddedInvoive.PurchaseInvoiceItems)
        //        {
        //            item.TotalNetAmount = (decimal)item.TotalNetAmount * (decimal)factor;
        //        }
        //        #endregion

        //        #region Tax
        //        if (dto.taxPrecentage != null && dto.taxPrecentage > 0)
        //        {
        //            AddedInvoive.TaxPrecentage = dto.taxPrecentage;
        //            AddedInvoive.TaxValue =
        //                AddedInvoive.TotalNetAmount * (dto.taxPrecentage / 100);

        //            AddedInvoive.TotalNetAmount += AddedInvoive.TaxValue;
        //        }
        //        #endregion

        //        #region Distribute Tax across items
        //        var totalAfterDiscounts =
        //            AddedInvoive.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);
        //        var taxValue = AddedInvoive.TaxValue ?? 0m;

        //        foreach (var item in AddedInvoive.PurchaseInvoiceItems)
        //        {
        //            var ratio = item.TotalNetAmount / totalAfterDiscounts;
        //            var itemTax = taxValue * ratio;
        //            item.TotalNetAmount += itemTax;
        //        }

        //        #endregion

        //        #region Check total with front
        //        var backendTotal = Math.Round((decimal)AddedInvoive.TotalNetAmount, 2);
        //        var frontendTotal = Math.Round((decimal)dto.totalNetAmount, 2);

        //        if (backendTotal != frontendTotal)
        //            return Result<string>.Failure("تم إرسال بيانات خاطئة في الفاتورة", HttpStatusCode.BadRequest);


        //        #endregion

        //        await unitOfWork.GetRepository<PurchaseInvoice, int>().AddAsync(AddedInvoive);

        //        #region Journal Entry
        //        var AddedJournalEntry = new JournalEntries()
        //        {
        //            EntryDate = DateTime.Now,
        //            Desc = $"{dto.supplierName} فاتورة شراء {dto.invoiceNumber}",
        //            ReferenceNo = dto.invoiceNumber,
        //            referenceType = Domain.Enums.ReferenceType.PurchaseInvoice
        //        };
        //        await unitOfWork.GetRepository<JournalEntries, int>().AddAsync(AddedJournalEntry);

        //        AddedInvoive.JournalEntryId = AddedJournalEntry.Id;
        //        #endregion

        //        #region Accounting
        //        var supplierAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
        //            .FindAsync(a => a.UserId == dto.supplierId.ToString());
        //        if(supplierAccount==null) return Result<string>.Failure(" هذا المورد لا يوجد له حساب في شجرة الحسابات ", HttpStatusCode.BadRequest);
        //        var stockAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
        //            .FindAsync(a => a.AccountCode == "1012");
        //        if (stockAccount == null) return Result<string>.Failure(" حساب 1012 الخاص بمنتجات المخازن غير موجود ", HttpStatusCode.BadRequest);
        //        await unitOfWork.GetRepository<JournalEntryDetails, int>().AddWithoutSaveAsync(
        //            new JournalEntryDetails
        //            {
        //                AccountId = supplierAccount.Id,
        //                JournalEntryId = AddedJournalEntry.Id,
        //                Credit = (decimal)AddedInvoive.TotalNetAmount,
        //                Debit = 0
        //            });

        //        await unitOfWork.GetRepository<JournalEntryDetails, int>().AddWithoutSaveAsync(
        //            new JournalEntryDetails
        //            {
        //                AccountId = stockAccount.Id,
        //                JournalEntryId = AddedJournalEntry.Id,
        //                Debit = (decimal)AddedInvoive.TotalNetAmount,
        //                Credit = 0
        //            });
        //        #endregion

        //        #region Stock
        //        if (dto.settledStoreId != null)
        //        {
        //            AddedInvoive.SetteledStoreId = dto.settledStoreId;
        //            AddedInvoive.SettledStatus = Domain.Enums.PurchaseInvoivceStoresStatus.Settled;
        //             unitOfWork.GetRepository<PurchaseInvoice, int>().UpdateWithoutSaveAsync(AddedInvoive);
        //            foreach (var item in AddedInvoive.PurchaseInvoiceItems)
        //            {
        //                var stock = await unitOfWork.GetRepository<Stock, int>()
        //                    .FindAsync(s => s.ProductId == item.ProductId && s.StoreId == dto.settledStoreId);

        //                var unitCost = item.TotalNetAmount / item.Quantity;

        //                if (stock == null)
        //                {
        //                    await unitOfWork.GetRepository<Stock, int>().AddWithoutSaveAsync(new Stock
        //                    {
        //                        ProductId = (int)item.ProductId,
        //                        StoreId = (int)dto.settledStoreId,
        //                        Quantity = item.Quantity,
        //                        AvgCost = unitCost
        //                    });
        //                }
        //                else
        //                {
        //                    var newQty = stock.Quantity + item.Quantity;
        //                    stock.AvgCost =
        //                        ((stock.Quantity * stock.AvgCost) + (item.Quantity * unitCost)) / newQty;
        //                    stock.Quantity = newQty;
        //                }
        //            }
        //        }
        //        #endregion
        //        var res =await unitOfWork.SaveChangesAsync();
        //        if(res<=0)
        //            return Result<string>.Failure("خطأ في حفظ الفاتورة ", HttpStatusCode.InternalServerError);
        //        await unitOfWork.CommitAsync();
        //        return Result<string>.Success("تمت إضافة الفاتورة بنجاح");
        //    }
        //    catch (Exception ex)
        //    {
        //        await unitOfWork.RollbackAsync();
        //        return Result<string>.Failure(ex.Message, HttpStatusCode.InternalServerError);
        //    }
        //}
        public async Task<Result<string>> AddNewPurchaseInvoice(PurchaseInvoiceDtos dto)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                if (dto.supplierId == null)
                    return Result<string>.Failure("المورد مطلوب", HttpStatusCode.BadRequest);

                if (dto.items == null || dto.items.Count <= 0)
                    return Result<string>.Failure("يجب إضافة منتجات الفاتورة", HttpStatusCode.BadRequest);

                // إنشاء الفاتورة بدون رقم
                var invoice = new PurchaseInvoice
                {
                    SupplierId = (int)dto.supplierId,
                    CreateAt = DateTime.Now,
                    CreateBy = dto.createdBy,
                    PurchaseInvoiceItems = new List<PurchaseInvoiceItems>()
                };

                #region حساب عناصر الفاتورة
                foreach (var item in dto.items)
                {
                    if (item.quantity <= 0)
                        return Result<string>.Failure("لا يمكن إضافة منتج بدون كمية", HttpStatusCode.BadRequest);
                    if (item.buyingPricePerUnit < 0)
                        return Result<string>.Failure("لا يمكن إضافة منتج سعره أقل من 0", HttpStatusCode.BadRequest);

                    var addedItem = new PurchaseInvoiceItems
                    {
                        Quantity = item.quantity,
                        BuyingPricePerUnit = (decimal)item.buyingPricePerUnit,
                        ProductId = item.productId,
                        TotalGrowthAmount = item.quantity * item.buyingPricePerUnit
                    };

                    if (item.precentageRival != null && item.precentageRival > 0)
                    {
                        addedItem.PrecentageRival = item.precentageRival;
                        addedItem.TotalRivalValue = addedItem.TotalGrowthAmount * (item.precentageRival / 100);
                    }
                    else if (item.rivalValue != null && item.rivalValue > 0)
                    {
                        addedItem.RivalValue = item.rivalValue;
                        addedItem.TotalRivalValue = item.rivalValue;
                    }
                    else
                    {
                        addedItem.TotalRivalValue = 0;
                    }

                    addedItem.TotalNetAmount = (decimal)addedItem.TotalGrowthAmount - (decimal)addedItem.TotalRivalValue;
                    invoice.PurchaseInvoiceItems.Add(addedItem);
                }
                #endregion

                #region إجمالي الفاتورة قبل الخصم
                invoice.TotalGrowthAmount = invoice.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);
                invoice.TotalNetAmount = invoice.TotalGrowthAmount;
                #endregion

                #region خصم الفاتورة
                if (dto.precentageRival != null && dto.precentageRival > 0)
                {
                    invoice.PrecentageRival = dto.precentageRival;
                    invoice.TotalRivalValue = invoice.TotalNetAmount * (dto.precentageRival / 100);
                    invoice.TotalNetAmount -= invoice.TotalRivalValue;
                }
                else if (dto.rivalValue != null && dto.rivalValue > 0)
                {
                    invoice.TotalRivalValue = dto.rivalValue;
                    invoice.TotalNetAmount -= invoice.TotalRivalValue;
                }
                #endregion

                #region توزيع خصم الفاتورة على العناصر
                var factor = invoice.TotalNetAmount / invoice.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);
                foreach (var item in invoice.PurchaseInvoiceItems)
                    item.TotalNetAmount *= (decimal)factor;
                #endregion

                #region الضريبة
                if (dto.taxPrecentage != null && dto.taxPrecentage > 0)
                {
                    invoice.TaxPrecentage = dto.taxPrecentage;
                    invoice.TaxValue = invoice.TotalNetAmount * (dto.taxPrecentage / 100);
                    invoice.TotalNetAmount += invoice.TaxValue;
                }
                #endregion

                #region توزيع الضريبة على العناصر
                var totalAfterDiscounts = invoice.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);
                var taxValue = invoice.TaxValue ?? 0m;
                foreach (var item in invoice.PurchaseInvoiceItems)
                    item.TotalNetAmount += taxValue * (item.TotalNetAmount / totalAfterDiscounts);
                #endregion

                #region تحقق من المجموع مع الفرونت
                if (Math.Round((decimal)invoice.TotalNetAmount, 2) != Math.Round((decimal)dto.totalNetAmount, 2))
                    return Result<string>.Failure("تم إرسال بيانات خاطئة في الفاتورة", HttpStatusCode.BadRequest);
                #endregion

                // حفظ الفاتورة أول مرة للحصول على Id
                await unitOfWork.GetRepository<PurchaseInvoice, int>().AddAsync(invoice);
                await unitOfWork.SaveChangesAsync();

                // تحديث رقم الفاتورة بعد الحصول على Id
                invoice.InvoiceNumber = $"PO{invoice.Id}";

                #region Journal Entry
                var journal = new JournalEntries
                {
                    EntryDate = DateTime.Now,
                    Desc = $"{dto.supplierName} فاتورة شراء {invoice.InvoiceNumber}",
                    ReferenceNo = invoice.InvoiceNumber,
                    referenceType = Domain.Enums.ReferenceType.PurchaseInvoice
                };
                await unitOfWork.GetRepository<JournalEntries, int>().AddAsync(journal);
                invoice.JournalEntryId = journal.Id;
                #endregion

                #region Accounting
                var supplierAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
                    .FindAsync(a => a.UserId == dto.supplierId.ToString());
                if (supplierAccount == null)
                    return Result<string>.Failure("هذا المورد لا يوجد له حساب في شجرة الحسابات", HttpStatusCode.BadRequest);

                var stockAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
                    .FindAsync(a => a.AccountCode == "1.1.3");
                if (stockAccount == null)
                    return Result<string>.Failure("حساب 1.1.3 الخاص بمنتجات المخازن غير موجود", HttpStatusCode.BadRequest);

                await unitOfWork.GetRepository<JournalEntryDetails, int>().AddWithoutSaveAsync(new JournalEntryDetails
                {
                    AccountId = supplierAccount.Id,
                    JournalEntryId = journal.Id,
                    Credit = (decimal)invoice.TotalNetAmount,
                    Debit = 0
                });
                await unitOfWork.GetRepository<JournalEntryDetails, int>().AddWithoutSaveAsync(new JournalEntryDetails
                {
                    AccountId = stockAccount.Id,
                    JournalEntryId = journal.Id,
                    Debit = (decimal)invoice.TotalNetAmount,
                    Credit = 0
                });
                #endregion

                #region Stock
                if (dto.settledStoreId != null)
                {
                    invoice.SetteledStoreId = dto.settledStoreId;
                    invoice.SettledStatus = Domain.Enums.PurchaseInvoivceStoresStatus.Settled;

                    foreach (var item in invoice.PurchaseInvoiceItems)
                    {
                        var stock = await unitOfWork.GetRepository<Stock, int>()
                            .FindAsync(s => s.ProductId == item.ProductId && s.StoreId == dto.settledStoreId);

                        var unitCost = item.TotalNetAmount / item.Quantity;

                        if (stock == null)
                        {
                            await unitOfWork.GetRepository<Stock, int>().AddWithoutSaveAsync(new Stock
                            {
                                ProductId =(int)item.ProductId,
                                StoreId = (int)dto.settledStoreId,
                                Quantity = item.Quantity,
                                AvgCost = unitCost
                            });
                        }
                        else
                        {
                            var newQty = stock.Quantity + item.Quantity;
                            stock.AvgCost = ((stock.Quantity * stock.AvgCost) + (item.Quantity * unitCost)) / newQty;
                            stock.Quantity = newQty;
                        }
                    }
                }
                #endregion

                // حفظ نهائي لكل التغييرات
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success($"تمت إضافة الفاتورة بنجاح. رقم الفاتورة: {invoice.InvoiceNumber}");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        //public async Task<Result<string>> DeletePurchaseInvoice(PurchaseInvoiceDtos dto)
        //{
        //    await unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var invoice = await unitOfWork.GetRepository<PurchaseInvoice, int>()
        //            .GetQueryable()
        //            .Include(p => p.PurchaseInvoiceItems)
        //            .FirstOrDefaultAsync(p => p.Id == dto.id);

        //        if (invoice == null)
        //            return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);

        //        if (invoice.SettledStatus == PurchaseInvoivceStoresStatus.Settled)
        //            return Result<string>.Failure("لا يمكن حذف فاتورة تم تسكينها", HttpStatusCode.BadRequest);

        //        if (invoice.PurchaseInvoiceItems != null && invoice.PurchaseInvoiceItems.Count > 0)
        //        {
        //            await unitOfWork.GetRepository<PurchaseInvoiceItems, int>()
        //                .DeleteRangeAsync(invoice.PurchaseInvoiceItems);
        //        }

        //        await unitOfWork.GetRepository<PurchaseInvoice, int>().DeleteAsync(invoice);

        //        await unitOfWork.SaveChangesAsync();
        //        await unitOfWork.CommitAsync();

        //        return Result<string>.Success("تم حذف الفاتورة بنجاح");
        //    }
        //    catch (Exception ex)
        //    {
        //        await unitOfWork.RollbackAsync();
        //        return Result<string>.Failure(ex.Message, HttpStatusCode.InternalServerError);
        //    }
        //}
        public async Task<Result<string>> DeletePurchaseInvoice(int id)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                var invoice = await unitOfWork.GetRepository<PurchaseInvoice, int>()
                    .GetQueryable()
                    .Include(p => p.PurchaseInvoiceItems)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);

                #region If Settled → Handle Stock Reverse

                if (invoice.SettledStatus != null )
                {
                    if (invoice.SetteledStoreId == null)
                        return Result<string>.Failure("المخزن غير محدد", HttpStatusCode.BadRequest);

                    foreach (var item in invoice.PurchaseInvoiceItems)
                    {
                        var stock = await unitOfWork.GetRepository<Stock, int>()
                            .FindAsync(s => s.ProductId == item.ProductId
                                         && s.StoreId == invoice.SetteledStoreId);

                        if (stock == null || stock.Quantity < item.Quantity)
                            return Result<string>.Failure(
                                $"لا يمكن حذف الفاتورة لعدم توفر الكمية الكافية للصنف {item.ProductId}",
                                HttpStatusCode.BadRequest);
                    }

                    
                    foreach (var item in invoice.PurchaseInvoiceItems)
                    {
                        var stock = await unitOfWork.GetRepository<Stock, int>()
                            .FindAsync(s => s.ProductId == item.ProductId
                                         && s.StoreId == invoice.SetteledStoreId);

                        var totalStockCost = stock.Quantity * stock.AvgCost;
                        var invoiceItemTotalCost = item.TotalNetAmount;

                        var newQty = stock.Quantity - item.Quantity;

                        if (newQty == 0)
                        {
                            stock.Quantity = 0;
                            stock.AvgCost = 0;
                        }
                        else
                        {
                            var newTotalCost = totalStockCost - invoiceItemTotalCost;
                            stock.Quantity = newQty;
                            stock.AvgCost = newTotalCost / newQty;
                        }
                    }
                }

                #endregion

                #region Delete Journal Entry

                if (invoice.JournalEntryId != null)
                {
                    var journalId = invoice.JournalEntryId.Value;

                   
                    var journal = await unitOfWork.GetRepository<JournalEntries, int>()
                        .GetByIdAsync(journalId);

                    if (journal != null)
                    {
                     
                        var details = await unitOfWork.GetRepository<JournalEntryDetails, int>()
                            .GetQueryable()
                            .Where(d => d.JournalEntryId == journalId)
                            .ToListAsync();

                        if (details.Any())
                        {
                             unitOfWork.GetRepository<JournalEntryDetails, int>()
                                .DeleteRangeWithoutSaveAsync(details);
                        }

                         unitOfWork.GetRepository<JournalEntries, int>()
                            .DeleteWithoutSaveAsync(journal);
                    }
                }

                #endregion

                #region Delete Invoice Items

                if (invoice.PurchaseInvoiceItems != null && invoice.PurchaseInvoiceItems.Count > 0)
                {
                     unitOfWork.GetRepository<PurchaseInvoiceItems, int>()
                        .DeleteRangeWithoutSaveAsync(invoice.PurchaseInvoiceItems);
                }

                #endregion

                #region Delete Invoice

                 unitOfWork.GetRepository<PurchaseInvoice, int>()
                    .DeleteWithoutSaveAsync(invoice);

                #endregion

                var result = await unitOfWork.SaveChangesAsync();

                if (result <= 0)
                    return Result<string>.Failure("خطأ أثناء حذف الفاتورة", HttpStatusCode.InternalServerError);

                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم حذف الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure(ex.Message, HttpStatusCode.InternalServerError);
            }
        }


        public async Task<ApiResponse<List<PurchaseInvoiceDtos>>> GetAllPurchaseInvoicies(PurchaseInvoiceFilters req)
        {
            IQueryable<PurchaseInvoice> query = unitOfWork.GetRepository<PurchaseInvoice, int>().GetQueryable()
                .Include(p => p.supplier).Include(p => p.Store);

            if (req.invoiceNumber != null)
            {
                query = query.Where(p => p.InvoiceNumber == req.invoiceNumber);
            }

            if (req.supplierId != null)
            {
                query = query.Where(p => p.SupplierId == req.supplierId);
            }

            if (req.settledStatus != null)
            {
                query = query.Where(p => (int)p.SettledStatus == req.settledStatus);
            }
            if (req.deleteStatus != null) 
            {
                query = query.Where(p => (int)p.DeleteStatus == req.deleteStatus);
            }
            var totalCount = await query.CountAsync();

            // Only apply pagination if both page and pageSize are provided
            List<PurchaseInvoiceDtos> result;
            int page = req.page ?? 0;
            int pageSize = req.pageSize ?? 0;

            if (page > 0 && pageSize > 0)
            {
                result = await query
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select( p => new PurchaseInvoiceDtos
                    {
                        id = p.Id,
                        createdAt = p.CreateAt,
                        createdBy = p.CreateBy,
                        totalGrowthAmount = p.TotalGrowthAmount,
                        totalNetAmount = p.TotalNetAmount,
                        invoiceNumber = p.InvoiceNumber,
                        supplierId = p.SupplierId,
                        supplierName = p.supplier.Name,
                        settledStatus = (int)p.SettledStatus,
                        deleteStatus = (int)p.DeleteStatus,
                        precentageRival = p.PrecentageRival,
                        rivalValue = p.RivalValue,
                        totalRivalValue = p.TotalRivalValue,
                        taxPrecentage = p.TaxPrecentage,
                        taxValue = p.TaxValue,
                        settledStoreId = p.SetteledStoreId,
                        settledStoreName = p.Store.StoreName,
                        updatedAt   =p.UpdateAt,
                        updatedBy=p.UpdateBy
                    }).OrderByDescending(x=>x.createdAt).ToListAsync();
            }
            else
            {
                result = await query
                    .OrderBy(p => p.Id)
                    .Select(p => new PurchaseInvoiceDtos
                    {
                        id = p.Id,
                        createdAt = p.CreateAt,
                        createdBy = p.CreateBy,
                        totalGrowthAmount = p.TotalGrowthAmount,
                        totalNetAmount = p.TotalNetAmount,
                        invoiceNumber = p.InvoiceNumber,
                        supplierId = p.SupplierId,
                        supplierName = p.supplier.Name,
                        settledStatus = (int)p.SettledStatus,
                        deleteStatus = (int)p.DeleteStatus,
                        precentageRival = p.PrecentageRival,
                        rivalValue = p.RivalValue,
                        totalRivalValue = p.TotalRivalValue,
                        taxPrecentage = p.TaxPrecentage,
                        taxValue = p.TaxValue,
                        settledStoreId = p.SetteledStoreId,
                        settledStoreName = p.Store.StoreName
                    })
                    .ToListAsync();

                page = 1;
                pageSize = result.Count;
            }

            var response = new ApiResponse<List<PurchaseInvoiceDtos>>
            {
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize > 0 ? pageSize : 1)),
                data = result
            };

            return response;
        }
        public async Task<Result<PurchaseInvoiceDtos>> GetById(int id)
        {
            var invoice = await unitOfWork.GetRepository<PurchaseInvoice, int>()
                .GetQueryable()
                .Include(p => p.supplier)
                .Include(p => p.Store)
                .Include(p => p.PurchaseInvoiceItems).ThenInclude(i=>i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (invoice == null)
                return Result<PurchaseInvoiceDtos>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);

            var dto = new PurchaseInvoiceDtos
            {
                id = invoice.Id,
                invoiceNumber = invoice.InvoiceNumber,
                supplierId = invoice.SupplierId,
                supplierName = invoice.supplier.Name,
                createdAt = invoice.CreateAt,
                createdBy = invoice.CreateBy,
                updatedAt = invoice.UpdateAt,
                updatedBy = invoice.UpdateBy,
                totalGrowthAmount = invoice.TotalGrowthAmount,
                totalNetAmount = invoice.TotalNetAmount,
                precentageRival = invoice.PrecentageRival,
                rivalValue = invoice.RivalValue,
                totalRivalValue = invoice.TotalRivalValue,
                taxPrecentage = invoice.TaxPrecentage,
                taxValue = invoice.TaxValue,
                settledStatus = invoice.SettledStatus != null? (int?)invoice.SettledStatus: null,
                deleteStatus = invoice.DeleteStatus != null? (int?)invoice.DeleteStatus: null,
                settledStoreId = invoice.SetteledStoreId,
                settledStoreName = invoice.Store?.StoreName,
                items = invoice.PurchaseInvoiceItems.Select(i => new PurchaseInvoiceItemsDtos
                {
                    productId = i.Product.Id,
                   productName = i.Product.Name+ "|"+ i.Product.productCode,
                    quantity = i.Quantity,
                    buyingPricePerUnit = i.BuyingPricePerUnit,
                    precentageRival = i.PrecentageRival,
                    rivalValue = i.RivalValue,
                    totalRivalValue = i.TotalRivalValue,
                    totalNetAmount = i.TotalNetAmount
                }).ToList()
            };

            return Result<PurchaseInvoiceDtos>.Success(dto);
        }




        public async Task<Result<string>> EditPurchaseInvoice(PurchaseInvoiceDtos dto)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await unitOfWork.GetRepository<PurchaseInvoice, int>()
                    .GetQueryable()
                    .Include(p => p.PurchaseInvoiceItems)
                    .FirstOrDefaultAsync(p => p.Id == dto.id);

                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);


                // Update basic data
                invoice.SupplierId = (int)dto.supplierId;
                invoice.InvoiceNumber = dto.invoiceNumber;
                invoice.PrecentageRival = dto.precentageRival;
                invoice.RivalValue = dto.rivalValue;
                invoice.TaxPrecentage = dto.taxPrecentage;
                invoice.DeleteStatus = (PurchaseInvoivceDeleteStatus)dto.deleteStatus;
                invoice.SettledStatus = (PurchaseInvoivceStoresStatus)dto.settledStatus;
                invoice.UpdateBy = dto.updatedBy;
                invoice.UpdateAt = DateTime.UtcNow;
                // Remove old items
                if (dto.items.Count > 0)
                {
                    await unitOfWork.GetRepository<PurchaseInvoiceItems, int>()
                         .DeleteRangeAsync(invoice.PurchaseInvoiceItems);

                    invoice.PurchaseInvoiceItems.Clear();

                    // Re-add items (reuse same logic as Add)
                    foreach (var item in dto.items)
                    {
                        var newItem = new PurchaseInvoiceItems
                        {
                            ProductId = item.productId,
                            Quantity = item.quantity,
                            BuyingPricePerUnit = (decimal)item.buyingPricePerUnit
                        };

                        newItem.TotalGrowthAmount = item.quantity * item.buyingPricePerUnit;

                        if (item.precentageRival > 0)
                            newItem.TotalRivalValue = newItem.TotalGrowthAmount * (item.precentageRival / 100);
                        else if (item.rivalValue > 0)
                            newItem.TotalRivalValue = item.rivalValue;
                        else
                            newItem.TotalRivalValue = 0;

                        newItem.TotalNetAmount =
                            (decimal)newItem.TotalGrowthAmount - (decimal)newItem.TotalRivalValue;

                        invoice.PurchaseInvoiceItems.Add(newItem);
                    }

                    // Recalculate invoice totals
                    invoice.TotalGrowthAmount =
                        invoice.PurchaseInvoiceItems.Sum(x => x.TotalNetAmount);

                    invoice.TotalNetAmount = invoice.TotalGrowthAmount;

                    if (invoice.PrecentageRival > 0)
                    {
                        invoice.TotalRivalValue =
                            invoice.TotalNetAmount * (invoice.PrecentageRival / 100);
                        invoice.TotalNetAmount -= invoice.TotalRivalValue;
                    }
                    else if (invoice.RivalValue > 0)
                    {
                        invoice.TotalRivalValue = invoice.RivalValue;
                        invoice.TotalNetAmount -= invoice.TotalRivalValue;
                    }

                    if (invoice.TaxPrecentage > 0)
                    {
                        invoice.TaxValue =
                            invoice.TotalNetAmount * (invoice.TaxPrecentage / 100);
                        invoice.TotalNetAmount += invoice.TaxValue;
                    }
                }
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم تعديل الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<byte[]>> GeneratePdf(int id, bool isSimple)
        {
            var invoiceResult = await GetById(id);

            if (!invoiceResult.IsSuccess)
                return Result<byte[]>.Failure("Invoice not found", HttpStatusCode.NotFound);

            var invoice = invoiceResult.Data;
            var logoBytes = File.ReadAllBytes("Assets/Images/logo.png");



            var document = new PurchaseInvoicePdfDocument(invoice,logoBytes , isSimple);

            var pdfBytes = document.GeneratePdf();

            return Result<byte[]>.Success(pdfBytes);
        }
    }
}
