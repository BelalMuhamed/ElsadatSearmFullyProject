using AlSadatSeram.Services;
using AlSadatSeram.Services.contract;
using Application.DTOs;
using Application.DTOs.SalesInvoices;
using Application.Services.contract;
using Domain.Enums;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadatSeram.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant,StockManager")]

    public class SalesInvoicesController : ControllerBase
    {
        private readonly IServiceManager _ServiceManager;
        public SalesInvoicesController(IServiceManager ServiceManager)
        {
            _ServiceManager = ServiceManager;
        }
        [Authorize(Roles = "Admin,Accountant")]

        [HttpPost]
        public async Task<IActionResult> Add(SalesInvoicesResponse dto)
        {
            var result = await _ServiceManager
                .SalesInvoiceService
                .AddNewSalesInvoice(dto);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpGet]
        public async Task<ActionResult> GetAllSalesInvoices([FromQuery] SalesInvoiceFilters filters)
        {
           
                var res = await _ServiceManager.SalesInvoiceService.GetAllSalesInvoicies(filters);
                return Ok(res);
          
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.GetById(id);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Accountant")]

        [HttpPut("{id:int}")]

        public async Task<IActionResult> Edit(int id, [FromBody] SalesInvoicesResponse dto)
        {
            if (id != dto.id)
                return BadRequest("رقم الفاتورة غير مطابق");

            var result = await _ServiceManager
                .SalesInvoiceService
                .EditSalesInvoice(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        [HttpPut("change-status")]
        public async Task<IActionResult> ChangeInvoiceStatus([FromBody] InvoiceChangeStatusReq req)
        {


            var result = await _ServiceManager
                .SalesInvoiceService
                .ChangInvoiceStatus(req);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Delete(int id)
        {


            var result = await _ServiceManager.SalesInvoiceService.DeleteSalesInvoice(id);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,Accountant")]

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmInvoice([FromBody] invoiceConfirmationProductsStock req)
        {
            var result = await _ServiceManager
                .SalesInvoiceService
                .ConfirmInvoice(req);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }
        [HttpGet("{id:int}/details")]
        public async Task<IActionResult> GetInvoiceDetails(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.GetInvoiceDetails(id);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }
        [HttpPatch("{id}/ask-to-reverse")]
        [Authorize(Roles = "Admin,Accountant")]

        public async Task<IActionResult> AskToReverse(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.AskToReverse(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPatch("{id}/refused-reverse")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> RefusedReverse(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.RefusedReverse(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("{id}/reverse")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> ReverseInvoice(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.ReverseInvoice(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpGet("{id:int}/pdf/prepareInovoice")]
        [Authorize(Roles = "Admin,StockManager,Accountant")]

        public async Task<IActionResult> GetSimplePdf(int id)
        {


            var result = await _ServiceManager.SalesInvoiceService.GeneratePdf(id, true);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return File(result.Data, "application/pdf", $"invoice-{id}-simple.pdf");
        }
        [HttpGet("{id:int}/pdf/Confirmed/store")]
        [Authorize(Roles = "Admin,StockManager,Accountant")]
        public async Task<IActionResult> GetSimpleConfirmedPdf(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.GenerateConfirmedPdf(id, true);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return File(result.Data, "application/pdf", $"invoice-{id}-confirmed.pdf");
        }
        [HttpGet("{id:int}/pdf/confirmed")]
        [Authorize(Roles = "Admin,StockManager,Accountant")]
        public async Task<IActionResult> GetConfirmedPdf(int id)
        {
            var result = await _ServiceManager.SalesInvoiceService.GenerateConfirmedPdf(id,false);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return File(result.Data, "application/pdf", $"invoice-{id}-confirmed.pdf");
        }
    }
}
