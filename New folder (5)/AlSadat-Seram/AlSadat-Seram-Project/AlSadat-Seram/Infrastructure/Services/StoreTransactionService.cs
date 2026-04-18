using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Transactions;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class StoreTransactionService : IStoreTransactionService
    {
        private readonly IUnitOfWork unitOfWork;

        public StoreTransactionService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        //public async Task<Result<string>> AddNewTransaction(StoreTransactionDto dto)
        //{
        //    // بدء Transaction
        //    await unitOfWork.BeginTransactionAsync();

        //    try
        //    {
        //        // ----------------------
        //        // 1) Validations
        //        // ----------------------
        //        if (dto == null)
        //            return Result<string>.Failure("جميع البيانات مطلوبة !");

        //        if (string.IsNullOrWhiteSpace(dto.makeTransactionUser))
        //            return Result<string>.Failure("خطأ لم يتم تسجيل مستخدم ");

        //        if (dto.sourceId == null || dto.sourceId <= 0)
        //            return Result<string>.Failure("المخزن المصدر لم يضاف ");

        //        if (dto.destenationId == null || dto.destenationId <= 0)
        //            return Result<string>.Failure("المخزن المستقبل لم يضاف ");

        //        if (dto.sourceId == dto.destenationId)
        //            return Result<string>.Failure("المصدر والمستقبل يجب ان يكونوا مختلفين ");

        //        if (dto.transactionProducts == null || dto.transactionProducts.Count == 0)
        //            return Result<string>.Failure("لم يتم اختيار المنتجات ");

        //        // ----------------------
        //        // 2) جلب المخازن
        //        // ----------------------
        //        var sourceStore = await unitOfWork.GetRepository<Store, int>().GetByIdAsync((int)dto.sourceId);
        //        if (sourceStore == null)
        //            return Result<string>.Failure("المخزن المصدر غير موجود");

        //        var destStore = await unitOfWork.GetRepository<Store, int>().GetByIdAsync((int)dto.destenationId);
        //        if (destStore == null)
        //            return Result<string>.Failure("المخزن المستقبل غير موجود ");

        //        var stockRepo = unitOfWork.GetRepository<Stock, (int, int)>();
        //        var sourceStocks = await stockRepo.GetQueryable()
        //            .Where(s => s.StoreId == dto.sourceId)
        //            .ToListAsync();

        //        var destStocks = await stockRepo.GetQueryable()
        //            .Where(s => s.StoreId == dto.destenationId)
        //            .ToListAsync();

        //        // ----------------------
        //        // 3) إنشاء Transaction Master
        //        // ----------------------
        //        var transaction = new StoresTransaction
        //        {
        //            SourceId = dto.sourceId.Value,
        //            DestenationId = dto.destenationId.Value,
        //            MakeTransactionUser = dto.makeTransactionUser,
        //            CreatedAt = DateTime.UtcNow
        //        };

        //        var transRepo = unitOfWork.GetRepository<StoresTransaction, int>();
        //        await transRepo.AddWithoutSaveAsync(transaction);

        //        // حفظ مؤقت للحصول على Id
        //        await unitOfWork.SaveChangesAsync();
        //        int transactionId = transaction.Id;

        //        // ----------------------
        //        // 4) تحديث المخزون
        //        // ----------------------
        //        foreach (var item in dto.transactionProducts)
        //        {
        //            var sourceStock = sourceStocks.FirstOrDefault(s => s.ProductId == item.productId);
        //            if (sourceStock == null || sourceStock.Quantity < item.quantity)
        //                return Result<string>.Failure($"الكمية المطلوبة للمنتج {item.productId} غير كافية في المخزن المصدر");

        //            // خصم من المصدر
        //            sourceStock.Quantity -= item.quantity;
        //             stockRepo.UpdateWithoutSaveAsync(sourceStock);

        //            // إضافة أو تحديث في المخزن المستقبل
        //            var destStock = destStocks.FirstOrDefault(s => s.ProductId == item.productId);
        //            if (destStock != null)
        //            {
        //                destStock.Quantity += item.quantity;
        //                 stockRepo.UpdateWithoutSaveAsync(destStock);
        //            }
        //            else
        //            {
        //                var newStock = new Stock
        //                {
        //                    StoreId = dto.destenationId.Value,
        //                    ProductId = item.productId,
        //                    Quantity = item.quantity
        //                };
        //                await stockRepo.AddWithoutSaveAsync(newStock);
        //            }
        //        }

        //        // ----------------------
        //        // 5) إضافة تفاصيل TransactionProducts
        //        // ----------------------
        //        var transProductsRepo = unitOfWork.GetRepository<TransactionProducts, int>();
        //        foreach (var item in dto.transactionProducts)
        //        {
        //            var productRow = new TransactionProducts
        //            {
        //                TransactionId = transactionId,
        //                ProductId = item.productId,
        //                Quantity = item.quantity
        //            };

        //            await transProductsRepo.AddWithoutSaveAsync(productRow);
        //        }

        //        // ----------------------
        //        // 6) حفظ كل شيء + Commit
        //        // ----------------------
        //        await unitOfWork.SaveChangesAsync();
        //        await unitOfWork.CommitAsync();

        //        return Result<string>.Success("تم التحويل بنجاح");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback عند أي خطأ
        //        await unitOfWork.RollbackAsync();
        //        await unitOfWork.LogError(ex);

        //        return Result<string>.Failure("حدث خطأ أثناء حفظ التحويل: " + ex.Message);
        //    }
        //}
        public async Task<Result<string>> AddNewTransaction(StoreTransactionDto dto)
        {
            // بدء Transaction
            await unitOfWork.BeginTransactionAsync();

            try
            {
                // ----------------------
                // 1) Validations
                // ----------------------
                if (dto == null)
                    return Result<string>.Failure("جميع البيانات مطلوبة !");

                if (string.IsNullOrWhiteSpace(dto.makeTransactionUser))
                    return Result<string>.Failure("خطأ لم يتم تسجيل مستخدم ");

                if (dto.sourceId == null || dto.sourceId <= 0)
                    return Result<string>.Failure("المخزن المصدر لم يضاف ");

                if (dto.destenationId == null || dto.destenationId <= 0)
                    return Result<string>.Failure("المخزن المستقبل لم يضاف ");

                if (dto.sourceId == dto.destenationId)
                    return Result<string>.Failure("المصدر والمستقبل يجب ان يكونوا مختلفين ");

                if (dto.transactionProducts == null || dto.transactionProducts.Count == 0)
                    return Result<string>.Failure("لم يتم اختيار المنتجات ");

                // ----------------------
                // 2) جلب المخازن والمخزون
                // ----------------------
                var sourceStore = await unitOfWork.GetRepository<Store, int>().GetByIdAsync((int)dto.sourceId);
                if (sourceStore == null)
                    return Result<string>.Failure("المخزن المصدر غير موجود");

                var destStore = await unitOfWork.GetRepository<Store, int>().GetByIdAsync((int)dto.destenationId);
                if (destStore == null)
                    return Result<string>.Failure("المخزن المستقبل غير موجود ");

                var stockRepo = unitOfWork.GetRepository<Stock, (int, int)>();
                var sourceStocks = await stockRepo.GetQueryable()
                    .Where(s => s.StoreId == dto.sourceId)
                    .ToListAsync();

                var destStocks = await stockRepo.GetQueryable()
                    .Where(s => s.StoreId == dto.destenationId)
                    .ToListAsync();

                // ----------------------
                // 3) إنشاء Transaction Master
                // ----------------------
                var transaction = new StoresTransaction
                {
                    SourceId = dto.sourceId.Value,
                    DestenationId = dto.destenationId.Value,
                    MakeTransactionUser = dto.makeTransactionUser,
                    CreatedAt = DateTime.UtcNow
                };

                var transRepo = unitOfWork.GetRepository<StoresTransaction, int>();
                await transRepo.AddWithoutSaveAsync(transaction);

                // حفظ مؤقت للحصول على Id
                await unitOfWork.SaveChangesAsync();
                int transactionId = transaction.Id;

                // ----------------------
                // 4) تحديث المخزون مع حساب AvgCost
                // ----------------------
                foreach (var item in dto.transactionProducts)
                {
                    var sourceStock = sourceStocks.FirstOrDefault(s => s.ProductId == item.productId);
                    if (sourceStock == null || sourceStock.Quantity < item.quantity)
                        return Result<string>.Failure($"الكمية المطلوبة للمنتج {item.productId} غير كافية في المخزن المصدر");

                    // خصم من المصدر
                    sourceStock.Quantity -= item.quantity;
                     stockRepo.UpdateWithoutSaveAsync(sourceStock);

                    // إضافة أو تحديث في المخزن المستقبل مع متوسط السعر
                    var destStock = destStocks.FirstOrDefault(s => s.ProductId == item.productId);
                    if (destStock != null)
                    {
                        decimal totalQuantity = destStock.Quantity + item.quantity;
                        decimal totalCost = (destStock.Quantity * destStock.AvgCost) + (item.quantity * sourceStock.AvgCost);
                        destStock.AvgCost = totalCost / totalQuantity;

                        destStock.Quantity += item.quantity;
                         stockRepo.UpdateWithoutSaveAsync(destStock);
                    }
                    else
                    {
                        var newStock = new Stock
                        {
                            StoreId = dto.destenationId.Value,
                            ProductId = item.productId,
                            Quantity = item.quantity,
                            AvgCost = sourceStock.AvgCost // نقل AvgCost من المصدر
                        };
                        await stockRepo.AddWithoutSaveAsync(newStock);
                    }
                }

                // ----------------------
                // 5) إضافة تفاصيل TransactionProducts
                // ----------------------
                var transProductsRepo = unitOfWork.GetRepository<TransactionProducts, int>();
                foreach (var item in dto.transactionProducts)
                {
                    var productRow = new TransactionProducts
                    {
                        TransactionId = transactionId,
                        ProductId = item.productId,
                        Quantity = item.quantity
                    };
                    await transProductsRepo.AddWithoutSaveAsync(productRow);
                }

                // ----------------------
                // 6) حفظ كل شيء + Commit
                // ----------------------
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitAsync();

                return Result<string>.Success("تم التحويل بنجاح");
            }
            catch (Exception ex)
            {
                // Rollback عند أي خطأ
                await unitOfWork.RollbackAsync();
                await unitOfWork.LogError(ex);

                return Result<string>.Failure("حدث خطأ أثناء حفظ التحويل: " + ex.Message);
            }
        }


        public async Task<ApiResponse<List<StoreTransactionDto>>> GetAllTransacctions(StoreTransactionFilters req)
        {
            try
            {
                int page = req.page ?? 1;
                int pageSize = req.pageSize ?? 10;

                var repo = unitOfWork.GetRepository<StoresTransaction, int>();

                var query = repo.GetQueryable()
                                .Include(x => x.Source)
                                .Include(x => x.Destenation)
                                .AsQueryable();

                // ---------------------------
                // 1) Apply Filters
                // ---------------------------

                if (!string.IsNullOrWhiteSpace(req.sourceName))
                {
                    string name = req.sourceName.Trim();
                    query = query.Where(x => x.Source.StoreName.Contains(name));
                }

                if (!string.IsNullOrWhiteSpace(req.destenationName))
                {
                    string name = req.destenationName.Trim();
                    query = query.Where(x => x.Destenation.StoreName.Contains(name));
                }

                if (req.createdAt != default(DateTime))
                {
                    DateTime nextDay = req.createdAt.Date.AddDays(1);

                    query = query.Where(x => x.CreatedAt.Date == nextDay);
                }


                // ---------------------------
                // 2) Count BEFORE pagination
                // ---------------------------
                int totalCount = await query.CountAsync();

                // ---------------------------
                // 3) Apply pagination
                // ---------------------------
                var data = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new StoreTransactionDto
                    {
                        id = x.Id,
                        sourceId = x.SourceId,
                        destenationId = x.DestenationId,
                        sourceName = x.Source.StoreName,
                        destenationName = x.Destenation.StoreName,
                        makeTransactionUser = x.MakeTransactionUser,
                        createdAt = x.CreatedAt
                    })
                    .ToListAsync();

                // ---------------------------
                // 4) Calculate total pages
                // ---------------------------
                int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // ---------------------------
                // 5) Build response
                // ---------------------------
                return new ApiResponse<List<StoreTransactionDto>>
                {
                    page = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = totalPages,
                    data = data
                };
            }
            catch (Exception ex)
            {
                await unitOfWork.LogError(ex);

                return null;
            }
        }

        public async Task<List<StoreTransactionProductsDto>> GetTransactionProductsById(int id)
        {
            var transProductsRepo = unitOfWork.GetRepository<TransactionProducts, int>();

            var products = await transProductsRepo
                .GetQueryable().Include(x=>x.Product)
                .Where(tp => tp.TransactionId == id)
                .Select(tp => new StoreTransactionProductsDto
                {
                    transactionId = tp.TransactionId,
                    productId = tp.ProductId,
                    quantity = tp.Quantity,
                    productName=tp.Product.Name
                })
                .ToListAsync();

            return products;
        }


    }
}
