using AlSadatSeram.Services.contract;
using Application.Common;
using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Transactions;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.StoreTransactionServices
{
    /// <summary>
    /// Application service for store-to-store stock transfers.
    /// <para>Orchestration only:</para>
    /// <list type="number">
    ///   <item>delegate validation to <see cref="IStoreTransactionValidator"/>,</item>
    ///   <item>load the affected stock rows once,</item>
    ///   <item>delegate cost math to <see cref="StockMovementCalculator"/>,</item>
    ///   <item>persist master + details + stock mutations in a single DB transaction.</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Onion alignment: depends on Application + Domain only. EF Core is used here
    /// (Infrastructure layer) by design — same pattern as <c>SupplierService</c>.
    /// </remarks>
    public sealed class StoreTransactionService : IStoreTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStoreTransactionValidator _validator;

        public StoreTransactionService(
            IUnitOfWork unitOfWork,
            IStoreTransactionValidator validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        // ====================================================================
        // CREATE
        // ====================================================================
        /// <inheritdoc />
        public async Task<Result<string>> AddNewTransaction(StoreTransactionDto dto)
        {
            // ---- 1) Validate first — no DB transaction yet, no rollback needed.
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsSuccess)
                return Result<string>.Failure(validationResult.Message!, validationResult.StatusCode);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ---- 2) Load stock rows for both warehouses in two queries.
                //         Composite-key repo per existing convention.
                var stockRepo = _unitOfWork.GetRepository<Stock, (int, int)>();

                var sourceStocks = await stockRepo.GetQueryable()
                    .Where(s => s.StoreId == dto.sourceId!.Value)
                    .ToListAsync();

                var destinationStocks = await stockRepo.GetQueryable()
                    .Where(s => s.StoreId == dto.destenationId!.Value)
                    .ToListAsync();

                // ---- 3) Sufficient-stock check — done here so it sits inside the
                //         same transaction as the mutations, avoiding TOCTOU drift.
                var stockCheck = EnsureSourceHasEnoughStock(dto, sourceStocks);
                if (!stockCheck.IsSuccess)
                {
                    await _unitOfWork.RollbackAsync();
                    return stockCheck;
                }

                // ---- 4) Insert transaction master and obtain its Id.
                var transaction = new StoresTransaction
                {
                    SourceId = dto.sourceId!.Value,
                    DestenationId = dto.destenationId!.Value,
                    MakeTransactionUser = dto.makeTransactionUser,
                    CreatedAt = DateTime.UtcNow
                };

                var transRepo = _unitOfWork.GetRepository<StoresTransaction, int>();
                await transRepo.AddWithoutSaveAsync(transaction);
                await _unitOfWork.SaveChangesAsync();

                // ---- 5) Mutate stock + insert detail rows.
                await ApplyStockMovementsAsync(dto, sourceStocks, destinationStocks, stockRepo);
                await AppendTransactionDetailsAsync(dto, transaction.Id);

                // ---- 6) Persist + commit.
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return Result<string>.Success(
                    "تم تنفيذ التحويل بنجاح",
                    HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                await _unitOfWork.LogError(ex);
                return Result<string>.Failure(
                    "حدث خطأ غير متوقع أثناء تنفيذ التحويل",
                    HttpStatusCode.InternalServerError);
            }
        }

        // ====================================================================
        // QUERIES
        // ====================================================================
        /// <inheritdoc />
        public async Task<ApiResponse<List<StoreTransactionDto>>> GetAllTransacctions(
            StoreTransactionFilters req)
        {
            // NOTE: Signature kept (return type, method name) to avoid breaking the
            // existing controller. A nicer evolution would be Result<ApiResponse<...>>,
            // but that would ripple into other consumers — out of scope.
            try
            {
                var page = req.page ?? 1;
                var pageSize = req.pageSize ?? 10;

                var query = _unitOfWork.GetRepository<StoresTransaction, int>()
                    .GetQueryable()
                    .Include(t => t.Source)
                    .Include(t => t.Destenation)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(req.sourceName))
                    query = query.Where(t =>
                        t.Source != null && t.Source.StoreName.Contains(req.sourceName));

                if (!string.IsNullOrWhiteSpace(req.destenationName))
                    query = query.Where(t =>
                        t.Destenation != null && t.Destenation.StoreName.Contains(req.destenationName));

                if (req.createdAt != default)
                {
                    var day = req.createdAt.Date;
                    query = query.Where(t => t.CreatedAt.Date == day);
                }

                var totalCount = await query.CountAsync();
                var totalPages = pageSize > 0
                    ? (int)Math.Ceiling(totalCount / (double)pageSize)
                    : 1;

                var data = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new StoreTransactionDto
                    {
                        id = t.Id,
                        sourceId = t.SourceId,
                        destenationId = t.DestenationId,
                        sourceName = t.Source != null ? t.Source.StoreName : null,
                        destenationName = t.Destenation != null ? t.Destenation.StoreName : null,
                        makeTransactionUser = t.MakeTransactionUser,
                        createdAt = t.CreatedAt
                    })
                    .ToListAsync();

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
                await _unitOfWork.LogError(ex);
                return null!; // Controller treats null as a server error — preserved for compatibility.
            }
        }

        /// <inheritdoc />
        public async Task<List<StoreTransactionProductsDto>> GetTransactionProductsById(int id)
        {
            // Returning an empty list (rather than null) so the controller can
            // reliably distinguish "no items" from "DB failure".
            var transProductsRepo = _unitOfWork.GetRepository<TransactionProducts, int>();

            return await transProductsRepo
                .GetQueryable()
                .Include(x => x.Product)
                .Where(tp => tp.TransactionId == id)
                .Select(tp => new StoreTransactionProductsDto
                {
                    transactionId = tp.TransactionId,
                    productId = tp.ProductId,
                    productName = tp.Product.Name,
                    quantity = tp.Quantity
                })
                .ToListAsync();
        }

        // ====================================================================
        // PRIVATE HELPERS — small, focused, intention-revealing names
        // ====================================================================

        /// <summary>
        /// Ensures every requested line has a matching stock row at the source
        /// warehouse with a sufficient on-hand quantity.
        /// </summary>
        private static Result<string> EnsureSourceHasEnoughStock(
            StoreTransactionDto dto,
            IReadOnlyList<Stock> sourceStocks)
        {
            foreach (var line in dto.transactionProducts!)
            {
                var sourceStock = sourceStocks.FirstOrDefault(s => s.ProductId == line.productId);

                if (sourceStock is null)
                    return Result<string>.Failure(
                        $"المنتج رقم {line.productId} غير موجود في المخزن المصدر",
                        HttpStatusCode.BadRequest);

                if (sourceStock.Quantity < line.quantity)
                    return Result<string>.Failure(
                        $"الكمية المطلوبة للمنتج رقم {line.productId} غير كافية في المخزن المصدر " +
                        $"(المتاح: {sourceStock.Quantity:0.##})",
                        HttpStatusCode.BadRequest);
            }

            return Result<string>.Success(string.Empty);
        }

        /// <summary>
        /// Applies the source-decrement and destination-increment with a fresh
        /// weighted-average cost calculated by <see cref="StockMovementCalculator"/>.
        /// </summary>
        private static async Task ApplyStockMovementsAsync(
            StoreTransactionDto dto,
            List<Stock> sourceStocks,
            List<Stock> destinationStocks,
            Domain.Repositories.contract.IGenericRepository<Stock, (int, int)> stockRepo)
        {
            foreach (var line in dto.transactionProducts!)
            {
                // Decrement source — already validated as sufficient.
                var sourceStock = sourceStocks.First(s => s.ProductId == line.productId);
                sourceStock.Quantity -= line.quantity;
                stockRepo.UpdateWithoutSaveAsync(sourceStock);

                // Increment destination (or create if first arrival).
                var destinationStock = destinationStocks
                    .FirstOrDefault(s => s.ProductId == line.productId);

                if (destinationStock is not null)
                {
                    destinationStock.AvgCost = StockMovementCalculator.ComputeNewDestinationAvgCost(
                        destinationCurrentQuantity: destinationStock.Quantity,
                        destinationCurrentAvgCost: destinationStock.AvgCost,
                        transferQuantity: line.quantity,
                        sourceAvgCost: sourceStock.AvgCost);

                    destinationStock.Quantity += line.quantity;
                    stockRepo.UpdateWithoutSaveAsync(destinationStock);
                }
                else
                {
                    var newRow = new Stock
                    {
                        StoreId = dto.destenationId!.Value,
                        ProductId = line.productId,
                        Quantity = line.quantity,
                        AvgCost = sourceStock.AvgCost
                    };
                    await stockRepo.AddWithoutSaveAsync(newRow);
                }
            }
        }

        /// <summary>
        /// Inserts one TransactionProducts row per requested line.
        /// </summary>
        private async Task AppendTransactionDetailsAsync(
            StoreTransactionDto dto,
            int transactionId)
        {
            var detailsRepo = _unitOfWork.GetRepository<TransactionProducts, int>();

            foreach (var line in dto.transactionProducts!)
            {
                var detailRow = new TransactionProducts
                {
                    TransactionId = transactionId,
                    ProductId = line.productId,
                    Quantity = line.quantity
                };
                await detailsRepo.AddWithoutSaveAsync(detailRow);
            }
        }
    }
}
