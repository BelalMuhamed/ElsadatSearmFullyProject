using Application.DTOs.BillBiscountsDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.BillDiscountsServiceContract
{
    public interface IBillDiscount
    {
         Task<BillDisountDto> GetBillDiscounts();
        Task EditDiscounts(BillDisountDto dto);
    }
}
