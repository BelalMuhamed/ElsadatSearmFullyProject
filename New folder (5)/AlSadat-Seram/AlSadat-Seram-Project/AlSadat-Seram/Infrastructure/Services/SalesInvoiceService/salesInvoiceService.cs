using AlSadatSeram.Services.contract;
using AlSadatSeram.Services.contract.SalesInvoiceItemsDD.Dtos;
using Application.DTOs;
using Application.DTOs.SalesInvoices;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.SalesInvoiceService;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.copounModel;
using Domain.Entities.Finance;
using Domain.Entities.Invoices;
using Domain.Entities.Transactions;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.SalesInvoiceService
{
    public class salesInvoiceService:IsalesInvoiceService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;

        public salesInvoiceService(IUnitOfWork unitOfWork, ICurrentUserService _currentUserService)
        {
            this.unitOfWork = unitOfWork;
            currentUserService = _currentUserService;
        }

        //public async Task<Result<string>> AddNewSalesInvoice(SalesInvoicesResponse dto)
        //{
        //    await unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        List<SalesInvoiceItems> AddedItems = new List<SalesInvoiceItems>();
        //        #region validation
        //        bool exists = await unitOfWork.GetRepository<SalesInvoices, int>()
        //           .GetQueryable()
        //           .AnyAsync(s => s.InvoiceNumber == dto.invoiceNumber);

        //        if (exists)
        //            return Result<string>.Failure("رقم الفاتورة موجود بالفعل", HttpStatusCode.Conflict);
        //        if (dto.distributorId == null)
        //            return Result<string>.Failure("العميل مطلوب ", HttpStatusCode.BadRequest);
        //        var IsCustomerExist =await unitOfWork.GetRepository<Distributor_Merchant_Agent, string>().GetQueryable().Include(x => x.User).AnyAsync(x => x.UserId == dto.distributorId);
        //        if(!IsCustomerExist)
        //            return Result<string>.Failure("العميل غير مسجل في قاعدة البيانات ", HttpStatusCode.BadRequest);
        //        if(dto.items== null || dto.items.Count()<=0)
        //            return Result<string>.Failure("يجب إضافة منتجات  ", HttpStatusCode.BadRequest);
        //        #endregion

        //        #region handle basic data of invoice 
        //        var AddedInvoice = new SalesInvoices()
        //        {
        //            TotalPoints=0,
        //            SalesInvoiceStatus=Domain.Enums.SalesInvoiceStatus.New,
        //            DistributorID=dto.distributorId,
        //            InvoiceNumber=dto.invoiceNumber,
        //            CreateBy= dto.createdBy,
        //            TotalNetAmount=0,
        //            TotalGrowthAmount=0

        //        };
        //        await unitOfWork.GetRepository<SalesInvoices, int>().AddWithoutSaveAsync(AddedInvoice);
        //        var isInvoiceAdded = await unitOfWork.SaveChangesAsync();
        //        if(isInvoiceAdded<=0)
        //            return Result<string>.Failure(" حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
        //        #endregion

        //        #region Handle Items
        //        for (int i = 0; i < dto.items.Count; i++)
        //        {
        //            var PointsPerItemUnit= await unitOfWork.GetRepository<Products, int>().GetQueryable().Where(p => p.Id == dto.items[i].productID).Select(p=>p.PointPerUnit).FirstOrDefaultAsync();
        //            if(PointsPerItemUnit == null)
        //            {
        //                await unitOfWork.RollbackAsync();
        //                return Result<string>.Failure("لا يمكن اضافة منتج غير مسجل  ", HttpStatusCode.BadRequest);
        //            }
        //            var TotalPointsForItem = PointsPerItemUnit * dto.items[i].quantity;
        //            AddedInvoice.TotalPoints += TotalPointsForItem;
        //            var AddedItem = new SalesInvoiceItems()
        //            {
        //                SalesInvoiceID=AddedInvoice.Id,
        //                SellingPrice = dto.items[i].sellingPrice,
        //                ProductID=dto.items[i].productID,
        //                Quantity = dto.items[i].quantity,
        //                TotalGrowthAmount= dto.items[i].sellingPrice * dto.items[i].quantity

        //            };
        //            if(dto.items[i].precentageRival >0 && dto.items[i].rivalValue>0)
        //            {
        //                await unitOfWork.RollbackAsync();
        //                return Result<string>.Failure($"{dto.items[i].productName} لا يمكن وضع نسبة خصم وخصم نقدي لنفس المنتج ", HttpStatusCode.BadRequest);
        //            }

        //            if(dto.items[i].precentageRival > 0)
        //            {
        //                AddedItem.PrecentageRival = dto.items[i].precentageRival;
        //                AddedItem.TotalRivalValue= AddedItem.TotalGrowthAmount * AddedItem.PrecentageRival/100;
        //                AddedItem.TotalNetAmount=(decimal)AddedItem.TotalGrowthAmount-(decimal)AddedItem.TotalRivalValue;
        //                AddedInvoice.TotalGrowthAmount += AddedItem.TotalNetAmount;
        //            }
        //            else if(dto.items[i].rivalValue > 0)
        //            {
        //                AddedItem.RivalValue = dto.items[i].rivalValue;
        //                AddedItem.TotalRivalValue = AddedItem.RivalValue;
        //                AddedItem.TotalNetAmount = (decimal)AddedItem.TotalGrowthAmount - (decimal)AddedItem.TotalRivalValue;
        //                AddedInvoice.TotalGrowthAmount += AddedItem.TotalNetAmount;
        //            }
        //            else
        //            {
        //                AddedItem.TotalNetAmount=(decimal)AddedItem.TotalGrowthAmount;
        //                AddedInvoice.TotalGrowthAmount += AddedItem.TotalNetAmount;
        //            }
        //            AddedItems.Add(AddedItem);
        //        }
        //        await unitOfWork.GetRepository<SalesInvoiceItems, int>().AddRangeAsyncWithoutSave(AddedItems);
        //        #endregion

        //        #region Handle Invoice Rivals and Tax
        //        AddedInvoice.TotalNetAmount = AddedInvoice.TotalGrowthAmount;
        //        if (dto.firstDiscount >0)
        //        {
        //            AddedInvoice.FirstDiscount=dto.firstDiscount;
        //            var FirstDiscountValue = AddedInvoice.TotalGrowthAmount * (decimal)dto.firstDiscount / 100;
        //            AddedInvoice.TotalNetAmount -=  FirstDiscountValue;
        //        }
        //        if (dto.secondDiscount > 0)
        //        {
        //            AddedInvoice.SecondDiscount = dto.secondDiscount;
        //            var SecondDiscountValue = AddedInvoice.TotalNetAmount * (decimal)dto.secondDiscount / 100;
        //            AddedInvoice.TotalNetAmount -=  SecondDiscountValue;
        //        }
        //        if (dto.thirdDiscount > 0)
        //        {
        //            AddedInvoice.ThirdDiscount = dto.thirdDiscount;
        //            var ThirdDiscountValue = AddedInvoice.TotalNetAmount * (decimal)dto.thirdDiscount / 100;
        //            AddedInvoice.TotalNetAmount -= ThirdDiscountValue;
        //        }
        //        if(dto.taxPrecentage >0)
        //        {
        //            AddedInvoice.TaxPrecentage = dto.taxPrecentage;
        //            AddedInvoice.TaxValue = AddedInvoice.TotalNetAmount * AddedInvoice.TaxPrecentage / 100;
        //            AddedInvoice.TotalNetAmount += AddedInvoice.TaxValue;
        //        }
        //       var IsAllUpdated= await unitOfWork.SaveChangesAsync();

        //        if (IsAllUpdated <= 0)
        //        {
        //            await unitOfWork.RollbackAsync();
        //            return Result<string>.Failure(" حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
        //        }
        //        await unitOfWork.CommitAsync();
        //        return Result<string>.Success("تمت إضافة الفاتورة بنجاح");
        //        #endregion
        //    }
        //    catch (Exception ex) 
        //    {
        //        await unitOfWork.RollbackAsync();
        //        return Result<string>.Failure(" حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
        //    }
        //}
        

        public async Task<Result<string>> AskToReverse(int id)
        {
            try
            {
                var SelectedInvoice=await unitOfWork.GetRepository<SalesInvoices,int>().GetByIdAsync(id);
                if (SelectedInvoice == null) 
                    return Result<string>.Failure("لا  يوجد فاتورة في قاعدة البيانات", HttpStatusCode.NotFound);
                SelectedInvoice.DeleteStatus = PurchaseInvoivceDeleteStatus.AskedToDelete;
                await unitOfWork.GetRepository<SalesInvoices,int>().UpdateAsync(SelectedInvoice);
                return Result<string>.Success("تم تقديم طلب القيد المعاكس بنجاح ");
            }
            catch
            {
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات ", HttpStatusCode.InternalServerError);

            }
        }

        public async Task<Result<string>> ChangInvoiceStatus(InvoiceChangeStatusReq req)
        {
            try
            {
                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(p => p.Distributor)
                    .Include(p => p.SalesInvoiceItems)
                    .ThenInclude(i => i.Product) 
                    .FirstOrDefaultAsync(p => p.Id == req.id);

                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.BadRequest);

                invoice.DeleteStatus = (PurchaseInvoivceDeleteStatus)Enum.ToObject(typeof(PurchaseInvoivceDeleteStatus), req.deleteStatus);
                invoice.UpdateAt = DateTime.UtcNow;
                invoice.UpdateBy = req.updateBy;

               await unitOfWork.GetRepository< SalesInvoices ,int >().UpdateAsync(invoice);
                return Result<string>.Success("تم تعديل حالة الفاتورة بنجاح ");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure("هناك مشكلة بالخادم حاول لاحقا !", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<string>> AddNewSalesInvoice(SalesInvoicesResponse dto)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                List<SalesInvoiceItems> AddedItems = new List<SalesInvoiceItems>();

                #region Validation
                if (dto.distributorId == null)
                    return Result<string>.Failure("العميل مطلوب", HttpStatusCode.BadRequest);

                var IsCustomerExist = await unitOfWork.GetRepository<Distributor_Merchant_Agent, string>()
                    .GetQueryable()
                    .Include(x => x.User)
                    .AnyAsync(x => x.UserId == dto.distributorId);

                if (!IsCustomerExist)
                    return Result<string>.Failure("العميل غير مسجل في قاعدة البيانات", HttpStatusCode.BadRequest);

                if (dto.items == null || dto.items.Count <= 0)
                    return Result<string>.Failure("يجب إضافة منتجات", HttpStatusCode.BadRequest);
                #endregion

                #region Handle Basic Data of Invoice
                var AddedInvoice = new SalesInvoices()
                {
                    TotalPoints = 0,
                    SalesInvoiceStatus = Domain.Enums.SalesInvoiceStatus.New,
                    DistributorID = dto.distributorId,
                    InvoiceNumber = "", // temp, will set after saving to get Id
                    CreateBy = dto.createdBy,
                    CreateAt = DateTime.Now,
                    TotalNetAmount = 0,
                    TotalGrowthAmount = 0
                };

                await unitOfWork.GetRepository<SalesInvoices, int>().AddWithoutSaveAsync(AddedInvoice);

                var isInvoiceAdded = await unitOfWork.SaveChangesAsync();
                if (isInvoiceAdded <= 0)
                    return Result<string>.Failure("حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);

                // Generate invoice number using Id
                AddedInvoice.InvoiceNumber = $"SO{AddedInvoice.Id}";
                await unitOfWork.SaveChangesAsync(); // Save the updated InvoiceNumber
                #endregion

                #region Handle Items
                foreach (var itemDto in dto.items)
                {
                    var PointsPerItemUnit = await unitOfWork.GetRepository<Products, int>()
                        .GetQueryable()
                        .Where(p => p.Id == itemDto.productID)
                        .Select(p => p.PointPerUnit)
                        .FirstOrDefaultAsync();

                    if (PointsPerItemUnit == null)
                    {
                        await unitOfWork.RollbackAsync();
                        return Result<string>.Failure($"لا يمكن اضافة منتج غير مسجل: {itemDto.productName}", HttpStatusCode.BadRequest);
                    }

                    var TotalPointsForItem = PointsPerItemUnit * itemDto.quantity;
                    AddedInvoice.TotalPoints += TotalPointsForItem;

                    var AddedItem = new SalesInvoiceItems()
                    {
                        SalesInvoiceID = AddedInvoice.Id,
                        SellingPrice = itemDto.sellingPrice,
                        ProductID = itemDto.productID,
                        Quantity = itemDto.quantity,
                        TotalGrowthAmount = itemDto.sellingPrice * itemDto.quantity
                    };

                    if (itemDto.precentageRival > 0 && itemDto.rivalValue > 0)
                    {
                        await unitOfWork.RollbackAsync();
                        return Result<string>.Failure($"{itemDto.productName} لا يمكن وضع نسبة خصم وخصم نقدي لنفس المنتج", HttpStatusCode.BadRequest);
                    }

                    if (itemDto.precentageRival > 0)
                    {
                        AddedItem.PrecentageRival = itemDto.precentageRival;
                        AddedItem.TotalRivalValue = AddedItem.TotalGrowthAmount * AddedItem.PrecentageRival / 100;
                        AddedItem.TotalNetAmount = (decimal)AddedItem.TotalGrowthAmount - (decimal)AddedItem.TotalRivalValue;
                        AddedInvoice.TotalGrowthAmount += AddedItem.TotalNetAmount;
                    }
                    else if (itemDto.rivalValue > 0)
                    {
                        AddedItem.RivalValue = itemDto.rivalValue;
                        AddedItem.TotalRivalValue = AddedItem.RivalValue;
                        AddedItem.TotalNetAmount = (decimal)AddedItem.TotalGrowthAmount - (decimal)AddedItem.TotalRivalValue;
                        AddedInvoice.TotalGrowthAmount += AddedItem.TotalNetAmount;
                    }
                    else
                    {
                        AddedItem.TotalNetAmount = (decimal)AddedItem.TotalGrowthAmount;
                        AddedInvoice.TotalGrowthAmount += AddedItem.TotalNetAmount;
                    }

                    AddedItems.Add(AddedItem);
                }

                await unitOfWork.GetRepository<SalesInvoiceItems, int>().AddRangeAsyncWithoutSave(AddedItems);
                #endregion

                #region Handle Invoice Discounts and Tax
                AddedInvoice.TotalNetAmount = AddedInvoice.TotalGrowthAmount;

                if (dto.firstDiscount > 0)
                {
                    AddedInvoice.FirstDiscount = dto.firstDiscount;
                    AddedInvoice.TotalNetAmount -= AddedInvoice.TotalGrowthAmount * (decimal)dto.firstDiscount / 100;
                }

                if (dto.secondDiscount > 0)
                {
                    AddedInvoice.SecondDiscount = dto.secondDiscount;
                    AddedInvoice.TotalNetAmount -= AddedInvoice.TotalNetAmount * (decimal)dto.secondDiscount / 100;
                }

                if (dto.thirdDiscount > 0)
                {
                    AddedInvoice.ThirdDiscount = dto.thirdDiscount;
                    AddedInvoice.TotalNetAmount -= AddedInvoice.TotalNetAmount * (decimal)dto.thirdDiscount / 100;
                }

                if (dto.taxPrecentage > 0)
                {
                    AddedInvoice.TaxPrecentage = dto.taxPrecentage;
                    AddedInvoice.TaxValue = AddedInvoice.TotalNetAmount * AddedInvoice.TaxPrecentage / 100;
                    AddedInvoice.TotalNetAmount += AddedInvoice.TaxValue;
                }

                var IsAllUpdated = await unitOfWork.SaveChangesAsync();
                if (IsAllUpdated <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
                }

                await unitOfWork.CommitAsync();
                return Result<string>.Success($"تمت إضافة الفاتورة بنجاح. رقم الفاتورة: {AddedInvoice.InvoiceNumber}");
                #endregion
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure("حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
            }
        }
        public async Task<Result<string>> ConfirmInvoice(invoiceConfirmationProductsStock req)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                decimal TotalCostAmount = 0;

                if (req.invoiceId == null)
                    return Result<string>.Failure("عفوا لا يوجد معرف فاتورة ", HttpStatusCode.BadRequest);
                if (req.withdrwanItemsQuantities.Count <= 0 || req.withdrwanItemsQuantities == null)
                    return Result<string>.Failure("لا يمكن تأكيد فاتورة بدون منتجات  ", HttpStatusCode.BadRequest);
                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(p => p.Distributor)
                    .Include(p => p.SalesInvoiceItems)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(p => p.Id == req.invoiceId);
                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.BadRequest);
                if (invoice.SalesInvoiceStatus == SalesInvoiceStatus.Accepted)
                    return Result<string>.Failure("الفاتورة مؤكدة بالفعل", HttpStatusCode.BadRequest);
                var TargetDis = await unitOfWork.GetRepository<Distributor_Merchant_Agent, int>().FindAsync(d => d.UserId == invoice.DistributorID);
                var customerAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
               .FindAsync(a => a.UserId == invoice.DistributorID);
                if (customerAccount == null) return Result<string>.Failure(" هذا العميل لا يوجد له حساب في شجرة الحسابات ", HttpStatusCode.BadRequest);
                var stockAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
                    .FindAsync(a => a.AccountCode == "1012");
                if (stockAccount == null) return Result<string>.Failure(" حساب 1012 الخاص بمنتجات المخازن غير موجود ", HttpStatusCode.BadRequest);
                var SalesAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
              .FindAsync(a => a.AccountCode == "401");
                if (SalesAccount == null) return Result<string>.Failure(" لا يمكن اثبات القيد بدون وحجود حساب 401 المبيعات في شجرة الحسابات ", HttpStatusCode.BadRequest);
                var CostOfGoodsSoldAccount = await unitOfWork.GetRepository<ChartOfAccounts, int>()
              .FindAsync(a => a.AccountCode == "503");
                if (CostOfGoodsSoldAccount == null) return Result<string>.Failure(" لا يمكن اثبات القيد بدون وحجود حساب 503 تكلفة البضاعة المباعة في شجرة الحسابات ",HttpStatusCode.BadRequest);
                #region Update Stock
                foreach (var pro in req.withdrwanItemsQuantities)
                {
                    var withdrawnquantityPerProduct = 0;
                    var targetInvoiceItem = invoice.SalesInvoiceItems.FirstOrDefault(p => p.ProductID == pro.productId);
                    var expQuantityPerPRo = targetInvoiceItem.Quantity;
                    if (pro.stocks.Count <= 0 || pro.stocks == null)
                        return Result<string>.Failure($"{pro.productName} : لا يمكن ـاكيد فاتورة بدون تحديد أماكن سحب منتجات الفاتورة من المخازن", HttpStatusCode.BadRequest);
                    foreach (var stock in pro.stocks)
                    {
                        if (stock.withdrawnQuantity == 0 || stock.withdrawnQuantity == null)
                            continue;
                        if (stock.withdrawnQuantity > stock.avaliableQuantity)
                            return Result<string>.Failure($"أقصي كمية يمكن سحبها ل{pro.productName}هي {stock.avaliableQuantity}", HttpStatusCode.BadRequest);

                        withdrawnquantityPerProduct += (int)stock.withdrawnQuantity;
                       
                        var targetStock = await unitOfWork.GetRepository<Stock, int>().FindAsync(s => s.StoreId == stock.storeId && s.ProductId == pro.productId);
                        if (targetStock == null)
                            return Result<string>.Failure($"{pro.productName}عفوا لا يوجد مخزون من المنتج " + $"{stock.storeName} داخل المخزن ", HttpStatusCode.BadRequest);
                        TotalCostAmount += ((decimal)targetStock.AvgCost * (decimal)stock.withdrawnQuantity);
                        targetStock.Quantity -= (decimal)stock.withdrawnQuantity;
                        unitOfWork.GetRepository<Stock, int>().UpdateWithoutSaveAsync(targetStock);
                        var SavedWithdrwanItem = new SalesInvoiceItemStoresQuantities()
                        {
                            InvoiceId = (int)req.invoiceId,
                            ProductId = pro.productId,
                            Quantity = (int)stock.withdrawnQuantity,
                            StoreID = stock.storeId
                        };
                        await unitOfWork.GetRepository<SalesInvoiceItemStoresQuantities, int>().AddWithoutSaveAsync(SavedWithdrwanItem);
                    }

                    if (withdrawnquantityPerProduct != expQuantityPerPRo)
                        return Result<string>.Failure($"{pro.productName}" + $"{withdrawnquantityPerProduct} الكمية المنقولة " + "لا تساوي " + $"{expQuantityPerPRo} الكمية المطلوبة ", HttpStatusCode.BadRequest);
                }
                #endregion

                #region WithdrawPoints
                var CurrentUser = await unitOfWork.GetRepository<ApplicationUser, string>().FindAsync(u => req.updateBy.Contains(u.FullName));
                if(CurrentUser == null)
                    return Result<string>.Failure("لا يمكن ايجاد المستخدم الحالي في قاعدة البيانات", HttpStatusCode.BadRequest);

                var addedWithdrawnRecord = new PointTransactions()
                {
                    CreatedAt = DateTime.Now,
                    SenderId = CurrentUser.Id,
                    ReceverId = invoice.DistributorID,
                    TotalPoints = invoice.TotalPoints
                };
                await unitOfWork.GetRepository<PointTransactions, int>().AddWithoutSaveAsync(addedWithdrawnRecord);

                #endregion
                #region updateinvoice
             
                invoice.SalesInvoiceStatus = SalesInvoiceStatus.Accepted;
                invoice.DeleteStatus = null;
                invoice.UpdateAt = DateTime.Now;
                invoice.UpdateBy = req.updateBy;
                 unitOfWork.GetRepository<SalesInvoices,int>().UpdateWithoutSaveAsync(invoice);
                #endregion
                #region Accounting
                var journalEntry = new JournalEntries
                {
                    EntryDate = (DateTime)invoice.CreateAt,
                    PostedDate = DateTime.Now,
                    referenceType = ReferenceType.SalesInvoice,
                    Desc = $"قيد إثبات فاتورة بيع رقم {invoice.InvoiceNumber}",
                    ReferenceNo = invoice.Id.ToString()
                };

                await unitOfWork
                    .GetRepository<JournalEntries, int>()
                    .AddWithoutSaveAsync(journalEntry);

               var res = await unitOfWork.SaveChangesAsync();
                if(res<=0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("حدث خطأ أثنا الحفظ حاو مجددا ", HttpStatusCode.InternalServerError);
                }
                await unitOfWork.GetRepository<JournalEntryDetails, int>()
                    .AddWithoutSaveAsync(new JournalEntryDetails
                    {
                        JournalEntryId = journalEntry.Id,
                        AccountId = SalesAccount.Id,
                        Debit = 0,
                        Credit = invoice.TotalNetAmount ?? 0
                    });
                await unitOfWork.GetRepository<JournalEntryDetails, int>()
                    .AddWithoutSaveAsync(new JournalEntryDetails
                    {
                        JournalEntryId = journalEntry.Id,
                        AccountId = CostOfGoodsSoldAccount.Id,
                        Debit = TotalCostAmount ,
                        Credit = 0
                    });
                await unitOfWork.GetRepository<JournalEntryDetails, int>()
                    .AddWithoutSaveAsync(new JournalEntryDetails
                    {
                        JournalEntryId = journalEntry.Id,
                        AccountId = stockAccount.Id,
                        Debit = 0,
                        Credit = TotalCostAmount
                    });
                await unitOfWork.GetRepository<JournalEntryDetails, int>()
                .AddWithoutSaveAsync(new JournalEntryDetails
                {
                    JournalEntryId = journalEntry.Id,
                    AccountId = customerAccount.Id,
                    Debit = invoice.TotalNetAmount ?? 0,
                    Credit = 0
                });
                #endregion
                res = await unitOfWork.SaveChangesAsync();
                if (res <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("حدث خطأ أثنا الحفظ حاو مجددا ", HttpStatusCode.InternalServerError);
                }
                await unitOfWork.CommitAsync();
                return Result<string>.Success("تمت تأكيد الفاتورة بنجاح");
            
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure(" حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
            }

        }

        public async Task<Result<string>> DeleteSalesInvoice(int id)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
               
                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(p => p.SalesInvoiceItems)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);

                
                if (invoice.SalesInvoiceStatus != Domain.Enums.SalesInvoiceStatus.New)
                    return Result<string>.Failure("لا يمكن حذف الفاتورة إلا إذا كانت جديدة", HttpStatusCode.BadRequest);

              
                if (invoice.SalesInvoiceItems != null && invoice.SalesInvoiceItems.Any())
                {
                     unitOfWork.GetRepository<SalesInvoiceItems, int>()
                        .DeleteRangeWithoutSaveAsync(invoice.SalesInvoiceItems);

                    invoice.SalesInvoiceItems.Clear();
                }

           
                 unitOfWork.GetRepository<SalesInvoices, int>()
                    .DeleteWithoutSaveAsync(invoice);

                var saved = await unitOfWork.SaveChangesAsync();

                if (saved <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("حدث خطأ أثناء حذف الفاتورة", HttpStatusCode.InternalServerError);
                }

                await unitOfWork.CommitAsync();
                return Result<string>.Success("تم حذف الفاتورة بنجاح");
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure("هناك مشكلة بالخادم حاول لاحقاً!", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<string>> EditSalesInvoice(SalesInvoicesResponse dto)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(x => x.SalesInvoiceItems)
                    .FirstOrDefaultAsync(x => x.Id == dto.id);

                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);
              


                #region Validation

                bool exists = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .AnyAsync(s => s.InvoiceNumber == dto.invoiceNumber && s.Id != dto.id);

                if (exists)
                    return Result<string>.Failure("رقم الفاتورة موجود بالفعل", HttpStatusCode.Conflict);

                if (dto.distributorId == null)
                    return Result<string>.Failure("العميل مطلوب", HttpStatusCode.BadRequest);

                if (dto.items == null || dto.items.Count <= 0)
                    return Result<string>.Failure("يجب إضافة منتجات", HttpStatusCode.BadRequest);

                #endregion

                #region Update Basic Data

                invoice.DistributorID = dto.distributorId;
                invoice.InvoiceNumber = dto.invoiceNumber;
                invoice.UpdateBy = dto.updateBy;
                invoice.UpdateAt = DateTime.UtcNow;

                invoice.TotalPoints = 0;
                invoice.TotalGrowthAmount = 0;
                invoice.TotalNetAmount = 0;

                #endregion

                #region Remove Old Items

                await unitOfWork.GetRepository<SalesInvoiceItems, int>()
                    .DeleteRangeAsync(invoice.SalesInvoiceItems);

                invoice.SalesInvoiceItems.Clear();

                #endregion

                #region Handle Items

                List<SalesInvoiceItems> newItems = new();

                foreach (var item in dto.items)
                {
                    var pointsPerUnit = await unitOfWork
                        .GetRepository<Products, int>()
                        .GetQueryable()
                        .Where(p => p.Id == item.productID)
                        .Select(p => p.PointPerUnit)
                        .FirstOrDefaultAsync();

                    if (pointsPerUnit == null)
                    {
                        await unitOfWork.RollbackAsync();
                        return Result<string>.Failure("لا يمكن اضافة منتج غير مسجل", HttpStatusCode.BadRequest);
                    }

                    invoice.TotalPoints += pointsPerUnit * item.quantity;

                    var newItem = new SalesInvoiceItems()
                    {
                        SalesInvoiceID = invoice.Id,
                        ProductID = item.productID,
                        Quantity = item.quantity,
                        SellingPrice = item.sellingPrice,
                        TotalGrowthAmount = item.sellingPrice * item.quantity
                    };

                    if (item.precentageRival > 0 && item.rivalValue > 0)
                    {
                        await unitOfWork.RollbackAsync();
                        return Result<string>.Failure(
                            $"{item.productName} لا يمكن وضع نسبة خصم وخصم نقدي لنفس المنتج",
                            HttpStatusCode.BadRequest);
                    }

                    if (item.precentageRival > 0)
                    {
                        newItem.PrecentageRival = item.precentageRival;
                        newItem.TotalRivalValue = newItem.TotalGrowthAmount * newItem.PrecentageRival / 100;
                    }
                    else if (item.rivalValue > 0)
                    {
                        newItem.RivalValue = item.rivalValue;
                        newItem.TotalRivalValue = newItem.RivalValue;
                    }
                    else
                    {
                        newItem.TotalRivalValue = 0;
                    }

                    newItem.TotalNetAmount =
                        (decimal)newItem.TotalGrowthAmount - (decimal)newItem.TotalRivalValue;

                    invoice.TotalGrowthAmount += newItem.TotalNetAmount;

                    newItems.Add(newItem);
                }

                await unitOfWork.GetRepository<SalesInvoiceItems, int>()
                    .AddRangeAsyncWithoutSave(newItems);

                #endregion

                #region Handle Sequential Discounts + Tax

                invoice.TotalNetAmount = invoice.TotalGrowthAmount;

                if (dto.firstDiscount > 0)
                {
                    invoice.FirstDiscount = dto.firstDiscount;
                    var firstValue = (decimal)invoice.TotalNetAmount * (decimal)dto.firstDiscount / 100;
                    invoice.TotalNetAmount -= firstValue;
                }

                if (dto.secondDiscount > 0)
                {
                    invoice.SecondDiscount = dto.secondDiscount;
                    var secondValue = (decimal)invoice.TotalNetAmount * (decimal)dto.secondDiscount / 100;
                    invoice.TotalNetAmount -= secondValue;
                }

                if (dto.thirdDiscount > 0)
                {
                    invoice.ThirdDiscount = dto.thirdDiscount;
                    var thirdValue = (decimal)invoice.TotalNetAmount * (decimal)dto.thirdDiscount / 100;
                    invoice.TotalNetAmount -= thirdValue;
                }

                if (dto.taxPrecentage > 0)
                {
                    invoice.TaxPrecentage = dto.taxPrecentage;
                    invoice.TaxValue = invoice.TotalNetAmount * invoice.TaxPrecentage / 100;
                    invoice.TotalNetAmount += invoice.TaxValue;
                }

                #endregion
               
                var saved = await unitOfWork.SaveChangesAsync();

                if (saved <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
                }

                await unitOfWork.CommitAsync();
                return Result<string>.Success("تم تعديل الفاتورة بنجاح");
            }
            catch
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure("حدث خطأ أثناء حفظ الفاتورة", HttpStatusCode.InternalServerError);
            }
        }

        //public async Task<Result<ApiResponse<List<SalesInvoicesResponse>>>> GetAllSalesInvoicies(SalesInvoiceFilters req)
        //{
        //    IQueryable<SalesInvoices> query = unitOfWork.GetRepository<SalesInvoices, int>()
        //        .GetQueryable()
        //        .Include(p => p.Distributor)
        //        .Include(p => p.SalesInvoiceItems);

        //    if (!string.IsNullOrEmpty(req.invoiceNumber))
        //        query = query.Where(p => p.InvoiceNumber.ToLower().Contains (req.invoiceNumber.ToLower()));

        //    if (!string.IsNullOrEmpty(req.customerId))
        //        query = query.Where(p => p.DistributorID == req.customerId);

        //    if (req.deleteStatus != null)
        //        query = query.Where(p => (int)p.DeleteStatus == req.deleteStatus);

        //    if (req.createAt.HasValue)
        //    {
        //        var date = req.createAt.Value.Date;
        //        query = query.Where(p =>
        //            p.CreateAt.HasValue &&
        //            p.CreateAt.Value.Date == date
        //        );
        //    }

        //    var totalCount = await query.CountAsync();

        //    int page = req.page ?? 1;
        //    int pageSize = req.pageSize ?? totalCount;

        //    var result = await query
        //        .OrderBy(p => p.Id)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(p => new SalesInvoicesResponse
        //        {
        //            id = p.Id,
        //            distributorId = p.DistributorID,
        //            distributorName = p.Distributor.FullName,
        //            invoiceNumber = p.InvoiceNumber,
        //            totalPoints = p.TotalPoints,
        //            createdAt = (DateTime)p.CreateAt,
        //            createdBy = p.CreateBy,
        //            salesInvoiceStatus = (int?)p.SalesInvoiceStatus,
        //            deleteStatus = (int?)p.DeleteStatus,
        //            updateBy = p.UpdateBy,
        //            updateAt = p.UpdateAt,
        //            totalGrowthAmount = p.TotalGrowthAmount,
        //            totalNetAmount = p.TotalNetAmount,
        //            taxPrecentage = p.TaxPrecentage,
        //            taxValue = p.TaxValue,
        //            firstDiscount = p.FirstDiscount,
        //            secondDiscount = p.SecondDiscount,
        //            thirdDiscount = p.ThirdDiscount,
        //            reverseJournalEntry=p.ReverseJournalEntryId

        //        }).OrderByDescending(x => x.createdAt).ToListAsync();

        //    return new ApiResponse<List<SalesInvoicesResponse>>
        //    {
        //        data = result,
        //        totalCount = totalCount,
        //        page = page,
        //        pageSize = pageSize,
        //        totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize > 0 ? pageSize : 1))
        //    };
        //}
        public async Task<Result<ApiResponse<List<SalesInvoicesResponse>>>> GetAllSalesInvoicies(SalesInvoiceFilters req)
        {
            try
            {
                IQueryable<SalesInvoices> query = unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(p => p.Distributor)
                    .Include(p => p.SalesInvoiceItems);

                if (!string.IsNullOrEmpty(req.invoiceNumber))
                    query = query.Where(p => p.InvoiceNumber.ToLower().Contains(req.invoiceNumber.ToLower()));

                if (!string.IsNullOrEmpty(req.customerId))
                    query = query.Where(p => p.DistributorID == req.customerId);

                if (req.deleteStatus != null)
                    query = query.Where(p => (int)p.DeleteStatus == req.deleteStatus);

                if (req.createAt.HasValue)
                {
                    var date = req.createAt.Value.Date;
                    query = query.Where(p =>
                        p.CreateAt.HasValue &&
                        p.CreateAt.Value.Date == date
                    );
                }

                var totalCount = await query.CountAsync();

                int page = req.page ?? 1;
                int pageSize = req.pageSize ?? totalCount;

                var result = await query
                    .OrderBy(p => p.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new SalesInvoicesResponse
                    {
                        id = p.Id,
                        distributorId = p.DistributorID,
                        distributorName = p.Distributor.FullName,
                        invoiceNumber = p.InvoiceNumber,
                        totalPoints = p.TotalPoints,
                        createdAt = (DateTime)p.CreateAt,
                        createdBy = p.CreateBy,
                        salesInvoiceStatus = (int?)p.SalesInvoiceStatus,
                        deleteStatus = (int?)p.DeleteStatus,
                        updateBy = p.UpdateBy,
                        updateAt = p.UpdateAt,
                        totalGrowthAmount = p.TotalGrowthAmount,
                        totalNetAmount = p.TotalNetAmount,
                        taxPrecentage = p.TaxPrecentage,
                        taxValue = p.TaxValue,
                        firstDiscount = p.FirstDiscount,
                        secondDiscount = p.SecondDiscount,
                        thirdDiscount = p.ThirdDiscount,
                        reverseJournalEntry = p.ReverseJournalEntryId
                    })
                    .OrderByDescending(x => x.createdAt)
                    .ToListAsync();

                var response = new ApiResponse<List<SalesInvoicesResponse>>
                {
                    data = result,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)(pageSize > 0 ? pageSize : 1))
                };

                return Result<ApiResponse<List<SalesInvoicesResponse>>>
                    .Success(response);
            }
            catch (Exception ex)
            {
                return Result<ApiResponse<List<SalesInvoicesResponse>>>
                    .Failure("حدث خطأ أثناء جلب الفواتير");
            }
        }

        public async Task<Result<SalesInvoicesResponse>> GetById(int id)
        {
            try
            {
                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(p => p.Distributor)
                    .Include(p => p.SalesInvoiceItems)
                    .ThenInclude(i => i.Product) // لو محتاج بيانات المنتج في كل item
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (invoice == null)
                    return Result<SalesInvoicesResponse>.Failure("الفاتورة غير موجودة",HttpStatusCode.BadRequest);

                var response = new SalesInvoicesResponse
                {
                    id = invoice.Id,
                    distributorId = invoice.DistributorID,
                    distributorName = invoice.Distributor?.FullName,
                    invoiceNumber = invoice.InvoiceNumber,
                    totalPoints = invoice.TotalPoints,
                    createdAt = invoice.CreateAt ?? DateTime.MinValue,
                    createdBy = invoice.CreateBy,
                    salesInvoiceStatus = (int?)invoice.SalesInvoiceStatus,
                    deleteStatus = (int?)invoice.DeleteStatus,
                    updateBy = invoice.UpdateBy,
                    updateAt = invoice.UpdateAt,
                    totalGrowthAmount = invoice.TotalGrowthAmount,
                    totalNetAmount = invoice.TotalNetAmount,
                    taxPrecentage = invoice.TaxPrecentage,
                    taxValue = invoice.TaxValue,
                    firstDiscount = invoice.FirstDiscount,
                    secondDiscount = invoice.SecondDiscount,
                    thirdDiscount = invoice.ThirdDiscount,

                    // تفاصيل المنتجات
                    items = invoice.SalesInvoiceItems.Select(i => new salesInvoiceItemsResp
                    {
                        id= i.Id,
                        sellingPrice=i.SellingPrice,
                        precentageRival=i.PrecentageRival,
                        productID=(int)i.ProductID,
                        productName=i.Product.Name + i.Product.productCode,
                        quantity=i.Quantity,
                        rivalValue=i.RivalValue,
                        totalGrowthAmount=i.TotalGrowthAmount,
                        totalNetAmount=i.TotalNetAmount,
                        totalRivalValue=i.TotalRivalValue,
                        

                    }).ToList()
                };

                return Result<SalesInvoicesResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<SalesInvoicesResponse>.Failure("لم يتم استرجاع بيانات الفاتورة!",HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<SalesInvoiceDetails>> GetInvoiceDetails(int id)
        {
            try
            {
                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(p => p.Distributor)
                    .Include(p => p.SalesInvoiceItems)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (invoice == null)
                    return Result<SalesInvoiceDetails>.Failure("الفاتورة غير موجودة", HttpStatusCode.NotFound);

                // جلب كل السحوبات المرتبطة بالـ invoiceId
                var withdrawnList = await unitOfWork.GetRepository<SalesInvoiceItemStoresQuantities, int>()
                    .GetQueryable()
                    .Include(s => s.Store)
                    .Where(s => s.InvoiceId == id)
                    .ToListAsync();

                // بناء تفاصيل المنتجات
                var items = invoice.SalesInvoiceItems.Select(i =>
                {
                    var stocks = withdrawnList
                        .Where(w => w.ProductId == i.ProductID)
                        .Select(s => new ProductStockPerStoreDto
                        {
                            storeId = s.StoreID,
                            storeName = s.Store?.StoreName,
                            avaliableQuantity = null,
                            withdrawnQuantity = s.Quantity
                        })
                        .ToList();

                    return new salesInvoiceItemsDetails
                    {
                        id = i.Id,
                        productID = (int)i.ProductID,
                        productName = i.Product.Name + (i.Product.productCode ?? ""),
                        sellingPrice = i.SellingPrice,
                        quantity = i.Quantity,
                        precentageRival = i.PrecentageRival,
                        rivalValue = i.RivalValue,
                        totalRivalValue = i.TotalRivalValue,
                        totalGrowthAmount = i.TotalGrowthAmount,
                        totalNetAmount = i.TotalNetAmount,
                        WithdrwanStock = stocks
                    };
                }).ToList();

                var result = new SalesInvoiceDetails
                {
                    id = invoice.Id,
                    distributorId = invoice.DistributorID,
                    distributorName = invoice.Distributor?.FullName,
                    totalPoints = invoice.TotalPoints,
                    createdAt = invoice.CreateAt ?? DateTime.Now,
                    createdBy = invoice.CreateBy,
                    salesInvoiceStatus = (int?)invoice.SalesInvoiceStatus,
                    deleteStatus = (int?)invoice.DeleteStatus,
                    updateBy = invoice.UpdateBy,
                    updateAt = invoice.UpdateAt,
                    totalGrowthAmount = invoice.TotalGrowthAmount,
                    totalNetAmount = invoice.TotalNetAmount,
                    taxPrecentage = invoice.TaxPrecentage,
                    taxValue = invoice.TaxValue,
                    firstDiscount = invoice.FirstDiscount,
                    secondDiscount = invoice.SecondDiscount,
                    thirdDiscount = invoice.ThirdDiscount,
                    WithdrwanStock = items,
                    invoiceNumber=invoice.InvoiceNumber
                };

                return Result<SalesInvoiceDetails>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<SalesInvoiceDetails>.Failure("حدث خطأ أثناء جلب تفاصيل الفاتورة", HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<string>> RefusedReverse(int id)
        {
            try
            {
                var SelectedInvoice = await unitOfWork.GetRepository<SalesInvoices, int>().GetByIdAsync(id);
                if (SelectedInvoice == null )
                    return Result<string>.Failure("لا  يوجد فاتورة في قاعدة البيانات", HttpStatusCode.NotFound);
                if( SelectedInvoice.DeleteStatus != PurchaseInvoivceDeleteStatus.AskedToDelete)
                    return Result<string>.Failure("لا يوجد طلب حذف مقدم لتلك الفاتورة ", HttpStatusCode.BadRequest);

                SelectedInvoice.DeleteStatus = PurchaseInvoivceDeleteStatus.refused;
                await unitOfWork.GetRepository<SalesInvoices, int>().UpdateAsync(SelectedInvoice);
                return Result<string>.Success("تم رفض طلب القيد المعاكس بنجاح ");
            }
            catch
            {
                return Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات ", HttpStatusCode.InternalServerError);
            }
        }

        //public async Task<Result<string>> ReverseInvoice(int id)
        //{
        //    await unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        #region Get Invoice

        //        var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
        //            .GetQueryable()
        //            .Include(i => i.SalesInvoiceItems)
        //            .FirstOrDefaultAsync(i => i.Id == id);

        //        if (invoice == null)
        //            return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.BadRequest);


        //        #endregion

        //        #region Reverse Stock

        //        var withdrawnItems = await unitOfWork
        //            .GetRepository<SalesInvoiceItemStoresQuantities, int>()
        //            .GetQueryable()
        //            .Where(x => x.InvoiceId == id)
        //            .ToListAsync();

        //        foreach (var item in withdrawnItems)
        //        {
        //            var stock = await unitOfWork
        //                .GetRepository<Stock, int>()
        //                .FindAsync(s => s.StoreId == item.StoreID && s.ProductId == item.ProductId);

        //            if (stock == null)
        //                return Result<string>.Failure("المخزون غير موجود", HttpStatusCode.BadRequest);

        //            stock.Quantity += item.Quantity;

        //            unitOfWork.GetRepository<Stock, int>().UpdateWithoutSaveAsync(stock);
        //        }

        //        #endregion

        //        #region Reverse Points

        //        var currentPoints = await unitOfWork
        //            .GetRepository<PointTransactions, int>()
        //            .GetQueryable()
        //            .Where(p => p.ReceverId == invoice.DistributorID)
        //            .SumAsync(p => p.TotalPoints);

        //        if (currentPoints < invoice.TotalPoints)
        //            return Result<string>.Failure(
        //                "لا يمكن عكس الفاتورة لأن التاجر لا يملك نقاط كافية",
        //                HttpStatusCode.BadRequest);
        //        var ReqUpdatedUserMail= invoice.UpdateBy.Split('|')[1];
        //        if(ReqUpdatedUserMail==null)
        //        {
        //            return Result<string>.Failure("لا يمكن تحديد المستخدم الحالي", HttpStatusCode.BadRequest);

        //        }
        //        var currentUser = await unitOfWork
        //            .GetRepository<ApplicationUser, string>()
        //            .FindAsync(u => u.Email == ReqUpdatedUserMail);

        //        if (currentUser == null)
        //            return Result<string>.Failure("لا يمكن تحديد المستخدم الحالي", HttpStatusCode.BadRequest);

        //        var reversePoints = new PointTransactions
        //        {
        //            CreatedAt = DateTime.Now,
        //            SenderId = invoice.DistributorID,
        //            ReceverId = currentUser.Id,
        //            TotalPoints = invoice.TotalPoints
        //        };

        //        await unitOfWork
        //            .GetRepository<PointTransactions, int>()
        //            .AddWithoutSaveAsync(reversePoints);

        //        #endregion

        //        #region Reverse Journal Entry

        //        var originalEntry = await unitOfWork
        //            .GetRepository<JournalEntries, int>()
        //            .FindAsync(j =>
        //                j.referenceType == ReferenceType.SalesInvoice &&
        //                j.ReferenceNo == invoice.Id.ToString());

        //        if (originalEntry == null)
        //            return Result<string>.Failure("لا يوجد قيد محاسبي مرتبط بالفاتورة", HttpStatusCode.BadRequest);


        //        var originalDetails = await unitOfWork
        //            .GetRepository<JournalEntryDetails, int>()
        //            .GetQueryable()
        //            .Where(d => d.JournalEntryId == originalEntry.Id)
        //            .ToListAsync();


        //        var reverseEntry = new JournalEntries
        //        {
        //            EntryDate = DateTime.Now,
        //            PostedDate = DateTime.Now,
        //            referenceType = ReferenceType.SalesInvoice,
        //            Desc = $"قيد عكسي لفاتورة بيع رقم {invoice.InvoiceNumber}",
        //            ReferenceNo = invoice.Id.ToString()
        //        };

        //        await unitOfWork
        //            .GetRepository<JournalEntries, int>()
        //            .AddWithoutSaveAsync(reverseEntry);

        //        var res = await unitOfWork.SaveChangesAsync();

        //        if (res <= 0)
        //        {
        //            await unitOfWork.RollbackAsync();
        //            return Result<string>.Failure("فشل إنشاء القيد العكسي", HttpStatusCode.InternalServerError);
        //        }


        //        foreach (var line in originalDetails)
        //        {
        //            await unitOfWork
        //                .GetRepository<JournalEntryDetails, int>()
        //                .AddWithoutSaveAsync(new JournalEntryDetails
        //                {
        //                    JournalEntryId = reverseEntry.Id,
        //                    AccountId = line.AccountId,
        //                    Debit = line.Credit,
        //                    Credit = line.Debit
        //                });
        //        }

        //        #endregion

        //        #region Update Invoice

        //        invoice.SalesInvoiceStatus = SalesInvoiceStatus.canceled;

        //        invoice.UpdateAt = DateTime.Now;

        //        unitOfWork
        //            .GetRepository<SalesInvoices, int>()
        //            .UpdateWithoutSaveAsync(invoice);

        //        #endregion

        //        res = await unitOfWork.SaveChangesAsync();

        //        if (res <= 0)
        //        {
        //            await unitOfWork.RollbackAsync();
        //            return Result<string>.Failure("حدث خطأ أثناء عكس الفاتورة", HttpStatusCode.InternalServerError);
        //        }

        //        await unitOfWork.CommitAsync();

        //        return Result<string>.Success("تم عكس الفاتورة بنجاح");
        //    }
        //    catch (Exception)
        //    {
        //        await unitOfWork.RollbackAsync();
        //        return Result<string>.Failure("حدث خطأ أثناء عكس الفاتورة", HttpStatusCode.InternalServerError);
        //    }
        //}
        public async Task<Result<string>> ReverseInvoice(int id)
        {
            await unitOfWork.BeginTransactionAsync();
            try
            {
                #region Get Invoice

                var invoice = await unitOfWork.GetRepository<SalesInvoices, int>()
                    .GetQueryable()
                    .Include(i => i.SalesInvoiceItems)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoice == null)
                    return Result<string>.Failure("الفاتورة غير موجودة", HttpStatusCode.BadRequest);

                #endregion

                #region Reverse Stock

                var withdrawnItems = await unitOfWork
                    .GetRepository<SalesInvoiceItemStoresQuantities, int>()
                    .GetQueryable()
                    .Where(x => x.InvoiceId == id)
                    .ToListAsync();

                foreach (var item in withdrawnItems)
                {
                    var stock = await unitOfWork
                        .GetRepository<Stock, int>()
                        .FindAsync(s => s.StoreId == item.StoreID && s.ProductId == item.ProductId);

                    if (stock == null)
                        return Result<string>.Failure("المخزون غير موجود", HttpStatusCode.BadRequest);

                    stock.Quantity += item.Quantity;
                    unitOfWork.GetRepository<Stock, int>().UpdateWithoutSaveAsync(stock);
                }

                #endregion

                #region Reverse Points

                var currentPoints = await unitOfWork
                    .GetRepository<PointTransactions, int>()
                    .GetQueryable()
                    .Where(p => p.ReceverId == invoice.DistributorID)
                    .SumAsync(p => p.TotalPoints);

                if (currentPoints < invoice.TotalPoints)
                    return Result<string>.Failure(
                        "لا يمكن عكس الفاتورة لأن التاجر لا يملك نقاط كافية",
                        HttpStatusCode.BadRequest);

                var ReqUpdatedUserMail = invoice.UpdateBy.Split('|')[1];
                if (ReqUpdatedUserMail == null)
                    return Result<string>.Failure("لا يمكن تحديد المستخدم الحالي", HttpStatusCode.BadRequest);

                var currentUser = await unitOfWork
                    .GetRepository<ApplicationUser, string>()
                    .FindAsync(u => u.Email == ReqUpdatedUserMail);

                if (currentUser == null)
                    return Result<string>.Failure("لا يمكن تحديد المستخدم الحالي", HttpStatusCode.BadRequest);

                var reversePoints = new PointTransactions
                {
                    CreatedAt = DateTime.Now,
                    SenderId = invoice.DistributorID,
                    ReceverId = currentUser.Id,
                    TotalPoints = invoice.TotalPoints
                };

                await unitOfWork
                    .GetRepository<PointTransactions, int>()
                    .AddWithoutSaveAsync(reversePoints);

                #endregion

                #region Reverse Journal Entry

                var originalEntry = await unitOfWork
                    .GetRepository<JournalEntries, int>()
                    .FindAsync(j =>
                        j.referenceType == ReferenceType.SalesInvoice &&
                        j.ReferenceNo == invoice.Id.ToString());

                if (originalEntry == null)
                    return Result<string>.Failure("لا يوجد قيد محاسبي مرتبط بالفاتورة", HttpStatusCode.BadRequest);

                var originalDetails = await unitOfWork
                    .GetRepository<JournalEntryDetails, int>()
                    .GetQueryable()
                    .Where(d => d.JournalEntryId == originalEntry.Id)
                    .ToListAsync();

                var reverseEntry = new JournalEntries
                {
                    EntryDate = DateTime.Now,
                    PostedDate = DateTime.Now,
                    referenceType = ReferenceType.SalesInvoice,
                    Desc = $"قيد عكسي لفاتورة بيع رقم {invoice.InvoiceNumber}",
                    ReferenceNo = invoice.Id.ToString()
                };

                await unitOfWork
                    .GetRepository<JournalEntries, int>()
                    .AddWithoutSaveAsync(reverseEntry);

                var res = await unitOfWork.SaveChangesAsync();
                if (res <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("فشل إنشاء القيد العكسي", HttpStatusCode.InternalServerError);
                }

                // إضافة تفاصيل القيد العكسي
                foreach (var line in originalDetails)
                {
                    await unitOfWork
                        .GetRepository<JournalEntryDetails, int>()
                        .AddWithoutSaveAsync(new JournalEntryDetails
                        {
                            JournalEntryId = reverseEntry.Id,
                            AccountId = line.AccountId,
                            Debit = line.Credit,
                            Credit = line.Debit
                        });
                }

                #endregion

                #region Update Invoice

                invoice.SalesInvoiceStatus = SalesInvoiceStatus.canceled;
                invoice.UpdateAt = DateTime.Now;

                // تحديث الحقل ReverseJournalEntryId بالمعرف الجديد
                invoice.ReverseJournalEntryId = reverseEntry.Id;

                unitOfWork
                    .GetRepository<SalesInvoices, int>()
                    .UpdateWithoutSaveAsync(invoice);

                res = await unitOfWork.SaveChangesAsync();
                if (res <= 0)
                {
                    await unitOfWork.RollbackAsync();
                    return Result<string>.Failure("حدث خطأ أثناء تحديث الفاتورة بالقيد العكسي", HttpStatusCode.InternalServerError);
                }

                #endregion

                await unitOfWork.CommitAsync();
                return Result<string>.Success("تم عكس الفاتورة بنجاح");
            }
            catch (Exception)
            {
                await unitOfWork.RollbackAsync();
                return Result<string>.Failure("حدث خطأ أثناء عكس الفاتورة", HttpStatusCode.InternalServerError);
            }
        }
    }
}
