using Application.CommonPagination;
using Application.DTOs;
using Application.DTOs.ProductsDtos;
using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,StockManager,Accountant")]

    public class PurchaseInvoiceController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public PurchaseInvoiceController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
       

        [HttpGet]

        public async Task<ActionResult> GetAllPurchaseInvoices([FromQuery] PurchaseInvoiceFilters filters)
        {
            try
            {
                var res = await serviceManager.purchaseInvoiceService.GetAllPurchaseInvoicies(filters);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]

        public async Task<IActionResult> Add(PurchaseInvoiceDtos dto)
        {
            var result = await serviceManager
                .purchaseInvoiceService
                .AddNewPurchaseInvoice(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result); // ✅ مش result.Data
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Accountant")]

        public async Task<IActionResult> Edit(int id, [FromBody] PurchaseInvoiceDtos dto)
        {
            //if (id != dto.id)
            //    return BadRequest("رقم الفاتورة غير مطابق");
            dto.id = id;
            var result = await serviceManager
                .purchaseInvoiceService
                .EditPurchaseInvoice(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await serviceManager.purchaseInvoiceService.GetById(id);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            
           

            var result = await serviceManager.purchaseInvoiceService.DeletePurchaseInvoice(id);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }
        [HttpGet("{id:int}/pdf/simple")]
        [Authorize(Roles = "Admin,StockManager,Accountant")]

        public async Task<IActionResult> GetSimplePdf(int id)
        {

            
            var result = await serviceManager.purchaseInvoiceService.GeneratePdf(id, true);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return File(result.Data, "application/pdf", $"invoice-{id}-simple.pdf");
        }
        [Authorize(Roles = "Admin,Accountant")]

        [HttpGet("{id:int}/pdf/full")]
        public async Task<IActionResult> GetFullPdf(int id)
        {
            var result = await serviceManager.purchaseInvoiceService.GeneratePdf(id, false);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return File(result.Data, "application/pdf", $"invoice-{id}-full.pdf");
        }

    }
}
