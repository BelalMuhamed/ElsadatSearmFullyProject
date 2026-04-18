using AlSadatSeram.Services.contract;
using Application.DTOs.FinanceDtos;
using Application.Services.contract;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,HR,Accountant")]

    public class JournalEntryController : ControllerBase
    {
        private readonly IServiceManager serviceManager;

        public JournalEntryController(IServiceManager serviceManager)
        {
            this.serviceManager = serviceManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetJournalDeatilsForAccount([FromQuery]AccountDetailsDtoReq req)
        {
            var res =  await serviceManager.journalEntryDeatils.GetAccountDetails(req);
            if (res==null) 
            {
                return BadRequest(res);

             
            }
            return Ok(res );
        }
        [HttpGet("journal-entries")]
        public async Task<IActionResult> GetAllJournalEntries([FromQuery] JournalEntryFilterationReq req)
        {
            try
            {
                var res = await serviceManager.journalEntry.GetAll(req);
                return Ok(Result<ApiResponse<List<JournalEntriesDto>>>.Success(res));
            }
            catch (Exception ex)
            {
                return BadRequest(Result<ApiResponse<string>>.Failure("حدث خطأ أثناء استرجاع بينات القيود "));
            }
        }

        [HttpPost("add-new-entry")]
        public async Task<IActionResult> AddNewEntry([FromBody] JournalEntriesDto req)
        {
            var res = await serviceManager.journalEntry.AddNewJournalEntry(req);
            if (!res.IsSuccess)
            {
                return BadRequest(res);


            }
            return Ok(res);
        }


        [HttpPut("edit-entry")]
        public async Task<ActionResult> EditEntry([FromBody] JournalEntriesDto req)
        {
            var res = await serviceManager.journalEntry.UpdateJournalEntry(req);
            if (!res.IsSuccess)
            {
                return BadRequest(res);


            }
            return Ok(res);
        }
        [HttpPatch("{id}/post")]
        public async Task<IActionResult> Post(int id)
        {
            var result = await serviceManager.journalEntry.PostEntry(id);

            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(Result<string>.Failure("رقم غير صالح"));

            var result = await serviceManager.journalEntry.GetById(id);

            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        [Authorize(Roles = "Admin")]

        [HttpDelete("delete-entry/{id}")]
        public async Task<IActionResult> Deleted(int id)
        {
            if (id <= 0)
                return BadRequest(Result<string>.Failure("رقم غير صالح"));

            var res = await serviceManager.journalEntry.DeleteJournalEntry(id);
            if (!res.IsSuccess)
            {
                return BadRequest(res);


            }
            return Ok(res);
        }
    }
}
