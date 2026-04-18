using Application.DTOs.BillBiscountsDtos;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,HR,Accountant")]
    public class GeneralSettingController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public GeneralSettingController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                return Ok(await serviceManager.BillService.GetBillDiscounts());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut]
        public async Task<ActionResult> Update(BillDisountDto req)
        {
            try
            {
                await serviceManager.BillService.EditDiscounts(req);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}
