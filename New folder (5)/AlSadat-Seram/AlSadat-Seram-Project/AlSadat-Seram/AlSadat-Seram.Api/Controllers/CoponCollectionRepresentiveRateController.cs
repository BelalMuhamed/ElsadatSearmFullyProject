using Application.CommonPagination;
using Application.Services.contract;
using Domain.Entities.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoponCollectionRepresentiveRateController : ControllerBase
    {
        private readonly IServiceManager _ServiceManager;

        public CoponCollectionRepresentiveRateController(IServiceManager serviceManager)
        {
            _ServiceManager = serviceManager;
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetAllCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> GetAllCoponCollectionRepresentiveRate([FromQuery] PaginationParams paginationParams)
        {
            var resulte = await _ServiceManager.CoponCollectionRepresentiveRateService.GetAllCoponCollectionRepresentiveRate(paginationParams);
            return Ok(resulte);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetAllActiveCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> GetAllActiveCoponCollectionRepresentiveRate([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.GetAllActiveCoponCollectionRepresentiveRate(paginationParams);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetCoponCollectionRepresentiveRateById/{id}")]
        public async Task<IActionResult> GetCoponCollectionRepresentiveRateById(int id)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.GetCoponCollectionRepresentiveRateById(id);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetSoftDeleteCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> GetSoftDeleteCoponCollectionRepresentiveRate([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.GetSoftDeleteCoponCollectionRepresentiveRate(paginationParams);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPost("CreateCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> CreateCoponCollectionRepresentiveRate([FromBody] CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.CreateCoponCollectionRepresentiveRate(coponCollectionRepresentiveRate);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPut("UpdateCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> UpdateCoponCollectionRepresentiveRate([FromBody] CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.UpdateCoponCollectionRepresentiveRate(coponCollectionRepresentiveRate);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPut("SoftDeleteCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> SoftDeleteCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.SoftDeleteCoponCollectionRepresentiveRate(coponCollectionRepresentiveRate);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPut("RestoreCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> RestoreCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.RestoreCoponCollectionRepresentiveRate(coponCollectionRepresentiveRate);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpDelete("HardDeleteCoponCollectionRepresentiveRate")]
        public async Task<IActionResult> HardDeleteCoponCollectionRepresentiveRate(CoponCollectionRepresentiveRate coponCollectionRepresentiveRate)
        {
            var result = await _ServiceManager.CoponCollectionRepresentiveRateService.HardDeleteCoponCollectionRepresentiveRate(coponCollectionRepresentiveRate);
            return Ok(result);
        }
        //---------------------------------------------------------------
    }
}
