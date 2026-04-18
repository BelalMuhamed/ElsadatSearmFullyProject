using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeLoan;
using Application.DTOs.EmployeeLoanPayments;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeeLoan;
using Domain.Common;
using Domain.Entities.Finance;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.EmployeeLoanServices
{
    internal class EmployeeLoanService:IEmployeeLoanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<EmployeeLoanService> _logger;

        public EmployeeLoanService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService
            ,ILogger<EmployeeLoanService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }
        #region Helper Methods
        // ---------- الدوال المساعدة ----------
        private async Task<(string? EmployeeCode, string? RepresentativeCode)> GetUserCodesAsync(string? code)
        {
            if(string.IsNullOrEmpty(code))
                return (null, null);
            // التحقق من وجود الموظف
            var employee = await _unitOfWork.GetRepository<Employee,string>()
                .FindAsync(e => e.EmployeeCode == code && !e.IsDeleted);
            if(employee != null)
                return (code, null);
            // التحقق من وجود المندوب
            var representative = await _unitOfWork.GetRepository<Representatives,string>()
                .FindAsync(r => r.RepresentativesCode == code && !r.IsDeleted);
            if(representative != null)
                return (null, code);
            return (null, null);
        }

        private async Task<decimal> GetUserMaxLoanAmountAsync(string? employeeCode,string? representativeCode)
        {
            if(!string.IsNullOrEmpty(employeeCode))
            {
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .FindAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
                return employee?.MaxLoanAmount ?? 0;
            }
            else if(!string.IsNullOrEmpty(representativeCode))
            {
                var representative = await _unitOfWork.GetRepository<Representatives,string>()
                    .FindAsync(r => r.RepresentativesCode == representativeCode && !r.IsDeleted);
                // افترض أن المندوب له حد أقصى للقروض (إذا لم يكن موجوداً، نرجع 0 أو قيمة افتراضية)
                return representative?.MaxLoanAmount ?? 0; // تأكد من وجود MaxLoanAmount في Representatives
            }
            return 0;
        }

        private async Task<string> GetUserNameAsync(string? employeeCode,string? representativeCode)
        {
            if(!string.IsNullOrEmpty(employeeCode))
            {
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .GetQueryable()
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
                return employee?.User?.FullName ?? string.Empty;
            }
            else if(!string.IsNullOrEmpty(representativeCode))
            {
                var representative = await _unitOfWork.GetRepository<Representatives,string>()
                    .GetQueryable()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RepresentativesCode == representativeCode && !r.IsDeleted);
                return representative?.User?.FullName ?? string.Empty;
            }
            return string.Empty;
        }
        
        private async Task<string> GenerateLoanNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("D2");

            var lastLoan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                .GetQueryable()
                .OrderByDescending(l => l.Id)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if(lastLoan != null && lastLoan.LoanNumber.StartsWith($"LN/{year}/{month}/"))
            {
                var lastSequence = lastLoan.LoanNumber.Split('/').Last();
                if(int.TryParse(lastSequence,out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"LN/{year}/{month}/{sequence.ToString("D4")}";
        }

        private async Task<EmployeeLoanDto> MapToLoanDtoAsync(EmployeeLoan loan)
        {
            string employeeName = string.Empty;
            if(!string.IsNullOrEmpty(loan.EmployeeCode))
            {
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .GetQueryable()
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeCode == loan.EmployeeCode);
                employeeName = employee?.User?.FullName ?? "غير معروف";
            }
            else if(!string.IsNullOrEmpty(loan.RepresentativeCode))
            {
                var representative = await _unitOfWork.GetRepository<Representatives,string>()
                    .GetQueryable()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RepresentativesCode == loan.RepresentativeCode);
                employeeName = representative?.User?.FullName ?? "غير معروف";
            }
            return new EmployeeLoanDto
            {
                Id = loan.Id,
                LoanNumber = loan.LoanNumber,
                EmployeeCode = loan.EmployeeCode,
                RepresentativeCode = loan.RepresentativeCode,
                EmployeeName = employeeName,
                LoanAmount = loan.LoanAmount,
                InstallmentsCount = loan.InstallmentsCount,
                InstallmentAmount = loan.InstallmentAmount,
                RemainingAmount = loan.RemainingAmount,
                PaidAmount = loan.PaidAmount,
                IsPaidOff = loan.IsPaidOff,
                Status = loan.Status.ToString(),
                LoanDate = loan.LoanDate,
                FirstInstallmentDate = loan.FirstInstallmentDate,
                ExpectedEndDate = loan.ExpectedEndDate,
                ActualEndDate = loan.ActualEndDate
            };
        }
       
        public async Task<Result<decimal>> CalculateUserMonthlyDeductionAsync(string userCode,DateTime month)
        {
            try
            {
                var codes = await GetUserCodesAsync(userCode);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<decimal>.Failure("المستخدم غير موجود");

                var query = _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Where(l => l.Status == LoanStatus.Active && !l.IsDeleted);

                if(codes.EmployeeCode != null)
                    query = query.Where(l => l.EmployeeCode == codes.EmployeeCode);
                else
                    query = query.Where(l => l.RepresentativeCode == codes.RepresentativeCode);

                var loans = await query.ToListAsync();

                decimal totalDeduction = 0;

                foreach(var loan in loans)
                {
                    if(IsInstallmentDueThisMonth(loan,month))
                    {
                        totalDeduction += loan.InstallmentAmount;
                    }
                }

                return Result<decimal>.Success(totalDeduction,$"إجمالي الخصومات الشهرية: {totalDeduction}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في حساب الخصومات الشهرية للمستخدم {UserCode}",userCode);
                return Result<decimal>.Failure($"خطأ في حساب الخصومات: {ex.Message}");
            }
        }

        private bool IsInstallmentDueThisMonth(EmployeeLoan loan,DateTime month)
        {
            // إذا كان القرض مسدد بالكامل لا يوجد أقساط مستحقة
            if(loan.IsPaidOff || loan.RemainingAmount <= 0)
                return false;

            // حساب عدد الأقساط المدفوعة
            int paidInstallments = (int) Math.Floor(loan.PaidAmount / loan.InstallmentAmount);

            // حساب تاريخ القسط التالي
            DateTime nextDueDate = loan.FirstInstallmentDate.AddMonths(paidInstallments);

            // التحقق إذا كان تاريخ الاستحقاق في الشهر المطلوب
            return nextDueDate.Year == month.Year && nextDueDate.Month == month.Month;
        }

        private async Task CreateLoanDisbursementAccountingEntryAsync(EmployeeLoan loan,string userId)
        {
            try
            {

                // الحصول على حسابات شجرة الحسابات
                var loanAccount = await GetChartOfAccountByCodeAsync("1013"); // قروض الموظفين تحت السداد
                var cashAccount = await GetChartOfAccountByCodeAsync("1011"); // النقدية
                var journalEntry = new JournalEntries();

                if(!string.IsNullOrEmpty(loan.EmployeeCode))
                {
                    journalEntry = new JournalEntries
                    {
                        EntryDate = DateTime.Now,
                        Desc = $"صرف قرض للموظف {loan.Employee!.User!.FullName} - رقم {loan.LoanNumber}",
                        ReferenceNo = GenerateAccountingReference().ToString(),
                    };
                }
                if(!string.IsNullOrEmpty(loan.RepresentativeCode))
                {
                    journalEntry = new JournalEntries
                    {
                        EntryDate = DateTime.Now,
                        Desc = $"صرف قرض للموظف {loan.Representative!.User!.FullName} - رقم {loan.LoanNumber}",
                        ReferenceNo = GenerateAccountingReference().ToString(),
                    };
                }





                await _unitOfWork.GetRepository<JournalEntries,int>().AddAsync(journalEntry);
                await _unitOfWork.SaveChangesAsync();

                var debitEntry = new JournalEntryDetails
                {
                    JournalEntryId = journalEntry.Id,
                    AccountId = loanAccount.Id,
                    Debit = loan.LoanAmount,
                    Credit = 0,
                };

                var creditEntry = new JournalEntryDetails
                {
                    JournalEntryId = journalEntry.Id,
                    AccountId = cashAccount.Id,
                    Debit = 0,
                    Credit = loan.LoanAmount,
                };

                await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(debitEntry);
                await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(creditEntry);
                //await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم إنشاء قيد محاسبي لصرف القرض {LoanNumber}",loan.LoanNumber);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء القيد المحاسبي لصرف القرض {LoanNumber}",loan.LoanNumber);
                // لا نرمي استثناء هنا حتى لا نوقف عملية الموافقة على القرض
            }
        }

        private async Task<string> GeneratePaymentNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("D2");
            var day = DateTime.UtcNow.Day.ToString("D2");

            var lastPayment = await _unitOfWork.GetRepository<EmployeeLoanPayments,int>()
                .GetQueryable()
                .OrderByDescending(p => p.Id)
                .FirstOrDefaultAsync();

            var sequence = 1;
            if(lastPayment != null && lastPayment.PaymentNumber.StartsWith($"PAY/{year}/{month}/{day}/"))
            {
                var lastSequence = lastPayment.PaymentNumber.Split('/').Last();
                if(int.TryParse(lastSequence,out int lastSeq))
                {
                    sequence = lastSeq + 1;
                }
            }

            return $"PAY/{year}/{month}/{day}/{sequence.ToString("D4")}";
        }

        private int CalculatePaidInstallments(EmployeeLoan loan)
        {
            if(loan.InstallmentAmount <= 0)
                return 0;
            return (int) Math.Floor(loan.PaidAmount / loan.InstallmentAmount);
        }

        private DateTime CalculateNextDueDate(EmployeeLoan loan)
        {
            if(loan.IsPaidOff || loan.RemainingAmount <= 0)
                return DateTime.MinValue;

            int paidInstallments = CalculatePaidInstallments(loan);
            return loan.FirstInstallmentDate.AddMonths(paidInstallments);
        }

        private async Task<ChartOfAccounts> GetChartOfAccountByCodeAsync(string accountCode)
        {
            var account = await _unitOfWork.GetRepository<ChartOfAccounts,int>()
                .FindAsync(a => a.AccountCode == accountCode && a.IsActive);

            if(account == null)
            {
                _logger.LogWarning("الحساب المحاسبي بالكود {AccountCode} غير موجود أو غير نشط",accountCode);
                throw new Exception($"الحساب المحاسبي بالكود {accountCode} غير موجود أو غير نشط");
            }

            return account;
        }

        private int GenerateAccountingReference()
        {
            return new Random().Next(100000,999999);
        }

        private async Task CreatePaymentAccountingEntryAsync(EmployeeLoanPayments payment,EmployeeLoan loan,string userId)
        {
            try
            {
                var loanAccount = await GetChartOfAccountByCodeAsync("1013"); // قروض الموظفين تحت السداد
                var cashAccount = await GetChartOfAccountByCodeAsync("1011"); // النقدية
                var journalEntry = new JournalEntries();

                if(!string.IsNullOrEmpty(loan.EmployeeCode))
                {
                    journalEntry = new JournalEntries
                    {
                        EntryDate = payment.PaymentDate,
                        Desc = $"سداد دفعة قرض {loan.LoanNumber} - دفعة رقم {payment.PaymentNumber}-للموظف {loan.Employee!.User!.FullName}",
                        ReferenceNo = GenerateAccountingReference().ToString(),
                    };
                }
                if(!string.IsNullOrEmpty(loan.RepresentativeCode))
                {
                    journalEntry = new JournalEntries
                    {
                        EntryDate = payment.PaymentDate,
                        Desc = $"سداد دفعة قرض {loan.LoanNumber} - دفعة رقم {payment.PaymentNumber}-للموظف {loan.Representative!.User!.FullName}",
                        ReferenceNo = GenerateAccountingReference().ToString(),
                    };
                }         

                await _unitOfWork.GetRepository<JournalEntries,int>().AddAsync(journalEntry);
                await _unitOfWork.SaveChangesAsync();

                var debitEntry = new JournalEntryDetails
                {
                    JournalEntryId = journalEntry.Id,
                    AccountId = cashAccount.Id,
                    Debit = payment.PaymentAmount,
                    Credit = 0,
                };

                var creditEntry = new JournalEntryDetails
                {
                    JournalEntryId = journalEntry.Id,
                    AccountId = loanAccount.Id,
                    Debit = 0,
                    Credit = payment.PaymentAmount,
                };

                await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(debitEntry);
                await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(creditEntry);
                //await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم إنشاء قيد محاسبي لدفعة القرض {PaymentNumber}",payment.PaymentNumber);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء القيد المحاسبي للدفعة {PaymentNumber}",payment.PaymentNumber);
                // لا نرمي استثناء هنا حتى لا نوقف عملية الدفع
            }
        }
        #endregion

        public async Task<Result<EmployeeLoanDto>> CreateLoanAsync(CreateEmployeeLoanDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(userId == null)
                    return Result<EmployeeLoanDto>.Failure("غير مصرح بالدخول");

                bool hasEmployeeCode = !string.IsNullOrEmpty(dto.EmployeeCode);
                bool hasRepresentativeCode = !string.IsNullOrEmpty(dto.RepresentativeCode);
                if(!hasEmployeeCode && !hasRepresentativeCode)
                    return Result<EmployeeLoanDto>.Failure("يرجى تحديد كود الموظف أو المندوب");

                if(hasEmployeeCode)
                {
                    var employee = await _unitOfWork.GetRepository<Employee,string>()
                        .FindAsync(e => e.EmployeeCode == dto.EmployeeCode && !e.IsDeleted);
                    if(employee == null)
                        return Result<EmployeeLoanDto>.Failure("الموظف غير موجود");

                    if(dto.LoanAmount > employee.MaxLoanAmount)
                        return Result<EmployeeLoanDto>.Failure($"الحد الأقصى للقرض هو {employee.MaxLoanAmount}");

                    dto.RepresentativeCode = null;
                }
                else 
                {
                    var representative = await _unitOfWork.GetRepository<Representatives,string>()
                        .FindAsync(r => r.RepresentativesCode == dto.RepresentativeCode && !r.IsDeleted);
                    if(representative == null)
                        return Result<EmployeeLoanDto>.Failure("المندوب غير موجود");

                    if(dto.LoanAmount > representative.MaxLoanAmount)
                        return Result<EmployeeLoanDto>.Failure($"الحد الأقصى للقرض هو {representative.MaxLoanAmount}");
                    dto.EmployeeCode = null;
                }

                decimal installmentAmount = dto.LoanAmount / dto.InstallmentsCount;
                string loanNumber = await GenerateLoanNumberAsync();

                var loan = new EmployeeLoan
                {
                    LoanNumber = loanNumber,
                    EmployeeCode = dto.EmployeeCode,
                    RepresentativeCode = dto.RepresentativeCode,
                    LoanAmount = dto.LoanAmount,
                    InstallmentsCount = dto.InstallmentsCount,
                    InstallmentAmount = installmentAmount,
                    RemainingAmount = dto.LoanAmount,
                    PaidAmount = 0,
                    IsPaidOff = false,
                    Status = LoanStatus.PendingApproval,
                    LoanDate = DateTime.UtcNow,
                    FirstInstallmentDate = dto.FirstInstallmentDate,
                    ExpectedEndDate = dto.FirstInstallmentDate.AddMonths(dto.InstallmentsCount - 1),
                    CreateBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.GetRepository<EmployeeLoan,int>().AddAsync(loan);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم إنشاء قرض جديد رقم {LoanNumber} للمستخدم {EmployeeCode} {RepresentativeCode}",
                 loanNumber,dto.EmployeeCode,dto.RepresentativeCode);
                var loanDto = await MapToLoanDtoAsync(loan);

                return Result<EmployeeLoanDto>.Success(loanDto,$"تم إنشاء القرض رقم {loanNumber} في انتظار الموافقة");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء القرض للموظف {EmployeeCode}",dto.EmployeeCode);
                return Result<EmployeeLoanDto>.Failure($"خطأ في إنشاء القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<string>> ApproveLoanAsync(ApproveLoanDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("غير مصرح بالدخول");

                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Include(l => l.Employee).ThenInclude(e => e.User)
                    .Include(l => l.Representative).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(l => l.Id == dto.LoanId && !l.IsDeleted);

                if(loan == null)
                    return Result<string>.Failure("القرض غير موجود");

                if(loan.Status != LoanStatus.PendingApproval)
                    return Result<string>.Failure("القرض ليس في حالة انتظار الموافقة");

                // 1. تحديث حالة القرض
                loan.Status = LoanStatus.Active;
                loan.ApprovedBy = userId;
                loan.ApprovedDate = DateTime.UtcNow;
                loan.UpdateBy = userId;
                loan.UpdateAt = DateTime.UtcNow;

                // 2. تسجيل في المحاسبة (صرف القرض)
                await CreateLoanDisbursementAccountingEntryAsync(loan,userId);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تمت الموافقة على القرض رقم {LoanNumber} بواسطة {UserId}",loan.LoanNumber,userId);

                return Result<string>.Success("تمت الموافقة على القرض وصرفه للموظف");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في الموافقة على القرض {LoanId}",dto.LoanId);
                return Result<string>.Failure($"خطأ في الموافقة على القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<string>> RejectLoanAsync(RejectLoanDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("غير مصرح بالدخول");

                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Include(l => l.Employee).ThenInclude(e => e.User)
                    .Include(l => l.Representative).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(l => l.Id == dto.LoanId && !l.IsDeleted);

                if(loan == null)
                    return Result<string>.Failure("القرض غير موجود");

                if(loan.Status != LoanStatus.PendingApproval)
                    return Result<string>.Failure("القرض ليس في حالة انتظار الموافقة");

                // تحديث حالة القرض
                loan.Status = LoanStatus.Rejected;
                loan.RejectedBy = userId;
                loan.RejectedDate = DateTime.UtcNow;
                loan.RejectionReason = dto.Reason;
                loan.UpdateBy = userId;
                loan.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم رفض القرض رقم {LoanNumber} بواسطة {UserId}",loan.LoanNumber,userId);

                return Result<string>.Success("تم رفض القرض");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في رفض القرض {LoanId}",dto.LoanId);
                return Result<string>.Failure($"خطأ في رفض القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<EmployeeLoanDto>> UpdateLoanAsync(int loanId,UpdateEmployeeLoanDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<EmployeeLoanDto>.Failure("غير مصرح بالدخول");

                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Include(l => l.Employee).ThenInclude(e => e.User)
                    .Include(l => l.Representative).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(l => l.Id == loanId && !l.IsDeleted);

                if(loan == null)
                    return Result<EmployeeLoanDto>.Failure("القرض غير موجود");

                if(loan.Status != LoanStatus.PendingApproval)
                    return Result<EmployeeLoanDto>.Failure("لا يمكن تعديل القرض بعد الموافقة أو الرفض");

                // التحديثات المسموح بها فقط
                // لا يمكن تحديث المبالغ أو الأقساط بعد الإنشاء

                loan.UpdateBy = userId;
                loan.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم تحديث القرض {LoanId} بواسطة {UserId}",loanId,userId);

                var loanDto = await MapToLoanDtoAsync(loan);
                return Result<EmployeeLoanDto>.Success(loanDto,"تم تحديث القرض بنجاح");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في تحديث القرض {LoanId}",loanId);
                return Result<EmployeeLoanDto>.Failure($"خطأ في تحديث القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<string>> MakePaymentAsync(LoanPaymentsDTo dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("غير مصرح بالدخول");

                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Include(l => l.Employee).ThenInclude(e => e.User)
                    .Include(l => l.Representative).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(l => l.Id == dto.LoanId && !l.IsDeleted);

                if(loan == null)
                    return Result<string>.Failure("القرض غير موجود");

                if(loan.Status != LoanStatus.Active)
                    return Result<string>.Failure("القرض غير نشط حالياً");

                if(dto.PaymentAmount > loan.RemainingAmount)
                    return Result<string>.Failure("مبلغ الدفع أكبر من المتبقي");

                string paymentNumber = await GeneratePaymentNumberAsync();

                decimal newRemainingAmount = loan.RemainingAmount - dto.PaymentAmount;

                var payment = new EmployeeLoanPayments
                {
                    PaymentNumber = paymentNumber,
                    LoanId = loan.Id,
                    PaymentAmount = dto.PaymentAmount,
                    PaymentDate = dto.PaymentDate,
                    RemainingAmount = newRemainingAmount,
                    PaymentMethod = dto.PaymentMethod,
                    CreateBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                loan.PaidAmount += dto.PaymentAmount;
                loan.RemainingAmount = newRemainingAmount;

                if(loan.RemainingAmount <= 0)
                {
                    loan.IsPaidOff = true;
                    loan.Status = LoanStatus.PaidOff;
                    loan.ActualEndDate = DateTime.UtcNow;
                }

                loan.UpdateBy = userId;
                loan.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.GetRepository<EmployeeLoanPayments,int>().AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                await CreatePaymentAccountingEntryAsync(payment,loan,userId);
                //if(dto.PaymentMethod != PaymentMethod.SalaryDeduction)
                //{
                //    await CreatePaymentAccountingEntryAsync(payment,loan,userId);
                //}

                _logger.LogInformation("تم تسجيل دفعة بقيمة {Amount} للقرض {LoanNumber}",dto.PaymentAmount,loan.LoanNumber);

                return Result<string>.Success("تم تسجيل الدفعة بنجاح");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في تسجيل الدفعة للقرض {LoanId}",dto.LoanId);
                return Result<string>.Failure($"خطأ في تسجيل الدفعة: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<decimal>> CalculateEmployeeMonthlyDeductionAsync(string employeeCode,DateTime month)
        {
            try
            {
                // حساب إجمالي أقساط القروض المستحقة لهذا الشهر
                var loans = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Where(l => l.EmployeeCode == employeeCode &&
                               l.Status == LoanStatus.Active &&
                               !l.IsDeleted)
                    .ToListAsync();

                decimal totalDeduction = 0;

                foreach(var loan in loans)
                {
                    // التحقق إذا كان هناك قسط مستحق هذا الشهر
                    if(IsInstallmentDueThisMonth(loan,month))
                    {
                        totalDeduction += loan.InstallmentAmount;
                    }
                }

                return Result<decimal>.Success(totalDeduction,$"إجمالي الخصومات الشهرية: {totalDeduction}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في حساب الخصومات الشهرية للموظف {EmployeeCode}",employeeCode);
                return Result<decimal>.Failure($"خطأ في حساب الخصومات: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<EmployeeLoanSummaryDto>> GetEmployeeLoanSummaryAsync(string userCode)
        {
            try
            {
                var codes = await GetUserCodesAsync(userCode);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<EmployeeLoanSummaryDto>.Failure("المستخدم غير موجود");

                var query = _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Where(l => !l.IsDeleted);

                if(codes.EmployeeCode != null)
                    query = query.Where(l => l.EmployeeCode == codes.EmployeeCode).Include(l => l.Employee).ThenInclude(e => e.User).Include(l => l.Payments);
                else
                    query = query.Where(l => l.RepresentativeCode == codes.RepresentativeCode).Include(l => l.Representative).ThenInclude(r => r.User).Include(l => l.Payments);

                var loans = await query.ToListAsync();

                string userName = "";
                decimal maxLoanAmount = 0;
                string department = "";

                if(codes.EmployeeCode != null)
                {
                    var employee = loans.FirstOrDefault()?.Employee;
                    if(employee != null)
                    {
                        userName = employee.User?.FullName ?? "";
                        maxLoanAmount = employee.MaxLoanAmount;
                        department = employee.Department?.Name ?? "";
                    }
                }
                else
                {
                    var representative = loans.FirstOrDefault()?.Representative;
                    if(representative != null)
                    {
                        userName = representative.User?.FullName ?? "";
                        maxLoanAmount = representative.MaxLoanAmount;
                    }
                }

                var activeLoans = loans.Where(l => l.Status == LoanStatus.Active).ToList();
                var paidLoans = loans.Where(l => l.Status == LoanStatus.PaidOff).ToList();
                var pendingLoans = loans.Where(l => l.Status == LoanStatus.PendingApproval).ToList();
                var rejectedLoans = loans.Where(l => l.Status == LoanStatus.Rejected).ToList();

                var currentMonth = DateTime.UtcNow;
                var currentMonthDeduction = await CalculateUserMonthlyDeductionAsync(userCode,currentMonth);

                var summary = new EmployeeLoanSummaryDto
                {
                    EmployeeCode = codes.EmployeeCode,
                    RepresentativeCode = codes.RepresentativeCode,
                    EmployeeName = userName,
                    EmployeeDepartment = department,
                    TotalLoansCount = loans.Count,
                    ActiveLoansCount = activeLoans.Count,
                    PaidLoansCount = paidLoans.Count,
                    PendingLoansCount = pendingLoans.Count,
                    RejectedLoansCount = rejectedLoans.Count,
                    TotalBorrowed = loans.Sum(l => l.LoanAmount),
                    TotalPaid = loans.Sum(l => l.PaidAmount),
                    TotalRemaining = loans.Sum(l => l.RemainingAmount),
                    CurrentMonthDeduction = currentMonthDeduction.Data,
                    MaxLoanAmount = maxLoanAmount,
                    AvailableLoanAmount = maxLoanAmount - activeLoans.Sum(l => l.RemainingAmount),
                    LoanDetails = activeLoans.Select(l => new LoanDetailDto
                    {
                        LoanId = l.Id,
                        LoanNumber = l.LoanNumber,
                        LoanAmount = l.LoanAmount,
                        PaidAmount = l.PaidAmount,
                        RemainingAmount = l.RemainingAmount,
                        InstallmentAmount = l.InstallmentAmount,
                        InstallmentsCount = l.InstallmentsCount,
                        InstallmentsPaid = CalculatePaidInstallments(l),
                        InstallmentsRemaining = l.InstallmentsCount - CalculatePaidInstallments(l),
                        NextDueDate = CalculateNextDueDate(l),
                        LoanDate = l.LoanDate,
                        FirstInstallmentDate = l.FirstInstallmentDate,
                        ExpectedEndDate = l.ExpectedEndDate,
                        Status = l.Status.ToString(),
                        IsPaidOff = l.IsPaidOff
                    }).ToList()
                };
                return Result<EmployeeLoanSummaryDto>.Success(summary);              
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء تقرير القروض للمستخدم {UserCode}",userCode);
                return Result<EmployeeLoanSummaryDto>.Failure($"خطأ في إنشاء التقرير: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<List<EmployeeLoanPayments>>> GetLoanPaymentsAsync(int loanId)
        {
            try
            {
                var payments = await _unitOfWork.GetRepository<EmployeeLoanPayments,int>()
                    .GetQueryable()
                    .Where(p => p.LoanId == loanId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return Result<List<EmployeeLoanPayments>>.Success(payments);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب مدفوعات القرض {LoanId}",loanId);
                return Result<List<EmployeeLoanPayments>>.Failure($"خطأ في جلب المدفوعات: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<EmployeeLoanDto>> GetLoanByIdAsync(int id)
        {
            try
            {
                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                   .GetQueryable()
                   .Include(l => l.Employee).ThenInclude(e => e.User)
                   .Include(l => l.Representative).ThenInclude(r => r.User)
                   .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted);

                if(loan == null)
                    return Result<EmployeeLoanDto>.Failure("القرض غير موجود");

                var loanDto = await MapToLoanDtoAsync(loan);
                return Result<EmployeeLoanDto>.Success(loanDto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب القرض {LoanId}",id);
                return Result<EmployeeLoanDto>.Failure($"خطأ في جلب القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<EmployeeLoanDto>> GetLoanByNumberAsync(string loanNumber)
        {
            try
            {
                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Include(l => l.Employee).ThenInclude(e => e.User)
                    .Include(l => l.Representative).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(l => l.LoanNumber == loanNumber && !l.IsDeleted);

                if(loan == null)
                    return Result<EmployeeLoanDto>.Failure("القرض غير موجود");

                var loanDto = await MapToLoanDtoAsync(loan);
                return Result<EmployeeLoanDto>.Success(loanDto);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب القرض برقم {LoanNumber}",loanNumber);
                return Result<EmployeeLoanDto>.Failure($"خطأ في جلب القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<PagedList<EmployeeLoanDto>> GetEmployeeLoansAsync(string userCode,PaginationParams pagination)
        {
            var userId = _currentUserService.UserId;
            if(userId is null)
                return new PagedList<EmployeeLoanDto>(new List<EmployeeLoanDto>(),0,pagination.PageNumber,pagination.PageSize);

            var codes = await GetUserCodesAsync(userCode);

            var query = _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .Where(l => !l.IsDeleted);

            if(codes.EmployeeCode != null)
                query = query.Where(l => l.EmployeeCode == codes.EmployeeCode);
            else if(codes.RepresentativeCode != null)
                query = query.Where(l => l.RepresentativeCode == codes.RepresentativeCode);
            else
                return new PagedList<EmployeeLoanDto>(new List<EmployeeLoanDto>(),0,pagination.PageNumber,pagination.PageSize);

            query = query.OrderByDescending(l => l.CreatedAt);

            var result = await query.Select(l => new EmployeeLoanDto
            {
                Id = l.Id,
                LoanNumber = l.LoanNumber,
                EmployeeCode = l.EmployeeCode,
                RepresentativeCode = l.RepresentativeCode,
                EmployeeName = l.Employee != null ? l.Employee.User!.FullName : (l.Representative != null ? l.Representative.User!.FullName : ""),
                LoanAmount = l.LoanAmount,
                InstallmentsCount = l.InstallmentsCount,
                InstallmentAmount = l.InstallmentAmount,
                RemainingAmount = l.RemainingAmount,
                PaidAmount = l.PaidAmount,
                IsPaidOff = l.IsPaidOff,
                Status = l.Status.ToString(),
                LoanDate = l.LoanDate,
                FirstInstallmentDate = l.FirstInstallmentDate,
                ExpectedEndDate = l.ExpectedEndDate,
                ActualEndDate = l.ActualEndDate
            }).ToPagedListAsync(pagination.PageNumber,pagination.PageSize);

            return result;
        }
        //---------------------------------------------------------
        public async Task<PagedList<EmployeeLoanDto>> GetAllLoansAsync(PaginationParams pagination,LoanFilterDto? filter = null)
        {
            var userId = _currentUserService.UserId;
            if(userId is null)
                return new PagedList<EmployeeLoanDto>(new List<EmployeeLoanDto>(),0,pagination.PageNumber,pagination.PageSize);

            var query = _unitOfWork.GetRepository<EmployeeLoan,int>()
           .GetQueryable()
           .Include(l => l.Employee).ThenInclude(e => e.User)
           .Include(l => l.Representative).ThenInclude(r => r.User)
           .Where(l => !l.IsDeleted);

            // تطبيق الفلاتر إذا وجدت
            if(filter != null)
            {
                if(!string.IsNullOrEmpty(filter.UserCode)) // نضيف خاصية UserCode في الفلتر
                {
                    var codes = await GetUserCodesAsync(filter.UserCode);
                    if(codes.EmployeeCode != null)
                        query = query.Where(l => l.EmployeeCode == codes.EmployeeCode);
                    else if(codes.RepresentativeCode != null)
                        query = query.Where(l => l.RepresentativeCode == codes.RepresentativeCode);
                }
                else
                {
                    if(!string.IsNullOrEmpty(filter.EmployeeCode))
                        query = query.Where(l => l.EmployeeCode == filter.EmployeeCode);
                    else if(!string.IsNullOrEmpty(filter.RepresentativeCode))
                        query = query.Where(l => l.RepresentativeCode == filter.RepresentativeCode);
                }

                if(!string.IsNullOrEmpty(filter.EmployeeName))
                {
                    // البحث في اسم الموظف أو المندوب
                    query = query.Where(l => (l.Employee != null && l.Employee.User!.FullName.Contains(filter.EmployeeName)) ||
                                             (l.Representative != null && l.Representative.User!.FullName.Contains(filter.EmployeeName)));
                }
                if(filter.Status.HasValue)
                    query = query.Where(l => l.Status == filter.Status.Value);

                if(filter.FromDate.HasValue)
                    query = query.Where(l => l.LoanDate >= filter.FromDate.Value);

                if(filter.ToDate.HasValue)
                    query = query.Where(l => l.LoanDate <= filter.ToDate.Value);
            }

            var result = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new EmployeeLoanDto
                {
                    Id = l.Id,
                    LoanNumber = l.LoanNumber,
                    EmployeeCode = l.EmployeeCode,
                    RepresentativeCode = l.RepresentativeCode,
                    EmployeeName = (l.Employee != null ? l.Employee.User!.FullName : (l.Representative != null ? l.Representative.User!.FullName : "")),
                    LoanAmount = l.LoanAmount,
                    InstallmentsCount = l.InstallmentsCount,
                    InstallmentAmount = l.InstallmentAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    IsPaidOff = l.IsPaidOff,
                    Status = l.Status.ToString(),
                    LoanDate = l.LoanDate,
                    FirstInstallmentDate = l.FirstInstallmentDate,
                    ExpectedEndDate = l.ExpectedEndDate,
                    ActualEndDate = l.ActualEndDate
                }).ToPagedListAsync(pagination.PageNumber,pagination.PageSize);

            return result;
        }
        //---------------------------------------------------------
        public async Task<Result<string>> SoftDeleteLoanAsync(int loanId)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("غير مصرح بالدخول");

                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(l => l.Id == loanId && !l.IsDeleted);

                if(loan == null)
                    return Result<string>.Failure("القرض غير موجود");

                // لا يمكن حذف قرض تمت الموافقة عليه أو مسدد
                if(loan.Status == LoanStatus.Active || loan.Status == LoanStatus.PaidOff)
                    return Result<string>.Failure("لا يمكن حذف قرض تمت الموافقة عليه أو مسدد");

                loan.IsDeleted = true;
                loan.DeleteBy = userId;
                loan.DeleteAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم حذف القرض {LoanId} بواسطة {UserId}",loanId,userId);

                return Result<string>.Success("تم حذف القرض بنجاح");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في حذف القرض {LoanId}",loanId);
                return Result<string>.Failure($"خطأ في حذف القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------
        public async Task<Result<string>> RestoreLoanAsync(int loanId)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("غير مصرح بالدخول");

                var loan = await _unitOfWork.GetRepository<EmployeeLoan,int>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(l => l.Id == loanId && l.IsDeleted);

                if(loan == null)
                    return Result<string>.Failure("القرض غير موجود");

                loan.IsDeleted = false;
                loan.UpdateBy = userId;
                loan.UpdateAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("تم استعادة القرض {LoanId} بواسطة {UserId}",loanId,userId);

                return Result<string>.Success("تم استعادة القرض بنجاح");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في استعادة القرض {LoanId}",loanId);
                return Result<string>.Failure($"خطأ في استعادة القرض: {ex.Message}");
            }
        }
        //---------------------------------------------------------

    }

}
