using AlSadatSeram.Services.contract;
using Application.DTOs.CopounDtos;
using Application.Services.contract.CopounServiceContract;
using Domain.Entities.copounModel;
using Domain.Entities.Invoices;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.CopounServices
{
    public class CopounService : ICopounService
    {
        private readonly IUnitOfWork unitOfWork;

        public CopounService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task AddNewCopoun(CopounReqDto req)
        {
            var addedCopoun = new Copoun()
            {
                CopounDesc = req.copounDesc,
                CopounPaiedType = req.copounPaiedType,
                PaiedCash = req.paiedCash,
                PointsToCollectCopoun = req.pointsToCollectCopoun,
                Stars = req.stars
            };
            await unitOfWork.GetRepository<Copoun, string>().AddAsync(addedCopoun);
        }

        public async Task<IReadOnlyList<Copoun>> CheckExist(string Id)
        {
            var result =await unitOfWork.GetRepository<Copoun,string>().GetQueryable().Where(x=>x.CopounDesc== Id).ToListAsync();
            return result; 
        }

        public async Task EditCopoun(CopounRespDto req)
        {
            var existing = await unitOfWork.GetRepository<Copoun, string>()
                                           .FindAsync(c=>c.CopounDesc==req.copounDesc);

            if (existing == null)
                throw new Exception("الكوبون غير موجود");

            existing.CopounPaiedType = req.copounPaiedType;
            existing.PaiedCash = req.paiedCash;
            existing.PointsToCollectCopoun = req.pointsToCollectCopoun;
            existing.Stars = req.stars;
            existing.IsActive = req.isActive;

            await unitOfWork.GetRepository<Copoun, string>().UpdateAsync(existing);
        }


        public async Task<IReadOnlyList<CopounRespDto>> GetAllCopouns()
        {
            var data = await unitOfWork.GetRepository<Copoun, string>().GetAllAsync();

            var result = data.Select(c => new CopounRespDto
            {
                copounDesc = c.CopounDesc,
                copounPaiedType = c.CopounPaiedType,
                pointsToCollectCopoun = c.PointsToCollectCopoun,
                stars = c.Stars,
                paiedCash = c.PaiedCash,
                isActive = c.IsActive
            })
            .ToList()
            .AsReadOnly();

            return result;
        }

        public async Task<CopounRespDto?> GetByID(string Id)
        {
            CopounRespDto result = null;
            var res = await unitOfWork.GetRepository<Copoun, string>().FindAsync(c => c.CopounDesc == Id);
            if (res == null) return null;
            result = new CopounRespDto()
            {
                copounDesc = res.CopounDesc,
                copounPaiedType = res.CopounPaiedType,
                isActive = res.IsActive,
                paiedCash = res.PaiedCash,
                pointsToCollectCopoun = res.PointsToCollectCopoun,
                stars = res.Stars,

            };
            return result;
        }

        public async Task UpdateAllCopounPoints(int newPoints)
        {
            var allCopouns = await unitOfWork.GetRepository<Copoun,string>().GetAllAsync();

            foreach (var c in allCopouns)
            {
                c.PointsToCollectCopoun = newPoints;
                await unitOfWork.GetRepository<Copoun, string>().UpdateAsync(c);
            }

        }
    }
}
