using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.GovernrateDtos;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract
{
    public interface IStockService
    {
        Task<ApiResponse<List<StockDto>>> GetAllStocks(StockFilterations req);

        Task<Result<StockDto>> GetByStoreID(int id);

        Task<Result<ProductStockDto>> GetByProductID(int productID);




    }
}
