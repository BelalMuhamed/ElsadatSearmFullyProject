using Application.CommonPagination;
using Application.Services.contract;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.SupplierDtos;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Accountant")]

    public class SupplierController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public SupplierController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers([FromQuery] SupplierFilteration SuppllierFilters)
        {
            try
            {
                var resulte = await serviceManager.supplierService.GetAllSuppliers(SuppllierFilters);
                return Ok(resulte);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "لا يمكن الاتصال بالخادم " });
            }

        }
        [HttpGet("Supplier/Details")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            try
            {
                var result = await serviceManager.supplierService.GetById(id);

                if (!result.IsSuccess)
                    return NotFound(new { Message = result.Message });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "لا يمكن الاتصال بالخادم " });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddNewSupplier([FromBody] SupplierDto dto)
        {
            try
            {
                var result = await serviceManager.supplierService.AddNewSupllier(dto);

                if (!result.IsSuccess)
                    return BadRequest(new { Message = result.Message });

                return Ok(new { Message = result.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "لا يمكن الاتصال بالخادم" });
            }
        }

        [HttpPut("Edit")]
        public async Task<IActionResult> EditSupplier([FromBody] SupplierDto dto)
        {
            try
            {
                var result = await serviceManager.supplierService.EditSupplier(dto);

                if (!result.IsSuccess)
                    return BadRequest(new { message = result.Message });

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "لا يمكن الاتصال بالخادم" });
            }
        }


    }
}
