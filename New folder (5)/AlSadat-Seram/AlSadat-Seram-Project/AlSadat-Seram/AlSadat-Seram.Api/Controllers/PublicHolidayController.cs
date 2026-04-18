using Application.CommonPagination;
using Application.Services.contract;
using Domain.Entities.HR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicHolidayController : ControllerBase
    {
        private readonly IServiceManager _ServiceManager;

        public PublicHolidayController(IServiceManager serviceManager)
        {
            _ServiceManager = serviceManager;
        }
        //----------------------------------------------------------
        [HttpGet("GetAllPublicHoliday")]
        public async Task<IActionResult> GetAllPublicHoliday([FromQuery] PaginationParams paginationParams)
        {
            var resulte = await _ServiceManager.PublicHolidayService.GetAllPublicHoliday(paginationParams);
            return Ok(resulte);

        }
        //----------------------------------------------------------
        [HttpGet("GetAllActivePublicHoliday")]
        public async Task<IActionResult> GetAllActivePublicHoliday([FromQuery]  PaginationParams paginationParams)
        {
            var resulte = await _ServiceManager.PublicHolidayService.GetAllActivePublicHoliday(paginationParams);
            return Ok(resulte);
        }
        //----------------------------------------------------------
        [HttpGet("GetPublicHolidayByID/{id}")]
        public async Task<IActionResult> GetPublicHolidayByID (int id)
        {
            var resulte = await _ServiceManager.PublicHolidayService.GetPublicHolidayByID(id);
            return Ok(resulte);
        }
        //----------------------------------------------------------
        [HttpGet("GetSoftDeletePublicHoliday")]
        public async Task<IActionResult> GetSoftDeletePublicHoliday([FromQuery]  PaginationParams paginationParams)
        {
            var resulte = await _ServiceManager.PublicHolidayService.GetSoftDeletePublicHoliday(paginationParams);
            return Ok(resulte);
        }
        //----------------------------------------------------------
        [HttpPost("CreatePublicHoliday")]
        public async Task<IActionResult> CreatePublicHoliday([FromBody] PublicHoliday Model)
        {
            var resulte = await _ServiceManager.PublicHolidayService.CreatePublicHoliday(Model);
            return Ok(resulte);
        }
        //----------------------------------------------------------
        [HttpPut("UpdatePublicHoliday")]
        public async Task<IActionResult> UpdatePublicHoliday([FromBody] PublicHoliday Model)
        {
            var resulte = await _ServiceManager.PublicHolidayService.UpdatePublicHoliday(Model);
            return Ok(resulte);
        }
        //----------------------------------------------------------
        [HttpPut("SoftDeletePublicHoliday")]
        public async Task<IActionResult> SoftDeletePublicHoliday([FromBody] PublicHoliday Model)
        {
            var resulte = await _ServiceManager.PublicHolidayService.SoftDeletePublicHoliday(Model);
            return Ok(resulte);
        }
        //----------------------------------------------------------
        [HttpPut("RestorePublicHoliday")]
        public async Task<IActionResult> RestorePublicHoliday([FromBody] PublicHoliday Model)
        {
            var result = await _ServiceManager.PublicHolidayService.RestorePublicHoliday(Model);
            return Ok(result);
        }
        //----------------------------------------------------------
        [HttpDelete("HardDeletePublicHoliday")]
        public async Task<IActionResult> HardDeletePublicHoliday([FromBody] PublicHoliday Model)
        {
            var resulte = await _ServiceManager.PublicHolidayService.HardDeletePublicHoliday(Model);
            return Ok(resulte);
        }
        //----------------------------------------------------------

    }
}
