using AlSadatSeram.Services.contract;
using Application.DTOs.CityDtos;
using Application.DTOs.GovernrateDtos;
using Application.Services.contract;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CityService : ICityContract
    {
        private readonly IUnitOfWork unitOfWork;

        public CityService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task AddNewCity(CityDto city)
        {
            var AddedCity = new City()
            {
                Name = city.cityName,
                GovernrateId=(int)city.governrateId

            };


            await unitOfWork.GetRepository<City, int>().AddAsync(AddedCity);

        }

        public async Task EditCity(CityDto city)
        {
            var UpdatedCity = await unitOfWork.GetRepository<City, int>().FindAsync(c => c.Id == city.id);

            UpdatedCity.Name = city.cityName;
            UpdatedCity.GovernrateId=(int)city.governrateId;
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<ApiResponse<List<CityDto>>> GetAllCities(CityFilteration req)
        {
            IQueryable<City> query = unitOfWork.GetRepository<City, int>()
                                   .GetQueryable()
                                   .Include(c => c.Governrate)
                                   .AsQueryable();


            if (!string.IsNullOrWhiteSpace(req.cityName))
            {
                query = query.Where(g => g.Name.ToLower().Contains(req.cityName.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(req.governrateName))
            {
                query = query.Where(g => g.Governrate.Name.Contains(req.governrateName.ToLower()));
            }
            var totalCount = await query.CountAsync();

            List<CityDto> cities;

            int page = req.page ?? 1;
            int pageSize = req.pageSize ?? 0;
            int totalPages = 1;

            if (req.page.HasValue && req.pageSize.HasValue)
            {
                cities = await query
                    .OrderBy(g => g.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new CityDto
                    {
                        id = g.Id,
                        cityName = g.Name,
                        governrateName=g.Governrate.Name,
                        governrateId=g.Id,
                    })
                    .ToListAsync();

                totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            }
            else
            {
                cities = await query
                    .OrderBy(g => g.Id)
                    .Select(g => new CityDto
                    {

                        id = g.Id,
                        cityName = g.Name,
                        governrateName = g.Governrate.Name,
                        governrateId = g.Id,
                    })
                    .ToListAsync();

                page = 1;
                pageSize = totalCount;
                totalPages = 1;
            }

            return new ApiResponse<List<CityDto>>
            {
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = totalPages,
                data = cities
            };
        }

        public async Task<List<City>> GetAsync(CityDto dto)
        {
           var  CheckCities = await unitOfWork.GetRepository<City, int>().GetAsync(x => x.Name == dto.cityName && x.GovernrateId==dto.governrateId);
            return CheckCities.ToList();
        }

        public async Task<CityDto> GetByID(int id)
        {
            var model = await unitOfWork.GetRepository<City, int>().GetQueryable().Include(c=>c.Governrate).FirstOrDefaultAsync(c=>c.Id==id);
            if (model != null)
            {
                var dto = new CityDto()
                {
                    id = model.Id,
                    cityName = model.Name,
                    governrateName = model.Governrate.Name,
                    governrateId = model.Id,
                };
                return dto;
            }
            return null;
        }
    }
}
