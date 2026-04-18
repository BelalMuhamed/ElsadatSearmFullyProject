using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.CityDtos;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract
{
    public interface IDistributorsAndMerchantsService
    {
        Task<ApiResponse<List<DistributorsAndMerchantsAndAgentsDto>>> GetAllDistributorsAndMerchants(DistributorsAndMerchantsFilters req);
        Task<Result<string>> AddNewDistributorOrMerchant(DistributorsAndMerchantsAndAgentsDto dto);
        Task<Result<string>> EditDistributorOrMerchant(DistributorsAndMerchantsAndAgentsDto dto);
        Task<Result<DistributorsAndMerchantsAndAgentsDto>> GetDistributorOrMerchantById(string userId);

    }
}
