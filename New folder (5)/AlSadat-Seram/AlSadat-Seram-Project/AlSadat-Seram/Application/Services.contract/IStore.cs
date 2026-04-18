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
    public interface IStore
    {
        Task<ApiResponse<List<StoreDto>>> GetAllStores( StoreFilteration req);
        Task<Result<string>> AddNewStore(StoreDto dto);
        Task<Result<string>> EditStore(StoreDto dto);
        Task<Result<string>> DeleteStore(StoreDeleteDto dto);

    }
}
