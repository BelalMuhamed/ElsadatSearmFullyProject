using Application.DTOs.CityDtos;
using Application.DTOs.GovernrateDtos;
using Application.Services.contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CityController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public CityController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAllCities([FromQuery] CityFilteration req)
        {
            try
            {
                var res = await serviceManager.CityContract.GetAllCities(req);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPost]
        public async Task<ActionResult> AddCity(CityDto dto)
        {
            try
            {
                var CheckExisted = await serviceManager.CityContract.GetAsync(dto);
                if (CheckExisted.Count() >= 1) { return BadRequest(new { message = "يوجد مدينه بنفس الاسم داخل نفس المحافظة !" }); }
                await serviceManager.CityContract.AddNewCity(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPut]
        public async Task<ActionResult> UpdateCity(CityDto req)
        {
            try
            {
                var checkThatNameAddedMoreThanOnce = await serviceManager.CityContract.GetAsync(req);
                if (checkThatNameAddedMoreThanOnce.Count() >= 1) return BadRequest(new { message = "يوجد مدينه بنفس الاسم داخل نفس المحافظة !" });
                await serviceManager.CityContract.EditCity(req);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "مشكلة بالخادم من فضلك حاول الاتصال بالخادم مجددا !" });
            }
        }
        [Authorize]
        [HttpGet]
        [Route("cities/{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                var res = await serviceManager.CityContract.GetByID(id);
                if (res != null) return Ok(res);
                return BadRequest(new { message = "لا يوجد مدينه بهذا المعرف !" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "خطأ في الاتصال بقاعدة البيانات !" });

            }
        }

    }
}
