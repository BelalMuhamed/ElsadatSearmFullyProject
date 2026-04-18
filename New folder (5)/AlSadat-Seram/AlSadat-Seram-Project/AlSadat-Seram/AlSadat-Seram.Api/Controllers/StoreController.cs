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

    public class StoreController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public StoreController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        [HttpGet]
        public async Task<ActionResult> GetAllStores([FromQuery]StoreFilteration req)
        {
             
            var res = await serviceManager.storeService.GetAllStores(req);
            if (res == null)
            {
                return BadRequest(new { message = "خطأ في الاتصال بقاعدة البيانات !" });
            }
            return Ok(res);
        }


        
        [HttpPost]
        public async Task<ActionResult> AddNewStore([FromBody] StoreDto dto)
        {
            var result = await serviceManager.storeService.AddNewStore(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, new { message = result.Message });

            return StatusCode((int)result.StatusCode, new { message = result.Data });
        }

       
        [HttpPut]
        public async Task<ActionResult> EditStore([FromBody] StoreDto dto)
        {
            var result = await serviceManager.storeService.EditStore(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, new { message = result.Message });

            return StatusCode((int)result.StatusCode, new { message = result.Data });
        }

       
        [HttpDelete]
        public async Task<ActionResult> DeleteStore([FromBody] StoreDeleteDto dto)
        {
            var result = await serviceManager.storeService.DeleteStore(dto);

            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, new { message = result.Message });

            return StatusCode((int)result.StatusCode, new { message = result.Data });
        }
    }

}
