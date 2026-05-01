using Application.DTOs;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    /// <summary>
    /// Stock-transfer (store-to-store) endpoints.
    /// <para>
    /// The global <c>ExceptionHandlingMiddleware</c> already converts unexpected
    /// exceptions into a uniform <c>Result&lt;string&gt;</c> envelope, so this
    /// controller does NOT wrap calls in a broad try/catch — that was previously
    /// hiding 500s as 400s.
    /// </para>
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,StockManager,Accountant")]
    public sealed class TransactionController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public TransactionController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        /// <summary>
        /// Paginated, filterable list of stock transfers.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTransactions([FromQuery] StoreTransactionFilters req)
        {
            var response = await _serviceManager.storeTransactionService.GetAllTransacctions(req);

            // The service returns null only on infrastructure failure; surface that as 500.
            if (response is null)
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "حدث خطأ أثناء الاتصال بقاعدة البيانات" });

            return Ok(response);
        }

        /// <summary>
        /// Creates a new stock transfer between two warehouses.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddNewTransaction([FromBody] StoreTransactionDto dto)
        {
            var result = await _serviceManager.storeTransactionService.AddNewTransaction(dto);

            return result.IsSuccess
                ? Ok(result)
                : StatusCode((int)result.StatusCode, result);
        }

        /// <summary>
        /// Returns the line-items of a given transfer.
        /// An empty list is a valid 200 response — it means the transfer exists
        /// but has no detail rows. Returning 404 here was incorrect.
        /// </summary>
        [HttpGet("{id:int}/products")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransactionProducts(int id)
        {
            var products = await _serviceManager.storeTransactionService.GetTransactionProductsById(id);
            return Ok(products ?? new List<StoreTransactionProductsDto>());
        }
    }
}
