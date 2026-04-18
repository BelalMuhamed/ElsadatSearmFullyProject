using Application.Services.contract.EmployeeLoan;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace AlSadat_Seram.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class Testcontroller:ControllerBase
{
    private readonly IEmployeeLoanService _employeeLoanService;
    private readonly ILogger<EmployeeLoanController> _logger;
    public Testcontroller(
            IEmployeeLoanService employeeLoanService)
            //ILogger<EmployeeLoanController> logger)
    {
        _employeeLoanService = employeeLoanService;
        //_logger = logger;
    }
    [HttpGet("testingNewComp")]
    public async Task<IActionResult> newFuncation()
    {
        return Ok("testing");
    }
}
