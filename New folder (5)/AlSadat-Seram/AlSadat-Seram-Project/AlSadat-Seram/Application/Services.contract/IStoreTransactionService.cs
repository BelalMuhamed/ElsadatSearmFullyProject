using AlSadatSeram.Services.contract;
using Application.DTOs;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract
{
    public interface IStoreTransactionService
    {
        Task<ApiResponse<List<StoreTransactionDto>>> GetAllTransacctions(StoreTransactionFilters req);
        Task<Result<string>> AddNewTransaction(StoreTransactionDto dto);
        Task<List<StoreTransactionProductsDto>> GetTransactionProductsById(int id );
    }
}
