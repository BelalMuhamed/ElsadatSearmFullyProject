using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeSalary;
using Application.DTOs.RepresentativeDtos;
using Application.Helper;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.RepresentativeService;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection.Emit;
using System.Text;

namespace Infrastructure.Services.RepresentativeServices;
internal class RepresentativeService:IRepresentativeService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly ICurrentUserService _CurrentUserService;
    private readonly UserManager<ApplicationUser> _UserManager;
    private readonly RoleManager<ApplicationRole> _RoleManager;

    public RepresentativeService(IUnitOfWork unitOfWork 
        ,ICurrentUserService currentUserService
        ,UserManager<ApplicationUser> userManager
        ,RoleManager<ApplicationRole> roleManager)
    {
        _UnitOfWork = unitOfWork;
        _CurrentUserService = currentUserService;
        _UserManager = userManager;
        _RoleManager = roleManager;
    }
    //-----------------------------------------------------------------------------
    public async Task<Result<string>> AddRepresentativeAsync(RepresentativeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);
        // Check if employee with the same Email already exists
        if(await _UserManager.Users.AnyAsync(u => u.Email == DTo.Email))
            return Result<string>.Failure("يوجد مندوب بنفس الايميل",HttpStatusCode.Conflict);
        // Check if employee with the same EmployeeCode already exists
        if(await _UnitOfWork.GetRepository<Representatives,string>().AnyAsync(e => e.RepresentativesCode == DTo.RepresentativeCode))
            return Result<string>.Failure("يوجد مندوب بنفس الكود",HttpStatusCode.Conflict);

        using var transaction = await _UnitOfWork.BeginTransactionAsyncM();
        try
        {
            var user = new ApplicationUser
            {
                UserName = DTo.Email,
                Email = DTo.Email,
                FullName = DTo.FullName,
                PhoneNumber = DTo.PhoneNumber,
                Address = DTo.Address,
                Gender = DTo.Gender,
                CreateBy = userId,
                CityID = DTo.CityID
            };
            var result = await _UserManager.CreateAsync(user,DTo.Password);
            if(!result.Succeeded)
            {
                var errors = string.Join("; ",result.Errors.Select(e => e.Description));
                return Result<string>.Failure($"حدث خطاء اثناء انشاء المندوب: {errors}",HttpStatusCode.BadRequest);
            }
            if(!string.IsNullOrWhiteSpace(DTo.RoleName))
            {
                var currentRoles = await _UserManager.GetRolesAsync(user);

                // Remove old roles
                await _UserManager.RemoveFromRolesAsync(user,currentRoles);

                // Add new role
                var roleAddResult = await _UserManager.AddToRoleAsync(user,DTo.RoleName);

                if(!roleAddResult.Succeeded)
                {
                    var errors = string.Join("; ",roleAddResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"فشل في ربط المندوب بالدور الخاص به: {errors}");
                }
            }
            var representatives = new Representatives
            {
                UserId = user.Id,
                RepresentativesCode = DTo.RepresentativeCode,
                SNO = DTo.SNO,
                PointsWallet = DTo.PointsWallet,
                MoneyDeposit = DTo.MoneyDeposit,
                OvertimeRatePerHour = DTo.OvertimeRatePerHour,
                BirthDate = DTo.BirthDate,
                HireDate = DTo.HireDate,
                Salary = DTo.Salary,
                TimeIn = DTo.TimeIn,
                TimeOut = DTo.TimeOut,
                WeekHoliday1 = DTo.WeekHoliday1,
                WeekHoliday2 = DTo.WeekHoliday2,
                RepresentiveType=DTo.RepresentiveType,
                CreateBy = userId,

            };
            await _UnitOfWork.GetRepository<Representatives,string>().AddAsync(representatives);
            await _UnitOfWork.SaveChangesAsync();
            // إضافة المدن المرتبطة بالمندوب
            if(DTo.SpecialRepresentiveCities != null && DTo.SpecialRepresentiveCities.Any())
            {
                foreach(var cityDto in DTo.SpecialRepresentiveCities)
                {
                    var specialCity = new SpecialRepresentiveCity
                    {
                        RepresentiveCode = representatives.RepresentativesCode!,
                        CityID = cityDto.CityId,
                        CreateAt = DateTime.Now,
                        CreateBy = userId
                    };
                    await _UnitOfWork.GetRepository<SpecialRepresentiveCity,int>().AddAsync(specialCity);
                }
                await _UnitOfWork.SaveChangesAsync();
            }
            await transaction.CommitAsync();
            return Result<string>.Success("تم انشاء المندوب بنجاح",HttpStatusCode.OK);
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<string>.Failure($"فشل انشاء المندوب: {ex.Message}",HttpStatusCode.InternalServerError);
        }
    }
    //----------------------------------------------------------------------------
    public async Task<Result<string>> SoftDeleteRepresentativeAsync(RepresentativeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);
        if(DTo == null)
            return Result<string>.Failure("يرجا مراجعه البيانات المدخله");
        if(string.IsNullOrWhiteSpace(DTo.RepresentativeCode))
            return Result<string>.Failure("يرجا مراجعه كود المندوب");

        var repo = _UnitOfWork.GetRepository<Representatives,string>();
        var representative = await repo.FindAsync(e => e.RepresentativesCode == DTo.RepresentativeCode);
        if(representative == null)
            return Result<string>.Failure("هذا المندوب غير موجود",HttpStatusCode.NotFound);
        representative.IsDeleted = true;
        representative.DeleteBy = userId;
        representative.DeleteAt = DateTime.UtcNow;
        await repo.UpdateAsync(representative);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("تم حذف المندوب بنجاح");
    }
    //----------------------------------------------------------------------------
    public async Task<Result<string>> RestoreRepresentativeAsync(RepresentativeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);
        if(DTo == null)
            return Result<string>.Failure("يرجا مراجعه البيانات المدخله");
        if(string.IsNullOrWhiteSpace(DTo.RepresentativeCode))
            return Result<string>.Failure("يرجا مراجعه كود المندوب");

        var repo = _UnitOfWork.GetRepository<Representatives,string>();
        var representative = await repo.FindAsync(e => e.RepresentativesCode == DTo.RepresentativeCode);
        if(representative == null)
            return Result<string>.Failure("هذا المندوب غير موجود",HttpStatusCode.NotFound);
        representative.IsDeleted = false;
        representative.DeleteBy = null;
        representative.DeleteAt = null;
        representative.UpdateBy = userId;
        representative.UpdateAt = DateTime.UtcNow;
        await repo.UpdateAsync(representative);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("تم استعادة المندوب بنجاح");

    }
    //----------------------------------------------------------------------------
    public async Task<Result<string>> UpdateRepresentativeAsync(RepresentativeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);
        if(DTo == null)
            return Result<string>.Failure("يرجا مراجعه البيانات المدخله");
        await _UnitOfWork.BeginTransactionAsync();
        try
        {
            var repo = _UnitOfWork.GetRepository<Representatives,string>();
            var representative = await repo.GetQueryable()
                                      .Include(e => e.User).ThenInclude(c => c.City)
                                      .Include(e => e.SpecialRepresentiveCities).ThenInclude(sc => sc.City)
                                      .FirstOrDefaultAsync(e => e.RepresentativesCode == DTo.RepresentativeCode);
            if(representative == null)
                return Result<string>.Failure("هذا المندوب غير موجود",HttpStatusCode.NotFound);

            var user = representative.User;
            if(user == null)
                return Result<string>.Failure("المستخدم المرتبط بهذا المندوب غير موجود",HttpStatusCode.NotFound);

            bool emailExists = await _UserManager.Users
                .AnyAsync(u => u.Email == DTo.Email && u.Id != user.Id);
            if(emailExists)
                return Result<string>.Failure("يوجد مندوب اخر بنفس الايميل",HttpStatusCode.Conflict);

            // Update user email if it has changed
            if(user.Email != DTo.Email)
            {
                var setEmailResult = await _UserManager.SetEmailAsync(user,DTo.Email);
                if(!setEmailResult.Succeeded)
                {
                    var errors = string.Join("; ",setEmailResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"فشل في تحديث ايميل المندوب: {errors}",HttpStatusCode.BadRequest);
                }
                var setUserNameResult = await _UserManager.SetUserNameAsync(user,DTo.Email);
                if(!setUserNameResult.Succeeded)
                {
                    var errors = string.Join("; ",setUserNameResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"فشل في تحديث اسم المستخدم للمندوب: {errors}",HttpStatusCode.BadRequest);
                }
            }
            // Update role if it has changed
            if(!string.IsNullOrWhiteSpace(DTo.RoleName))
            {
                var currentRoles = await _UserManager.GetRolesAsync(user);
                // Remove old roles
                await _UserManager.RemoveFromRolesAsync(user,currentRoles);
                // Add new role
                var roleAddResult = await _UserManager.AddToRoleAsync(user,DTo.RoleName);
                if(!roleAddResult.Succeeded)
                {
                    var errors = string.Join("; ",roleAddResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"فشل في ربط المندوب بالدور الخاص به: {errors}",HttpStatusCode.BadRequest);
                }
            }
            user.FullName = DTo.FullName;
            user.Email = DTo.Email;
            user.UserName = DTo.Email;
            user.PhoneNumber = DTo.PhoneNumber;
            user.Address = DTo.Address;
            user.Gender = DTo.Gender;
            user.CityID = DTo.CityID;
            user.UpdateBy = userId;
            user.UpdateAt = DateTime.UtcNow;
            if(string.IsNullOrEmpty(user.SecurityStamp))
            {
                user.SecurityStamp = Guid.NewGuid().ToString();
            }
            var updateUserResult = await _UserManager.UpdateAsync(user);
            if(!updateUserResult.Succeeded)
            {
                var errors = string.Join("; ",updateUserResult.Errors.Select(e => e.Description));
                return Result<string>.Failure($"فشل في تحديث بيانات المندوب: {errors}",HttpStatusCode.BadRequest);
            }
            // Update representative details
            if(representative.RepresentativesCode != DTo.RepresentativeCode)
            {
                // Check for duplicate RepresentativeCode
                bool codeExists = await repo.AnyAsync(e => e.RepresentativesCode == DTo.RepresentativeCode && e.UserId != representative.UserId);
                if(codeExists)
                    return Result<string>.Failure("يوجد مندوب اخر بنفس الكود",HttpStatusCode.Conflict);
                representative.RepresentativesCode = DTo.RepresentativeCode;
            }
            representative.SNO = DTo.SNO;
            representative.PointsWallet = DTo.PointsWallet;
            representative.MoneyDeposit = DTo.MoneyDeposit;
            representative.OvertimeRatePerHour = DTo.OvertimeRatePerHour;
            representative.BirthDate = DTo.BirthDate;
            representative.HireDate = DTo.HireDate;
            representative.Salary = DTo.Salary;
            representative.TimeIn = DTo.TimeIn;
            representative.TimeOut = DTo.TimeOut;
            representative.WeekHoliday1 = DTo.WeekHoliday1;
            representative.WeekHoliday2 = DTo.WeekHoliday2;
            representative.UpdateBy = userId;
            representative.UpdateAt = DateTime.Now;
            representative.RepresentiveType = DTo.RepresentiveType;
            if(DTo.SpecialRepresentiveCities != null)
            {
                // حذف المدن القديمة
                var existingCities = representative.SpecialRepresentiveCities.ToList();
                foreach(var city in existingCities)
                {
                    await _UnitOfWork.GetRepository<SpecialRepresentiveCity,int>().DeleteAsync(city);
                }
                // إضافة المدن الجديدة
                foreach(var cityDto in DTo.SpecialRepresentiveCities)
                {
                    var specialCity = new SpecialRepresentiveCity
                    {
                        RepresentiveCode = representative.RepresentativesCode!,
                        CityID = cityDto.CityId,
                        CreateAt = DateTime.Now,
                        CreateBy = userId
                    };
                    await _UnitOfWork.GetRepository<SpecialRepresentiveCity,int>().AddAsync(specialCity);
                }
            }

            await repo.UpdateAsync(representative);
            await _UnitOfWork.SaveChangesAsync();
            await _UnitOfWork.CommitAsync();
            return Result<string>.Success("تم تحديث المندوب بنجاح",HttpStatusCode.OK);

        }
        catch(Exception ex)
        {
            await _UnitOfWork.RollbackAsync();
            return Result<string>.Failure($"فشل تحديث المندوب: {ex.Message}",HttpStatusCode.InternalServerError);

        }
    }
    //----------------------------------------------------------------------------
    public async Task<PagedList<RepresentativeDTo>> GetRepresentativeByFilterAsync(PaginationParams paginationParams,RepresentativeHelper search)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return new PagedList<RepresentativeDTo>(
                new List<RepresentativeDTo>(),0,paginationParams.PageNumber,paginationParams.PageSize);

        var query = _UnitOfWork.GetRepository<Representatives,string>().GetQueryable()
            .AsNoTracking();
        if(query == null)
            return new PagedList<RepresentativeDTo>(new List<RepresentativeDTo>(),0,paginationParams.PageNumber,paginationParams.PageSize);
        query = query.Include(e => e.User).ThenInclude(c => c.City)
            .Include(e => e.SpecialRepresentiveCities).ThenInclude(sc => sc.City);

        // Apply filters based on search criteria
        if(!string.IsNullOrEmpty(search.RepresentativeCode))
            query = query.Where(e => e.RepresentativesCode != null &&
                                     e.RepresentativesCode.Contains(search.RepresentativeCode));

        if(!string.IsNullOrEmpty(search.RepresentativeName))
            query = query.Where(e => e.User != null &&
                                     e.User.FullName != null &&
                                     e.User.FullName.Contains(search.RepresentativeName));
        
        if(!string.IsNullOrEmpty(search.CityName))
            query = query.Where(e => e.User != null &&
                                     e.User.City != null &&
                                     e.User.City.Name != null &&
                                     e.User.City.Name.Contains(search.CityName));

        if(search.RepresentiveType != 0)
        {
            query = query.Where(e => e.RepresentiveType == search.RepresentiveType);
        }

        if(search.IsActive)
            query = query.Where(e => !e.IsDeleted);
       

        var totalCount = await query.CountAsync();

        // Project directly to DTO
        var projectedQuery = query.Select(e => new RepresentativeDTo
        {
            UserId = e.UserId,
            Email = e.User!.Email ?? "",
            FullName = e.User.FullName,
            Gender =  (Gender)e.User.Gender,
            PhoneNumber = e.User.PhoneNumber,
            RepresentativeCode = e.RepresentativesCode,
            Address = e.User.Address,
            SNO = e.SNO,
            PointsWallet = e.PointsWallet,
            MoneyDeposit = e.MoneyDeposit,
            OvertimeRatePerHour = e.OvertimeRatePerHour,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            Salary = e.Salary,
            TimeIn = e.TimeIn,
            TimeOut = e.TimeOut,
            WeekHoliday1 = e.WeekHoliday1,
            WeekHoliday2 = e.WeekHoliday2,
            CityName = e.User.City != null ? e.User.City.Name : null,
            CreateBy = e.CreateBy,
            RepresentiveType=e.RepresentiveType,
            IsDeleted=e.IsDeleted,
            NameOfCreatedBy = _UserManager.Users
                .Where(u => u.Id == e.CreateBy)
                .Select(u => u.FullName)
                .FirstOrDefault() ?? "System",


            SpecialRepresentiveCities = e.SpecialRepresentiveCities
            .Select(sc => new SpecialRepresentiveCityDto
            {
                Id = sc.Id,
                CityId = sc.CityID,
                CityName = sc.City != null ? sc.City.Name : null
            })
            .ToList(),         
            
        });

        // Fetch all employees first (to map roles)
        var employeesList = await projectedQuery.ToListAsync();
        var userIds = employeesList.Select(e => e.UserId).Distinct().ToList();

        var users = await _UserManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var rolesMap = new Dictionary<string,List<string>>();
        foreach(var user in users)
        {
            var roles = await _UserManager.GetRolesAsync(user);
            rolesMap[user.Id] = roles.ToList(); // safe even if empty
        }

        var allRoles = await _RoleManager.Roles.ToListAsync();
        
            foreach(var dto in employeesList)
            {
                var userRoles = rolesMap.GetValueOrDefault(dto.UserId!,new List<string>());
            //dto.RolesName = userRoles;
            //dto.RolesId = userRoles
            //    .Select(roleName => allRoles.FirstOrDefault(r => r.Name == roleName)?.Id)
            //    .Where(id => id != null)
            //    .ToList()!;
            // حوّل أسماء الرولز لـ IDs
            var roleIds = userRoles
                .Select(roleName => allRoles.FirstOrDefault(r => r.Name == roleName)?.Id)
                .Where(id => id != null)
                .Select(id => id!) // عشان null-forgiving
                .ToList();
            if(userRoles.Count == 1)
            {
                // Role واحد
                dto.RoleName = userRoles[0];
                dto.RoleId = roleIds.FirstOrDefault();

                // فضّي الليستات (اختياري للتنضيف)
                dto.RolesName.Clear();
                dto.RolesId.Clear();
            }
            else if(userRoles.Count > 1)
            {
                // أكتر من Role
                dto.RolesName = userRoles;
                dto.RolesId = roleIds;

                // فضّي السينجل
                dto.RoleName = null;
                dto.RoleId = null;
            }
            else
            {
                // مفيش Roles أصلاً
                dto.RoleName = null;
                dto.RoleId = null;
                dto.RolesName.Clear();
                dto.RolesId.Clear();
            }
        }
        
       
   

        
        // Map roles safely
        // Pagination manually after projection (safe)
        var pagedResult = employeesList
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();
        return new PagedList<RepresentativeDTo>(
            pagedResult,
            employeesList.Count,
            paginationParams.PageNumber,
            paginationParams.PageSize);
    }
    //----------------------------------------------------------------------------
    public async Task<Result<RepresentativeSalaryDTo>> GetRepresentativeSalaryByYearAndMonth(string RepresentativeCode,int? Month,int? Year)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<RepresentativeSalaryDTo>.Failure("يجب تسجيل الدخول اولا",HttpStatusCode.Unauthorized);
        // تحديد الشهر والسنة
        int selectedYear = Year ?? DateTime.Now.Year;
        int selectedMonth = Month ?? DateTime.Now.Month;

        // الحصول على الموظف مع جميع البيانات ذات الصلة
        var Representative = await _UnitOfWork.GetRepository<Representatives,string>()
            .GetQueryable()
            .Include(e => e.User)
            .Include(e => e.EmployeeLoans)
            .FirstOrDefaultAsync(e => e.RepresentativesCode == RepresentativeCode);

        if(Representative == null)
            return Result<RepresentativeSalaryDTo>.Failure("هذا المندوب غير موجود",HttpStatusCode.NotFound);

        // تحويل WeekDays إلى DayOfWeek
        var weekHoliday1 = ConvertWeekDaysToDayOfWeek(Representative.WeekHoliday1);
        var weekHoliday2 = Representative.WeekHoliday2.HasValue ?
           ConvertWeekDaysToDayOfWeek(Representative.WeekHoliday2.Value) : (DayOfWeek?) null;

        if(weekHoliday1 == null && weekHoliday2 == null)
            return Result<RepresentativeSalaryDTo>.Failure("يرجى تحديد العطلات الأسبوعية للموظف في ملفه الشخصي",HttpStatusCode.BadRequest);
        
        // الحصول على العطلات الرسمية للشهر
        var publicHolidays = await _UnitOfWork.GetRepository<PublicHoliday,int>()
            .GetQueryable()
            .AsNoTracking()
            .Where(ph => ph.Date.Year == selectedYear && ph.Date.Month == selectedMonth && !ph.IsDeleted)
            .ToListAsync();

        // حساب أيام العمل الإجمالية
        int totalWorkingDays = CalculateWorkingDays(
            selectedYear,selectedMonth,weekHoliday1,weekHoliday2,publicHolidays);

        // الحصول على سجلات الحضور للشهر
        var attendanceRecords = await _UnitOfWork.GetRepository<RepresentativeAttendance,int>()
            .GetQueryable().Include(a => a.Representatives).ThenInclude(e => e!.User)
            .Where(a => a.RepresentativeCode == RepresentativeCode &&
                       a.AttendanceDate.HasValue &&
                       a.AttendanceDate.Value.Year == selectedYear &&
                       a.AttendanceDate.Value.Month == selectedMonth)
            .OrderBy(a => a.AttendanceDate)
            .ToListAsync();

        // حساب الأيام حسب الحالة
        var attendanceDetails = new List<AttendanceDetailDto>();
        int presentDays = 0;
        int lateDays = 0;
        int earlyLeaveDays = 0;
        double totalOvertimeHours = 0;
        double totalDeductionHours = 0;

        // حساب ساعات العمل المجدولة يومياً
        TimeSpan scheduledDuration = Representative.TimeOut - Representative.TimeIn;
        // التأكد من أن الوقت المجدول صالح
        double scheduledHours = scheduledDuration.TotalHours;

        // حساب تفاصيل الحضور
        foreach(var record in attendanceRecords)
        {
            var detail = new AttendanceDetailDto
            {
                Date = record.AttendanceDate!.Value,
                CheckIn = record.CheckInTime,
                CheckOut = record.CheckOutTime,
                Status = record.AttendanceStatus.ToString() ?? "Unknown"
            };

            // حساب ساعات العمل
            if(record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
            {
                // حساب الفرق بين وقت الدخول والخروج
                TimeSpan actualDuration = record.CheckOutTime.Value - record.CheckInTime.Value;

                detail.WorkHours = actualDuration.TotalHours;

                // حساب الوقت الإضافي والخصم
                if(detail.WorkHours > scheduledHours)
                {
                    detail.OvertimeHours = detail.WorkHours - scheduledHours;
                    totalOvertimeHours += detail.OvertimeHours;
                }
                else if(detail.WorkHours < scheduledHours)
                {
                    detail.DeductionHours = scheduledHours - detail.WorkHours;
                    totalDeductionHours += detail.DeductionHours;
                }
            }

            // حساب الحالات
            switch(record.AttendanceStatus)
            {
                case AttendanceStatus.Present:
                    presentDays++;
                    break;
                case AttendanceStatus.Late:
                    lateDays++;
                    break;
                case AttendanceStatus.Absent:
                    // سيتم حسابها لاحقاً
                    break;
                case AttendanceStatus.EarlyLeave:
                    earlyLeaveDays++;
                    break;
            }
            attendanceDetails.Add(detail);
        }

        // --- حساب الإجازات ---
        var leaveDetails = await CalculateLeavesForMonth(RepresentativeCode,selectedMonth,selectedYear,
            weekHoliday1,weekHoliday2,publicHolidays);

        // حساب أيام الإجازة المدفوعة وغير المدفوعة
        int paidLeaveDays = leaveDetails.Where(l => l.IsPaid).Sum(l => l.Days);
        int unpaidLeaveDays = leaveDetails.Where(l => !l.IsPaid).Sum(l => l.Days);
        
        // حساب الغياب الفعلي (بعد استبعاد الإجازات)
        int actualAbsentDays = Math.Max(0,totalWorkingDays - presentDays - paidLeaveDays - unpaidLeaveDays);

        // --- حساب القروض ---
        var loanDetails = await CalculateLoanDeductionsForMonth(RepresentativeCode,selectedMonth,selectedYear);
        decimal loanDeduction = loanDetails.Sum(l => l.InstallmentAmount);

        // --- حساب العقوبات والخصومات ---
        var sanctionDetails = await CalculateSanctionDeductionsForMonth(RepresentativeCode,selectedMonth,selectedYear);
        decimal sanctionAmount = sanctionDetails.Sum(s => s.Amount);

        // --- حساب المرتب الأساسي ---
        decimal basicSalary = Representative.Salary;
        decimal salaryPerDay = totalWorkingDays > 0 ? basicSalary / totalWorkingDays : 0;
        decimal salaryPerHour = scheduledHours > 0 ? salaryPerDay / (decimal) scheduledHours : 0;

        // --- حساب المستحقات ---
        decimal totalOvertimePay = (decimal) totalOvertimeHours * Representative.OvertimeRatePerHour * salaryPerHour;
        //decimal timeDeductionAmount = (decimal) totalDeductionHours * Representative.DeductionRatePerHour * salaryPerHour;
        decimal absentDeduction = actualAbsentDays * salaryPerDay;
        decimal unpaidLeaveDeduction = unpaidLeaveDays * salaryPerDay;

        decimal lateDeduction = lateDays * (salaryPerHour * 2); // مثال: خصم ساعتين لكل تأخير
        decimal earlyLeaveDeduction = earlyLeaveDays * (salaryPerHour * 1); // مثال: خصم ساعة لكل خروج مبكر

        // --- حساب الإجماليات ---
        decimal totalAdditions = totalOvertimePay;
        decimal totalDeductions = 
            //timeDeductionAmount + 
            absentDeduction + unpaidLeaveDeduction +
                                 lateDeduction + earlyLeaveDeduction + loanDeduction + sanctionAmount;

        decimal totalDeductionsWithLoans =
            //timeDeductionAmount +
            absentDeduction + unpaidLeaveDeduction +
                                 lateDeduction + earlyLeaveDeduction + loanDeduction + sanctionAmount;

        decimal totalDeductionsWithoutLoans =
            //timeDeductionAmount +
            absentDeduction + unpaidLeaveDeduction +
                                     lateDeduction + earlyLeaveDeduction + sanctionAmount;

        // --- حساب صافي المرتب ---
        decimal netSalary = basicSalary + totalAdditions - totalDeductions;
        netSalary = Math.Max(netSalary,0); // التأكد من أن المرتب لا يقل عن الصفر

        decimal grossSalary = basicSalary + totalOvertimePay; // الراتب الإجمالي قبل الخصومات
        decimal netSalaryWithoutLoans = grossSalary - totalDeductionsWithoutLoans;
        netSalaryWithoutLoans = Math.Max(netSalaryWithoutLoans,0);

        // إنشاء ملخص نصي
        string summary = $@"
                            المرتب الأساسي: {basicSalary:C}
                            أيام العمل: {totalWorkingDays} يوم
                            أيام الحضور: {presentDays} يوم
                            أيام الغياب: {actualAbsentDays} يوم
                            أيام الإجازة المدفوعة: {paidLeaveDays} يوم
                            أيام الإجازة بدون أجر: {unpaidLeaveDays} يوم
                            ساعات إضافية: {totalOvertimeHours:F2} ساعة
                            خصومات الوقت: {totalDeductionHours:F2} ساعة
                            إضافة الوقت الإضافي: {totalOvertimePay:C}
                            إجمالي الخصومات: {totalDeductions:C}
                            صافي المرتب: {netSalary:C}
                            ";
        var salaryDto = new RepresentativeSalaryDTo
            {
            RepresentativeId = Representative.UserId,
            RepresentativeCode = Representative.RepresentativesCode,
            RepresentativeName = Representative.User!.FullName,
            SelectedMonth = selectedMonth,
            SelectedYear = selectedYear,

            TotalWorkingDays = totalWorkingDays,
            PresentDays = presentDays,
            AbsentDays = actualAbsentDays,
            PaidLeaveDays = paidLeaveDays,
            UnpaidLeaveDays = unpaidLeaveDays,
            LateDays = lateDays,
            EarlyLeaveDays = earlyLeaveDays,

            BasicSalary = basicSalary,
            SalaryPerDay = salaryPerDay,
            SalaryPerHour = salaryPerHour,

            OvertimeHours = totalOvertimeHours,
            OvertimeRatePerHour = Representative.OvertimeRatePerHour,
            TotalOvertimePay = totalOvertimePay,

            DeductionHours = totalDeductionHours,
            //DeductionRatePerHour = Representative.DeductionRatePerHour,
            //TimeDeductionAmount = timeDeductionAmount,
            AbsentDeduction = absentDeduction,
            UnpaidLeaveDeduction = unpaidLeaveDeduction,
            LateDeduction = lateDeduction,
            EarlyLeaveDeduction = earlyLeaveDeduction,

            LoanDeduction = loanDeduction,
            LoanInstallmentsCount = loanDetails.Count,

            SanctionAmount = sanctionAmount,
            SanctionsCount = sanctionDetails.Count,


            TotalAdditions = totalAdditions,
            TotalDeductions = totalDeductions,
            NetSalary = netSalary,
            GrossSalary = grossSalary,
            NetSalaryBeforeLoans = netSalaryWithoutLoans,

            Summary = summary,

            LeaveDetails = leaveDetails,
            LoanDetails = loanDetails,
            SanctionDetails = sanctionDetails,
            AttendanceDetails = attendanceDetails

        };


        return Result<RepresentativeSalaryDTo>.Success(salaryDto,HttpStatusCode.OK);
    }
    //----------------------------------------------------------------------------
    public Task<Result<MonthlySalarySummaryDto>> GetRepresentativeMonthlySalarySummaryAsync(string empCode,int? month,int? year)
    {
        throw new NotImplementedException();
    }
    //----------------------------------------------------------------------------
    public Task<Result<MonthlyStatisticsDto>> GetRepresentativeMonthlyStatisticsAsync(string empCode,int? month,int? year)
    {
        throw new NotImplementedException();
    }
    //----------------------------------------------------------------------------
    public Task<Result<SalaryComparisonDto>> CompareRepresentativeMonthlySalariesAsync(string empCode,int baseMonth,int baseYear,int compareMonth,int compareYear)
    {
        throw new NotImplementedException();
    }
    //----------------------------------------------------------------------------
    public Task<Result<SalaryHistoryDto>> GetRepresentativeSalaryHistoryAsync(string empCode,int? year)
    {
        throw new NotImplementedException();
    }
    //--------------------------------------------------------------------------
    // الدوال المساعدة
    private int CalculateWorkingDays(int year,int month,DayOfWeek? weekHoliday1,
                            DayOfWeek? weekHoliday2,List<PublicHoliday> publicHolidays)
    {
        int totalDays = DateTime.DaysInMonth(year,month);
        int workingDays = 0;

        for(int day = 1;day <= totalDays;day++)
        {
            DateTime currentDate = new DateTime(year,month,day);
            DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

            // التحقق إذا كان يوم عطلة أسبوعية
            if(currentDayOfWeek == weekHoliday1 || currentDayOfWeek == weekHoliday2)
                continue;

            // التحقق إذا كان يوم عطلة رسمية
            DateOnly dateOnly = DateOnly.FromDateTime(currentDate);
            if(publicHolidays.Any(ph => ph.Date == dateOnly))
                continue;

            workingDays++;
        }

        return workingDays;
    }


    private List<DeductionBreakdownDto> CalculateDeductionBreakdown(EmployeeSalaryDTo salary)
    {
        var breakdown = new List<DeductionBreakdownDto>();
        decimal totalDeductions = salary.TotalDeductions > 0 ? salary.TotalDeductions : 1;

        breakdown.Add(new DeductionBreakdownDto
        {
            DeductionType = "خصم الوقت",
            Amount = salary.TimeDeductionAmount,
            Percentage = (salary.TimeDeductionAmount / totalDeductions) * 100
        });

        breakdown.Add(new DeductionBreakdownDto
        {
            DeductionType = "خصم الغياب",
            Amount = salary.AbsentDeduction,
            Percentage = (salary.AbsentDeduction / totalDeductions) * 100
        });

        breakdown.Add(new DeductionBreakdownDto
        {
            DeductionType = "خصم إجازات بدون أجر",
            Amount = salary.UnpaidLeaveDeduction,
            Percentage = (salary.UnpaidLeaveDeduction / totalDeductions) * 100
        });

        if(salary.LateDeduction > 0)
        {
            breakdown.Add(new DeductionBreakdownDto
            {
                DeductionType = "خصم تأخير",
                Amount = salary.LateDeduction,
                Percentage = (salary.LateDeduction / totalDeductions) * 100
            });
        }

        if(salary.LoanDeduction > 0)
        {
            breakdown.Add(new DeductionBreakdownDto
            {
                DeductionType = "أقساط قروض",
                Amount = salary.LoanDeduction,
                Percentage = (salary.LoanDeduction / totalDeductions) * 100
            });
        }

        if(salary.SanctionAmount > 0)
        {
            breakdown.Add(new DeductionBreakdownDto
            {
                DeductionType = "عقوبات",
                Amount = salary.SanctionAmount,
                Percentage = (salary.SanctionAmount / totalDeductions) * 100
            });
        }

        return breakdown;
    }

    private List<AdditionBreakdownDto> CalculateAdditionBreakdown(EmployeeSalaryDTo salary)
    {
        var breakdown = new List<AdditionBreakdownDto>();
        decimal totalAdditions = salary.TotalAdditions > 0 ? salary.TotalAdditions : 1;

        breakdown.Add(new AdditionBreakdownDto
        {
            AdditionType = "وقت إضافي",
            Amount = salary.TotalOvertimePay,
            Percentage = (salary.TotalOvertimePay / totalAdditions) * 100
        });

        return breakdown;
    }

    private async Task<List<DailyAttendanceDto>> GetDailyAttendanceBreakdownAsync(string empCode,int month,int year)
    {
        var dailyBreakdown = new List<DailyAttendanceDto>();

        var attendanceRecords = await _UnitOfWork.GetRepository<EmployeeAttendance,int>()
            .GetQueryable()
            .Where(a => a.EmployeeCode == empCode &&
                       a.AttendanceDate.HasValue &&
                       a.AttendanceDate.Value.Year == year &&
                       a.AttendanceDate.Value.Month == month)
            .ToListAsync();

        var employee = await _UnitOfWork.GetRepository<Employee,string>()
            .GetQueryable()
            .FirstOrDefaultAsync(e => e.EmployeeCode == empCode);

        if(employee == null)
            return dailyBreakdown;

        // تحويل WeekDays إلى DayOfWeek
        var weekHoliday1 = ConvertWeekDaysToDayOfWeek(employee.WeekHoliday1);
        var weekHoliday2 = employee.WeekHoliday2.HasValue ?
            ConvertWeekDaysToDayOfWeek(employee.WeekHoliday2.Value) : (DayOfWeek?) null;

        // الحصول على العطلات الرسمية
        var publicHolidays = await _UnitOfWork.GetRepository<PublicHoliday,int>()
            .GetQueryable()
            .Where(ph => ph.Date.Year == year && ph.Date.Month == month && !ph.IsDeleted)
            .ToListAsync();

        // عدد أيام الشهر
        int daysInMonth = DateTime.DaysInMonth(year,month);

        for(int day = 1;day <= daysInMonth;day++)
        {
            var currentDate = new DateOnly(year,month,day);
            var attendance = attendanceRecords.FirstOrDefault(a => a.AttendanceDate == currentDate);

            var isHoliday = IsHoliday(currentDate.ToDateTime(TimeOnly.MinValue),
                weekHoliday1,weekHoliday2,publicHolidays);

            double workHours = 0;
            if(attendance?.CheckInTime.HasValue == true && attendance.CheckOutTime.HasValue == true)
            {
                workHours = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
            }

            dailyBreakdown.Add(new DailyAttendanceDto
            {
                Date = currentDate,
                DayOfWeek = GetArabicDayName(currentDate.DayOfWeek),
                Status = attendance?.AttendanceStatus.ToString() ?? (isHoliday ? "عطلة" : "غير مسجل"),
                WorkHours = workHours,
                IsHoliday = isHoliday
            });
        }

        return dailyBreakdown;
    }

    private PerformanceAnalysisDto CalculatePerformanceAnalysis(EmployeeSalaryDTo salary)
    {
        var performance = new PerformanceAnalysisDto();

        // درجة الالتزام بالمواعيد (100 - (أيام التأخير * 10))
        performance.PunctualityScore = Math.Max(0,100 - (salary.LateDays * 10));

        // درجة الحضور (نسبة الحضور * 100)
        performance.AttendanceScore = salary.TotalWorkingDays > 0 ?
            (salary.PresentDays * 100m) / salary.TotalWorkingDays : 0;

        // درجة الإضافي (كل 5 ساعات إضافية = 10 نقاط، بحد أقصى 30)
        performance.OvertimeScore = Math.Min(30,(decimal) salary.OvertimeHours / 5 * 10);

        // درجة الإنتاجية (متوسط الدرجات)
        performance.ProductivityScore = (performance.PunctualityScore +
                                         performance.AttendanceScore +
                                         performance.OvertimeScore) / 3;

        // تحديد مستوى الأداء
        performance.PerformanceLevel = performance.ProductivityScore switch
        {
            >= 90 => "ممتاز",
            >= 80 => "جيد جداً",
            >= 70 => "جيد",
            >= 60 => "مقبول",
            _ => "ضعيف"
        };

        // نقاط القوة
        if(performance.PunctualityScore >= 90)
            performance.Strengths.Add("التزام عالي بالمواعيد");

        if(performance.AttendanceScore >= 95)
            performance.Strengths.Add("حضور ممتاز");

        if(performance.OvertimeScore >= 20)
            performance.Strengths.Add("تفاني في العمل");

        // مجالات التحسين
        if(salary.AbsentDays > 2)
            performance.AreasForImprovement.Add("تحسين معدل الحضور");

        if(salary.LateDays > 3)
            performance.AreasForImprovement.Add("الالتزام بالمواعيد");

        if(salary.DeductionHours > 10)
            performance.AreasForImprovement.Add("تحسين عدد ساعات العمل");

        return performance;
    }

    private List<KeyMetricDto> CalculateKeyMetrics(EmployeeSalaryDTo salary)
    {
        var metrics = new List<KeyMetricDto>();

        metrics.Add(new KeyMetricDto
        {
            MetricName = "نسبة الحضور",
            Value = salary.TotalWorkingDays > 0 ? (salary.PresentDays * 100m) / salary.TotalWorkingDays : 0,
            Target = 95,
            Status = salary.TotalWorkingDays > 0 && (salary.PresentDays * 100m) / salary.TotalWorkingDays >= 95 ? "جيد" : "ضعيف",
            Trend = "مستقر"
        });

        metrics.Add(new KeyMetricDto
        {
            MetricName = "نسبة الوقت الإضافي",
            Value = salary.TotalOvertimePay > 0 ? (salary.TotalOvertimePay / salary.BasicSalary) * 100 : 0,
            Target = 10,
            Status = salary.TotalOvertimePay > 0 && (salary.TotalOvertimePay / salary.BasicSalary) * 100 <= 15 ? "جيد" : "مرتفع",
            Trend = "مستقر"
        });

        metrics.Add(new KeyMetricDto
        {
            MetricName = "نسبة الخصومات",
            Value = salary.TotalDeductions > 0 ? (salary.TotalDeductions / salary.BasicSalary) * 100 : 0,
            Target = 5,
            Status = salary.TotalDeductions > 0 && (salary.TotalDeductions / salary.BasicSalary) * 100 <= 5 ? "جيد" : "مرتفع",
            Trend = "مستقر"
        });

        return metrics;
    }

    private List<string> GenerateSalaryRecommendations(EmployeeSalaryDTo salary)
    {
        var recommendations = new List<string>();

        if(salary.AbsentDays > 2)
            recommendations.Add($"مستوى الغياب مرتفع ({salary.AbsentDays} يوم). ينصح بمتابعة الموظف.");

        if(salary.LateDays > 3)
            recommendations.Add($"عدد أيام التأخير كبير ({salary.LateDays} يوم). ينصح باتخاذ إجراء.");

        if(salary.UnpaidLeaveDays > 0)
            recommendations.Add($"يوجد {salary.UnpaidLeaveDays} يوم إجازة بدون أجر.");

        if(salary.LoanDeduction > 0)
            recommendations.Add($"يتم خصم {salary.LoanDeduction:C} كأقساط قرض هذا الشهر.");

        if(salary.SanctionAmount > 0)
            recommendations.Add($"يوجد {salary.SanctionAmount:C} كخصومات عقوبات.");

        if(salary.OvertimeHours > 40)
            recommendations.Add($"عدد ساعات العمل الإضافي مرتفع ({salary.OvertimeHours:F2} ساعة). ينصح بمراجعة عبء العمل.");

        if(salary.DeductionHours > 10)
            recommendations.Add($"عدد ساعات الخصم مرتفع ({salary.DeductionHours:F2} ساعة). ينصح بمتابعة الدوام.");

        // توصيات إيجابية
        if(salary.AbsentDays == 0)
            recommendations.Add("الأداء ممتاز في الحضور - لم يغب الموظف طوال الشهر.");

        if(salary.LateDays == 0)
            recommendations.Add("التزام ممتاز بالمواعيد - لم يتأخر الموظف طوال الشهر.");

        if(salary.OvertimeHours > 0 && salary.OvertimeHours <= 20)
            recommendations.Add("الموظف يبذل جهداً إضافياً معقولاً.");

        if(salary.NetSalary / salary.BasicSalary >= 0.95m)
            recommendations.Add("المرتب قريب من الراتب الأساسي - أداء جيد.");

        return recommendations;
    }





    private async Task<List<LeaveDetailDto>> CalculateLeavesForMonth(string RepresentativeCode,int month,int year,
        DayOfWeek? weekHoliday1,DayOfWeek? weekHoliday2,List<PublicHoliday> publicHolidays)
    {
        var leaveDetails = new List<LeaveDetailDto>();

        var leaveRequests = await _UnitOfWork.GetRepository<EmployeeLeaveRequest,int>()
            .GetQueryable()
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.RepresentativeCode == RepresentativeCode &&
                        lr.Status == LeaveRequestStatus.Approved &&
                        !lr.IsDeleted &&
                        lr.FromDate <= new DateTime(year,month,DateTime.DaysInMonth(year,month)) &&
                        lr.ToDate >= new DateTime(year,month,1))
            .ToListAsync();

        foreach(var request in leaveRequests)
        {
            DateTime startDate = request.FromDate > new DateTime(year,month,1)
                ? request.FromDate
                : new DateTime(year,month,1);

            DateTime endDate = request.ToDate < new DateTime(year,month,DateTime.DaysInMonth(year,month))
                ? request.ToDate
                : new DateTime(year,month,DateTime.DaysInMonth(year,month));

            // حساب أيام العمل في فترة الإجازة
            int workingDaysInLeave = 0;
            for(DateTime date = startDate;date <= endDate;date = date.AddDays(1))
            {
                DayOfWeek dayOfWeek = date.DayOfWeek;
                DateOnly dateOnly = DateOnly.FromDateTime(date);

                // استبعاد العطلات الأسبوعية
                if(dayOfWeek == weekHoliday1 || dayOfWeek == weekHoliday2)
                    continue;

                // استبعاد العطلات الرسمية
                if(publicHolidays.Any(ph => ph.Date == dateOnly))
                    continue;

                workingDaysInLeave++;
            }

            if(workingDaysInLeave > 0)
            {
                leaveDetails.Add(new LeaveDetailDto
                {
                    LeaveType = request.LeaveType?.Name ?? "غير معروف",
                    FromDate = startDate,
                    ToDate = endDate,
                    Days = workingDaysInLeave,
                    IsPaid = request.LeaveType?.IsPaid ?? false,
                    Status = request.Status.ToString()
                });
            }
        }

        return leaveDetails;
    }
    private async Task<List<LoanDetailDto>> CalculateLoanDeductionsForMonth(string RepresentativeCode,int month,int year)
    {
        var loanDetails = new List<LoanDetailDto>();

        var targetMonth = new DateTime(year,month,1);

        // الحصول على القروض النشطة
        var activeLoans = await _UnitOfWork.GetRepository<EmployeeLoan,int>()
            .GetQueryable()
            .Include(l => l.Employee)
            .ThenInclude(e => e!.User)
            .Where(l => l.RepresentativeCode == RepresentativeCode &&
                       l.Status == LoanStatus.Active &&
                       !l.IsDeleted &&
                       !l.IsPaidOff)
            .ToListAsync();

        foreach(var loan in activeLoans)
        {
            // حساب عدد الأقساط المدفوعة
            int paidInstallments = (int) Math.Floor(loan.PaidAmount / loan.InstallmentAmount);

            // تاريخ القسط التالي
            DateTime nextDueDate = loan.FirstInstallmentDate.AddMonths(paidInstallments);

            // التحقق إذا كان القسط مستحق هذا الشهر
            if(nextDueDate.Year == year && nextDueDate.Month == month)
            {
                loanDetails.Add(new LoanDetailDto
                {
                    LoanNumber = loan.LoanNumber,
                    LoanAmount = loan.LoanAmount,
                    InstallmentAmount = loan.InstallmentAmount,
                    DueDate = nextDueDate,
                    IsPaid = false,
                    Status = loan.Status.ToString()
                });
            }
        }

        return loanDetails;
    }
    private async Task<List<SanctionDetailDto>> CalculateSanctionDeductionsForMonth(string RepresentativeCode,int month,int year)
    {
        var sanctionDetails = new List<SanctionDetailDto>();

        // الحصول على خصومات العقوبات للشهر
        var sanctions = await _UnitOfWork.GetRepository<PayrollDeductions,int>()
            .GetQueryable()
            .Where(d => d.RepresentativeCode == RepresentativeCode &&
                       d.DeductionDate.Year == year &&
                       d.DeductionDate.Month == month &&
                       !d.IsDeleted)
            .ToListAsync();

        foreach(var sanction in sanctions)
        {
            sanctionDetails.Add(new SanctionDetailDto
            {
                Date = sanction.DeductionDate,
                Reason = sanction.DeductionReason,
                Amount = sanction.MoneyAmount,
                DeductionAmount = sanction.DeductionAmount,
                Type = "خصم"
            });
        }

        return sanctionDetails;
    }
    private static DayOfWeek ConvertWeekDaysToDayOfWeek(WeekDays weekDay)
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
                return DayOfWeek.Saturday; // Default value
        }
    }
    private string GetArabicDayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الاثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => "غير معروف"
        };
    }
    private async Task<decimal> CalculateSalaryGrowthRateAsync(string RepresentativeCode,int? month,int? year)
    {
        try
        {
            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;

            // الحصول على الشهر السابق
            DateTime previousMonthDate = new DateTime(currentYear,currentMonth,1).AddMonths(-1);
            int previousMonth = previousMonthDate.Month;
            int previousYear = previousMonthDate.Year;

            // جلب راتب الشهر الحالي
            var currentSalaryResult = await GetRepresentativeSalaryByYearAndMonth(RepresentativeCode,currentMonth,currentYear);
            if(!currentSalaryResult.IsSuccess)
                return 0;

            // جلب راتب الشهر السابق
            var previousSalaryResult = await GetRepresentativeSalaryByYearAndMonth(RepresentativeCode,previousMonth,previousYear);
            if(!previousSalaryResult.IsSuccess)
                return 0;

            decimal currentNetSalary = currentSalaryResult.Data!.NetSalary;
            decimal previousNetSalary = previousSalaryResult.Data!.NetSalary;

            if(previousNetSalary == 0)
                return 0;

            return ((currentNetSalary - previousNetSalary) / previousNetSalary) * 100;
        }
        catch
        {
            return 0;
        }
    }
    private decimal CalculatePunctualityIndex(RepresentativeSalaryDTo salary)
    {
        // مؤشر الالتزام بالمواعيد: 100 - (عدد أيام التأخير * 5) - (عدد أيام الخروج المبكر * 3)
        decimal index = 100 - (salary.LateDays * 5) - (salary.EarlyLeaveDays * 3);
        return Math.Max(index,0);
    }
    private string CalculateAttendanceGrade(RepresentativeSalaryDTo salary)
    {
        decimal attendanceRate = salary.TotalWorkingDays > 0 ?
            (salary.PresentDays * 100m) / salary.TotalWorkingDays : 0;

        return attendanceRate switch
        {
            >= 95 => "ممتاز",
            >= 90 => "جيد جداً",
            >= 80 => "جيد",
            >= 70 => "مقبول",
            _ => "ضعيف"
        };
    }
    private string GetMostUsedLeaveType(List<LeaveDetailDto> leaveDetails)
    {
        if(!leaveDetails.Any())
            return "لا توجد إجازات";

        var mostUsed = leaveDetails
            .GroupBy(l => l.LeaveType)
            .Select(g => new { LeaveType = g.Key,Days = g.Sum(x => x.Days) })
            .OrderByDescending(x => x.Days)
            .FirstOrDefault();

        return mostUsed?.LeaveType ?? "غير معروف";
    }
    private string AnalyzeOvertimePattern(List<AttendanceDetailDto> attendanceDetails)
    {
        if(!attendanceDetails.Any())
            return "لا يوجد عمل إضافي";

        var overtimeDays = attendanceDetails.Count(a => a.OvertimeHours > 0);
        var totalDays = attendanceDetails.Count(a => a.WorkHours > 0);

        if(totalDays == 0)
            return "لا يوجد عمل إضافي";

        decimal overtimeFrequency = (decimal) overtimeDays / totalDays * 100;

        if(overtimeFrequency >= 80)
            return "منتظم";
        if(overtimeFrequency >= 50)
            return "موسمي";
        return "عشوائي";
    }
    private string CalculatePerformanceChange(RepresentativeSalaryDTo baseSalary,RepresentativeSalaryDTo compareSalary)
    {
        // حساب أداء الشهر الأساسي
        decimal basePerformance = (baseSalary.PresentDays * 100m / baseSalary.TotalWorkingDays)
                                - (baseSalary.LateDays * 5)
                                - (baseSalary.EarlyLeaveDays * 3);

        // حساب أداء الشهر المقارن
        decimal comparePerformance = (compareSalary.PresentDays * 100m / compareSalary.TotalWorkingDays)
                                   - (compareSalary.LateDays * 5)
                                   - (compareSalary.EarlyLeaveDays * 3);

        decimal change = comparePerformance - basePerformance;

        if(change > 10)
            return "تحسن كبير";
        else if(change > 5)
            return "تحسن";
        else if(change < -10)
            return "تراجع كبير";
        else if(change < -5)
            return "تراجع";
        else
            return "مستقر";
    }
    private string GetMonthName(int month)
    {
        return month switch
        {
            1 => "يناير",
            2 => "فبراير",
            3 => "مارس",
            4 => "أبريل",
            5 => "مايو",
            6 => "يونيو",
            7 => "يوليو",
            8 => "أغسطس",
            9 => "سبتمبر",
            10 => "أكتوبر",
            11 => "نوفمبر",
            12 => "ديسمبر",
            _ => "غير معروف"
        };
    }
    private string CalculatePerformanceLevel(RepresentativeSalaryDTo salary)
    {
        decimal attendanceRate = salary.TotalWorkingDays > 0 ?
            (salary.PresentDays * 100m) / salary.TotalWorkingDays : 0;

        if(attendanceRate >= 95 && salary.LateDays == 0 && salary.EarlyLeaveDays == 0)
            return "ممتاز";
        else if(attendanceRate >= 90 && salary.LateDays <= 2 && salary.EarlyLeaveDays <= 2)
            return "جيد جداً";
        else if(attendanceRate >= 80)
            return "جيد";
        else if(attendanceRate >= 70)
            return "مقبول";
        else
            return "ضعيف";
    }
    private string GenerateComparisonSummary(RepresentativeSalaryDTo baseSalary,RepresentativeSalaryDTo compareSalary,decimal salaryChangePercentage)
    {
        var summary = new StringBuilder();

        summary.AppendLine($"مقارنة بين {baseSalary.SelectedMonth}/{baseSalary.SelectedYear} و {compareSalary.SelectedMonth}/{compareSalary.SelectedYear}");
        summary.AppendLine();

        if(salaryChangePercentage > 0)
        {
            summary.AppendLine($"✅ زيادة في صافي الراتب بنسبة {salaryChangePercentage:F2}%");
            summary.AppendLine($"   - الراتب الأساسي: {baseSalary.NetSalary:C} → {compareSalary.NetSalary:C}");
        }
        else if(salaryChangePercentage < 0)
        {
            summary.AppendLine($"⚠️ نقصان في صافي الراتب بنسبة {Math.Abs(salaryChangePercentage):F2}%");
            summary.AppendLine($"   - الراتب الأساسي: {baseSalary.NetSalary:C} → {compareSalary.NetSalary:C}");
        }
        else
        {
            summary.AppendLine("⚖️ لا يوجد تغير في صافي الراتب");
        }

        // مقارنة الحضور
        if(compareSalary.PresentDays > baseSalary.PresentDays)
        {
            summary.AppendLine($"✅ تحسن في الحضور: +{compareSalary.PresentDays - baseSalary.PresentDays} يوم");
        }
        else if(compareSalary.PresentDays < baseSalary.PresentDays)
        {
            summary.AppendLine($"⚠️ تراجع في الحضور: -{baseSalary.PresentDays - compareSalary.PresentDays} يوم");
        }

        // مقارنة الوقت الإضافي
        if(compareSalary.OvertimeHours > baseSalary.OvertimeHours)
        {
            summary.AppendLine($"📈 زيادة في ساعات العمل الإضافي: +{(compareSalary.OvertimeHours - baseSalary.OvertimeHours):F2} ساعة");
        }
        else if(compareSalary.OvertimeHours < baseSalary.OvertimeHours)
        {
            summary.AppendLine($"📉 نقصان في ساعات العمل الإضافي: -{(baseSalary.OvertimeHours - compareSalary.OvertimeHours):F2} ساعة");
        }

        return summary.ToString();
    }
    private bool IsHoliday(DateTime date,DayOfWeek? weekHoliday1,DayOfWeek? weekHoliday2,List<PublicHoliday> publicHolidays)
    {
        try
        {
            // 1. التحقق من العطلات الأسبوعية
            DayOfWeek currentDay = date.DayOfWeek;

            if(weekHoliday1.HasValue && currentDay == weekHoliday1.Value)
                return true;

            if(weekHoliday2.HasValue && currentDay == weekHoliday2.Value)
                return true;

            // 2. التحقق من العطلات الرسمية
            DateOnly dateOnly = DateOnly.FromDateTime(date);

            if(publicHolidays.Any(ph => ph.Date == dateOnly))
                return true;


            return false;
        }
        catch(Exception)
        {
            // في حالة حدوث خطأ، نعتبره يوم عمل لتجنب التأثير على الحسابات
            return false;
        }
    }
}
