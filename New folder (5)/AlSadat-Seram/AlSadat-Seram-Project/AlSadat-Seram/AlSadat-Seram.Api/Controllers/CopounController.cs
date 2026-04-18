using Application.DTOs.CopounDtos;
using Application.DTOs.SalesInvoices;
using Application.Services.contract;
using Domain.Entities.copounModel;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CopounController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public CopounController(IServiceManager ServiceManager)
        {
            serviceManager = ServiceManager;
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpGet]
        public async Task<ActionResult> GetAllCopouns()
        {
            try
            {
                var result = await serviceManager.CopounService.GetAllCopouns();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "please check server connection !" });
            }
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPost]
        public async Task<ActionResult> AddNemCopoun(CopounReqDto req)
        {
            try
            {
                var NameExist = await serviceManager.CopounService.GetByID(req.copounDesc);
                if (NameExist != null) return BadRequest(new { message = "نوع الكوبون مضاف مسبقا !" });
                await serviceManager.CopounService.AddNewCopoun(req);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "مشكلة بالخادم من فضلك حاول الاتصال بالخادم مجددا !" });
            }

        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPut]
        public async Task<ActionResult> UpdateCopoun(CopounRespDto req)
        {
            try
            {
                var checkThatNameAddedMoreThanOnce = await serviceManager.CopounService.CheckExist(req.copounDesc);
                if (checkThatNameAddedMoreThanOnce.Count() > 1) return BadRequest(new { message = "نوع الكوبون مضاف مسبقا !" });
                await serviceManager.CopounService.EditCopoun(req);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "مشكلة بالخادم من فضلك حاول الاتصال بالخادم مجددا !" });
            }
        }
        [Authorize(Roles = "Admin,HR,Accountant")]
        [HttpPut("UpdateAllPoints")]
        public async Task<IActionResult> UpdateAllCopounPoints(UpdatePointsDto dto)
        {
            try
            {
                await serviceManager.CopounService.UpdateAllCopounPoints(dto.pointsToCollectCopoun);

                return Ok(new { message = "تم تحديث نقاط التجميع لكل الكوبونات بنجاح" });
            }
            catch (Exception)
            {
                return BadRequest(new { message = "حدث خطأ أثناء تحديث البيانات" });
            }
        }

    }
}
