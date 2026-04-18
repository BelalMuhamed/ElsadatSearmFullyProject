using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.PayrollDeductions;
using Application.Helper;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.PayrollDeduction;
using Domain.Common;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.PayrollDeductionServices
{
    internal class PayrollDeductionService : IPayrollDeductionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public PayrollDeductionService(IUnitOfWork unitOfWork,ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        #region Helper Methods
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
        private async Task<(bool IsSuccess, decimal Salary, TimeSpan WorkHoursPerDay, WeekDays WeekHoliday1, WeekDays? WeekHoliday2, string ErrorMessage)> GetUserSalaryInfoAsync(string? employeeCode,string? representativeCode)
        {
            try
            {
                // التأكد من وجود كود واحد فقط
                if(string.IsNullOrEmpty(employeeCode) && string.IsNullOrEmpty(representativeCode))
                    return (false, 0, TimeSpan.Zero, default, null, "يجب توفير كود الموظف أو المندوب");

                // تحديد النوع
                bool hasEmployee = !string.IsNullOrEmpty(employeeCode);

                if(hasEmployee)
                {
                    var employee = await _unitOfWork.GetRepository<Employee,string>()
                        .GetQueryable()
                        .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);

                    if(employee == null)
                        return (false, 0, TimeSpan.Zero, default, null, "الموظف غير موجود");

                    var workHoursPerDay = employee.TimeOut.ToTimeSpan() - employee.TimeIn.ToTimeSpan();
                    return (true, employee.Salary, workHoursPerDay, employee.WeekHoliday1, employee.WeekHoliday2, null);
                }
                else
                {
                    var representative = await _unitOfWork.GetRepository<Representatives,string>()
                        .GetQueryable()
                        .FirstOrDefaultAsync(r => r.RepresentativesCode == representativeCode && !r.IsDeleted);

                    if(representative == null)
                        return (false, 0, TimeSpan.Zero, default, null, "المندوب غير موجود");

                    var workHoursPerDay = representative.TimeOut.ToTimeSpan() - representative.TimeIn.ToTimeSpan();
                    return (true, representative.Salary, workHoursPerDay, representative.WeekHoliday1, representative.WeekHoliday2, null);
                }
            }
            catch(Exception ex)
            {
                return (false, 0, TimeSpan.Zero, default, null, $"خطأ في جلب معلومات المستخدم: {ex.Message}");
            }
        }
        private async Task<Result<decimal>> CalculateDeductionAmount(string? employeeCode,string? representativeCode,
            decimal deductionAmount,DateTime deductionDate)
        {
            try
            {
                var (isSuccess, salary, workHoursPerDay, weekHoliday1, weekHoliday2, errorMessage) =
                    await GetUserSalaryInfoAsync(employeeCode,representativeCode);

                if(!isSuccess)
                    return Result<decimal>.Failure(errorMessage);

                if(salary <= 0)
                    return Result<decimal>.Failure("الراتب غير محدد أو يساوي صفر");

                // جلب العطلات الرسمية
                var publicHolidays = await _unitOfWork.GetRepository<PublicHoliday,int>()
                    .GetQueryable()
                    .Where(ph => ph.Date.Year == deductionDate.Year && ph.Date.Month == deductionDate.Month)
                    .ToListAsync();

                // حساب أيام العمل
                int totalWorkingDays = EmployeeWorkingDaysCalculate.CalculateWorkingDays(
                    deductionDate.Year,
                    deductionDate.Month,
                    weekHoliday1,
                    weekHoliday2,
                    publicHolidays);

                // حساب إجمالي ساعات العمل في الشهر
                double totalHoursWorked = totalWorkingDays * workHoursPerDay.TotalHours;

                if(totalHoursWorked <= 0)
                    return Result<decimal>.Failure("لا يمكن حساب المعدل الساعي لأن ساعات العمل صفر أو أقل");

                decimal hourlyRate = salary / (decimal) totalHoursWorked;
                decimal moneyAmount = deductionAmount * hourlyRate;

                return Result<decimal>.Success(moneyAmount);
            }
            catch(Exception ex)
            {
                return Result<decimal>.Failure($"خطأ في حساب المبلغ: {ex.Message}");
            }
        }
        #endregion
        public async Task<Result<string>> AddPayrollDeductionAsync(PayrollDeductionsDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                bool hasEmployeeCode = !string.IsNullOrEmpty(dto.EmployeeCode);
                bool hasRepresentativeCode = !string.IsNullOrEmpty(dto.RepresentativeCode);
                if(!hasEmployeeCode && !hasRepresentativeCode)
                    return Result<string>.Failure("يجب إدخال كود الموظف أو المندوب");

                if(hasEmployeeCode)
                {
                    var employeeExists = await _unitOfWork.GetRepository<Employee,string>()
                        .AnyAsync(e => e.EmployeeCode == dto.EmployeeCode && !e.IsDeleted);
                    if(!employeeExists)
                        return Result<string>.Failure("الموظف غير موجود أو محذوف");
                }
                else
                {
                    var representativeExists = await _unitOfWork.GetRepository<Representatives,string>()
                        .AnyAsync(r => r.RepresentativesCode == dto.RepresentativeCode && !r.IsDeleted);
                    if(!representativeExists)
                        return Result<string>.Failure("المندوب غير موجود أو محذوف");
                }

                var existingQuery = _unitOfWork.GetRepository<PayrollDeductions,int>()
                    .GetQueryable()
                    .Where(d => d.DeductionDate.Date == dto.DeductionDate.Date && !d.IsDeleted);

                if(hasEmployeeCode)
                    existingQuery = existingQuery.Where(d => d.EmployeeCode == dto.EmployeeCode);
                else
                    existingQuery = existingQuery.Where(d => d.RepresentativeCode == dto.RepresentativeCode);

                var existingDeduction = await existingQuery.AnyAsync();
                if(existingDeduction)
                    return Result<string>.Failure("يوجد خصم لهذا المستخدم في نفس التاريخ بالفعل");

                var calculationResult = await CalculateDeductionAmount(
                  dto.EmployeeCode,
                  dto.RepresentativeCode,
                  dto.DeductionAmount,
                  dto.DeductionDate);

                if(!calculationResult.IsSuccess)
                    return Result<string>.Failure("يوجد مشكله في حساب قيمه الخصم");

                var deduction = new PayrollDeductions
                {
                    EmployeeCode = dto.EmployeeCode,
                    RepresentativeCode = dto.RepresentativeCode,
                    DeductionDate = dto.DeductionDate,
                    DeductionAmount = dto.DeductionAmount,
                    MoneyAmount = calculationResult.Data,
                    DeductionReason = dto.DeductionReason,
                    CreatedAt = DateTime.Now,
                    CreateBy = userId,
                    IsDeleted = false
                };

                await _unitOfWork.GetRepository<PayrollDeductions, int>().AddAsync(deduction);
                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تمت اضافه الخصم بنجاح");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ أثناء إضافة الخصم: {ex.Message}");
            }
        }
        public async Task<PagedList<DeductionDetailDto>> GetAllPayrollDeductionsAsync(PaginationParams paginationParams)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return new PagedList<DeductionDetailDto>(
                        new List<DeductionDetailDto>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
                                         
                var query = _unitOfWork.GetRepository<PayrollDeductions, int>()
                   .GetQueryable()
                   .AsNoTracking()
                   .Where(d => !d.IsDeleted);

                var employees = _unitOfWork.GetRepository<Employee,string>().GetQueryable()
                   .Include(e => e.User);
                var representatives = _unitOfWork.GetRepository<Representatives,string>().GetQueryable()
                    .Include(r => r.User);

                var joinedQuery = from deduction in query
                                  join employee in employees on deduction.EmployeeCode equals employee.EmployeeCode into empJoin
                                  from emp in empJoin.DefaultIfEmpty()
                                  join representative in representatives on deduction.RepresentativeCode equals representative.RepresentativesCode into repJoin
                                  from rep in repJoin.DefaultIfEmpty()
                                  select new DeductionDetailDto
                                  {
                                      Id = deduction.Id,
                                      EmployeeCode = deduction.EmployeeCode,
                                      RepresentativeCode = deduction.RepresentativeCode,
                                      EmployeeName = emp != null ? emp.User!.FullName : (rep != null ? rep.User!.FullName : ""),
                                      DeductionDate = deduction.DeductionDate,
                                      DeductionAmount = deduction.DeductionAmount,
                                      MonayAmount = deduction.MoneyAmount,
                                      DeductionReason = deduction.DeductionReason,
                                      CreatedAt = deduction.CreatedAt,
                                      CreateBy = deduction.CreateBy,
                                      UpdateBy = deduction.UpdateBy,
                                      DeleteBy = deduction.DeleteBy,
                                      UpdateAt = deduction.UpdateAt,
                                      IsDeleted = deduction.IsDeleted,
                                      DeleteAt = deduction.DeleteAt
                                  };

                var orderedQuery = joinedQuery.OrderByDescending(d => d.DeductionDate);
                var pagedResult = await orderedQuery.ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);
                return pagedResult;
            }
            catch (Exception)
            {
                return new PagedList<DeductionDetailDto>(
                    new List<DeductionDetailDto>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            }
        }
        public async Task<Result<DeductionDetailDto>> GetPayrollDeductionByIdAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<DeductionDetailDto>.Failure("يجب تسجيل الدخول أولاً");

                var deduction = await _unitOfWork.GetRepository<PayrollDeductions,int>()
                    .GetQueryable()
                    .Where(d => d.Id == id)
                    .Select(d => new { Deduction = d })
                    .FirstOrDefaultAsync();

                if(deduction == null)
                    return Result<DeductionDetailDto>.Failure("هذا الخصم غير موجود");

                string employeeName = await GetUserNameAsync(deduction.Deduction.EmployeeCode,deduction.Deduction.RepresentativeCode);

                var dto = new DeductionDetailDto
                {
                    Id = deduction.Deduction.Id,
                    EmployeeCode = deduction.Deduction.EmployeeCode,
                    RepresentativeCode = deduction.Deduction.RepresentativeCode,
                    EmployeeName = employeeName,
                    DeductionDate = deduction.Deduction.DeductionDate,
                    DeductionAmount = deduction.Deduction.DeductionAmount,
                    MonayAmount = deduction.Deduction.MoneyAmount,
                    DeductionReason = deduction.Deduction.DeductionReason,
                    CreatedAt = deduction.Deduction.CreatedAt,
                    CreateBy = deduction.Deduction.CreateBy,
                    UpdateBy = deduction.Deduction.UpdateBy,
                    DeleteBy = deduction.Deduction.DeleteBy,
                    UpdateAt = deduction.Deduction.UpdateAt,
                    IsDeleted = deduction.Deduction.IsDeleted,
                    DeleteAt = deduction.Deduction.DeleteAt
                };
                return Result<DeductionDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<DeductionDetailDto>.Failure($"حدث خطأ أثناء جلب الخصم: {ex.Message}");
            }
        }
        public async Task<Result<string>> UpdatePayrollDeductionAsync(PayrollDeductionsDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                bool hasEmployeeCode = !string.IsNullOrEmpty(dto.EmployeeCode);
                bool hasRepresentativeCode = !string.IsNullOrEmpty(dto.RepresentativeCode);
                if(!hasEmployeeCode && !hasRepresentativeCode)
                    return Result<string>.Failure("يجب إدخال كود الموظف أو المندوب");

                var query = _unitOfWork.GetRepository<PayrollDeductions,int>()
                   .GetQueryable()
                   .Where(d => !d.IsDeleted);

                if(hasEmployeeCode)
                    query = query.Where(d => d.EmployeeCode == dto.EmployeeCode);
                else
                    query = query.Where(d => d.RepresentativeCode == dto.RepresentativeCode);

                var deduction = await query.FirstOrDefaultAsync(d => d.DeductionDate == dto.DeductionDate);

                if(deduction == null)
                    return Result<string>.Failure("هذا الخصم غير موجود");
                if (deduction.IsDeleted)
                    return Result<string>.Failure("لا يمكن تعديل خصم محذوف");

                if(deduction.DeductionAmount != dto.DeductionAmount)
                {
                    deduction.DeductionAmount = dto.DeductionAmount;

                    // إعادة حساب المبلغ
                    var calculationResult = await CalculateDeductionAmount(
                        dto.EmployeeCode,
                        dto.RepresentativeCode,
                        dto.DeductionAmount,
                        dto.DeductionDate);

                    if(!calculationResult.IsSuccess)
                        return Result<string>.Failure("يوجد مشكلة في حساب قيمة الخصم");

                    deduction.MoneyAmount = calculationResult.Data;
                }

                deduction.DeductionReason = dto.DeductionReason;
                deduction.UpdateAt = DateTime.UtcNow;
                deduction.UpdateBy = userId;

                await _unitOfWork.GetRepository<PayrollDeductions,int>().UpdateAsync(deduction);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم تحديث الخصم بنجاح");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ أثناء تحديث الخصم: {ex.Message}");
            }
        }
        public async Task<Result<string>> SoftDeletePayrollDeductionAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var deduction = await _unitOfWork.GetRepository<PayrollDeductions, int>().GetByIdAsync(id);
                if (deduction == null)
                    return Result<string>.Failure("هذا الخصم غير موجود");
                if (deduction.IsDeleted)
                    return Result<string>.Failure("هذا الخصم محذوف بالفعل");

                deduction.IsDeleted = true;
                deduction.DeleteBy = userId;
                deduction.DeleteAt = DateTime.Now;

                await _unitOfWork.GetRepository<PayrollDeductions, int>().UpdateAsync(deduction);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم حذف الخصم بنجاح");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ أثناء تحديث الخصم: {ex.Message}");
            }
        }
        public async Task<Result<string>> RestorePayrollDeductionAsync(int id)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var deduction = await _unitOfWork.GetRepository<PayrollDeductions, int>().GetByIdAsync(id);
                if (deduction == null)
                    return Result<string>.Failure("هذا الخصم غير موجود");

                if (!deduction.IsDeleted)
                    return Result<string>.Failure("هذا الخصم غير محذوف");

                deduction.IsDeleted = false;
                deduction.UpdateAt = DateTime.Now;
                deduction.UpdateBy = userId;
                deduction.DeleteBy = null;
                deduction.DeleteAt = null;

                await _unitOfWork.GetRepository<PayrollDeductions, int>().UpdateAsync(deduction);
                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم استعادة الخصم بنجاح");
            }
            catch (Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ أثناء استعادة الخصم: {ex.Message}");
            }
        }
        public async Task<Result<EmployeeDeductionsSummaryDto>> GetEmployeeDeductionsWithSummaryAsync
            (string userCode,int? selectedMonth = null,int? selectedYear = null)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return Result<EmployeeDeductionsSummaryDto>.Failure("يجب تسجيل الدخول أولاً");

                var codes = await GetUserCodesAsync(userCode);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<EmployeeDeductionsSummaryDto>.Failure("المستخدم غير موجود");

                string userName = await GetUserNameAsync(codes.EmployeeCode,codes.RepresentativeCode);

                var query = _unitOfWork.GetRepository<PayrollDeductions,int>()
                                   .GetQueryable()
                                   .AsNoTracking()
                                   .Where(d => !d.IsDeleted);
                
                if(codes.EmployeeCode != null)
                    query = query.Where(d => d.EmployeeCode == codes.EmployeeCode);
                else
                    query = query.Where(d => d.RepresentativeCode == codes.RepresentativeCode);

                if(selectedMonth.HasValue)
                {
                    query = query.Where(d => d.DeductionDate.Month == selectedMonth.Value);
                }

                if (selectedYear.HasValue)
                {
                    query = query.Where(d => d.DeductionDate.Year == selectedYear.Value);
                }

                var deductionsList = await query
                                   .OrderByDescending(d => d.DeductionDate)
                                   .Select(d => new DeductionDetailDto
                                   {
                                       Id = d.Id,
                                       EmployeeCode = d.EmployeeCode,
                                       RepresentativeCode = d.RepresentativeCode,
                                       EmployeeName = userName,
                                       DeductionDate = d.DeductionDate,
                                       DeductionAmount = d.DeductionAmount,
                                       MonayAmount = d.MoneyAmount,
                                       DeductionReason = d.DeductionReason,
                                       CreatedAt = d.CreatedAt,
                                       CreateBy = d.CreateBy,
                                       UpdateBy = d.UpdateBy,
                                       DeleteBy = d.DeleteBy,
                                       UpdateAt = d.UpdateAt,
                                       IsDeleted = d.IsDeleted,
                                       DeleteAt = d.DeleteAt
                                   })
                                   .ToListAsync();
                var totals = await query
                                   .GroupBy(d => 1)
                                   .Select(g => new DeductionTotalsDto
                                   {
                                       TotalDeductionHours = g.Sum(d => d.DeductionAmount),
                                       TotalMoneyAmount = g.Sum(d => d.MoneyAmount),
                                       TotalRecords = g.Count(),
                                       EmployeeCode = userCode,
                                       EmployeeName = userName,
                                       Month = selectedMonth,
                                       Year = selectedYear
                                   })
                                   .FirstOrDefaultAsync();

                var result = new EmployeeDeductionsSummaryDto
                {
                    Deductions = deductionsList,
                    Totals = totals ?? new DeductionTotalsDto
                    {
                        EmployeeCode = userCode,
                        EmployeeName = userName,
                        Month = selectedMonth,
                        Year = selectedYear
                    }
                };
                return Result<EmployeeDeductionsSummaryDto>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<EmployeeDeductionsSummaryDto>.Failure($"حدث خطأ أثناء جلب البيانات: {ex.Message}");
            }
        }
        public async Task<PagedList<DeductionDetailDto>> SearchPayrollDeductionsAsync(
            PayrollDeductionSearchDto searchDto,
            PaginationParams paginationParams)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return new PagedList<DeductionDetailDto>(
                        new List<DeductionDetailDto>(), 0, paginationParams.PageNumber, paginationParams.PageSize);

                var query = _unitOfWork.GetRepository<PayrollDeductions, int>()
                    .GetQueryable()
                    .AsNoTracking();
                
                if(!string.IsNullOrEmpty(searchDto.EmployeeCode))
                {
                    var codes = await GetUserCodesAsync(searchDto.EmployeeCode);
                    if(codes.EmployeeCode != null)
                        query = query.Where(d => d.EmployeeCode == codes.EmployeeCode);
                    else if(codes.RepresentativeCode != null)
                        query = query.Where(d => d.RepresentativeCode == codes.RepresentativeCode);
                }

                if (searchDto.Month.HasValue)
                {
                    query = query.Where(d => d.DeductionDate.Month == searchDto.Month.Value);
                }

                if (searchDto.Year.HasValue)
                {
                    query = query.Where(d => d.DeductionDate.Year == searchDto.Year.Value);
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(d => d.DeductionDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(d => d.DeductionDate <= searchDto.ToDate.Value);
                }

                if (!searchDto.IncludeDeleted.GetValueOrDefault())
                {
                    query = query.Where(d => !d.IsDeleted);
                }

                var employees = _unitOfWork.GetRepository<Employee,string>().GetQueryable()
                   .Include(e => e.User);
                var representatives = _unitOfWork.GetRepository<Representatives,string>().GetQueryable()
                    .Include(r => r.User);

                var joinedQuery = from deduction in query
                                  join employee in employees on deduction.EmployeeCode equals employee.EmployeeCode into empJoin
                                  from emp in empJoin.DefaultIfEmpty()
                                  join representative in representatives on deduction.RepresentativeCode equals representative.RepresentativesCode into repJoin
                                  from rep in repJoin.DefaultIfEmpty()
                                  select new DeductionDetailDto
                                  {
                                      Id = deduction.Id,
                                      EmployeeCode = deduction.EmployeeCode,
                                      RepresentativeCode = deduction.RepresentativeCode,
                                      EmployeeName = emp != null ? emp.User!.FullName : (rep != null ? rep.User!.FullName : ""),
                                      DeductionDate = deduction.DeductionDate,
                                      DeductionAmount = deduction.DeductionAmount,
                                      MonayAmount = deduction.MoneyAmount,
                                      DeductionReason = deduction.DeductionReason,
                                      CreatedAt = deduction.CreatedAt,
                                      CreateBy = deduction.CreateBy,
                                      UpdateBy = deduction.UpdateBy,
                                      DeleteBy = deduction.DeleteBy,
                                      UpdateAt = deduction.UpdateAt,
                                      IsDeleted = deduction.IsDeleted,
                                      DeleteAt = deduction.DeleteAt
                                  };

                var orderedQuery = joinedQuery.OrderByDescending(d => d.DeductionDate);
                var pagedResult = await orderedQuery.ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);
                return pagedResult;
            }
            catch (Exception)
            {
                return new PagedList<DeductionDetailDto>(
                    new List<DeductionDetailDto>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            }
        }
 
    }
}
