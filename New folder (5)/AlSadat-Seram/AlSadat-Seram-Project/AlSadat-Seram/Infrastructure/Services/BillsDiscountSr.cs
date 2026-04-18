using Application.DTOs.BillBiscountsDtos;
using Application.Services.contract.BillDiscountsServiceContract;
using Domain.Common;
using Domain.Entities.Invoices;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BillsDiscountSr : IBillDiscount
    {
        private readonly IUnitOfWork unitOfWork;

        public BillsDiscountSr(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task EditDiscounts(BillDisountDto dto)
        {
            var UpdatedModel = await unitOfWork.GetRepository<Billdiscounts, int>().FindAsync(x => true);
            UpdatedModel.FirstDiscount = dto.firstDiscount;
            UpdatedModel.SecondDiscount = dto.secondDiscount;
            UpdatedModel.ThirdDiscount = dto.thirdDiscount;
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<BillDisountDto> GetBillDiscounts()
        {

            var model = await unitOfWork.GetRepository<Billdiscounts, int>().FindAsync(x => true);
            if (model == null)
                return null;
            var res = new BillDisountDto()
            {
                firstDiscount = model.FirstDiscount,
                secondDiscount = model.SecondDiscount,
                thirdDiscount = model.ThirdDiscount

            };
            return res;

        }
       
    }
}
