using Application.DTOs;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Commonitems;
using Domain.UnitOfWork.Contract;
using System.Net;

namespace Infrastructure.Services.StoreTransactionServices.Validators
{
    /// <summary>
    /// Validates store-transfer requests. Performs <b>read-only</b> aggregate checks
    /// (existence of source/destination warehouses) and structural input checks.
    /// <para>
    /// This validator does NOT touch stock quantities — sufficient-stock validation
    /// happens later in the service, inside the same transaction that mutates the
    /// stock rows, so the read-and-mutate window is as small as possible.
    /// </para>
    /// </summary>
    public sealed class StoreTransactionValidator : IStoreTransactionValidator
    {
        private readonly IUnitOfWork _unitOfWork;

        public StoreTransactionValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ValidateAsync(StoreTransactionDto dto)
        {
            // ---- 1) Structural checks ----
            if (dto is null)
                return Result<bool>.Failure("جميع البيانات مطلوبة", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(dto.makeTransactionUser))
                return Result<bool>.Failure("لم يتم تسجيل المستخدم", HttpStatusCode.BadRequest);

            if (dto.sourceId is null or <= 0)
                return Result<bool>.Failure("المخزن المصدر مطلوب", HttpStatusCode.BadRequest);

            if (dto.destenationId is null or <= 0)
                return Result<bool>.Failure("المخزن المستقبل مطلوب", HttpStatusCode.BadRequest);

            if (dto.sourceId == dto.destenationId)
                return Result<bool>.Failure(
                    "المخزن المصدر والمخزن المستقبل يجب أن يكونا مختلفين",
                    HttpStatusCode.BadRequest);

            if (dto.transactionProducts is null || dto.transactionProducts.Count == 0)
                return Result<bool>.Failure("لم يتم اختيار المنتجات", HttpStatusCode.BadRequest);

            // ---- 2) Per-line checks ----
            foreach (var line in dto.transactionProducts)
            {
                if (line.productId <= 0)
                    return Result<bool>.Failure(
                        "أحد المنتجات يحمل معرّف غير صالح",
                        HttpStatusCode.BadRequest);

                if (line.quantity <= 0)
                    return Result<bool>.Failure(
                        "كمية التحويل لكل منتج يجب أن تكون أكبر من صفر",
                        HttpStatusCode.BadRequest);
            }

            // Reject duplicate product lines — cleaner than silently summing them server-side.
            var duplicates = dto.transactionProducts
                .GroupBy(p => p.productId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count > 0)
                return Result<bool>.Failure(
                    "يوجد منتج مكرر في قائمة التحويل — يجب أن يظهر كل منتج مرة واحدة فقط",
                    HttpStatusCode.BadRequest);

            // ---- 3) Aggregate-existence checks ----
            var storeRepo = _unitOfWork.GetRepository<Store, int>();

            var sourceStore = await storeRepo.GetByIdAsync(dto.sourceId!.Value);
            if (sourceStore is null)
                return Result<bool>.Failure("المخزن المصدر غير موجود", HttpStatusCode.NotFound);

            var destinationStore = await storeRepo.GetByIdAsync(dto.destenationId!.Value);
            if (destinationStore is null)
                return Result<bool>.Failure("المخزن المستقبل غير موجود", HttpStatusCode.NotFound);

            return Result<bool>.Success(true);
        }
    }
}
