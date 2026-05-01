using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.CityDtos;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class StockService : IStockService
    {
        private readonly IUnitOfWork unitOfWork;

        public StockService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ApiResponse<List<StockDto>>> GetAllStocks(StockFilterations req)
        {
            try
            {
                var productRepo = unitOfWork.GetRepository<Products, int>();
                var storeRepo = unitOfWork.GetRepository<Store, int>();
                var stockRepo = unitOfWork.GetRepository<Stock, int>();

                var allProducts = await productRepo
                    .GetQueryable()
                    
                    .ToListAsync();

                var storesQuery = storeRepo.GetQueryable();

                if (!string.IsNullOrWhiteSpace(req.storeName))
                    storesQuery = storesQuery.Where(s => s.StoreName.Contains(req.storeName));

                int totalCount = await storesQuery.CountAsync();
                int totalPages = (int)Math.Ceiling(totalCount / (double)req.pageSize);

                var stores = await storesQuery
                    .Skip((req.page - 1) * req.pageSize)
                    .Take(req.pageSize)
                    .ToListAsync();

                var allStocks = await stockRepo.GetQueryable().ToListAsync();

                var result = new List<StockDto>();

                foreach (var store in stores)
                {
                    var storeStockItems = new List<StockProducts>();

                    foreach (var product in allProducts)
                    {
                        var stockRecord = allStocks
                            .FirstOrDefault(s => s.StoreId == store.Id && s.ProductId == product.Id);

                        decimal qty = stockRecord?.Quantity ?? 0;

                        bool isDeleted = product.IsDeleted;



                        storeStockItems.Add(new StockProducts
                        {
                            productId = product.Id,
                            productName = product.Name,
                            quantity = qty,
                            isDeleted = isDeleted,
                            lowQuantity = product.TheSmallestPossibleQuantity,
                           
                        });
                    }

                    result.Add(new StockDto
                    {
                        storeID = store.Id,
                        storeName = store.StoreName,
                        storeStocks = storeStockItems,
                        isDeleted=store.isDeleted
                    });
                }

                return new ApiResponse<List<StockDto>>
                {
                    totalCount = totalCount,
                    page = req.page,
                    pageSize = req.pageSize,
                    totalPages = totalPages,
                    data = result
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<StockDto>>
                {
                    totalCount = 0,
                    page = req.page,
                    pageSize = req.pageSize,
                    totalPages = 0,
                    data = null
                };
            }
        }

        public async Task<Result<ProductStockDto>> GetByProductID(int productID)
        {
            try
            {
                var productRepo = unitOfWork.GetRepository<Products, int>();
                var stockRepo = unitOfWork.GetRepository<Stock, int>();

               
                var productData = await productRepo
                    .GetQueryable()
                    .Where(p => p.Id == productID)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.IsDeleted,
                       
                    })
                    .FirstOrDefaultAsync();

                if (productData == null)
                    return Result<ProductStockDto>
                        .Failure("المنتج غير موجود", HttpStatusCode.NotFound);

                
                var stockPerStores = await stockRepo
                    .GetQueryable()
                    .Where(s => s.ProductId == productID && s.Store.isDeleted == false)
                    .Select(s => new ProductStockPerStoreDto
                    {
                        storeId = s.StoreId,
                        storeName = s.Store.StoreName,
                        isStoreDeleted = s.Store.isDeleted,
                        avaliableQuantity = s.Quantity,
                        withdrawnQuantity = 0
                    })
                    .ToListAsync();

                
                var dto = new ProductStockDto
                {
                    productId = productData.Id,
                    productName = productData.Name,
                    isProductDeleted = productData.IsDeleted,
                    
                    stocks = stockPerStores
                };

                return Result<ProductStockDto>
                    .Success(dto, "Success", HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Result<ProductStockDto>
                    .Failure(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        public async Task<Result<StockDto>> GetByStoreID(int id)
        {
            try
            {
                var storeRepo = unitOfWork.GetRepository<Store, int>();
                var stockRepo = unitOfWork.GetRepository<Stock, int>();
                var productRepo = unitOfWork.GetRepository<Products, int>();

                var store = await storeRepo.FindAsync(s => s.Id == id);
                if (store == null)
                    return Result<StockDto>.Failure("المخزن غير موجود ", HttpStatusCode.NotFound);

                var storeStocks = await stockRepo
                    .GetQueryable()
                    .Where(s => s.StoreId == id)
                    .Include(s => s.Product)
                       
                    .ToListAsync();

                if (storeStocks == null || !storeStocks.Any())
                    return Result<StockDto>.Failure("لا يوجد اي منتجات داخل هذا المخزن ", HttpStatusCode.BadRequest);

                var allProducts = await productRepo
                    .GetQueryable()
                   
                    .ToListAsync();

                var resultProducts = new List<StockProducts>();

                foreach (var stock in storeStocks)
                {
                    var product = stock.Product; 
                    bool isDeleted = product.IsDeleted ;

                    resultProducts.Add(new StockProducts
                    {
                        productId = product.Id,
                        productName = product.Name,
                        quantity = stock.Quantity,
                        isDeleted = isDeleted,
                        lowQuantity = product.TheSmallestPossibleQuantity,           
                      
                    });
                }

                var dto = new StockDto
                {
                    storeID = store.Id,
                    storeName = store.StoreName,
                    storeStocks = resultProducts
                };

                return Result<StockDto>.Success(dto, "Success", HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return Result<StockDto>.Failure(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <inheritdoc />
        public async Task<Result<List<StoreStockProductDto>>> GetAvailableByStoreAsync(int storeId)
        {
            if (storeId <= 0)
                return Result<List<StoreStockProductDto>>.Failure(
                    "معرّف المخزن غير صالح", HttpStatusCode.BadRequest);

            try
            {
                // Confirm the store exists (and is not soft-deleted) before returning rows.
                var store = await unitOfWork.GetRepository<Store, int>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == storeId);

                if (store is null)
                    return Result<List<StoreStockProductDto>>.Failure(
                        "المخزن غير موجود", HttpStatusCode.NotFound);

                // Single projection — Include kept narrow on purpose.
                var rows = await unitOfWork.GetRepository<Stock, (int, int)>()
                    .GetQueryable()
                    .Include(s => s.Product)
                    .Where(s =>
                        s.StoreId == storeId &&
                        s.Quantity > 0 &&
                        s.Product != null &&
                        !s.Product.IsDeleted)
                    .OrderBy(s => s.Product!.Name)
                    .Select(s => new StoreStockProductDto
                    {
                        productId = s.ProductId,
                        productName = s.Product!.Name,
                        productCode = s.Product.productCode,
                        availableQuantity = s.Quantity,
                        avgCost = s.AvgCost
                    })
                    .ToListAsync();

                return Result<List<StoreStockProductDto>>.Success(rows);
            }
            catch (Exception ex)
            {
                await unitOfWork.LogError(ex);
                return Result<List<StoreStockProductDto>>.Failure(
                    "حدث خطأ أثناء تحميل مخزون المخزن",
                    HttpStatusCode.InternalServerError);
            }
        }

    }
}
