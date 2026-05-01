using Application.DTOs;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,StockManager,Accountant")]

    public class StockController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public StockController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllStocks([FromQuery] StockFilterations req)
        {
            var res = await serviceManager.stockService.GetAllStocks(req);
            if (res == null)
            {
                return BadRequest(new { message = "خطأ في الاتصال بقاعدة البيانات !" });
            }
            return Ok(res);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetStoreStockById(int id)
        {
            var res = await serviceManager.stockService.GetByStoreID(id);

            if (res == null)
                return BadRequest(new { message = "خطأ في الاتصال بقاعدة البيانات !" });

            if (!res.IsSuccess)
                return StatusCode((int)res.StatusCode, new { message = res.Message });

            return Ok(res);
        }

        [HttpGet("product/{id}")]
        public async Task<ActionResult> GetStockByProductId(int id)
        {
            var res = await serviceManager.stockService.GetByProductID(id);

            if (res == null)
                return BadRequest(new { message = "خطأ في الاتصال بقاعدة البيانات !" });

            if (!res.IsSuccess)
                return StatusCode((int)res.StatusCode, new { message = res.Message });

            return Ok(res);
        }
        /// <summary>
        /// Lightweight per-store stock list for the Stock-Transfer page.
        /// Returns only products with availableQuantity > 0.
        /// </summary>
        [HttpGet("by-store/{storeId:int}")]
        public async Task<ActionResult> GetAvailableByStore(int storeId)
        {
            var res = await serviceManager.stockService.GetAvailableByStoreAsync(storeId);

            if (!res.IsSuccess)
                return StatusCode((int)res.StatusCode, res);

            return Ok(res);
        }
    }
}
