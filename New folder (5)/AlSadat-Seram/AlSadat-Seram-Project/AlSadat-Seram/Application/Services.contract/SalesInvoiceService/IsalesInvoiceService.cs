using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.SalesInvoices;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.SalesInvoiceService
{
    public interface IsalesInvoiceService
    {
        Task<Result<ApiResponse<List<SalesInvoicesResponse>>>> GetAllSalesInvoicies(SalesInvoiceFilters req);
        Task<Result<string>> AddNewSalesInvoice(SalesInvoicesResponse dto);
        Task<Result<string>> EditSalesInvoice(SalesInvoicesResponse dto);
        Task<Result<string>> AskToReverse(int id);
        Task<Result<string>> RefusedReverse(int id);

        Task<Result<string>> DeleteSalesInvoice(int id);
        Task<Result<SalesInvoicesResponse>> GetById(int id);
        Task<Result<string>> ChangInvoiceStatus(InvoiceChangeStatusReq req);
        Task<Result<string>> ConfirmInvoice(invoiceConfirmationProductsStock req);
        Task<Result<SalesInvoiceDetails>> GetInvoiceDetails(int id);
        Task<Result<string>> ReverseInvoice(int id);
        Task<Result<byte[]>> GeneratePdf(int id, bool isSimple);
       Task<Result<byte[]>> GenerateConfirmedPdf(int id, bool isSimple);
    }
}
