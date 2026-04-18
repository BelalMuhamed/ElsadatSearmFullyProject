using Application.DTOs.FinanceDtos;
using Application.Services.contract;
using Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,StockManager,HR,Accountant")]

    public class TreeAccountsController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public TreeAccountsController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetAccountsTree()
        {
            try
            {
                var tree = await serviceManager.treeService.GetTree();

                if (tree == null || !tree.Any())
                    return NotFound(new { Message = "لا يوجد حسابات!" });

                return Ok( tree);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { Message = ex.Message, Success = false });
            }
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts([FromQuery] FilterationAccountsDto req)
        {
            try
            {
                var result = await serviceManager.treeService.GetAccounts(req);

                if (!result.IsSuccess)
                    return BadRequest(result);

                if (result.Data == null || !result.Data.Any())
                {
                    result.Message = "لا يوجد حسابات ";
                    return NotFound(result);

                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,Result<string>.Failure("خطأ أُناء الاتصال بقاعدة البيانات "));
            }
        }
        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(string userId)
        {
            try
            {
                var result = await serviceManager.treeService.GetDisAndMerchAccountByUserId(userId);

                if (!result.IsSuccess)
                    return NotFound(new
                    {
                        Message = result.Message,
                        Success = false
                    });

                return Ok(new
                {
                    Data = result.Data,
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "حدث خطأ غير متوقع",
                    Error = ex.Message,
                    Success = false
                });
            }
        }
        [HttpPut]
        public async Task<IActionResult> EditAccount( [FromBody] AccountDto dto)
        {
            try
            {
                var result = await serviceManager.treeService.EditAccount( dto);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "حدث خطأ غير متوقع",
                    Error = ex.Message,
                    Success = false
                });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AccountDto dto)
        {
            try
            {
                var result = await serviceManager.treeService.AddNewAccount(dto);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(Result<string>.Failure("خطأ في الاتصال بقاعدة البيانات "));
            }
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByAccountId(int id)
        {
            var result = await serviceManager.treeService.GetByAccountId(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok( result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var result = await serviceManager.treeService.DeleteAccount(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
