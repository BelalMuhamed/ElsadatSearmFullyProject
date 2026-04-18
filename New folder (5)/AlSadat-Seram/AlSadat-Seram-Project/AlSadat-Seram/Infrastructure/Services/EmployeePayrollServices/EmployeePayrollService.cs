using Application.DTOs.EmployeeSalary;
using Application.DTOs.Payroll;
using Application.DTOs.RepresentativeDtos;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeePayroll;
using Application.Services.contract.EmployeeService;
using Application.Services.contract.RepresentativeService;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.Finance;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Net;

namespace Infrastructure.Services.EmployeePayrollServices
{
    internal class PayrollService : IEmployeePayrollService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PayrollService> _logger;
        private readonly IRepresentativeService _representativeService;

        public PayrollService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService,
            IEmployeeService employeeService, ILogger<PayrollService> logger,IRepresentativeService representativeService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _employeeService = employeeService;
            _logger = logger;
            _representativeService = representativeService;
        }

        #region Helper Methods
        // =============== دوال مساعدة للتمييز بين الموظف والمندوب ==============
        private async Task<(string? EmployeeCode, string? RepresentativeCode)> GetUserCodesAsync(string? code)
        {
            if(string.IsNullOrEmpty(code))
                return (null, null);

            var employee = await _unitOfWork.GetRepository<Employee,string>()
                .FindAsync(e => e.EmployeeCode == code && !e.IsDeleted);
            if(employee != null)
                return (code, null);

            var representative = await _unitOfWork.GetRepository<Representatives,string>()
                .FindAsync(r => r.RepresentativesCode == code && !r.IsDeleted);
            if(representative != null)
                return (null, code);

            return (null, null);
        }
        private async Task<string> GetUserNameAsync(string? employeeCode,string? representativeCode)
        {
            if(!string.IsNullOrEmpty(employeeCode))
            {
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .GetQueryable()
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
                return employee?.User?.FullName ?? "";
            }
            else if(!string.IsNullOrEmpty(representativeCode))
            {
                var representative = await _unitOfWork.GetRepository<Representatives,string>()
                    .GetQueryable()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.RepresentativesCode == representativeCode && !r.IsDeleted);
                return representative?.User?.FullName ?? "";
            }
            return "";
        }
        private async Task<string> GetDepartmentNameAsync(string? employeeCode,string? representativeCode)
        {
            if(!string.IsNullOrEmpty(employeeCode))
            {
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .GetQueryable()
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
                return employee?.Department?.Name ?? "غير محدد";
            }
            else if(!string.IsNullOrEmpty(representativeCode))
            {
                // المندوب قد لا يكون له قسم، يمكن إرجاع قيمة ثابتة أو null
                return "مندوب";
            }
            return "غير محدد";
        }
        private async Task CreateLoanPaymentAndAccountingEntry(Payroll payroll,string userId)
        {
            string? employeeCode = payroll.EmployeeCode;
            string? representativeCode = payroll.RepresentativeCode;
            var targetMonth = payroll.PayPeriod;

            var loansQuery = _unitOfWork.GetRepository<EmployeeLoan,int>()
                  .GetQueryable()
                  .Where(l => l.Status == LoanStatus.Active && !l.IsDeleted && !l.IsPaidOff);
            if(!string.IsNullOrEmpty(employeeCode))
                loansQuery = loansQuery.Where(l => l.EmployeeCode == employeeCode);
            else if(!string.IsNullOrEmpty(representativeCode))
                loansQuery = loansQuery.Where(l => l.RepresentativeCode == representativeCode);
            else
                return;
            var loans = await loansQuery.ToListAsync();

            foreach(var loan in loans)
            {
                int paidInstallments = (int) Math.Floor(loan.PaidAmount / loan.InstallmentAmount);
                DateTime nextDueDate = loan.FirstInstallmentDate.AddMonths(paidInstallments);

                if(nextDueDate.Year == targetMonth.Year && nextDueDate.Month == targetMonth.Month)
                {
                    var paymentNumber = $"PAY/{DateTime.Now:yyyyMMdd}/{Guid.NewGuid().ToString().Substring(0,8)}";

                    var loanPayment = new EmployeeLoanPayments
                    {
                        PaymentNumber = paymentNumber,
                        LoanId = loan.Id,
                        PayrollId = payroll.Id,
                        PaymentAmount = loan.InstallmentAmount,
                        PaymentDate = DateTime.Now,
                        RemainingAmount = loan.RemainingAmount - loan.InstallmentAmount,
                        PaymentMethod = PaymentMethod.SalaryDeduction,
                        CreateBy = userId,
                        CreatedAt = DateTime.Now
                    };
                    await _unitOfWork.GetRepository<EmployeeLoanPayments,int>().AddAsync(loanPayment);

                    loan.PaidAmount += loan.InstallmentAmount;
                    loan.RemainingAmount -= loan.InstallmentAmount;

                    if(loan.RemainingAmount <= 0)
                    {
                        loan.IsPaidOff = true;
                        loan.Status = LoanStatus.PaidOff;
                        loan.ActualEndDate = DateTime.Now;
                    }
                    loan.UpdateAt = DateTime.Now;
                    loan.UpdateBy = userId;
                }
            }
            await CreateLoanRepaymentAccountingEntry(payroll,userId);
        }
        private async Task CreateSalaryAccountingEntry(Payroll payroll)
        {
            string userName = payroll.Employee?.User?.FullName ?? payroll.Representative?.User?.FullName ?? "المستخدم";

            var journalEntry = new JournalEntries
            {
                EntryDate = DateTime.Now,
                Desc = $"قيد صرف راتب المستخدم {userName} لشهر {payroll.PayPeriod:MM/yyyy}",
                ReferenceNo = GenerateAccountingReference().ToString(),
            };

            await _unitOfWork.GetRepository<JournalEntries,int>().AddAsync(journalEntry);
            await _unitOfWork.SaveChangesAsync();

            var salaryExpenseAccount = await GetChartOfAccountByCodeAsync("5011"); // مصروف الرواتب
            var cashAccount = await GetChartOfAccountByCodeAsync("1011"); // النقدية

            var debitEntry = new JournalEntryDetails
            {
                JournalEntryId = journalEntry.Id,
                AccountId = salaryExpenseAccount.Id,
                Debit = payroll.NetSalary,
                Credit = 0,
            };

            var creditEntry = new JournalEntryDetails
            {
                JournalEntryId = journalEntry.Id,
                AccountId = cashAccount.Id,
                Debit = 0,
                Credit = payroll.NetSalary,
            };

            await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(debitEntry);
            await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(creditEntry);

            payroll.AccountingEntryNumber = journalEntry.ReferenceNo.ToString();
            payroll.AccountingEntryId = journalEntry.Id;
        }
        private async Task CreateLoanRepaymentAccountingEntry(Payroll payroll,string userId)
        {
            string userName = payroll.Employee?.User?.FullName ?? payroll.Representative?.User?.FullName ?? "المستخدم";

            var journalEntry = new JournalEntries
            {
                EntryDate = DateTime.Now,
                Desc = $"قيد سداد قرض من راتب المستخدم {userName}",
                ReferenceNo = GenerateAccountingReference().ToString(),
            };

            await _unitOfWork.GetRepository<JournalEntries,int>().AddAsync(journalEntry);

            var loanAccount = await GetChartOfAccountByCodeAsync("1013"); // قروض الموظفين تحت السداد
            var cashAccount = await GetChartOfAccountByCodeAsync("1011"); // النقدية

            var debitEntry = new JournalEntryDetails
            {
                JournalEntryId = journalEntry.Id,
                AccountId = loanAccount.Id,
                Debit = payroll.LoanDeduction,
                Credit = 0,
            };
            var creditEntry = new JournalEntryDetails
            {
                JournalEntryId = journalEntry.Id,
                AccountId = cashAccount.Id,
                Debit = 0,
                Credit = payroll.LoanDeduction,
            };

            await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(debitEntry);
            await _unitOfWork.GetRepository<JournalEntryDetails,int>().AddAsync(creditEntry);
        }
        private async Task<ChartOfAccounts> GetChartOfAccountByCodeAsync(string accountCode)
        {
            var account = await _unitOfWork.GetRepository<ChartOfAccounts,int>()
                .FindAsync(a => a.AccountCode == accountCode && a.IsActive);

            if(account == null)
                throw new Exception($"الحساب المحاسبي {accountCode} غير موجود");

            return account;
        }
        private async Task<PayrollResponseDto> MapToPayrollResponseDto(Payroll payroll)
        {
            string employeeName = "";
            string department = "";

            if(payroll.Employee != null)
            {
                employeeName = payroll.Employee.User?.FullName ?? "غير معروف";
                department = payroll.Employee.Department?.Name ?? "غير محدد";
            }
            else if(payroll.Representative != null)
            {
                employeeName = payroll.Representative.User?.FullName ?? "غير معروف";
                department = "مندوب";
            }
            return new PayrollResponseDto
            {
                Id = payroll.Id,
                EmployeeCode = payroll.EmployeeCode,
                RepresentativeCode = payroll.RepresentativeCode,
                EmployeeName = employeeName,
                Department = department,
                PayPeriod = payroll.PayPeriod,

                BasicSalary = payroll.BasicSalary,
                GrossSalary = payroll.GrossSalary,
                OvertimePay = payroll.OvertimePay,

                TotalDeductions = payroll.TotalDeductions,
                TimeDeductions = payroll.TimeDeductions,
                AbsentDeductions = payroll.AbsentDeductions,
                LeaveDeductions = payroll.LeaveDeductions,
                LateDeductions = payroll.LateDeductions,
                EarlyLeaveDeductions = payroll.EarlyLeaveDeductions,
                SanctionDeductions = payroll.SanctionDeductions,

                HasPendingLoans = payroll.HasPendingLoans,
                PendingLoanAmount = payroll.PendingLoanAmount,
                LoanInstallmentsCount = payroll.LoanInstallmentsCount,

                NetSalary = payroll.NetSalary,

                Status = payroll.Status.ToString(),
                PaymentMethod = payroll.PaymentMethod,
                AccountingEntryNumber = payroll.AccountingEntryNumber,
                PostedToAccountingDate = payroll.PostedToAccountingDate,
                PaidDate = payroll.PaidDate,
                PaidBy = payroll.PaidBy,
                CreatedAt = payroll.CreatedAt
            };
        }
        private int GenerateAccountingReference()
        {
            return new Random().Next(100000,999999);
        }
        private SalaryCalculationResult MapFromEmployeeSalaryDto(EmployeeSalaryDTo dto)
        {
            return new SalaryCalculationResult
            {
                // معلومات الموظف
                EmployeeId = dto.EmployeeId,
                EmployeeCode = dto.EmployeeCode,
                EmployeeName = dto.EmployeeName,
                DepartmentName = dto.DepartmentName,

                // معلومات المندوب (تترك null للموظف)
                RepresentativeId = null,
                RepresentativeCode = null,
                RepresentativeName = null,
                PointsWallet = null,
                MoneyOfPointInWallet = null,
                MoneyDeposit = null,
                TotalDeductionFromMoneyDeposit = null,

                // الفترة
                SelectedMonth = dto.SelectedMonth,
                SelectedYear = dto.SelectedYear,

                // الأيام
                TotalWorkingDays = dto.TotalWorkingDays,
                PresentDays = dto.PresentDays,
                AbsentDays = dto.AbsentDays,
                PaidLeaveDays = dto.PaidLeaveDays,
                UnpaidLeaveDays = dto.UnpaidLeaveDays,
                LateDays = dto.LateDays,
                EarlyLeaveDays = dto.EarlyLeaveDays,

                // المرتب الأساسي
                BasicSalary = dto.BasicSalary,
                SalaryPerDay = dto.SalaryPerDay,
                SalaryPerHour = dto.SalaryPerHour,
                GrossSalary = dto.GrossSalary,
                NetSalaryBeforeLoans = dto.NetSalaryBeforeLoans,

                // الوقت الإضافي
                OvertimeHours = dto.OvertimeHours,
                OvertimeRatePerHour = dto.OvertimeRatePerHour,
                TotalOvertimePay = dto.TotalOvertimePay,

                // الخصومات
                DeductionHours = dto.DeductionHours,
                DeductionRatePerHour = dto.DeductionRatePerHour,
                TimeDeductionAmount = dto.TimeDeductionAmount,
                AbsentDeduction = dto.AbsentDeduction,
                UnpaidLeaveDeduction = dto.UnpaidLeaveDeduction,
                LateDeduction = dto.LateDeduction,
                EarlyLeaveDeduction = dto.EarlyLeaveDeduction,

                // القروض
                LoanDeduction = dto.LoanDeduction,
                LoanInstallmentsCount = dto.LoanInstallmentsCount,

                // العقوبات
                SanctionAmount = dto.SanctionAmount,
                SanctionsCount = dto.SanctionsCount,

                // الإجماليات
                TotalAdditions = dto.TotalAdditions,
                TotalDeductions = dto.TotalDeductions,

                // ملخص
                Summary = dto.Summary,

                // تفاصيل إضافية
                LeaveDetails = dto.LeaveDetails,
                LoanDetails = dto.LoanDetails,
                SanctionDetails = dto.SanctionDetails,
                AttendanceDetails = dto.AttendanceDetails
            };
        }
        private SalaryCalculationResult MapFromRepresentativeSalaryDto(RepresentativeSalaryDTo dto)
        {
            return new SalaryCalculationResult
            {
                // معلومات الموظف (تترك null للمندوب)
                EmployeeId = null,
                EmployeeCode = null,
                EmployeeName = null,
                DepartmentName = null,

                // معلومات المندوب
                RepresentativeId = dto.RepresentativeId,
                RepresentativeCode = dto.RepresentativeCode,
                RepresentativeName = dto.RepresentativeName,
                PointsWallet = dto.PointsWallet,
                MoneyOfPointInWallet = dto.MoneyOfPointInWallet,
                MoneyDeposit = dto.MoneyDeposit,
                TotalDeductionFromMoneyDeposit = dto.TotalDeductionFromMoneyDeposit,

                // الفترة
                SelectedMonth = dto.SelectedMonth,
                SelectedYear = dto.SelectedYear,

                // الأيام
                TotalWorkingDays = dto.TotalWorkingDays,
                PresentDays = dto.PresentDays,
                AbsentDays = dto.AbsentDays,
                PaidLeaveDays = dto.PaidLeaveDays,
                UnpaidLeaveDays = dto.UnpaidLeaveDays,
                LateDays = dto.LateDays,
                EarlyLeaveDays = dto.EarlyLeaveDays,

                // المرتب الأساسي
                BasicSalary = dto.BasicSalary,
                SalaryPerDay = dto.SalaryPerDay,
                SalaryPerHour = dto.SalaryPerHour,
                GrossSalary = dto.GrossSalary,
                NetSalaryBeforeLoans = dto.NetSalaryBeforeLoans,

                // الوقت الإضافي
                OvertimeHours = dto.OvertimeHours,
                OvertimeRatePerHour = dto.OvertimeRatePerHour,
                TotalOvertimePay = dto.TotalOvertimePay,

                // الخصومات
                DeductionHours = dto.DeductionHours,
                DeductionRatePerHour = dto.DeductionRatePerHour,
                TimeDeductionAmount = dto.TimeDeductionAmount,
                AbsentDeduction = dto.AbsentDeduction,
                UnpaidLeaveDeduction = dto.UnpaidLeaveDeduction,
                LateDeduction = dto.LateDeduction,
                EarlyLeaveDeduction = dto.EarlyLeaveDeduction,

                // القروض
                LoanDeduction = dto.LoanDeduction,
                LoanInstallmentsCount = dto.LoanInstallmentsCount,

                // العقوبات
                SanctionAmount = dto.SanctionAmount,
                SanctionsCount = dto.SanctionsCount,

                // الإجماليات
                TotalAdditions = dto.TotalAdditions,
                TotalDeductions = dto.TotalDeductions,

                // ملخص
                Summary = dto.Summary,

                // تفاصيل إضافية
                LeaveDetails = dto.LeaveDetails,
                LoanDetails = dto.LoanDetails,
                SanctionDetails = dto.SanctionDetails,
                AttendanceDetails = dto.AttendanceDetails
            };
        }
        #endregion

        public async Task<Result<PayrollPreviewDto>> PreviewPayrollAsync(GeneratePayrollRequestDto request)
        {
            var userId = _currentUserService.UserId;
            if(string.IsNullOrEmpty(userId))
                return Result<PayrollPreviewDto>.Failure("غير مصرح بالدخول",HttpStatusCode.Unauthorized);
            
            try
            {
                bool hasEmployeeCode = !string.IsNullOrEmpty(request.EmployeeCode);
                bool hasRepresentativeCode = !string.IsNullOrEmpty(request.RepresentativeCode);
                if(!hasEmployeeCode && !hasRepresentativeCode)
                    return Result<PayrollPreviewDto>.Failure("يجب إدخال كود الموظف أو المندوب");
                if(hasEmployeeCode && hasRepresentativeCode)
                    return Result<PayrollPreviewDto>.Failure("لا يمكن إرسال كود الموظف والمندوب معًا");
                string userCode = hasEmployeeCode ? request.EmployeeCode! : request.RepresentativeCode!;

                var existingPayroll = await _unitOfWork.GetRepository<Payroll,int>()
                     .GetQueryable()
                     .FirstOrDefaultAsync(p =>
                         ((hasEmployeeCode && p.EmployeeCode == request.EmployeeCode) ||
                          (hasRepresentativeCode && p.RepresentativeCode == request.RepresentativeCode)) &&
                         p.PayPeriod.Year == request.Year &&
                         p.PayPeriod.Month == request.Month &&
                         !p.IsDeleted);
                if(existingPayroll != null)
                    return Result<PayrollPreviewDto>.Failure("تم إنشاء كشف الراتب لهذا الموظف في هذا الشهر مسبقاً");

                SalaryCalculationResult salaryDetails;
                
                if(hasEmployeeCode)
                {
                    var salaryResult = await _employeeService.GetEmployeeSalaryByYearAndMonth(
                        request.EmployeeCode!,request.Month,request.Year);
                    if(!salaryResult.IsSuccess)
                        return Result<PayrollPreviewDto>.Failure(salaryResult.Message!);
                    salaryDetails = MapFromEmployeeSalaryDto(salaryResult.Data!);
                }
                else
                {
                    // استخدام خدمة المندوبين لحساب الراتب
                    var salaryResult = await _representativeService.GetRepresentativeSalaryByYearAndMonth(
                        request.RepresentativeCode!,request.Month,request.Year);
                    if(!salaryResult.IsSuccess)
                        return Result<PayrollPreviewDto>.Failure(salaryResult.Message!);
                    salaryDetails = MapFromRepresentativeSalaryDto(salaryResult.Data!);
                }

                decimal totalLoanDeduction = 0;
                if(request.PayLoansFromSalary == true && salaryDetails.LoanDeduction > 0)
                {
                    totalLoanDeduction = salaryDetails.LoanDeduction;
                }

                var preview = new PayrollPreviewDto
                {
                    //EmployeeCode = !string.IsNullOrEmpty(request.EmployeeCode)
                    //    ? request.EmployeeCode : !string.IsNullOrEmpty(request.RepresentativeCode)
                    //        ? request.RepresentativeCode
                    //        :"",
                    //RepresentativeCode = request.RepresentativeCode,
                    EmployeeCode = hasEmployeeCode ? request.EmployeeCode : null,
                    RepresentativeCode = hasRepresentativeCode ? request.RepresentativeCode : null,

                    EmployeeName = !string.IsNullOrEmpty(salaryDetails.EmployeeName)
                        ? salaryDetails.EmployeeName
                        : !string.IsNullOrEmpty(salaryDetails.RepresentativeName)
                            ? salaryDetails.RepresentativeName
                            : "غير معروف",
                    Department = salaryDetails.DepartmentName ?? "غير محدد",
                    Month = request.Month,
                    Year = request.Year,
                    // الأساسيات
                    BasicSalary = salaryDetails.BasicSalary,
                    GrossSalary = salaryDetails.GrossSalary,
                    OvertimePay = salaryDetails.TotalOvertimePay,
                    // الخصومات (بدون قروض)
                    TotalDeductions = salaryDetails.TimeDeductionAmount +
                                     salaryDetails.AbsentDeduction +
                                     salaryDetails.UnpaidLeaveDeduction +
                                     salaryDetails.LateDeduction +
                                     salaryDetails.EarlyLeaveDeduction +
                                     salaryDetails.SanctionAmount,

                    TimeDeductions = salaryDetails.TimeDeductionAmount,
                    AbsentDeductions = salaryDetails.AbsentDeduction,
                    LeaveDeductions = salaryDetails.UnpaidLeaveDeduction,
                    LateDeductions = salaryDetails.LateDeduction,
                    EarlyLeaveDeductions = salaryDetails.EarlyLeaveDeduction,
                    SanctionDeductions = salaryDetails.SanctionAmount,

                    // معلومات القروض
                    HasPendingLoans = salaryDetails.HasPendingLoans,
                    PendingLoanAmount = salaryDetails.LoanDeduction,
                    LoanInstallmentsCount = salaryDetails.LoanInstallmentsCount,

                    DueInstallments = salaryDetails.LoanDetails.Select(ld => new LoanInstallmentDto
                    {
                        LoanNumber = ld.LoanNumber,
                        InstallmentAmount = ld.InstallmentAmount,
                        DueDate = ld.DueDate
                    }).ToList(),

                    // النتيجة
                    NetSalaryBeforeLoan = salaryDetails.NetSalaryBeforeLoans,
                    NetSalaryAfterLoan = salaryDetails.NetSalaryBeforeLoans - totalLoanDeduction,
                    // خيارات
                    DeductLoan = request.PayLoansFromSalary
                };
                return Result<PayrollPreviewDto>.Success(preview);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء كشف الراتب للمستخدم {UserCode}",request.EmployeeCode ?? request.RepresentativeCode);
                return Result<PayrollPreviewDto>.Failure($"خطأ في إنشاء كشف الراتب: {ex.Message}");
            }
        }
        public async Task<Result<PayrollResponseDto>> GeneratePayrollAsync(GeneratePayrollRequestDto request)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<PayrollResponseDto>.Failure("غير مصرح بالدخول", HttpStatusCode.Unauthorized);

            try
            {
                bool hasEmployeeCode = !string.IsNullOrEmpty(request.EmployeeCode);
                bool hasRepresentativeCode = !string.IsNullOrEmpty(request.RepresentativeCode);
                if(!hasEmployeeCode && !hasRepresentativeCode)
                    return Result<PayrollResponseDto>.Failure("يجب إدخال كود الموظف أو المندوب");

                if(hasEmployeeCode && hasRepresentativeCode)
                    return Result<PayrollResponseDto>.Failure("لا يمكن إرسال كود الموظف والمندوب معًا");

                var previewResult = await PreviewPayrollAsync(request);
                if(!previewResult.IsSuccess)
                    return Result<PayrollResponseDto>.Failure(previewResult.Message);

                var preview = previewResult.Data;

                var existingPayroll = await _unitOfWork.GetRepository<Payroll,int>()
                             .GetQueryable()
                             .FirstOrDefaultAsync(p =>
                                 ((hasEmployeeCode && p.EmployeeCode == request.EmployeeCode) ||
                                  (hasRepresentativeCode && p.RepresentativeCode == request.RepresentativeCode)) &&
                                 p.PayPeriod.Year == request.Year &&
                                 p.PayPeriod.Month == request.Month &&
                                 !p.IsDeleted);

                if(existingPayroll != null)
                    return Result<PayrollResponseDto>.Failure("تم إنشاء كشف الراتب مسبقاً");
                if(request.PayLoansFromSalary && preview!.NetSalaryAfterLoan < 0)
                    return Result<PayrollResponseDto>.Failure("لا يمكن خصم قيمه القرض من المرتب لان قيمه القرض اكبر من قيمه صافي المرتب");
                var payroll = new Payroll
                {
                    EmployeeCode = hasEmployeeCode ? request.EmployeeCode : null,
                    RepresentativeCode = hasRepresentativeCode ? request.RepresentativeCode : null,
                    PayPeriod = new DateTime(request.Year,request.Month,1),
                    GenerationDate = DateTime.Now,
                    PayDate = DateTime.Now,

                    // الأساسيات
                    BasicSalary = preview!.BasicSalary,
                    GrossSalary = preview.GrossSalary,
                    OvertimePay = preview.OvertimePay,
                    LoanDeduction = preview.DeductLoan ? preview.PendingLoanAmount : 0,

                    // الخصومات
                    TotalDeductions = preview.TotalDeductions,
                    TimeDeductions = preview.TimeDeductions,
                    AbsentDeductions = preview.AbsentDeductions,
                    LeaveDeductions = preview.LeaveDeductions,
                    LateDeductions = preview.LateDeductions,
                    EarlyLeaveDeductions = preview.EarlyLeaveDeductions,
                    SanctionDeductions = preview.SanctionDeductions,

                    // صافي الراتب
                    NetSalaryBeforeLoan = preview.NetSalaryBeforeLoan,
                    NetSalary = preview.DeductLoan ? preview.NetSalaryAfterLoan : preview.NetSalaryBeforeLoan,

                    // معلومات القروض
                    IsLoanDeducted = preview.DeductLoan,
                    HasPendingLoans = preview.HasPendingLoans,
                    PendingLoanAmount = preview.PendingLoanAmount,
                    LoanInstallmentsCount = preview.LoanInstallmentsCount,

                    Status = PayrollStatus.Created,
                    CreatedAt = DateTime.Now,
                    CreateBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.GetRepository<Payroll,int>().AddAsync(payroll);
                await _unitOfWork.SaveChangesAsync();

                var payrollDto = await MapToPayrollResponseDto(payroll);
                return Result<PayrollResponseDto>.Success(payrollDto,"تم إنشاء كشف الراتب بنجاح");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في إنشاء كشف الراتب للمستخدم {UserCode}",request.EmployeeCode ?? request.RepresentativeCode);
                return Result<PayrollResponseDto>.Failure($"خطأ في إنشاء كشف الراتب: {ex.Message}");
            }
        }
        public async Task<Result<PreviewBulkPayrollDto>> PreviewBulkPayrollAsync(GenerateBulkPayrollRequestDto request)
        {
            var userId = _currentUserService.UserId;
            if(string.IsNullOrEmpty(userId))
                return Result<PreviewBulkPayrollDto>.Failure("غير مصرح بالدخول",HttpStatusCode.Unauthorized);

            var result = new PreviewBulkPayrollDto
            {
                PreviewedAt = DateTime.Now,
                PreviewedBy = userId
            };

            try
            {
                var allUserCodes = request.UserCodes ?? new List<string>();
                var employees = new List<Employee>();
                var representatives = new List<Representatives>();

                if(allUserCodes.Any())
                {
                    foreach(var code in allUserCodes)
                    {
                        var codes = await GetUserCodesAsync(code);
                        if(codes.EmployeeCode != null)
                        {
                            var emp = await _unitOfWork.GetRepository<Employee,string>()
                                .FindAsync(e => e.EmployeeCode == codes.EmployeeCode && !e.IsDeleted);
                            if(emp != null)
                                employees.Add(emp);
                        }
                        else if(codes.RepresentativeCode != null)
                        {
                            var rep = await _unitOfWork.GetRepository<Representatives,string>()
                                .FindAsync(r => r.RepresentativesCode == codes.RepresentativeCode && !r.IsDeleted);
                            if(rep != null)
                                representatives.Add(rep);
                        }
                    }
                }
                else
                {
                    employees = await _unitOfWork.GetRepository<Employee,string>()
                        .GetQueryable()
                        .Include(e => e.User)
                        .Include(e => e.Department)
                        .Where(e => !e.IsDeleted)
                        .ToListAsync();

                    representatives = await _unitOfWork.GetRepository<Representatives,string>()
                        .GetQueryable()
                        .Include(r => r.User)
                        .Where(r => !r.IsDeleted)
                        .ToListAsync();
                }

                result.TotalEmployees = employees.Count + representatives.Count;

                var existingPayrolls = await _unitOfWork.GetRepository<Payroll,int>()
                                  .GetQueryable()
                                  .Where(p => p.PayPeriod.Year == request.Year
                                           && p.PayPeriod.Month == request.Month
                                           && !p.IsDeleted)
                                  .Select(p => new { p.EmployeeCode,p.RepresentativeCode,p.Id })
                                  .ToListAsync();

                foreach(var employee in employees)
                {
                    var employeeCode = employee.EmployeeCode!;
                    var existing = existingPayrolls.FirstOrDefault(p => p.EmployeeCode == employeeCode);
                    if(existing != null)
                    {
                        result.FailedPreviews.Add(new FailedPreviewDto
                        {
                            EmployeeCode = employeeCode,
                            EmployeeName = employee.User?.FullName ?? "غير معروف",
                            Department = employee.Department?.Name ?? "غير محدد",
                            Status = "Existing",
                            Message = "تم إنشاء كشف مرتب لهذا الشهر مسبقاً",
                            ExistingPayrollId = existing.Id
                        });
                        result.FailedCount++;
                        continue;
                    }

                    try
                    {
                        var previewRequest = new GeneratePayrollRequestDto
                        {
                            EmployeeCode = employeeCode,
                            Month = request.Month,
                            Year = request.Year,
                            PayLoansFromSalary = request.PayLoansFromSalary
                        };

                        var previewResult = await PreviewPayrollAsync(previewRequest);

                        if(!previewResult.IsSuccess)
                        {
                            result.FailedPreviews.Add(new FailedPreviewDto
                            {
                                EmployeeCode = employeeCode,
                                EmployeeName = employee.User?.FullName ?? "غير معروف",
                                Department = employee.Department?.Name ?? "غير محدد",
                                Status = "Error",
                                Message = previewResult.Message!
                            });
                            result.FailedCount++;
                        }
                        else
                        {
                            var preview = previewResult.Data!;
                            result.SuccessPreviews.Add(preview);
                            result.SuccessCount++;
                            result.TotalNetSalary += preview.DeductLoan ? preview.NetSalaryAfterLoan : preview.NetSalaryBeforeLoan;
                            result.TotalDeductions += preview.TotalDeductions;
                            result.TotalOvertime += preview.OvertimePay;
                            result.TotalBasicSalary += preview.BasicSalary;
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex,"خطأ في معاينة الموظف {EmployeeCode}",employeeCode);
                        result.FailedPreviews.Add(new FailedPreviewDto
                        {
                            EmployeeCode = employeeCode,
                            EmployeeName = employee.User?.FullName ?? "غير معروف",
                            Department = employee.Department?.Name ?? "غير محدد",
                            Status = "Error",
                            Message = $"خطأ في المعاينة: {ex.Message}"
                        });
                        result.FailedCount++;
                    }
                }
                foreach(var representative in representatives)
                {
                    var repCode = representative.RepresentativesCode!;
                    var existing = existingPayrolls.FirstOrDefault(p => p.RepresentativeCode == repCode);
                    if(existing != null)
                    {
                        result.FailedPreviews.Add(new FailedPreviewDto
                        {
                            RepresentativeCode = repCode,
                            EmployeeName = representative.User?.FullName ?? "غير معروف",
                            Department = "مندوب",
                            Status = "Existing",
                            Message = "تم إنشاء كشف مرتب لهذا الشهر مسبقاً",
                            ExistingPayrollId = existing.Id
                        });
                        result.FailedCount++;
                        continue;
                    }

                    try
                    {
                        var previewRequest = new GeneratePayrollRequestDto
                        {
                            RepresentativeCode = repCode,
                            Month = request.Month,
                            Year = request.Year,
                            PayLoansFromSalary = request.PayLoansFromSalary
                        };

                        var previewResult = await PreviewPayrollAsync(previewRequest);

                        if(!previewResult.IsSuccess)
                        {
                            result.FailedPreviews.Add(new FailedPreviewDto
                            {
                                RepresentativeCode = repCode,
                                EmployeeName = representative.User?.FullName ?? "غير معروف",
                                Department = "مندوب",
                                Status = "Error",
                                Message = previewResult.Message!
                            });
                            result.FailedCount++;
                        }
                        else
                        {
                            var preview = previewResult.Data!;
                            result.SuccessPreviews.Add(preview);
                            result.SuccessCount++;
                            result.TotalNetSalary += preview.DeductLoan ? preview.NetSalaryAfterLoan : preview.NetSalaryBeforeLoan;
                            result.TotalDeductions += preview.TotalDeductions;
                            result.TotalOvertime += preview.OvertimePay;
                            result.TotalBasicSalary += preview.BasicSalary;
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex,"خطأ في معاينة المندوب {RepresentativeCode}",repCode);
                        result.FailedPreviews.Add(new FailedPreviewDto
                        {
                            RepresentativeCode = repCode,
                            EmployeeName = representative.User?.FullName ?? "غير معروف",
                            Department = "مندوب",
                            Status = "Error",
                            Message = $"خطأ في المعاينة: {ex.Message}"
                        });
                        result.FailedCount++;
                    }
                }
                return Result<PreviewBulkPayrollDto>.Success(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في معاينة الرواتب الجماعية");
                return Result<PreviewBulkPayrollDto>.Failure($"خطأ في المعاينة الجماعية: {ex.Message}");
            }
        }
        public async Task<Result<BulkPayrollResultDto>> GenerateBulkPayrollAsync(GenerateBulkPayrollRequestDto request)
        {
            var userId = _currentUserService.UserId;
            if(string.IsNullOrEmpty(userId))
                return Result<BulkPayrollResultDto>.Failure("غير مصرح بالدخول",HttpStatusCode.Unauthorized);

            try
            {
                var previewResult = await PreviewBulkPayrollAsync(request);
                if(!previewResult.IsSuccess)
                    return Result<BulkPayrollResultDto>.Failure(previewResult.Message!);

                var preview = previewResult.Data!;

                if(preview.SuccessCount == 0)
                {
                    return Result<BulkPayrollResultDto>.Failure(
                        "لا يوجد موظفين يمكن إنشاء كشوف رواتب لهم. جميع الموظفين إما لديهم كشوف موجودة أو هناك أخطاء في البيانات.",
                        HttpStatusCode.BadRequest);
                }

                var result = new BulkPayrollResultDto
                {
                    GeneratedAt = DateTime.Now,
                    GeneratedBy = userId,
                    TotalEmployees = preview.TotalEmployees,
                    ProcessedSuccessfully = 0,
                    Failed = preview.FailedCount,
                    TotalNetSalary = 0,
                    TotalDeductions = 0,
                    TotalOvertime = 0
                };

                var details = new List<PayrollGenerationDetailDto>();
                List<Payroll> payrolls = new List<Payroll>();
                // معالجة الموظفين الناجحين فقط من المعاينة
                foreach(var payrollPreview in preview.SuccessPreviews)
                {
                    var detail = new PayrollGenerationDetailDto
                    {
                        EmployeeCode = payrollPreview.EmployeeCode,
                        RepresentativeCode = payrollPreview.RepresentativeCode,
                        EmployeeName = payrollPreview.EmployeeName,
                        Department = payrollPreview.Department
                    };
                 

                    try
                    {
                        var codes = await GetUserCodesAsync(payrollPreview.EmployeeCode);
                        bool isEmployee = codes.EmployeeCode != null;
                        bool isRepresentive = codes.RepresentativeCode != null;
                        var payroll = new Payroll
                        {
                            EmployeeCode = payrollPreview.EmployeeCode,
                            RepresentativeCode = payrollPreview.RepresentativeCode,
                            PayPeriod = new DateTime(request.Year,request.Month,1),
                            GenerationDate = DateTime.Now,
                            PayDate = DateTime.Now,

                            BasicSalary = payrollPreview.BasicSalary,
                            GrossSalary = payrollPreview.GrossSalary,
                            OvertimePay = payrollPreview.OvertimePay,
                            LoanDeduction = payrollPreview.DeductLoan ? payrollPreview.PendingLoanAmount : 0,

                            TotalDeductions = payrollPreview.TotalDeductions,
                            TimeDeductions = payrollPreview.TimeDeductions,
                            AbsentDeductions = payrollPreview.AbsentDeductions,
                            LeaveDeductions = payrollPreview.LeaveDeductions,
                            LateDeductions = payrollPreview.LateDeductions,
                            EarlyLeaveDeductions = payrollPreview.EarlyLeaveDeductions,
                            SanctionDeductions = payrollPreview.SanctionDeductions,

                            NetSalaryBeforeLoan = payrollPreview.NetSalaryBeforeLoan,
                            NetSalary = payrollPreview.DeductLoan ? payrollPreview.NetSalaryAfterLoan : payrollPreview.NetSalaryBeforeLoan,

                            IsLoanDeducted = payrollPreview.DeductLoan,
                            HasPendingLoans = payrollPreview.HasPendingLoans,
                            PendingLoanAmount = payrollPreview.PendingLoanAmount,
                            LoanInstallmentsCount = payrollPreview.LoanInstallmentsCount,

                            Status = PayrollStatus.Created,
                            CreatedAt = DateTime.Now,
                            CreateBy = userId,
                            IsDeleted = false
                        };
                        payrolls.Add(payroll);
                        //await _unitOfWork.GetRepository<Payroll,int>().AddAsync(payroll);

                        detail.BasicSalary = payroll.BasicSalary;
                        detail.OvertimePay = payroll.OvertimePay;
                        detail.Deductions = payroll.TotalDeductions;
                        detail.NetSalary = payroll.NetSalary;
                        detail.Status = "Success";
                        detail.PayrollId = payroll.Id;

                        result.ProcessedSuccessfully++;
                        result.TotalNetSalary += payroll.NetSalary;
                        result.TotalDeductions += payroll.TotalDeductions;
                        result.TotalOvertime += payroll.OvertimePay;

                        details.Add(detail);

                    }
                   
                    catch(Exception ex)
                    {
                        _logger.LogError(ex,"خطأ في إنشاء كشف الراتب للمستخدم {EmployeeCode}",payrollPreview.EmployeeCode);

                        detail.Status = "Failed";
                        detail.Message = $"خطأ في الإنشاء: {ex.Message}";
                        result.Failed++;
                        details.Add(detail);
                    }
                }
                if(payrolls.Count == preview.SuccessPreviews.Count)
                {
                   await  _unitOfWork.GetRepository<Payroll,int>().AddRangeAsync(payrolls);
                   
                    //await _unitOfWork.SaveChangesAsync();
                    payrolls.Clear();
                    details.Clear();
                }
                //await _unitOfWork.SaveChangesAsync();

                if(request.AutoPostToAccounting && result.ProcessedSuccessfully > 0)
                {
                    var payrollIds = details.Where(d => d.Status == "Success" && d.PayrollId.HasValue)
                                          .Select(d => d.PayrollId!.Value)
                                          .ToList();

                    var accountingResult = await PostBulkPayrollToAccountingAsync(payrollIds,request.ConfirmLoans);
                    if(accountingResult.IsSuccess)
                    {
                        result.AccountingEntryNumber = accountingResult.Data;
                    }
                }

                result.Details = details;

                return Result<BulkPayrollResultDto>.Success(result,
                    $"تم إنشاء {result.ProcessedSuccessfully} كشف مرتب بنجاح، فشل {result.Failed}");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في الإنشاء الجماعي للرواتب");
                return Result<BulkPayrollResultDto>.Failure($"خطأ في الإنشاء الجماعي: {ex.Message}");
            }
        }
        public async Task<Result<string>> PostPayrollToAccountingAsync(int payrollId,bool confirmLoans)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<string>.Failure("غير مصرح بالدخول", HttpStatusCode.Unauthorized);

            try
            {
                var payroll = await _unitOfWork.GetRepository<Payroll,int>()
                   .GetQueryable()
                   .Include(p => p.Employee).ThenInclude(e => e!.User)
                   .Include(p => p.Representative).ThenInclude(r => r!.User)
                   .FirstOrDefaultAsync(p => p.Id == payrollId && !p.IsDeleted);

                if(payroll == null)
                    return Result<string>.Failure("كشف الراتب غير موجود");
                if(payroll.Status != PayrollStatus.Created)
                    return Result<string>.Failure("لا يمكن تأكيد كشف الراتب");

                //if(payroll!.NetSalaryAfterLoan < 0)
                //    return Result<PayrollResponseDto>.Failure("لا يمكن خصم قيمه القرض من المرتب لان قيمه القرض اكبر من قيمه صافي المرتب");


                await CreateSalaryAccountingEntry(payroll);

                if(confirmLoans && payroll.IsLoanDeducted && payroll.LoanDeduction > 0)
                {
                    await CreateLoanPaymentAndAccountingEntry(payroll,userId);
                }

                payroll.Status = PayrollStatus.Approved;
                payroll.PostedToAccountingDate = DateTime.Now;
                payroll.IsPostedToAccounting = true;
                payroll.UpdateAt = DateTime.Now;
                payroll.UpdateBy = userId;

                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم تأكيد كشف الراتب وتسجيله في المحاسبة");           
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في التسجيل المحاسبي لكشف الراتب {PayrollId}", payrollId);
                return Result<string>.Failure($"خطأ في التسجيل المحاسبي: {ex.Message}");
            }
        }
        public async Task<Result<string>> PostBulkPayrollToAccountingAsync(List<int> payrollIds,bool confirmLoans)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<string>.Failure("غير مصرح بالدخول", HttpStatusCode.Unauthorized);

            try
            {
                var payrolls = await _unitOfWork.GetRepository<Payroll,int>()
                    .GetQueryable()
                    .Include(p => p.Employee).ThenInclude(e => e!.User)
                    .Include(p => p.Representative).ThenInclude(r => r!.User)
                    .Where(p => payrollIds.Contains(p.Id)
                             && p.Status == PayrollStatus.Created
                             && !p.IsDeleted)
                    .ToListAsync();
                if(!payrolls.Any())
                    return Result<string>.Failure("لا توجد كشوف مرتبات قابلة للتسجيل");

                int successCount = 0;
                int failedCount = 0;
                List<string> errors = new();

                foreach(var payroll in payrolls)
                {
                    try
                    {
                        await CreateSalaryAccountingEntry(payroll);

                        if(confirmLoans && payroll.IsLoanDeducted && payroll.PendingLoanAmount > 0)
                        {
                            await CreateLoanPaymentAndAccountingEntry(payroll,userId);
                        }

                        payroll.Status = PayrollStatus.Approved;
                        payroll.PostedToAccountingDate = DateTime.Now;
                        payroll.IsPostedToAccounting = true;
                        payroll.UpdateAt = DateTime.Now;
                        payroll.UpdateBy = userId;

                        successCount++;
                    }
                    catch(Exception ex)
                    {
                        failedCount++;
                        errors.Add($"كشف الراتب {payroll.Id} للمستخدم {payroll.EmployeeCode ?? payroll.RepresentativeCode}: {ex.Message}");
                        _logger.LogError(ex,"خطأ في تسجيل كشف الراتب {PayrollId}",payroll.Id);
                    }
                }
                await _unitOfWork.SaveChangesAsync();

                string message = $"تم تسجيل {successCount} كشف راتب في المحاسبة";

                if(failedCount > 0)
                {
                    message += $"، فشل {failedCount} كشف";
                    if(errors.Any())
                    {
                        message += $". الأخطاء: {string.Join("; ",errors.Take(5))}"; // نأخذ أول 5 أخطاء فقط
                    }
                }
                return Result<string>.Success(message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في التسجيل المحاسبي الجماعي");
                return Result<string>.Failure($"خطأ في التسجيل المحاسبي: {ex.Message}");
            }
        }
        public async Task<Result<string>> MarkPayrollAsPaidAsync(int payrollId, string paymentMethod, string? paymentReference = null)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<string>.Failure("غير مصرح بالدخول", HttpStatusCode.Unauthorized);

            try
            {
                var payroll = await _unitOfWork.GetRepository<Payroll, int>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(p => p.Id == payrollId && !p.IsDeleted);

                if (payroll == null)
                    return Result<string>.Failure("كشف الراتب غير موجود");

                if (payroll.Status != PayrollStatus.Approved)
                    return Result<string>.Failure("لا يمكن دفع كشف الراتب إلا إذا كان في حالة Posted");

                payroll.Status = PayrollStatus.Paid;
                payroll.PaidDate = DateTime.Now;
                payroll.PaidBy = userId;
                payroll.PaymentMethod = paymentMethod;
                payroll.PaymentReference = paymentReference;
                payroll.UpdateAt = DateTime.Now;
                payroll.UpdateBy = userId;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم تعيين كشف الراتب كمدفوع بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تعيين كشف الراتب {PayrollId} كمدفوع", payrollId);
                return Result<string>.Failure($"خطأ في تعيين كشف الراتب كمدفوع: {ex.Message}");
            }
        }
        public async Task<Result<string>> MarkBulkPayrollAsPaidAsync(List<int> payrollIds, string paymentMethod, string? paymentReference = null)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return Result<string>.Failure("غير مصرح بالدخول", HttpStatusCode.Unauthorized);

            try
            {
                var payrolls = await _unitOfWork.GetRepository<Payroll, int>()
                    .GetQueryable()
                    .Where(p => payrollIds.Contains(p.Id) && p.Status == PayrollStatus.Approved && !p.IsDeleted)
                    .ToListAsync();

                if (!payrolls.Any())
                    return Result<string>.Failure("لا توجد كشوف مرتبات قابلة للدفع");

                foreach (var payroll in payrolls)
                {
                    payroll.Status = PayrollStatus.Paid;
                    payroll.PaidDate = DateTime.Now;
                    payroll.PaidBy = userId;
                    payroll.PaymentMethod = paymentMethod;
                    payroll.PaymentReference = paymentReference;
                    payroll.UpdateAt = DateTime.Now;
                    payroll.UpdateBy = userId;
                }

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success($"تم تعيين {payrolls.Count} كشف راتب كمدفوع بنجاح");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تعيين كشوف الرواتب كمدفوع");
                return Result<string>.Failure($"خطأ في تعيين كشوف الرواتب كمدفوع: {ex.Message}");
            }
        }
        public async Task<Result<List<PayrollResponseDto>>> GetPayrollsByFilterAsync(PayrollFilterDto filter)
        {
            try
            {
                var query = _unitOfWork.GetRepository<Payroll,int>()
                    .GetQueryable()
                    .Include(p => p.Employee).ThenInclude(e => e!.User)
                    .Include(p => p.Employee).ThenInclude(e => e!.Department)
                    .Include(p => p.Representative).ThenInclude(r => r!.User)
                    .Where(p => !p.IsDeleted);

                if (filter.Month.HasValue)
                    query = query.Where(p => p.PayPeriod.Month == filter.Month.Value);

                if (filter.Year.HasValue)
                    query = query.Where(p => p.PayPeriod.Year == filter.Year.Value);

                if(filter.Status.HasValue)
                    query = query.Where(p => p.Status == filter.Status.Value);

                if(filter.FromDate.HasValue)
                    query = query.Where(p => p.PayDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(p => p.PayDate <= filter.ToDate.Value);

                if (filter.MinNetSalary.HasValue)
                    query = query.Where(p => p.NetSalary >= filter.MinNetSalary.Value);

                if (filter.MaxNetSalary.HasValue)
                    query = query.Where(p => p.NetSalary <= filter.MaxNetSalary.Value);

                if (filter.Departments != null && filter.Departments.Any())
                {
                    query = query.Where(p =>
                        (p.Employee != null && p.Employee.Department != null && filter.Departments.Contains(p.Employee.Department.Name)) ||
                        (p.Representative != null && filter.Departments.Contains("مندوب"))); 
                }

                var payrolls = await query.OrderByDescending(p => p.PayPeriod).ToListAsync();
                var result = new List<PayrollResponseDto>();
                foreach(var p in payrolls)
                {
                    result.Add(await MapToPayrollResponseDto(p));
                }
                return Result<List<PayrollResponseDto>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب كشوف الرواتب");
                return Result<List<PayrollResponseDto>>.Failure($"خطأ في جلب كشوف الرواتب: {ex.Message}");
            }
        }
        public async Task<Result<PayrollExportDto>> ExportPayrollsToExcelAsync(PayrollFilterDto filter)
        {
            try
            {
                var payrollsResult = await GetPayrollsByFilterAsync(filter);
                if(!payrollsResult.IsSuccess)
                    return Result<PayrollExportDto>.Failure(payrollsResult.Message);

                var payrolls = payrollsResult.Data;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("كشوف المرتبات");

                worksheet.Cells[1,1].Value = "كشوف المرتبات";
                worksheet.Cells[1,1,1,11].Merge = true;
                worksheet.Cells[1,1].Style.Font.Bold = true;
                worksheet.Cells[1,1].Style.Font.Size = 16;
                worksheet.Cells[1,1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                worksheet.Cells[3,1].Value = "كود المستخدم";
                worksheet.Cells[3,2].Value = "اسم المستخدم";
                worksheet.Cells[3,3].Value = "القسم";
                worksheet.Cells[3,4].Value = "فترة الراتب";
                worksheet.Cells[3,5].Value = "الراتب الأساسي";
                worksheet.Cells[3,6].Value = "العمل الإضافي";
                worksheet.Cells[3,7].Value = "الخصومات";
                worksheet.Cells[3,8].Value = "الصافي";
                worksheet.Cells[3,9].Value = "الحالة";
                worksheet.Cells[3,10].Value = "رقم القيد";
                worksheet.Cells[3,11].Value = "تاريخ الإنشاء";

                using(var range = worksheet.Cells[3,1,3,11])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                int row = 4;
                foreach(var payroll in payrolls)
                {
                    worksheet.Cells[row,1].Value = payroll.EmployeeCode;
                    worksheet.Cells[row,2].Value = payroll.EmployeeName;
                    worksheet.Cells[row,3].Value = payroll.Department;
                    worksheet.Cells[row,4].Value = payroll.PayPeriod.ToString("yyyy-MM");
                    worksheet.Cells[row,5].Value = payroll.BasicSalary;
                    worksheet.Cells[row,6].Value = payroll.OvertimePay;
                    worksheet.Cells[row,7].Value = payroll.TotalDeductions;
                    worksheet.Cells[row,8].Value = payroll.NetSalary;
                    worksheet.Cells[row,9].Value = payroll.Status;
                    worksheet.Cells[row,10].Value = payroll.AccountingEntryNumber;
                    worksheet.Cells[row,11].Value = payroll.CreatedAt.ToString("yyyy-MM-dd");
                    row++;
                }

                worksheet.Cells[4,5,row - 1,8].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[row,4].Value = "الإجمالي:";
                worksheet.Cells[row,4].Style.Font.Bold = true;
                worksheet.Cells[row,5].Formula = $"SUM(E4:E{row - 1})";
                worksheet.Cells[row,6].Formula = $"SUM(F4:F{row - 1})";
                worksheet.Cells[row,7].Formula = $"SUM(G4:G{row - 1})";
                worksheet.Cells[row,8].Formula = $"SUM(H4:H{row - 1})";
                worksheet.Cells[row,5,row,8].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row,5,row,8].Style.Font.Bold = true;

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var fileBytes = package.GetAsByteArray();
                var fileName = $"كشوف_المرتبات_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return Result<PayrollExportDto>.Success(new PayrollExportDto
                {
                    FileContent = fileBytes,
                    FileName = fileName,
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في تصدير كشوف المرتبات");
                return Result<PayrollExportDto>.Failure($"خطأ في التصدير: {ex.Message}");
            }
        }
        public async Task<Result<PayrollSummaryDto>> GetPayrollSummaryAsync(int month, int year)
        {
            try
            {

                var payrolls = await _unitOfWork.GetRepository<Payroll, int>()
                    .GetQueryable()
                    .Where(p => p.PayPeriod.Month == month && p.PayPeriod.Year == year && !p.IsDeleted)
                    .ToListAsync();

                var summary = new PayrollSummaryDto
                {
                    Month = month,
                    Year = year,
                    TotalEmployees = payrolls.Count,
                    TotalBasicSalary = payrolls.Sum(p => p.BasicSalary),
                    TotalOvertime = payrolls.Sum(p => p.OvertimePay),
                    TotalDeductions = payrolls.Sum(p => p.TotalDeductions),
                    TotalNetSalary = payrolls.Sum(p => p.NetSalary),
                    GeneratedCount = payrolls.Count(p => p.Status == PayrollStatus.Created),
                    PostedCount = payrolls.Count(p => p.Status == PayrollStatus.Approved),
                    PaidCount = payrolls.Count(p => p.Status == PayrollStatus.Paid)
                };

                return Result<PayrollSummaryDto>.Success(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب ملخص الرواتب لشهر {Month}/{Year}", month, year);
                return Result<PayrollSummaryDto>.Failure($"خطأ في جلب الملخص: {ex.Message}");
            }
        }
        public async Task<Result<PayrollResponseDto>> GetPayrollByIdAsync(int id)
        {
            try
            {
                var payroll = await _unitOfWork.GetRepository<Payroll,int>()
                    .GetQueryable()
                    .Include(p => p.Employee).ThenInclude(e => e!.User)
                    .Include(p => p.Employee).ThenInclude(e => e!.Department)
                    .Include(p => p.Representative).ThenInclude(r => r!.User)
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
                if (payroll == null)
                    return Result<PayrollResponseDto>.Failure("كشف الراتب غير موجود");

                var payrollDto = await MapToPayrollResponseDto(payroll);
                return Result<PayrollResponseDto>.Success(payrollDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب كشف الراتب {Id}", id);
                return Result<PayrollResponseDto>.Failure($"خطأ في جلب كشف الراتب: {ex.Message}");
            }
        }
        public async Task<Result<List<PayrollResponseDto>>> GetEmployeePayrollsAsync(string employeeCode, int? year = null)
        {
            return await GetUserPayrollsAsync(employeeCode,year);
        }
        public async Task<Result<List<PayrollResponseDto>>> GetUserPayrollsAsync(string userCode,int? year = null)
        {
            try
            {
                var codes = await GetUserCodesAsync(userCode);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<List<PayrollResponseDto>>.Failure("المستخدم غير موجود");

                var query = _unitOfWork.GetRepository<Payroll,int>()
                    .GetQueryable()
                    .Include(p => p.Employee).ThenInclude(e => e!.User)
                    .Include(p => p.Employee).ThenInclude(e => e!.Department)
                    .Include(p => p.Representative).ThenInclude(r => r!.User)
                    .Where(p => !p.IsDeleted);

                if(codes.EmployeeCode != null)
                    query = query.Where(p => p.EmployeeCode == codes.EmployeeCode);
                else
                    query = query.Where(p => p.RepresentativeCode == codes.RepresentativeCode);

                if(year.HasValue)
                    query = query.Where(p => p.PayPeriod.Year == year.Value);

                var payrolls = await query.OrderByDescending(p => p.PayPeriod).ToListAsync();
                var result = new List<PayrollResponseDto>();
                foreach(var p in payrolls)
                {
                    result.Add(await MapToPayrollResponseDto(p));
                }

                return Result<List<PayrollResponseDto>>.Success(result);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"خطأ في جلب كشوف رواتب المستخدم {UserCode}",userCode);
                return Result<List<PayrollResponseDto>>.Failure($"خطأ في جلب كشوف الرواتب: {ex.Message}");
            }
        }
        public async Task<Result<string>> DeletePayrollAsync(int PayrollID)
        {
            var userId = _currentUserService.UserId;
            if(string.IsNullOrEmpty(userId))
                return Result<string>.Failure("غير مصرح بالدخول",HttpStatusCode.Unauthorized);

            try
            {
                var payroll = await _unitOfWork.GetRepository<Payroll,int>()
                   .GetByIdAsync(PayrollID);
                if(payroll == null)
                    return Result<string>.Failure("الكشف غير موجود ");

                if(payroll.Status == PayrollStatus.Approved || payroll.Status == PayrollStatus.Paid)
                    return Result<string>.Failure("لا يمكن حذف هذا الكشف ");

                await _unitOfWork.GetRepository<Payroll,int>().DeleteAsync(payroll);
                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم حذف الكشف بنجاح",HttpStatusCode.OK);
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"خطأ في حذف كشف المرتب : {ex.Message}");
            }
        }

    }
}