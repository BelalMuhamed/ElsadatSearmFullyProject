using AlSadatSeram.Services.contract;
using Application.DTOs.CityDtos;
using Application.DTOs.GovernrateDtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract
{
    public interface ICityContract
    {
        Task<ApiResponse<List<CityDto>>> GetAllCities(CityFilteration req);
        Task EditCity(CityDto city);
        Task AddNewCity(CityDto city);
        Task<List<City>> GetAsync(CityDto dto);
        Task<CityDto> GetByID(int id);

    }
}
