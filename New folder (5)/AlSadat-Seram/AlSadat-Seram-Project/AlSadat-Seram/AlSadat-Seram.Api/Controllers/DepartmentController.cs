using Application.CommonPagination;
using Application.Services.contract;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IServiceManager _ServiceManager;

        public DepartmentController(IServiceManager serviceManager)
        {
            _ServiceManager = serviceManager;
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpGet("GetAllDepartments")]
        public async Task<IActionResult> GetAllDepartments([FromQuery] PaginationParams paginationParams, string? search)
        {
            var result = await _ServiceManager.DepartmentService.GetAllDepartment(paginationParams, search);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpGet("GetAllActiveDepartment")]
        public async Task<IActionResult> GetAllActiveDepartment([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.DepartmentService.GetAllActiveDepartment(paginationParams);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpGet("GetDepartmentById")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var result = await _ServiceManager.DepartmentService.GetDepartmentById(id);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpGet("GetSoftDeleteDepartment")]
        public async Task<IActionResult> GetSoftDeleteDepartment([FromQuery] PaginationParams paginationParams)
        {
            var result = await _ServiceManager.DepartmentService.GetSoftDeleteDepartment(paginationParams);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpPost("CreateDepartment")]
        public async Task<IActionResult> CreateDepartment([FromBody] Department department)
        {
            var result = await _ServiceManager.DepartmentService.CreateDepartment(department);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpPut("UpdateDepartment")]
        public async Task<IActionResult> UpdateDepartment([FromBody] Department department)
        {
            var result = await _ServiceManager.DepartmentService.UpdateDepartment(department);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpPut("SoftDeleteDepartment")]
        public async Task<IActionResult> SoftDeleteDepartment([FromBody] Department department)
        {
            var result = await _ServiceManager.DepartmentService.SoftDeleteDepartment(department);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpPut("RestoreDepartment")]
        public async Task<IActionResult> RestoreDepartment([FromBody] Department department)
        {
            var result = await _ServiceManager.DepartmentService.RestoreDepartment(department);
            return Ok(result);
        }
        //---------------------------------------------------------------
        //[Authorize(Roles = "HR")]
        [HttpDelete("HardDeleteDepartment")]
        public async Task<IActionResult> HardDeleteDepartment([FromBody] Department department)
        {
            var result = await _ServiceManager.DepartmentService.HardDeleteDepartment(department);
            return Ok(result);
        }
        //---------------------------------------------------------------

    }
}
