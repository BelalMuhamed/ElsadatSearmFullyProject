using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeLeaveBalance;
using Application.DTOs.EmployeeLeaveRequest;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeeLeave;
using Domain.Common;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.EmployeeLeaveServices
{
    public class EmployeeLeaveService:IEmployeeLeaveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public EmployeeLeaveService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _UserManager = userManager;
        }
        #region Helper Methods
        // ---------- الدوال المساعدة ----------
        // دالة لتحديد نوع المستخدم بناءً على الكود
        private async Task<(string? EmployeeCode, string? RepresentativeCode)> GetUserCodesAsync(string? code)
        {
            if(string.IsNullOrEmpty(code))
                return (null, null);

            // التحقق من وجود الكود في جدول الموظفين
            var employee = await _unitOfWork.GetRepository<Employee,string>()
                .FindAsync(e => e.EmployeeCode == code && !e.IsDeleted);
            if(employee != null)
                return (code, null);

            // التحقق من وجود الكود في جدول المناديب
            var representative = await _unitOfWork.GetRepository<Representatives,string>()
                .FindAsync(r => r.RepresentativesCode == code && !r.IsDeleted);
            if(representative != null)
                return (null, code);

            return (null, null);
        }
        // دالة لجلب أو إنشاء رصيد إجازة لموظف أو مندوب
        private async Task<EmployeeLeaveBalance> GetOrCreateBalanceForUserAsync(
            string? employeeCode,string? representativeCode,int leaveTypeId,int year)
        {
            var repo = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>();

            // البحث عن الرصيد
            EmployeeLeaveBalance? balance = null;
            if(!string.IsNullOrEmpty(employeeCode))
            {
                balance = await repo.GetQueryable()
                    .FirstOrDefaultAsync(b => b.EmployeeCode == employeeCode &&
                                              b.LeaveTypeId == leaveTypeId &&
                                              b.Year == year &&
                                              !b.IsDeleted);
            }
            else if(!string.IsNullOrEmpty(representativeCode))
            {
                balance = await repo.GetQueryable()
                    .FirstOrDefaultAsync(b => b.RepresentativeCode == representativeCode &&
                                              b.LeaveTypeId == leaveTypeId &&
                                              b.Year == year &&
                                              !b.IsDeleted);
            }

            if(balance == null)
            {
                balance = new EmployeeLeaveBalance
                {
                    EmployeeCode = employeeCode,
                    RepresentativeCode = representativeCode,
                    LeaveTypeId = leaveTypeId,
                    Year = year,
                    OpeningBalance = 0,
                    Accrued = 0,
                    Used = 0,
                    Remaining = 0,
                    CreatedAt = DateTime.Now,
                    CreateBy = "System"
                };
                await repo.AddAsync(balance);
                await _unitOfWork.SaveChangesAsync();
            }

            return balance;
        }
        // دالة لجلب أيام العطل الأسبوعية للمستخدم
        private async Task<List<DayOfWeek>> GetUserWeekendDaysAsync(string? employeeCode,string? representativeCode)
        {
            if(!string.IsNullOrEmpty(employeeCode))
            {
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .FindAsync(e => e.EmployeeCode == employeeCode && !e.IsDeleted);
                if(employee != null)
                {
                    var weekends = new List<DayOfWeek> { ConvertWeekDaysToDayOfWeek(employee.WeekHoliday1) };
                    if(employee.WeekHoliday2.HasValue)
                        weekends.Add(ConvertWeekDaysToDayOfWeek(employee.WeekHoliday2.Value));
                    return weekends;
                }
            }
            else if(!string.IsNullOrEmpty(representativeCode))
            {
                var representative = await _unitOfWork.GetRepository<Representatives,string>()
                    .FindAsync(r => r.RepresentativesCode == representativeCode && !r.IsDeleted);
                if(representative != null)
                {
                    // افترض أن Representative لديه WeekHoliday1, WeekHoliday2
                    var weekends = new List<DayOfWeek> { ConvertWeekDaysToDayOfWeek(representative.WeekHoliday1) };
                    if(representative.WeekHoliday2.HasValue)
                        weekends.Add(ConvertWeekDaysToDayOfWeek(representative.WeekHoliday2.Value));
                    return weekends;
                }
            }
            return new List<DayOfWeek> { DayOfWeek.Friday }; // الافتراضي
        }
        // دالة لجلب معلومات المستخدم (الاسم) بناءً على الكود
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
        // دالة لحساب ايام العمل الفعليه بين تاريخين
        private async Task<decimal> CalculateWorkingDaysAsync(string? employeeCode,string? representativeCode,DateTime fromDate,DateTime toDate)
        {
            decimal totalDays = 0;
            var currentDate = fromDate;

            // الحصول على أيام العطل الأسبوعية حسب نوع المستخدم
            var weekends = await GetUserWeekendDaysAsync(employeeCode,representativeCode);

            // الحصول على العطلات الرسمية في الفترة
            var holidays = await GetPublicHolidaysBetweenDatesAsync(fromDate,toDate);

            while(currentDate <= toDate)
            {
                // استبعاد نهاية الأسبوع
                if(weekends.Contains(currentDate.DayOfWeek))
                {
                    currentDate = currentDate.AddDays(1);
                    continue;
                }

                // استبعاد العطلات الرسمية
                var dateOnly = DateOnly.FromDateTime(currentDate);
                if(!holidays.Any(h => h.Date == dateOnly))
                {
                    totalDays++;
                }

                currentDate = currentDate.AddDays(1);
            }

            return totalDays;
        }
        // دالة لجلب محفظه الاجازات لمستخدم معين بناءً على كود الموظف أو المندوب
        private async Task<EmployeeLeaveBalance?> GetLeaveBalanceForUserAsync(string? employeeCode,string? representativeCode,int leaveTypeId,int year)
        {
            var query = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                .GetQueryable()
                .Where(b => b.LeaveTypeId == leaveTypeId && b.Year == year && !b.IsDeleted);

            if(!string.IsNullOrEmpty(employeeCode))
                query = query.Where(b => b.EmployeeCode == employeeCode);
            else if(!string.IsNullOrEmpty(representativeCode))
                query = query.Where(b => b.RepresentativeCode == representativeCode);
            else
                return null;

            return await query.FirstOrDefaultAsync();
        }
        // دالة للتحقق من وجود طلبات إجازة متداخلة في نفس الفترة للمستخدم
        private async Task<bool> CheckOverlappingLeavesAsync(string? employeeCode,string? RepresentativeCode,DateTime fromDate,DateTime toDate)
        {
            if(!string.IsNullOrEmpty(employeeCode))
            {
                return await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
            .GetQueryable()
            .AnyAsync(lr =>
                lr.EmployeeCode == employeeCode &&
                !lr.IsDeleted &&
                lr.Status != LeaveRequestStatus.Rejected &&
                lr.Status != LeaveRequestStatus.Cancelled &&
                ((fromDate >= lr.FromDate && fromDate <= lr.ToDate) ||
                 (toDate >= lr.FromDate && toDate <= lr.ToDate) ||
                 (fromDate <= lr.FromDate && toDate >= lr.ToDate)));
            }
            else
            {
                return await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetQueryable()
                    .AnyAsync(lr =>
                        lr.RepresentativeCode == RepresentativeCode &&
                        !lr.IsDeleted &&
                        lr.Status != LeaveRequestStatus.Rejected &&
                        lr.Status != LeaveRequestStatus.Cancelled &&
                        ((fromDate >= lr.FromDate && fromDate <= lr.ToDate) ||
                         (toDate >= lr.FromDate && toDate <= lr.ToDate) ||
                         (fromDate <= lr.FromDate && toDate >= lr.ToDate)));
            }
        }
        // دالة لتحويل WeekDays إلى DayOfWeek
        private DayOfWeek ConvertWeekDaysToDayOfWeek(WeekDays weekDay)
        {
            switch(weekDay)
            {
                case WeekDays.Saturday:
                    return DayOfWeek.Saturday;
                case WeekDays.Sunday:
                    return DayOfWeek.Sunday;
                case WeekDays.Monday:
                    return DayOfWeek.Monday;
                case WeekDays.Tuesday:
                    return DayOfWeek.Tuesday;
                case WeekDays.Wednesday:
                    return DayOfWeek.Wednesday;
                case WeekDays.Thursday:
                    return DayOfWeek.Thursday;
                case WeekDays.Friday:
                    return DayOfWeek.Friday;
                default:
                    return DayOfWeek.Saturday;
            }
        }
        // دالة لجلب العطلات الرسمية بين تاريخين
        private async Task<List<PublicHoliday>> GetPublicHolidaysBetweenDatesAsync(DateTime fromDate,DateTime toDate)
        {
            var fromDateOnly = DateOnly.FromDateTime(fromDate);
            var toDateOnly = DateOnly.FromDateTime(toDate);

            return await _unitOfWork.GetRepository<PublicHoliday,int>()
                .GetQueryable()
                .Where(h => h.Date >= fromDateOnly && h.Date <= toDateOnly && !h.IsDeleted)
                .ToListAsync();
        }
        // دالة لتطبيق الترتيب على الاستعلام بناءً على الحقل والاتجاه
        private IQueryable<EmployeeLeaveRequest> ApplySorting(
            IQueryable<EmployeeLeaveRequest> query,string? sortBy,bool descending)
        {
            return (sortBy?.ToLower(), descending) switch
            {
                ("fromdate", true) => query.OrderByDescending(lr => lr.FromDate),
                ("fromdate", false) => query.OrderBy(lr => lr.FromDate),
                ("createdat", true) => query.OrderByDescending(lr => lr.CreatedAt),
                ("createdat", false) => query.OrderBy(lr => lr.CreatedAt),
                ("status", true) => query.OrderByDescending(lr => lr.Status),
                ("status", false) => query.OrderBy(lr => lr.Status),
                _ => query.OrderByDescending(lr => lr.CreatedAt)
            };
        }
        // دالة لجلب كود الموظف بناءً على البريد الإلكتروني (للتعامل مع حالة وجود Email فقط)
        private async Task<string> GetEmployeeCodeFromEmailAsync(string email)
        {
            if(string.IsNullOrEmpty(email))
                return string.Empty;

            var userExists = await _UserManager.Users
                .SingleOrDefaultAsync(a => a.Email == email);
            if(userExists == null)
                return string.Empty;

            var emp = await _unitOfWork.GetRepository<Employee,string>()
                .GetQueryable()
                .FirstOrDefaultAsync(e => e.UserId == userExists.Id);

            return emp?.EmployeeCode ?? string.Empty;
        }

        #endregion
        // 1. جلب طلبات إجازة الموظف
        public async Task<PagedList<EmployeeLeaveRequestDto>> GetEmployeeLeaveRequestsAsync(
            string employeeEmail,PaginationParams paginationParams)
        {
            var employeeCode = string.Empty;
            if(!string.IsNullOrEmpty(employeeEmail))
            {
                var userExists = await _UserManager.Users.SingleOrDefaultAsync(a => a.Email == employeeEmail);
                if(userExists != null)
                {
                    var emp = await _unitOfWork.GetRepository<Employee,string>().GetQueryable()
                        .FirstOrDefaultAsync(e => e.UserId == userExists.Id);
                    if(emp != null)
                        employeeCode = emp.EmployeeCode;
                }
            }
            var query = _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                .GetQueryable()
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
                .Where(lr => lr.EmployeeCode == employeeCode && !lr.IsDeleted)
                .OrderByDescending(lr => lr.CreatedAt);

            var result = query.Select(lr => new EmployeeLeaveRequestDto
            {
                Id = lr.Id,
                EmployeeCode = lr.EmployeeCode,
                EmployeeName = lr.Employee != null && lr.Employee.User != null
                    ? $"{lr.Employee.User.FullName}"
                    : "",
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "",
                FromDate = lr.FromDate,
                ToDate = lr.ToDate,
                DaysRequested = lr.DaysRequested,
                Status = lr.Status,
                ApprovedBy = lr.ApprovedBy ?? "",
                ApprovedAt = lr.ApprovedAt,
                RejectedBy = lr.RejectedBy,
                RejectedAt = lr.RejectedAt,
                RejectionReason = lr.RejectionReason,
                Notes = lr.Notes,
                CreatedAt = lr.CreatedAt,
                CreatedBy = lr.CreateBy ?? ""
            });

            return await result.ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);
        }
        // 2. البحث المتقدم في طلبات الإجازة
        public async Task<PagedList<EmployeeLeaveRequestDto>> SearchLeaveRequestsAsync(LeaveRequestFilterDto filter)
        {
            var query = _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                .GetQueryable()
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
                .Include(lr => lr.Representative)
                .Where(lr => !lr.IsDeleted)
                .AsQueryable();

            // تطبيق الفلاتر
            if(!string.IsNullOrEmpty(filter.EmployeeCode))
                query = query.Where(lr => lr.EmployeeCode == filter.EmployeeCode);

            if(!string.IsNullOrEmpty(filter.RepresentativeCode))
                query = query.Where(lr => lr.RepresentativeCode == filter.RepresentativeCode);

            if(filter.LeaveTypeId.HasValue)
                query = query.Where(lr => lr.LeaveTypeId == filter.LeaveTypeId.Value);

            if(filter.Status.HasValue)
                query = query.Where(lr => lr.Status == filter.Status.Value);

            if(filter.FromDate.HasValue)
                query = query.Where(lr => lr.FromDate >= filter.FromDate.Value);

            if(filter.ToDate.HasValue)
                query = query.Where(lr => lr.ToDate <= filter.ToDate.Value);

            // الترتيب
            query = ApplySorting(query,filter.SortBy,filter.SortDescending);

            var result = query.Select(lr => new EmployeeLeaveRequestDto
            {
                Id = lr.Id,
                EmployeeCode = lr.EmployeeCode,
                EmployeeName = lr.Employee != null && lr.Employee.User != null
                    ? $"{lr.Employee.User.FullName}"
                    : "",
                RepresentativeCode = lr.RepresentativeCode,
                RepresentativeName = lr.Representative != null && lr.Representative.User != null
                    ? $"{lr.Representative.User.FullName}"
                    : "",
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "",
                FromDate = lr.FromDate,
                ToDate = lr.ToDate,
                DaysRequested = lr.DaysRequested,
                Status = lr.Status,
                ApprovedBy = lr.ApprovedBy ?? "",
                ApprovedAt = lr.ApprovedAt,
                RejectedBy = lr.RejectedBy,
                RejectedAt = lr.RejectedAt,
                RejectionReason = lr.RejectionReason,
                Notes = lr.Notes,
                CreatedAt = lr.CreatedAt,
                CreatedBy = lr.CreateBy ?? ""
            });

            return await result.ToPagedListAsync(filter.PageNumber,filter.PageSize);
        }
        // 3. جلب طلب إجازة محدد
        public async Task<Result<EmployeeLeaveRequestDto>> GetLeaveRequestByIdAsync(int id)
        {
            var leaveRequest = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                .GetQueryable()
                .Include(lr => lr.LeaveType)
                .Include(lr => lr.Employee)
                .ThenInclude(e => e.User)
                .Include(lr => lr.Representative)
                .FirstOrDefaultAsync(lr => lr.Id == id && !lr.IsDeleted);

            if(leaveRequest == null)
                return Result<EmployeeLeaveRequestDto>.Failure("طلب الإجازة غير موجود");

            var dto = new EmployeeLeaveRequestDto
            {
                Id = leaveRequest.Id,
                EmployeeCode = leaveRequest.EmployeeCode,
                EmployeeName = leaveRequest.Employee != null && leaveRequest.Employee.User != null
                    ? $"{leaveRequest.Employee.User.FullName}"
                    : "",
                RepresentativeCode = leaveRequest.RepresentativeCode,
                RepresentativeName = leaveRequest.Representative != null && leaveRequest.Representative.User != null
                    ? $"{leaveRequest.Representative.User.FullName}"
                    : "",
                LeaveTypeId = leaveRequest.LeaveTypeId,
                LeaveTypeName = leaveRequest.LeaveType?.Name ?? "",
                FromDate = leaveRequest.FromDate,
                ToDate = leaveRequest.ToDate,
                DaysRequested = leaveRequest.DaysRequested,
                Status = leaveRequest.Status,
                ApprovedBy = leaveRequest.ApprovedBy ?? "",
                ApprovedAt = leaveRequest.ApprovedAt,
                RejectedBy = leaveRequest.RejectedBy,
                RejectedAt = leaveRequest.RejectedAt,
                RejectionReason = leaveRequest.RejectionReason,
                Notes = leaveRequest.Notes,
                CreatedAt = leaveRequest.CreatedAt,
                CreatedBy = leaveRequest.CreateBy ?? ""
            };

            return Result<EmployeeLeaveRequestDto>.Success(dto);
        }
        public async Task<Result<List<LeaveTypeBalanceDto>>> GetEmployeeLeaveTypesWithBalanceAsync(string code)
        {
            try
            {
                var currentYear = DateTime.Now.Year;
                string? employeeCode = null;
                string? representativeCode = null;

                if(string.IsNullOrEmpty(code))
                {
                    var currentUserId = _currentUserService.UserId;
                    if(string.IsNullOrEmpty(currentUserId))
                        return Result<List<LeaveTypeBalanceDto>>.Failure("يجب تسجيل الدخول أولاً");

                    // محاولة العثور على موظف أو مندوب
                    var employee = await _unitOfWork.GetRepository<Employee,string>()
                        .GetQueryable().FirstOrDefaultAsync(e => e.UserId == currentUserId && !e.IsDeleted);
                    if(employee != null)
                        employeeCode = employee.EmployeeCode;

                    var representative = await _unitOfWork.GetRepository<Representatives,string>()
                        .GetQueryable().FirstOrDefaultAsync(r => r.UserId == currentUserId && !r.IsDeleted);
                    if(representative != null)
                        representativeCode = representative.RepresentativesCode;

                    if(employeeCode == null && representativeCode == null)
                        return Result<List<LeaveTypeBalanceDto>>.Failure("لم يتم العثور على مستخدم مرتبط بموظف أو مندوب");
                }
                else
                {
                    var codes = await GetUserCodesAsync(code);
                    employeeCode = codes.EmployeeCode;
                    representativeCode = codes.RepresentativeCode;
                    if(employeeCode == null && representativeCode == null)
                        return Result<List<LeaveTypeBalanceDto>>.Failure("الكود غير موجود للموظفين أو المناديب");
                }


                // 2. جلب محافظ الإجازات للموظف
                var query = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                    .GetQueryable()
                    .Include(b => b.LeaveType)
                    .Include(b => b.Employee)
                    .ThenInclude(e => e!.User)
                    .Where(b => b.Year == currentYear &&
                               !b.IsDeleted &&
                               !b.LeaveType!.IsDeleted);

                if(!string.IsNullOrEmpty(employeeCode))
                    query = query.Where(b => b.EmployeeCode == employeeCode);
                else
                    query = query.Where(b => b.RepresentativeCode == representativeCode);

                var balances = await query.ToListAsync();

                if(!balances.Any())
                    return Result<List<LeaveTypeBalanceDto>>.Failure("لا توجد محافظ للمستخدم");

                var result = new List<LeaveTypeBalanceDto>();

                foreach(var balance in balances)
                {
                    // حساب الطلبات المعلقة
                    var pendingRequests = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                        .GetQueryable()
                        .Where(lr =>
                            (employeeCode != null && lr.EmployeeCode == employeeCode) ||
                            (representativeCode != null && lr.RepresentativeCode == representativeCode))
                        .Where(lr => lr.LeaveTypeId == balance.LeaveTypeId &&
                                   lr.Status == LeaveRequestStatus.Pending &&
                                   !lr.IsDeleted)
                        .SumAsync(lr => (decimal?) lr.DaysRequested) ?? 0;

                    result.Add(new LeaveTypeBalanceDto
                    {
                        LeaveTypeId = balance.LeaveTypeId,
                        LeaveTypeName = balance.LeaveType?.Name ?? "غير معروف",
                        IsPaid = balance.LeaveType?.IsPaid ?? false,
                        OpeningBalance = balance.OpeningBalance,
                        Accrued = balance.Accrued,
                        Used = balance.Used,
                        Remaining = balance.Remaining,
                        Pending = pendingRequests
                    });
                }

                return Result<List<LeaveTypeBalanceDto>>.Success(result);

            }
            catch(Exception ex)
            {
                return Result<List<LeaveTypeBalanceDto>>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 4. إنشاء طلب إجازة جديد
        public async Task<Result<string>> CreateLeaveRequestAsync(CreateLeaveRequestDto leaveRequest)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");
                var employeeCode = !string.IsNullOrWhiteSpace(leaveRequest.EmployeeCode) ? leaveRequest.EmployeeCode.Trim() : null;
                var representativeCode = !string.IsNullOrWhiteSpace(leaveRequest.RepresentativeCode) ? leaveRequest.RepresentativeCode.Trim() : null;

                // التعامل مع حالة وجود Email فقط (للموظفين)
                if(string.IsNullOrEmpty(leaveRequest.EmployeeCode) && !string.IsNullOrEmpty(leaveRequest.EmployeeEmail))
                {
                     employeeCode = await GetEmployeeCodeFromEmailAsync(leaveRequest.EmployeeEmail);
                    if(string.IsNullOrEmpty(employeeCode))
                        return Result<string>.Failure("الموظف غير موجود");
                    leaveRequest.EmployeeCode = employeeCode;
                }

                // التحقق من وجود كود الموظف أو المندوب
                bool hasEmployeeCode = !string.IsNullOrEmpty(leaveRequest.EmployeeCode);
                bool hasRepresentativeCode = !string.IsNullOrEmpty(leaveRequest.RepresentativeCode);

                if(!hasEmployeeCode && !hasRepresentativeCode)
                    return Result<string>.Failure("يرجى التحقق من كود الموظف أو المندوب");

                // التحقق من صحة الكود حسب النوع
                if(hasEmployeeCode)
                {
                    var emp = await _unitOfWork.GetRepository<Employee,string>()
                        .FindAsync(e => e.EmployeeCode == leaveRequest.EmployeeCode && !e.IsDeleted);
                    if(emp == null)
                        return Result<string>.Failure("الموظف غير موجود");
                }
                else // hasRepresentativeCode
                {
                    var rep = await _unitOfWork.GetRepository<Representatives,string>()
                        .FindAsync(r => r.RepresentativesCode == leaveRequest.RepresentativeCode && !r.IsDeleted);
                    if(rep == null)
                        return Result<string>.Failure("المندوب غير موجود");
                }

                // التحقق من وجود نوع الإجازة
                var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .FindAsync(lt => lt.Id == leaveRequest.LeaveTypeId && !lt.IsDeleted);
                if(leaveType == null)
                    return Result<string>.Failure("نوع الإجازة غير موجود");

                // التحقق من التواريخ
                if(leaveRequest.FromDate > leaveRequest.ToDate)
                    return Result<string>.Failure("تاريخ البداية يجب أن يكون قبل تاريخ النهاية");

                // التحقق من التداخل
                var overlapping = await CheckOverlappingLeavesAsync(
                    leaveRequest.EmployeeCode,
                    leaveRequest.RepresentativeCode,
                    leaveRequest.FromDate,
                    leaveRequest.ToDate);
                if(overlapping)
                    return Result<string>.Failure("يوجد إجازة معتمدة أو معلقة في نفس الفترة");

                // حساب أيام العمل
                decimal workingDays = await CalculateWorkingDaysAsync(
                    leaveRequest.EmployeeCode,
                    leaveRequest.RepresentativeCode,
                    leaveRequest.FromDate,
                    leaveRequest.ToDate);
                if(workingDays <= 0)
                    return Result<string>.Failure("لا توجد أيام عمل في الفترة المحددة");

                // جلب محفظة الإجازات للمستخدم (موظف أو مندوب)
                var balance = await GetLeaveBalanceForUserAsync(
                    leaveRequest.EmployeeCode,
                    leaveRequest.RepresentativeCode,
                    leaveRequest.LeaveTypeId,
                    leaveRequest.FromDate.Year);
                if(balance == null)
                    return Result<string>.Failure("لا توجد محفظة إجازات لهذا المستخدم على هذا النوع. يجب على HR إنشاء المحفظة أولاً.");

                if(balance.Remaining < workingDays)
                    return Result<string>.Failure($"رصيد الإجازة غير كافي. الرصيد المتاح: {balance.Remaining} يوم");

                // إنشاء طلب الإجازة
                var newRequest = new EmployeeLeaveRequest
                {
                    EmployeeCode = employeeCode,
                    RepresentativeCode = representativeCode,
                    LeaveTypeId = leaveRequest.LeaveTypeId,
                    FromDate = leaveRequest.FromDate,
                    ToDate = leaveRequest.ToDate,
                    DaysRequested = workingDays,
                    Status = LeaveRequestStatus.Pending,
                    Notes = leaveRequest.Notes,
                    CreatedAt = DateTime.Now,
                    CreateBy = userId
                };

                await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>().AddAsync(newRequest);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم إنشاء طلب الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 5. الموافقة على طلب الإجازة
        public async Task<Result<string>> ApproveLeaveRequestAsync(int leaveRequestId,string? reason = "")
        {
            try
            {
                var userid = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userid))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var userdata = await _UserManager.FindByIdAsync(userid);

                var leaveRequest = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetQueryable()
                    .Include(lr => lr.LeaveType)
                    .Include(lr => lr.Employee)
                    .Include(lr => lr.Representative)
                    .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId && !lr.IsDeleted);

                if(leaveRequest == null)
                    return Result<string>.Failure("طلب الإجازة غير موجود");

                if(leaveRequest.Status != LeaveRequestStatus.Pending)
                    return Result<string>.Failure("لا يمكن الموافقة على طلب إجازة غير معلق");

                // إذا كانت إجازة مدفوعة، خصم من الرصيد
                if(leaveRequest.LeaveType?.IsPaid == true)
                {
                    var balance = await GetOrCreateBalanceForUserAsync(
                        leaveRequest.EmployeeCode,
                        leaveRequest.RepresentativeCode,
                        leaveRequest.LeaveTypeId,
                        leaveRequest.FromDate.Year);

                    if(balance.Remaining < leaveRequest.DaysRequested)
                        return Result<string>.Failure($"رصيد الإجازة غير كافي. المتاح: {balance.Remaining} يوم");

                    balance.Used += leaveRequest.DaysRequested;
                    balance.Remaining -= leaveRequest.DaysRequested;
                    balance.UpdateAt = DateTime.Now;
                    balance.UpdateBy = userdata?.FullName ?? "";

                    await _unitOfWork.GetRepository<EmployeeLeaveBalance,int>().UpdateAsync(balance);
                }

                // تحديث حالة الطلب
                leaveRequest.Status = LeaveRequestStatus.Approved;
                leaveRequest.ApprovedBy = userdata?.FullName ?? "";
                leaveRequest.ApprovedAt = DateTime.Now;
                leaveRequest.UpdateAt = DateTime.Now;
                leaveRequest.UpdateBy = userdata?.FullName ?? "";

                await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>().UpdateAsync(leaveRequest);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تمت الموافقة على طلب الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ أثناء الموافقة: {ex.Message}");
            }
        }
        // 6. رفض طلب الإجازة
        public async Task<Result<string>> RejectLeaveRequestAsync(int leaveRequestId,string reason)
        {
            try
            {
                var userid = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userid))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var userdata = await _UserManager.FindByIdAsync(userid);

                var leaveRequest = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetByIdAsync(leaveRequestId);

                if(leaveRequest == null || leaveRequest.IsDeleted)
                    return Result<string>.Failure("طلب الإجازة غير موجود");

                if(leaveRequest.Status != LeaveRequestStatus.Pending)
                    return Result<string>.Failure("لا يمكن رفض طلب إجازة غير معلق");

                if(string.IsNullOrWhiteSpace(reason))
                    return Result<string>.Failure("يجب إدخال سبب الرفض");

                leaveRequest.Status = LeaveRequestStatus.Rejected;
                leaveRequest.RejectedBy = userdata?.FullName ?? "";
                leaveRequest.RejectedAt = DateTime.Now;
                leaveRequest.RejectionReason = reason;
                leaveRequest.UpdateAt = DateTime.Now;
                leaveRequest.UpdateBy = userdata?.FullName ?? "";

                await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>().UpdateAsync(leaveRequest);
                await _unitOfWork.SaveChangesAsync();

                return Result<string>.Success("تم رفض طلب الإجازة");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ أثناء الرفض: {ex.Message}");
            }
        }
        // 7. إلغاء طلب الإجازة
        public async Task<Result<string>> CancelLeaveRequestAsync(int leaveRequestId,string cancelledBy)
        {
            try
            {
                var userid = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userid))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var employee = await _unitOfWork.GetRepository<Employee,string>().GetQueryable()
                        .FirstOrDefaultAsync(x => x.UserId == userid);
                var representative = await _unitOfWork.GetRepository<Representatives,string>().GetQueryable()
                        .FirstOrDefaultAsync(x => x.UserId == userid);

                if(employee == null && representative == null)
                    return Result<string>.Failure("المستخدم غير مرتبط بموظف أو مندوب");

                var leaveRequest = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetByIdAsync(leaveRequestId);

                if(leaveRequest == null || leaveRequest.IsDeleted)
                    return Result<string>.Failure("طلب الإجازة غير موجود");

                // التحقق من أن الطلب يمكن إلغاؤه
                if(leaveRequest.Status != LeaveRequestStatus.Approved)
                    return Result<string>.Failure("لا يمكن إلغاء طلب الإجازة في هذه الحالة");

                // التحقق من أن الإلغاء قبل بدء الإجازة
                if(leaveRequest.FromDate <= DateTime.Today)
                    return Result<string>.Failure("لا يمكن إلغاء إجازة بدأت بالفعل");

                // التحقق من الصلاحيات (صاحب الطلب فقط)
                bool isOwner = (employee != null && leaveRequest.EmployeeCode == employee.EmployeeCode) ||
                               (representative != null && leaveRequest.RepresentativeCode == representative.RepresentativesCode);
                if(!isOwner)
                    return Result<string>.Failure("يمكن لصاحب الطلب فقط إلغاء طلب الإجازة الخاص به");

                // إذا كانت الإجازة معتمدة، استرجاع الرصيد
                if(leaveRequest.Status == LeaveRequestStatus.Approved)
                {
                    var balance = await GetOrCreateBalanceForUserAsync(
                                  leaveRequest.EmployeeCode,
                                  leaveRequest.RepresentativeCode,
                                  leaveRequest.LeaveTypeId,
                                  leaveRequest.FromDate.Year);

                    balance.Used -= leaveRequest.DaysRequested;
                    balance.Remaining += leaveRequest.DaysRequested;
                    balance.UpdateAt = DateTime.Now;
                    balance.UpdateBy = employee?.User?.FullName ?? representative?.User?.FullName ?? "";
                }

                leaveRequest.Status = LeaveRequestStatus.Cancelled;
                leaveRequest.UpdateAt = DateTime.Now;
                leaveRequest.UpdateBy = employee?.User?.FullName ?? representative?.User?.FullName ?? "";

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم إلغاء طلب الإجازة بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 8. جلب رصيد الإجازات للموظف الحالي
        public async Task<Result<LeaveBalanceSummaryDto>> GetLoginEmployeeLeaveBalanceAsync(int year)
        {
            try
            {
                var userid = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userid))
                    return Result<LeaveBalanceSummaryDto>.Failure("يجب تسجيل الدخول أولاً");

                string? employeeCode = null;
                string? representativeCode = null;
                string? userName = null;

                // البحث عن موظف
                var employee = await _unitOfWork.GetRepository<Employee,string>()
                    .GetQueryable()
                    .Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.UserId == userid && !e.IsDeleted);
                if(employee != null)
                {
                    employeeCode = employee.EmployeeCode;
                    userName = employee.User?.FullName ?? "";
                }
                // البحث عن مندوب
                var representative = await _unitOfWork.GetRepository<Representatives,string>()
                    .GetQueryable()
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.UserId == userid && !r.IsDeleted);
                if(representative != null)
                {
                    representativeCode = representative.RepresentativesCode;
                    userName = representative.User?.FullName ?? "";
                }

                if(employeeCode == null && representativeCode == null)
                    return Result<LeaveBalanceSummaryDto>.Failure("المستخدم غير مرتبط بموظف أو مندوب");

                // جلب الأرصدة
                var query = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                    .GetQueryable()
                    .Include(b => b.LeaveType)
                    .Where(b => b.Year == year && !b.IsDeleted && !b.LeaveType!.IsDeleted);

                if(!string.IsNullOrEmpty(employeeCode))
                    query = query.Where(b => b.EmployeeCode == employeeCode);
                else
                    query = query.Where(b => b.RepresentativeCode == representativeCode);

                var balances = await query.ToListAsync();

                var balanceSummary = new LeaveBalanceSummaryDto
                {
                    EmployeeCode = employeeCode ?? representativeCode,
                    EmployeeName = userName,
                    Year = year,
                    Balances = new List<LeaveTypeBalanceDto>()
                };

                foreach(var balance in balances)
                {
                    var pendingRequests = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetQueryable()
                    .Where(lr =>
                        (employeeCode != null && lr.EmployeeCode == employeeCode) ||
                        (representativeCode != null && lr.RepresentativeCode == representativeCode))
                    .Where(lr => lr.LeaveTypeId == balance.LeaveTypeId &&
                               lr.Status == LeaveRequestStatus.Pending &&
                               !lr.IsDeleted)
                    .SumAsync(lr => (decimal?) lr.DaysRequested) ?? 0;

                    balanceSummary.Balances.Add(new LeaveTypeBalanceDto
                    {
                        LeaveTypeId = balance.LeaveTypeId,
                        LeaveTypeName = balance.LeaveType!.Name,
                        IsPaid = balance.LeaveType.IsPaid,
                        OpeningBalance = balance.OpeningBalance,
                        Accrued = balance.Accrued,
                        Used = balance.Used,
                        Remaining = balance.Remaining,
                        Pending = pendingRequests
                    });
                }
                return Result<LeaveBalanceSummaryDto>.Success(balanceSummary);
            }
            catch(Exception ex)
            {
                return Result<LeaveBalanceSummaryDto>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 9. جلب رصيد الإجازات للموظف
        public async Task<Result<LeaveBalanceSummaryDto>> GetEmployeeLeaveBalanceAsync(string code,int year)
        {
            try
            {
                var userid = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userid))
                    return Result<LeaveBalanceSummaryDto>.Failure("يجب تسجيل الدخول أولاً");

                var codes = await GetUserCodesAsync(code);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<LeaveBalanceSummaryDto>.Failure("الكود غير موجود للموظفين أو المناديب");

                var userName = await GetUserNameAsync(codes.EmployeeCode,codes.RepresentativeCode);

                var query = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                    .GetQueryable()
                    .Include(b => b.LeaveType)
                    .Where(b => b.Year == year && !b.IsDeleted && !b.LeaveType!.IsDeleted);

                if(codes.EmployeeCode != null)
                    query = query.Where(b => b.EmployeeCode == codes.EmployeeCode);
                else
                    query = query.Where(b => b.RepresentativeCode == codes.RepresentativeCode);

                var balances = await query.ToListAsync();


                var balanceSummary = new LeaveBalanceSummaryDto
                {
                    EmployeeCode = code,
                    EmployeeName = userName,
                    Year = year,
                    Balances = new List<LeaveTypeBalanceDto>()
                };

                foreach(var balance in balances)
                {
                    var pendingRequests = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                       .GetQueryable()
                       .Where(lr =>
                           (codes.EmployeeCode != null && lr.EmployeeCode == codes.EmployeeCode) ||
                           (codes.RepresentativeCode != null && lr.RepresentativeCode == codes.RepresentativeCode))
                       .Where(lr => lr.LeaveTypeId == balance.LeaveTypeId &&
                                  lr.Status == LeaveRequestStatus.Pending &&
                                  !lr.IsDeleted)
                       .SumAsync(lr => (decimal?) lr.DaysRequested) ?? 0;

                    balanceSummary.Balances.Add(new LeaveTypeBalanceDto
                    {
                        LeaveTypeId = balance.LeaveTypeId,
                        LeaveTypeName = balance.LeaveType!.Name,
                        IsPaid = balance.LeaveType.IsPaid,
                        OpeningBalance = balance.OpeningBalance,
                        Accrued = balance.Accrued,
                        Used = balance.Used,
                        Remaining = balance.Remaining,
                        Pending = pendingRequests
                    });
                }

                return Result<LeaveBalanceSummaryDto>.Success(balanceSummary);
            }
            catch(Exception ex)
            {
                return Result<LeaveBalanceSummaryDto>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 10. إنشاء محافظ إجازات متعددة للموظف
        public async Task<Result<BulkLeaveBalanceResultDto>> CreateMultipleLeaveBalancesAsync(BulkLeaveBalanceRequestDto request)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<BulkLeaveBalanceResultDto>.Failure("يجب تسجيل الدخول أولاً");

                var codes = await GetUserCodesAsync(request.EmployeeCode);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<BulkLeaveBalanceResultDto>.Failure("الكود غير موجود للموظفين أو المناديب");

                var userName = await GetUserNameAsync(codes.EmployeeCode,codes.RepresentativeCode);

                var result = new BulkLeaveBalanceResultDto
                {
                    EmployeeCode = request.EmployeeCode,
                    EmployeeName = userName,
                    Year = request.Year,
                    CreatedBalances = new List<LeaveBalanceDetailDto>(),
                    FailedBalances = new List<FailedBalanceDto>()
                };

                foreach(var balanceRequest in request.Balances)
                {
                    try
                    {
                        var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                            .FindAsync(lt => lt.Id == balanceRequest.LeaveTypeId && !lt.IsDeleted);

                        if(leaveType == null)
                        {
                            result.FailedBalances.Add(new FailedBalanceDto
                            {
                                LeaveTypeId = balanceRequest.LeaveTypeId,
                                Reason = "نوع الإجازة غير موجود"
                            });
                            continue;
                        }

                        var existingQuery = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                           .GetQueryable()
                           .Where(b => b.LeaveTypeId == balanceRequest.LeaveTypeId &&
                                      b.Year == request.Year &&
                                      !b.IsDeleted);

                        if(codes.EmployeeCode != null)
                            existingQuery = existingQuery.Where(b => b.EmployeeCode == codes.EmployeeCode);
                        else
                            existingQuery = existingQuery.Where(b => b.RepresentativeCode == codes.RepresentativeCode);

                        var existingBalance = await existingQuery.FirstOrDefaultAsync();
                        if(existingBalance != null)
                        {
                            result.FailedBalances.Add(new FailedBalanceDto
                            {
                                LeaveTypeId = balanceRequest.LeaveTypeId,
                                LeaveTypeName = leaveType.Name,
                                Reason = "المحفظة موجودة بالفعل"
                            });
                            continue;
                        }

                        // إنشاء الرصيد الجديد
                        var newBalance = new EmployeeLeaveBalance
                        {
                            EmployeeCode = codes.EmployeeCode,
                            RepresentativeCode = codes.RepresentativeCode,
                            LeaveTypeId = balanceRequest.LeaveTypeId,
                            Year = request.Year,
                            OpeningBalance = balanceRequest.OpeningBalance,
                            Accrued = balanceRequest.Accrued ?? 0,
                            Used = 0,
                            Remaining = balanceRequest.OpeningBalance + (balanceRequest.Accrued ?? 0),
                            CreatedAt = DateTime.Now,
                            CreateBy = userId
                        };

                        await _unitOfWork.GetRepository<EmployeeLeaveBalance,int>().AddAsync(newBalance);

                        result.CreatedBalances.Add(new LeaveBalanceDetailDto
                        {
                            LeaveTypeId = leaveType.Id,
                            LeaveTypeName = leaveType.Name,
                            OpeningBalance = newBalance.OpeningBalance,
                            Accrued = newBalance.Accrued,
                            Remaining = newBalance.Remaining
                        });
                    }
                    catch(Exception ex)
                    {
                        result.FailedBalances.Add(new FailedBalanceDto
                        {
                            LeaveTypeId = balanceRequest.LeaveTypeId,
                            Reason = $"خطأ: {ex.Message}"
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                return Result<BulkLeaveBalanceResultDto>.Success(result);
            }
            catch(Exception ex)
            {
                return Result<BulkLeaveBalanceResultDto>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 11. جلب رصيد نوع إجازة محدد
        public async Task<Result<EmployeeLeaveBalanceDto>> GetLeaveBalanceByTypeAsync(
            string code,int leaveTypeId,int year)
        {
            try
            {
                var codes = await GetUserCodesAsync(code);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<EmployeeLeaveBalanceDto>.Failure("الكود غير موجود للموظفين أو المناديب");

                var query = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                 .GetQueryable()
                 .Include(b => b.LeaveType)
                 .Include(b => b.Employee).ThenInclude(e => e.User)
                 .Include(b => b.Representative).ThenInclude(r => r.User)
                 .Where(b => b.Year == year && !b.IsDeleted && b.LeaveTypeId == leaveTypeId);

                if(codes.EmployeeCode != null)
                    query = query.Where(b => b.EmployeeCode == codes.EmployeeCode);
                else
                    query = query.Where(b => b.RepresentativeCode == codes.RepresentativeCode);

                var balance = await query.FirstOrDefaultAsync();

                if(balance == null)
                    return Result<EmployeeLeaveBalanceDto>.Failure("رصيد الإجازات غير موجود");


                var pendingRequests = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetQueryable()
                    .Where(lr =>
                        (codes.EmployeeCode != null && lr.EmployeeCode == codes.EmployeeCode) ||
                        (codes.RepresentativeCode != null && lr.RepresentativeCode == codes.RepresentativeCode))
                    .Where(lr => lr.LeaveTypeId == leaveTypeId &&
                               lr.Status == LeaveRequestStatus.Pending &&
                               !lr.IsDeleted)
                    .SumAsync(lr => (decimal?) lr.DaysRequested) ?? 0;

                var dto = new EmployeeLeaveBalanceDto
                {
                    EmployeeCode = balance.EmployeeCode ?? balance.RepresentativeCode,
                    EmployeeName = balance.Employee?.User?.FullName ?? balance.Representative?.User?.FullName ?? "",
                    LeaveTypeId = balance.LeaveTypeId,
                    LeaveTypeName = balance.LeaveType?.Name ?? "",
                    Year = balance.Year,
                    OpeningBalance = balance.OpeningBalance,
                    Accrued = balance.Accrued,
                    Used = balance.Used,
                    Remaining = balance.Remaining,
                    PendingRequests = pendingRequests
                };

                return Result<EmployeeLeaveBalanceDto>.Success(dto);
            }
            catch(Exception ex)
            {
                return Result<EmployeeLeaveBalanceDto>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 12. تحديث رصيد الإجازات
        public async Task<Result<string>> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto leaveBalance)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var codes = await GetUserCodesAsync(leaveBalance.EmployeeCode);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<string>.Failure("الكود غير موجود للموظفين أو المناديب");

                var query = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                    .GetQueryable()
                    .Where(b => b.LeaveTypeId == leaveBalance.LeaveTypeId &&
                               b.Year == leaveBalance.Year &&
                               !b.IsDeleted);

                if(codes.EmployeeCode != null)
                    query = query.Where(b => b.EmployeeCode == codes.EmployeeCode);
                else
                    query = query.Where(b => b.RepresentativeCode == codes.RepresentativeCode);

                var existingBalance = await query.FirstOrDefaultAsync();
                if(existingBalance == null)
                    return Result<string>.Failure("رصيد الإجازات غير موجود");

                // التحقق من صحة البيانات
                if(leaveBalance.Remaining < 0)
                    return Result<string>.Failure("الرصيد المتبقي لا يمكن أن يكون سالباً");

                if(leaveBalance.Used < 0)
                    return Result<string>.Failure("الأيام المستخدمة لا يمكن أن تكون سالبة");

                existingBalance.OpeningBalance = leaveBalance.OpeningBalance;
                existingBalance.Accrued = leaveBalance.Accrued;
                existingBalance.Used = leaveBalance.Used;
                existingBalance.Remaining = leaveBalance.Remaining;
                existingBalance.UpdateAt = DateTime.Now;
                existingBalance.UpdateBy = userId;

                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم تحديث رصيد الإجازات بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 13. تهيئة رصيد إجازات مخصص للموظف
        public async Task<Result<string>> SetCustomLeaveBalanceAsync(string code,int LeaveTypeId,int OpeningBalance)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var codes = await GetUserCodesAsync(code);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<string>.Failure("الكود غير موجود للموظفين أو المناديب");

                var leaveType = await _unitOfWork.GetRepository<LeaveType,int>()
                    .FindAsync(lt => lt.Id == LeaveTypeId && !lt.IsDeleted);
                if(leaveType == null)
                    return Result<string>.Failure("نوع الإجازة غير موجود");

                var year = DateTime.Now.Year;

                var existingQuery = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                      .GetQueryable()
                      .Where(b => b.LeaveTypeId == leaveType.Id &&
                                 b.Year == year &&
                                 !b.IsDeleted);

                if(codes.EmployeeCode != null)
                    existingQuery = existingQuery.Where(b => b.EmployeeCode == codes.EmployeeCode);
                else
                    existingQuery = existingQuery.Where(b => b.RepresentativeCode == codes.RepresentativeCode);

                var existingBalance = await existingQuery.FirstOrDefaultAsync();

                if(existingBalance == null)
                {
                    var balance = new EmployeeLeaveBalance
                    {
                        EmployeeCode = codes.EmployeeCode,
                        RepresentativeCode = codes.RepresentativeCode,
                        LeaveTypeId = leaveType.Id,
                        Year = year,
                        OpeningBalance = OpeningBalance,
                        Accrued = 0,
                        Used = 0,
                        Remaining = OpeningBalance,
                        CreatedAt = DateTime.Now,
                        CreateBy = userId
                    };
                    await _unitOfWork.GetRepository<EmployeeLeaveBalance,int>().AddAsync(balance);
                    await _unitOfWork.SaveChangesAsync();
                    return Result<string>.Success("تم تهيئة رصيد الإجازات للموظف بنجاح");
                }
                else
                    return Result<string>.Failure(" هذا الموظف لديه محفظه اجازات لهذا النوع بالفعل ");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 14. تهيئة رصيد إجازات للموظف
        public async Task<Result<string>> InitializeLeaveBalanceAsync(string code,int year)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userId))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var codes = await GetUserCodesAsync(code);
                if(codes.EmployeeCode == null && codes.RepresentativeCode == null)
                    return Result<string>.Failure("الكود غير موجود للموظفين أو المناديب");

                var leaveTypes = await _unitOfWork.GetRepository<LeaveType,int>()
                    .GetQueryable()
                    .Where(lt => !lt.IsDeleted)
                    .ToListAsync();

                foreach(var leaveType in leaveTypes)
                {
                    var existingQuery = _unitOfWork.GetRepository<EmployeeLeaveBalance,int>()
                           .GetQueryable()
                           .Where(b => b.LeaveTypeId == leaveType.Id &&
                                      b.Year == year);

                    if(codes.EmployeeCode != null)
                        existingQuery = existingQuery.Where(b => b.EmployeeCode == codes.EmployeeCode);
                    else
                        existingQuery = existingQuery.Where(b => b.RepresentativeCode == codes.RepresentativeCode);

                    var existingBalance = await existingQuery.FirstOrDefaultAsync();

                    if(existingBalance == null)
                    {
                        var balance = new EmployeeLeaveBalance
                        {
                            EmployeeCode = codes.EmployeeCode,
                            RepresentativeCode = codes.RepresentativeCode,
                            LeaveTypeId = leaveType.Id,
                            Year = year,
                            OpeningBalance = 0,
                            Accrued = 0,
                            Used = 0,
                            Remaining = 0,
                            CreatedAt = DateTime.Now,
                            CreateBy = userId
                        };
                        await _unitOfWork.GetRepository<EmployeeLeaveBalance,int>().AddAsync(balance);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
                return Result<string>.Success("تم تهيئة رصيد الإجازات للموظف بنجاح");
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 15. جلب الطلبات المعلقة
        public async Task<Result<List<EmployeeLeaveRequestDto>>> GetPendingLeaveRequestsAsync()
        {
            try
            {
                var pendingRequests = await _unitOfWork.GetRepository<EmployeeLeaveRequest,int>()
                    .GetQueryable()
                    .Include(lr => lr.LeaveType)
                    .Include(lr => lr.Employee).ThenInclude(e => e.User)
                    .Include(lr => lr.Representative).ThenInclude(r => r.User)
                    .Where(lr => lr.Status == LeaveRequestStatus.Pending && !lr.IsDeleted)
                    .OrderBy(lr => lr.CreatedAt)
                    .Select(lr => new EmployeeLeaveRequestDto
                    {
                        Id = lr.Id,
                        EmployeeCode = lr.EmployeeCode,
                        EmployeeName = lr.Employee != null && lr.Employee.User != null
                            ? lr.Employee.User.FullName
                            : "",
                        RepresentativeCode = lr.RepresentativeCode,
                        RepresentativeName = lr.Representative != null && lr.Representative.User != null
                            ? lr.Representative.User.FullName
                            : "",
                        LeaveTypeId = lr.LeaveTypeId,
                        LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "",
                        FromDate = lr.FromDate,
                        ToDate = lr.ToDate,
                        DaysRequested = lr.DaysRequested,
                        Status = lr.Status,
                        Notes = lr.Notes,
                        CreatedAt = lr.CreatedAt,
                        CreatedBy = lr.CreateBy ?? ""
                    })
                    .ToListAsync();

                return Result<List<EmployeeLeaveRequestDto>>.Success(pendingRequests);
            }
            catch(Exception ex)
            {
                return Result<List<EmployeeLeaveRequestDto>>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
        // 16. الموافقة الجماعية
        public async Task<Result<string>> BulkApproveRequestsAsync(List<int> requestIds)
        {
            try
            {
                var userid = _currentUserService.UserId;
                if(string.IsNullOrEmpty(userid))
                    return Result<string>.Failure("يجب تسجيل الدخول أولاً");

                var userdata = await _UserManager.FindByIdAsync(userid);

                if(!requestIds.Any())
                    return Result<string>.Failure("لم يتم تحديد أي طلبات");

                var successCount = 0;
                var errors = new List<string>();

                foreach(var requestId in requestIds)
                {
                    var result = await ApproveLeaveRequestAsync(requestId,userdata!.FullName);
                    if(result.IsSuccess)
                        successCount++;
                    else
                        errors.Add($"طلب {requestId}: {result.Message}");
                }

                var message = $"تمت الموافقة على {successCount} من {requestIds.Count} طلب";
                if(errors.Any())
                    message += $". الأخطاء: {string.Join("; ",errors.Take(5))}";

                return Result<string>.Success(message);
            }
            catch(Exception ex)
            {
                return Result<string>.Failure($"حدث خطأ: {ex.Message}");
            }
        }
    }    
}