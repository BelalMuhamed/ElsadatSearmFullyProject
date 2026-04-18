using Application.DTOs;
using Application.DTOs.CityDtos;
using Application.Services.contract;
using Domain.UnitOfWork.Contract;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,StockManager,Accountant")]

    public class TransactionController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public TransactionController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllTransactions([FromQuery] StoreTransactionFilters req)
        {
            try
            {
                var res = await serviceManager.storeTransactionService.GetAllTransacctions(req);
                if (res == null) 
                {
                    return BadRequest(new { message = "حدث خطأ اثناء الاتصال بالخادم " });
                }
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "حدث خطأ اثناء الاتصال بالخادم " });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewTransaction([FromBody] StoreTransactionDto dto)
        {
            var result = await serviceManager.storeTransactionService.AddNewTransaction(dto);

            if (result.IsSuccess)
                return Ok(new { message = result.Data });

            return BadRequest(new { message = result.Message });
        }

        [HttpGet("{id}/products")]
        public async Task<IActionResult> GetTransactionProducts(int id)
        {
           
            var products = await serviceManager.storeTransactionService.GetTransactionProductsById(id);

            if (products == null || products.Count == 0)
                return NotFound(new { message = "لا توجد منتجات لهذا التحويل" });

            return Ok(products);
        }
    }
}

