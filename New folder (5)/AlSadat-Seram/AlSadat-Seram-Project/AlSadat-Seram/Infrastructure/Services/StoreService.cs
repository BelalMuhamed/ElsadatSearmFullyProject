using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Transactions;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services
{
    public class StoreService:IStore
    {
        private readonly IUnitOfWork unitOfWork;

        public StoreService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<Result<string>> AddNewStore(StoreDto dto)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(dto.storeName))
                    return Result<string>.Failure("الإسم مطلوب",HttpStatusCode.BadRequest);

                bool exists = await unitOfWork.GetRepository<Store, int>().GetQueryable()
                                            .AnyAsync(s=>s.StoreName==dto.storeName);

                if(exists)
                    return Result<string>.Failure("اسم المخزن موجود بالفعل",HttpStatusCode.Conflict);

                Store AddedStore = new Store()
                {
                    StoreName = dto.storeName
                };
                await unitOfWork.GetRepository<Store,int>().AddWithoutSaveAsync(AddedStore);
                var saved = await unitOfWork.SaveChangesAsync();
                if(saved > 0)
                {

                    return Result<string>.Success($"تم إضافة {dto.storeName} بنجاح",HttpStatusCode.OK);
                }
                else
                {

                    return Result<string>.Failure("  فشل إضافة المخزن ",HttpStatusCode.BadRequest);
                }
            }
            catch(Exception ex)
            {
                return Result<string>.Failure("حدث خطأ أثناء الإضافة",HttpStatusCode.InternalServerError);
            }
        }


        public async Task<Result<string>> DeleteStore(StoreDeleteDto dto)
        {
            await unitOfWork.BeginTransactionAsync();

            try
            {
                var storeRepo = unitOfWork.GetRepository<Store,int>();
                var stockRepo = unitOfWork.GetRepository<Stock,(int, int)>();
                var transactionRepo = unitOfWork.GetRepository<StoresTransaction,int>();
                var transProductsRepo = unitOfWork.GetRepository<TransactionProducts,int>();

                var transferedProducts = new List<StoreTransactionProductsDto>();

                // -------------------------
                // 1) Validations
                // -------------------------

                if(dto.id == null || dto.id <= 0)
                    return Result<string>.Failure("معرف المخزن المحذوف غير موجود");

                if(dto.transferedToStoreDto <= 0)
                    return Result<string>.Failure("معرف المخزن المنقول إليه غير صالح");

                if(dto.id == dto.transferedToStoreDto)
                    return Result<string>.Failure("لا يمكن نقل المنتجات لنفس المخزن");

                var storeToDelete = await storeRepo
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == dto.id);

                if(storeToDelete == null)
                    return Result<string>.Failure("المخزن المحذوف غير موجود");

                // -------------------------
                // 2) الحصول على المخزون
                // -------------------------

                var sourceStocks = await stockRepo.GetQueryable()
                    .Where(s => s.StoreId == dto.id)
                    .ToListAsync();

                var destinationStocks = await stockRepo.GetQueryable()
                    .Where(s => s.StoreId == dto.transferedToStoreDto)
                    .ToListAsync();

                // -------------------------
                // 3) نقل المخزون  
                // -------------------------

                foreach(var sourceStock in sourceStocks)
                {
                    var destStock = destinationStocks
                        .FirstOrDefault(s => s.ProductId == sourceStock.ProductId);

                    // إضافة للـ Transaction Products
                    transferedProducts.Add(new StoreTransactionProductsDto
                    {
                        productId = sourceStock.ProductId,
                        quantity = sourceStock.Quantity
                    });

                    if(destStock != null)
                    {
                        destStock.Quantity += sourceStock.Quantity;
                        stockRepo.UpdateWithoutSaveAsync(destStock);
                    }
                    else
                    {
                        var newStock = new Stock
                        {
                            StoreId = dto.transferedToStoreDto,
                            ProductId = sourceStock.ProductId,
                            Quantity = sourceStock.Quantity
                        };

                        stockRepo.AddWithoutSaveAsync(newStock);
                    }

                    // حذف المخزون القديم
                    stockRepo.DeleteWithoutSaveAsync(sourceStock);
                }

                // -------------------------
                // 4) Soft Delete للمخزن
                // -------------------------

                storeToDelete.isDeleted = true;
                storeRepo.UpdateWithoutSaveAsync(storeToDelete);

                // -------------------------
                // 5) تسجيل عملية التحويل (Transaction)
                // -------------------------

                var transaction = new StoresTransaction
                {
                    SourceId = storeToDelete.Id,
                    DestenationId = dto.transferedToStoreDto,
                    MakeTransactionUser = dto.makeActionUser,
                    CreatedAt = DateTime.Now
                };

                await transactionRepo.AddWithoutSaveAsync(transaction);

                // لازم Save عشان نجيب الـ Id
                await unitOfWork.SaveChangesAsync();
                int transId = transaction.Id;

                // -------------------------
                // 6) إضافة تفاصيل التحويل
                // -------------------------

                foreach(var item in transferedProducts)
                {
                    var prodRow = new TransactionProducts
                    {
                        TransactionId = transId,
                        ProductId = item.productId,
                        Quantity = item.quantity
                    };

                    await transProductsRepo.AddWithoutSaveAsync(prodRow);
                }

                // -------------------------
                // 7) حفظ كل شيء مرة واحدة + Commit
                // -------------------------

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم حذف المخزن ونقل المخزون وتسجيل المعاملة بنجاح");
            }
            catch(Exception ex)
            {
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);
                return Result<string>.Failure("حدث خطأ أثناء عملية النقل: " + ex.Message);
            }
        }

        public async Task<Result<string>> EditStore(StoreDto dto)
        {
            try
            {
                var repo = unitOfWork.GetRepository<Store,int>();

                if(dto.id == null || dto.id <= 0)
                    return Result<string>.Failure("مخزن غير موجود");

                if(string.IsNullOrWhiteSpace(dto.storeName))
                    return Result<string>.Failure("يجب ان يكون هناك اسم ");

                var store = await repo.GetQueryable()
                                      .FirstOrDefaultAsync(s => s.Id == dto.id);

                if(store == null)
                    return Result<string>.Failure("مخزن غير موجود");

                bool nameExists = await repo.GetQueryable()
                    .AnyAsync(s => s.StoreName == dto.storeName && s.Id != dto.id);

                if(nameExists)
                    return Result<string>.Failure("المخزن موجود بالفعل");

                store.StoreName = dto.storeName;
                store.isDeleted = dto.isDeleted ?? store.isDeleted;

                await repo.UpdateAsync(store);
                await unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم الإضافة بنجاح ");
            }
            catch(Exception ex)
            {
                await unitOfWork.LogError(ex);
                return Result<string>.Failure(ex.Message);
            }
        }


        public async Task<ApiResponse<List<StoreDto>>> GetAllStores(StoreFilteration req)
        {
            try
            {
                var repo = unitOfWork.GetRepository<Store,int>();

                IQueryable<Store> query = repo.GetQueryable();

                if(!string.IsNullOrWhiteSpace(req.storeName))
                {
                    query = query.Where(s => s.StoreName.Contains(req.storeName));
                }
                if(req.isDeleted != null)
                {
                    query = query.Where(s => s.isDeleted == req.isDeleted);

                }


                if(req.page == null || req.pageSize == null)
                {
                    var allData = await query
                        .Select(s => new StoreDto
                        {
                            id = s.Id,
                            storeName = s.StoreName
                        })
                        .ToListAsync();

                    return new ApiResponse<List<StoreDto>>
                    {
                        data = allData,
                        totalCount = allData.Count,
                        page = 1,
                        pageSize = allData.Count,
                        totalPages = 1
                    };
                }


                int page = req.page ?? 1;
                int pageSize = req.pageSize ?? 10;

                int totalCount = await query.CountAsync();
                int totalPages = (int) Math.Ceiling(totalCount / (double) pageSize);

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new StoreDto
                    {
                        id = s.Id,
                        storeName = s.StoreName,
                        isDeleted = s.isDeleted,
                    })
                    .ToListAsync();

                return new ApiResponse<List<StoreDto>>
                {
                    data = items,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = totalPages
                };
            }
            catch(Exception ex)
            {
                await unitOfWork.LogError(ex);
                return null;
            }

        }
    }
}
