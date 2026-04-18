using Application.DTOs;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DistAndMerchController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public DistAndMerchController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        // -----------------------------------------------------------
        // 1) Add New Distributor or Merchant
        // -----------------------------------------------------------
        [HttpPost("add/{userId}")]

        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddDistributorOrMerchant([FromBody] DistributorsAndMerchantsAndAgentsDto dto)
        {
            var result = await serviceManager.DistributorsAndMerchantsService.AddNewDistributorOrMerchant(dto);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        // -----------------------------------------------------------
        // 2) Edit Distributor or Merchant
        // -----------------------------------------------------------
        [Authorize(Roles = "Admin,Accountant")]
        [HttpPut("edit/{userId}")]
        public async Task<IActionResult> EditDistributorOrMerchant(string userId, [FromBody] DistributorsAndMerchantsAndAgentsDto dto)
        {
            dto.userId = userId; // inject from route

            var result = await serviceManager.DistributorsAndMerchantsService.EditDistributorOrMerchant(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }
        [Authorize(Roles = "Admin,Accountant")]
        [HttpGet("get/{userId}")]
        public async Task<IActionResult> GetDistributorOrMerchantById(string userId)
        {
            var result = await serviceManager.DistributorsAndMerchantsService.GetDistributorOrMerchantById(userId);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result);

            return Ok(result);
        }

        // -----------------------------------------------------------
        // 4) Get All With Filters + Pagination
        // -----------------------------------------------------------
        [HttpGet("list")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetAllDistributorsAndMerchants([FromQuery] DistributorsAndMerchantsFilters filters)
       {
            try
            {
                var result = await serviceManager.DistributorsAndMerchantsService.GetAllDistributorsAndMerchants(filters);
                return Ok(result);
            }
            catch (Exception ex) 
            {
                return BadRequest(new {message= ex.Message });
            }
        }
    }
}
