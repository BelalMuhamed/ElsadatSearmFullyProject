using Application.DTOs.CopounDtos;
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
    public class GovernrateController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public GovernrateController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetAllGovernrates([FromQuery]GovernrateFilteration req)
        {
            try
            {
                var res =await serviceManager.GovernrateService.GetAllGovernrates(req);
                return Ok(res);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPost]
        public async Task<ActionResult> AddGovernrate(GovernrateDto dto)
        {
            try
            {
                var CheckExisted = await serviceManager.GovernrateService.GetAsync(dto.name);
                if (CheckExisted.Count()>=1) { return BadRequest(new { message = "يوجد فئه بنفس الاسم من فضلك غير اسم الفئه!" }); }
                await serviceManager.GovernrateService.AddNewGovernrate(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPut]
        public async Task<ActionResult> UpdateGovernrate(GovernrateDto req)
        {
            try
            {
                var checkThatNameAddedMoreThanOnce = await serviceManager.GovernrateService.GetAsync(req.name);
                if (checkThatNameAddedMoreThanOnce.Count() >= 1) return BadRequest(new { message = "اسم المحافظة مضاف مسبقا" });
                await serviceManager.GovernrateService.EditGovernrate(req);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "مشكلة بالخادم من فضلك حاول الاتصال بالخادم مجددا !" });
            }
        }

        [Authorize]
        [HttpGet]
        [Route("Governrates/{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
              var res= await serviceManager.GovernrateService.GetByID(id);
                if(res!=null) return Ok(res);
                return BadRequest(new { message = "لا يوجد محافظة بهذا المعرف !" });
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "خطأ في الاتصال بقاعدة البيانات !" });

            }
        }

    }
}
