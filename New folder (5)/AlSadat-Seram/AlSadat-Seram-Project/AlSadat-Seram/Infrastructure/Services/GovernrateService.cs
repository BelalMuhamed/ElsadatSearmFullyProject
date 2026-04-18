using AlSadatSeram.Services.contract;
using Application.DTOs.GovernrateDtos;
using Application.Services.contract;
using Domain.Entities;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class GovernrateService : IGovernrateCaontract
    {
        private readonly IUnitOfWork unitOfWork;

        public GovernrateService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task AddNewGovernrate(GovernrateDto governrate)
        {
            var AddedGovernrate = new Governrate()
            {
                Name = governrate.name
            };


            await unitOfWork.GetRepository<Governrate, int>().AddAsync(AddedGovernrate);

        
        }

        public async Task EditGovernrate(GovernrateDto governrate)
        {
           

            var UpdatedCategory = await unitOfWork.GetRepository<Governrate, int>().FindAsync(c => c.Id == governrate.id);
            UpdatedCategory.Name = governrate.name;
            
            await unitOfWork.SaveChangesAsync();
        
        }

        public async Task<ApiResponse<List<GovernrateDto>>> GetAllGovernrates(GovernrateFilteration req)
        {
            var query = unitOfWork.GetRepository<Governrate, int>().GetQueryable();
            var x =await query.ToListAsync();
            

            if (!string.IsNullOrWhiteSpace(req.name))
            {
                query = query.Where(g => g.Name.ToLower().Contains(req.name.ToLower()));
            }

            var totalCount = await query.CountAsync();

            List<GovernrateDto> governrates;

            int page = req.page ?? 1;
            int pageSize = req.pageSize ?? 0;
            int totalPages = 1;

            if (req.page.HasValue && req.pageSize.HasValue)
            {
                governrates = await query
                    .OrderBy(g => g.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(g => new GovernrateDto
                    {
                        id = g.Id,
                        name = g.Name
                    })
                    .ToListAsync();

                totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            }
            else
            {
                governrates = await query
                    .OrderBy(g => g.Id)
                    .Select(g => new GovernrateDto
                    {
                        id = g.Id,
                        name = g.Name
                    })
                    .ToListAsync();

                page = 1;
                pageSize = totalCount; 
                totalPages = 1;
            }

            return new ApiResponse<List<GovernrateDto>>
            {
                totalCount = totalCount,
                page = page,
                pageSize = pageSize,
                totalPages = totalPages,
                data = governrates
            };
        }

        public async Task<List<Governrate>> GetAsync(string name)
        {
            var CheckGovernrates = await unitOfWork.GetRepository<Governrate, int>().GetAsync(x => x.Name == name);
            return CheckGovernrates.ToList();
        }

        public async Task<GovernrateDto> GetByID(int id)
        {
          var model=  await unitOfWork.GetRepository<Governrate, int>().FindAsync(g => g.Id == id);
            if(model!=null)
            {
                var dto = new GovernrateDto()
                {
                    id = model.Id,
                    name = model.Name
                };
                return dto;
            }
            return null;
        }
    }
}
