using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.CityDtos;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.SupplierDtos;

namespace Application.Services.contract
{
    public interface IPurchaseInvoiceContract
    {
        Task<ApiResponse<List<PurchaseInvoiceDtos>>> GetAllPurchaseInvoicies(PurchaseInvoiceFilters req);

        Task<Result<string>> AddNewPurchaseInvoice(PurchaseInvoiceDtos dto);
        Task<Result<string>> EditPurchaseInvoice(PurchaseInvoiceDtos dto);
        Task<Result<string>> DeletePurchaseInvoice(int id);
        Task<Result<PurchaseInvoiceDtos>> GetById(int id);
        Task<Result<byte[]>> GeneratePdf(int id, bool isSimple);
    }
}
