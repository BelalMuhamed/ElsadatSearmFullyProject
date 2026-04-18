using Application.CommonPagination;
using Application.DTOs.EmployeeLoan;
using Application.DTOs.EmployeeLoanPayments;
using Application.Services.contract;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlSadat_Seram.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // تأكد أن المستخدم مسجل دخول
    public class EmployeeLoanController:ControllerBase
    {
        private readonly IServiceManager _ServiceManager;
        private readonly ILogger<EmployeeLoanController> _logger;

        public EmployeeLoanController(
            IServiceManager serviceManager,
            ILogger<EmployeeLoanController> logger)
        {
            _ServiceManager = serviceManager;
            _logger = logger;
        }

        // ==================== عمليات القروض ====================
        //[Authorize(Roles = "StaffOnly")]
        [HttpPost("CreateLoan")]
        public async Task<IActionResult> CreateLoanAsync([FromBody] CreateEmployeeLoanDto dto)
        {
            try
            {
                _logger.LogInformation("طلب إنشاء قرض جديد للموظف {EmployeeCode}",dto.EmployeeCode);
                var result = await _ServiceManager.EmployeeLoanService.CreateLoanAsync(dto);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء القرض");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
        //[Authorize(Roles = "StaffOnly")]
        [HttpPost("ApproveLoan")]
        //[Authorize(Roles = "Admin,FinanceManager")] // صلاحيات خاصة
        public async Task<IActionResult> ApproveLoanAsync([FromBody] ApproveLoanDto dto)
        {
            try
            {
                _logger.LogInformation("طلب الموافقة على القرض {LoanId}",dto.LoanId);
                var result = await _ServiceManager.EmployeeLoanService.ApproveLoanAsync(dto);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في الموافقة على القرض");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
        //[Authorize(Roles = "StaffOnly")]
        [HttpPost("RejectLoan")]
        //[Authorize(Roles = "Admin,FinanceManager")]
        public async Task<IActionResult> RejectLoanAsync([FromBody] RejectLoanDto dto)
        {
            try
            {
                _logger.LogInformation("طلب رفض القرض {LoanId}",dto.LoanId);
                var result = await _ServiceManager.EmployeeLoanService.RejectLoanAsync(dto);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في رفض القرض");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
        //[Authorize(Roles = "StaffOnly")]
        [HttpPut("UpdateLoan")]
        public async Task<IActionResult> UpdateLoanAsync(int loanId,[FromBody] UpdateEmployeeLoanDto dto)
        {
            try
            {
                _logger.LogInformation("طلب تحديث القرض {LoanId}",loanId);
                var result = await _ServiceManager.EmployeeLoanService.UpdateLoanAsync(loanId,dto);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في تحديث القرض");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
        //[Authorize(Roles = "StaffOnly")]
        [HttpPost("MakePayment")]
        public async Task<IActionResult> MakePaymentAsync([FromBody] LoanPaymentsDTo dto)
        {
            try
            {
                _logger.LogInformation("طلب تسجيل دفعة للقرض {LoanId}",dto.LoanId);
                var result = await _ServiceManager.EmployeeLoanService.MakePaymentAsync(dto);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في تسجيل الدفعة");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        // ==================== عمليات الاستعلام ====================

        //[Authorize(Roles = "StaffOnly")]
        [HttpGet("GetLoanById")]
        public async Task<IActionResult> GetLoanByIdAsync(int id)
        {
            try
            {
                var result = await _ServiceManager.EmployeeLoanService.GetLoanByIdAsync(id);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data });

                return NotFound(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب القرض {LoanId}",id);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
        //[Authorize(Roles = "StaffOnly")]
        [HttpGet("GetLoanByNumber")]
        public async Task<IActionResult> GetLoanByNumberAsync(string loanNumber)
        {
            try
            {
                var result = await _ServiceManager.EmployeeLoanService.GetLoanByNumberAsync(loanNumber);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data });

                return NotFound(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب القرض برقم {LoanNumber}",loanNumber);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
        //[Authorize(Roles = "StaffOnly")]
        [HttpGet("GetEmployeeLoans")]
        public async Task<IActionResult> GetEmployeeLoansAsync(
            string employeeCode,
            [FromQuery] PaginationParams pagination)
        {
            try
            {
                var result = await _ServiceManager.EmployeeLoanService.GetEmployeeLoansAsync(employeeCode,pagination);

                if(result.Items.Any())
                    return Ok(new { Success = true,Data = result });

                return Ok(new { Success = true,Data = result,Message = "لا توجد قروض لهذا الموظف" });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب قروض الموظف {EmployeeCode}",employeeCode);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        //[Authorize(Roles = "StaffOnly")]
        //[Authorize(Roles = "Admin,FinanceManager")]
        [HttpGet("GetAllLoans")]
        public async Task<IActionResult> GetAllLoansAsync(
            [FromQuery] PaginationParams pagination,
            [FromQuery] string? employeeCode = null,
            [FromQuery] string? employeeName = null,
            [FromQuery] LoanStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] bool? isPaidOff = null
            )
        {
            try
            {
                var filter = new LoanFilterDto
                {
                    EmployeeCode = employeeCode,
                    EmployeeName = employeeName,
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    IsPaidOff = isPaidOff
                };

                var result = await _ServiceManager.EmployeeLoanService.GetAllLoansAsync(pagination,filter);

                return Ok(new { Success = true,Data = result });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب جميع القروض");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        //[Authorize(Roles = "StaffOnly")]
        [HttpGet("GetLoanPayments")]
        public async Task<IActionResult> GetLoanPaymentsAsync(int loanId)
        {
            try
            {
                var result = await _ServiceManager.EmployeeLoanService.GetLoanPaymentsAsync(loanId);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب مدفوعات القرض {LoanId}",loanId);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        // ==================== التقارير والحسابات ====================

        //[Authorize(Roles = "StaffOnly")]
        [HttpGet("CalculateMonthlyDeduction")]
        public async Task<IActionResult> CalculateMonthlyDeductionAsync(
            string employeeCode,
            [FromQuery] DateTime? month = null)
        {
            try
            {
                var targetMonth = month ?? DateTime.UtcNow;
                var result = await _ServiceManager.EmployeeLoanService.CalculateEmployeeMonthlyDeductionAsync(employeeCode,targetMonth);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في حساب الخصومات للموظف {EmployeeCode}",employeeCode);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        //[Authorize(Roles = "StaffOnly")]
        [HttpGet("GetEmployeeLoanSummary")]
        public async Task<IActionResult> GetEmployeeLoanSummaryAsync(string employeeCode)
        {
            try
            {
                var result = await _ServiceManager.EmployeeLoanService.GetEmployeeLoanSummaryAsync(employeeCode);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Data = result.Data });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء تقرير القروض للموظف {EmployeeCode}",employeeCode);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        // ==================== عمليات الإدارة ====================
        
        //[Authorize(Roles = "StaffOnly")]
        [HttpDelete("SoftDeleteLoan")]
        //[Authorize(Roles = "Admin,FinanceManager")]
        public async Task<IActionResult> SoftDeleteLoanAsync(int loanId)
        {
            try
            {
                _logger.LogInformation("طلب حذف ناعم للقرض {LoanId}",loanId);
                var result = await _ServiceManager.EmployeeLoanService.SoftDeleteLoanAsync(loanId);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في الحذف الناعم للقرض {LoanId}",loanId);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        //[Authorize(Roles = "StaffOnly")]
        [HttpPut("RestoreLoan")]
        //[Authorize(Roles = "Admin,FinanceManager")]
        public async Task<IActionResult> RestoreLoanAsync(int loanId)
        {
            try
            {
                _logger.LogInformation("طلب استعادة القرض {LoanId}",loanId);
                var result = await _ServiceManager.EmployeeLoanService.RestoreLoanAsync(loanId);

                if(result.IsSuccess)
                    return Ok(new { Success = true,Message = result.Message });

                return BadRequest(new { Success = false,Message = result.Message });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في استعادة القرض {LoanId}",loanId);
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }

        // ==================== عمليات الدفع التلقائي ====================

        //[Authorize(Roles = "StaffOnly")]
        [HttpPost("ProcessMonthlyInstallments")]
        //[Authorize(Roles = "Admin,FinanceManager")]
        public async Task<IActionResult> ProcessMonthlyInstallmentsAsync([FromQuery] DateTime month)
        {
            try
            {
                _logger.LogInformation("طلب معالجة أقساط شهر {Month}",month.ToString("yyyy-MM"));

                return Ok(new
                {
                    Success = true,
                    Message = "سيتم معالجة الأقساط قريباً",
                    Note = "هذه العملية تعمل كـ Background Service"
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في معالجة الأقساط الشهرية");
                return StatusCode(500,new { Success = false,Message = "حدث خطأ داخلي" });
            }
        }
    }

}
