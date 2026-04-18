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
    public class CollectionRepresentiveRateController : ControllerBase
    {
        private readonly IServiceManager _ServiceManager;

        public CollectionRepresentiveRateController(IServiceManager serviceManager)
        {
            _ServiceManager = serviceManager;
        }
        //--------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetAllCollectionRepresentiveRate")]
        public async Task<IActionResult> GetAllCollectionRepresentiveRate([FromQuery] PaginationParams paginationParams)
        {
            var resulte = await _ServiceManager.CollectionRepresentiveRateService.GetAllCollectionRepresentiveRate(paginationParams);
            return Ok(resulte);
        }
        //--------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetAllActiveCollectionRepresentiveRate")]
        public async Task<IActionResult> GetAllActiveCollectionRepresentiveRate([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.GetAllActiveCollectionRepresentiveRate(paginationParams);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetCollectionRepresentiveRateById/{id}")]
        public async Task<IActionResult> GetCollectionRepresentiveRateById(int id)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.GetCollectionRepresentiveRateById(id);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpGet("GetSoftDeleteCollectionRepresentiveRate")]
        public async Task<IActionResult> GetSoftDeleteCollectionRepresentiveRate([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.GetSoftDeleteCollectionRepresentiveRate(paginationParams);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPost("CreateCollectionRepresentiveRate")]
        public async Task<IActionResult> CreateCollectionRepresentiveRate([FromBody] CollectionRepresentiveRate Model)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.CreateCollectionRepresentiveRate(Model);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPut("UpdateCollectionRepresentiveRate")]
        public async Task<IActionResult> UpdateCollectionRepresentiveRate([FromBody] CollectionRepresentiveRate Model)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.UpdateCollectionRepresentiveRate(Model);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPut("SoftDeleteCollectionRepresentiveRate")]
        public async Task<IActionResult> SoftDeleteCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.SoftDeleteCollectionRepresentiveRate(Model);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpPut("RestoreCollectionRepresentiveRate")]
        public async Task<IActionResult> RestoreCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.RestoreCollectionRepresentiveRate(Model);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HROrAccountant")]
        [HttpDelete("HardDeleteCollectionRepresentiveRate")]
        public async Task<IActionResult> HardDeleteCollectionRepresentiveRate(CollectionRepresentiveRate Model)
        {
            var result = await _ServiceManager.CollectionRepresentiveRateService.HardDeleteCollectionRepresentiveRate(Model);
            return Ok(result);
        }
        //---------------------------------------------------------------

    }
}
