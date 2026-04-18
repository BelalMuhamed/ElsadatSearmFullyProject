using AlSadatSeram.Services.contract;
using Application.DTOs.GovernrateDtos;
using Application.DTOs.ProductsDtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract
{
    public interface IGovernrateCaontract
    {
        Task<ApiResponse<List<GovernrateDto>>> GetAllGovernrates(GovernrateFilteration req);
        Task EditGovernrate(GovernrateDto governrate);
        Task AddNewGovernrate(GovernrateDto governrate);
        Task<List<Governrate>> GetAsync(string governrateName);
        Task<GovernrateDto> GetByID(int id);
    }
}
