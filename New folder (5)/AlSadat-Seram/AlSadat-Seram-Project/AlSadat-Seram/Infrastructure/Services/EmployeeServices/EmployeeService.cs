using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeSalary;
using Application.Helper;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeeService;
using Application.Services.contract.PayrollDeduction;
using Domain.Common;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Infrastructure.Services.CurrentUserServices;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text;

namespace Infrastructure.Services.EmployeeServices;

internal class EmployeeService : IEmployeeService
{
    private readonly UserManager<ApplicationUser> _UserManager;
    private readonly ICurrentUserService _CurrentUserService;
    private readonly IUnitOfWork _UnitOfWork;
    private readonly RoleManager<ApplicationRole> _RoleManager;

    public EmployeeService(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,RoleManager<ApplicationRole> roleManager)
    {
        _UserManager = userManager;
        _CurrentUserService = currentUserService;
        _UnitOfWork = unitOfWork;
        _RoleManager = roleManager;
    }
    //--------------------------------------------------------------------------
    public async Task<Result<string>> AddEmployeeAsync(EmployeeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
        // Check if employee with the same Email already exists
        if (await _UserManager.Users.AnyAsync(u => u.Email == DTo.Email))
            return Result<string>.Failure("An Employee With The Same Email Already Exists.", HttpStatusCode.Conflict);
        // Check if employee with the same EmployeeCode already exists
        if (await _UnitOfWork.GetRepository<Employee, string>().AnyAsync(e => e.EmployeeCode == DTo.EmployeeCode))
            return Result<string>.Failure("Employee Code Already Exists.", HttpStatusCode.Conflict);

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
            var result = await _UserManager.CreateAsync(user, DTo.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return Result<string>.Failure($"Failed to create Employee. Errors: {errors}", HttpStatusCode.BadRequest);
            }
            // Assign Role to Employee
            //var roleResult = await _UserManager.AddToRoleAsync(user,DTo.RoleName);
            //if(!roleResult.Succeeded)
            //{
            //    var errors = string.Join("; ",roleResult.Errors.Select(e => e.Description));
            //    return Result<string>.Failure($"Failed to assign role to Employee. Errors: {errors}",HttpStatusCode.BadRequest);
            //}
            if (!string.IsNullOrWhiteSpace(DTo.RoleName))
            {
                var currentRoles = await _UserManager.GetRolesAsync(user);

                // Remove old roles
                await _UserManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new role
                var roleAddResult = await _UserManager.AddToRoleAsync(user, DTo.RoleName);

                if (!roleAddResult.Succeeded)
                {
                    var errors = string.Join("; ", roleAddResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"Failed to update role. Errors: {errors}");
                }
            }
            var employee = new Employee
            {
                UserId = user.Id,
                EmployeeCode = DTo.EmployeeCode,
                AccountNumber = DTo.AccountNumber,
                AccountName = DTo.AccountName,
                SNO = DTo.SNO,
                OvertimeRatePerHour = DTo.OvertimeRatePerHour,
                DeductionRatePerHour = DTo.DeductionRatePerHour,
                DepartmentID = DTo.DepartmentID,
                BirthDate = DTo.BirthDate,
                HireDate = DTo.HireDate,
                Salary = DTo.Salary,
                TimeIn = DTo.TimeIn,
                TimeOut = DTo.TimeOut,
                WeekHoliday1 = DTo.WeekHoliday1,
                WeekHoliday2 = DTo.WeekHoliday2,
                CreateBy = userId,
            };

            await _UnitOfWork.GetRepository<Employee, string>().AddAsync(employee);
            await _UnitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return Result<string>.Success("Employee created successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<string>.Failure($"Transaction failed: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    //--------------------------------------------------------------------------
    public async Task<PagedList<EmployeeDTo>> GetAllEmployeeAsync(PaginationParams paginationParams)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return new PagedList<EmployeeDTo>(new List<EmployeeDTo>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        var query = _UnitOfWork.GetRepository<Employee, string>().GetQueryable();
        if (query == null)
            return new PagedList<EmployeeDTo>(new List<EmployeeDTo>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        query = query.AsNoTracking()
                     .Include(e => e.User)
                     .ThenInclude(c => c.City)
                     .Include(e => e.Department);

        var result = query.Select(e => new EmployeeDTo
        {
            UserId = e.UserId,
            Email = e.User!.Email != null ? e.User.Email : "",
            FullName = e.User.FullName,
            Gender = e.User.Gender,
            PhoneNumber = e.User.PhoneNumber,
            EmployeeCode = e.EmployeeCode,
            Address = e.User.Address,
            AccountNumber = e.AccountNumber,
            AccountName = e.AccountName,
            SNO = e.SNO,
            OvertimeRatePerHour = e.OvertimeRatePerHour,
            DeductionRatePerHour = e.DeductionRatePerHour,
            DepartmentName = e.Department != null ? e.Department.Name : null,
            DepartmentID = e.DepartmentID,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            Salary = e.Salary,
            TimeIn = e.TimeIn,
            TimeOut = e.TimeOut,
            WeekHoliday1 = e.WeekHoliday1,
            WeekHoliday2 = e.WeekHoliday2,
            CityName = e.User.City != null ? e.User.City.Name : null,
            CreateBy = e.CreateBy,
            NameOfCreatedBy = _UserManager.Users
            .Where(u => u.Id == e.CreateBy)
            .Select(u => u.FullName)
            .FirstOrDefault() ?? "System",

        }).AsNoTracking();

        foreach (var emp in result)
        {
            var user = await _UserManager.FindByIdAsync(emp.UserId!);
            if (user != null)
            {
                var roles = await _UserManager.GetRolesAsync(user);
                emp.RolesName = roles.ToList();

                emp.RolesId = roles
                    .Select(roleName => _RoleManager.Roles
                        .Where(r => r.Name == roleName)
                        .Select(r => r.Id)
                        .FirstOrDefault())
                    .Where(id => id != null)
                    .ToList()!;
            }
        }

        var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        return pagedResult;

    }
    //--------------------------------------------------------------------------
    public async Task<PagedList<EmployeeDTo>> GetEmployeeByFilterAsync(
    PaginationParams paginationParams,
    EmployeeHelper search)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return new PagedList<EmployeeDTo>(
                new List<EmployeeDTo>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        var query = _UnitOfWork.GetRepository<Employee, string>().GetQueryable().AsNoTracking();
        if (query == null)
            return new PagedList<EmployeeDTo>(new List<EmployeeDTo>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        query = query.Include(e => e.User).ThenInclude(c => c.City).Include(e => e.Department);

        // Apply filters safely
        if (!string.IsNullOrEmpty(search.EmpCode))
            query = query.Where(e => e.EmployeeCode != null &&
                                     e.EmployeeCode.Contains(search.EmpCode));

        if (!string.IsNullOrEmpty(search.EmployeeName))
            query = query.Where(e => e.User != null &&
                                     e.User.FullName != null &&
                                     e.User.FullName.Contains(search.EmployeeName));

        if (!string.IsNullOrEmpty(search.DepartmentName))
            query = query.Where(e => e.Department != null &&
                                     e.Department.Name != null &&
                                     e.Department.Name.Contains(search.DepartmentName));

        if (!string.IsNullOrEmpty(search.CityName))
            query = query.Where(e => e.User != null &&
                                     e.User.City != null &&
                                     e.User.City.Name != null &&
                                     e.User.City.Name.Contains(search.CityName));


        // Project directly to DTO
        var projectedQuery = query.Select(e => new EmployeeDTo
        {
            UserId = e.UserId,
            Email = e.User!.Email ?? "",
            FullName = e.User.FullName,
            Gender = e.User.Gender,
            PhoneNumber = e.User.PhoneNumber,
            EmployeeCode = e.EmployeeCode,
            Address = e.User.Address,
            AccountNumber = e.AccountNumber,
            AccountName = e.AccountName,
            SNO = e.SNO,
            OvertimeRatePerHour = e.OvertimeRatePerHour,
            DeductionRatePerHour = e.DeductionRatePerHour,
            DepartmentName = e.Department != null ? e.Department.Name : null,
            DepartmentID = e.DepartmentID,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            Salary = e.Salary,
            TimeIn = e.TimeIn,
            TimeOut = e.TimeOut,
            WeekHoliday1 = e.WeekHoliday1,
            WeekHoliday2 = e.WeekHoliday2,
            CityName = e.User.City != null ? e.User.City.Name : null,
            CreateBy = e.CreateBy,
            NameOfCreatedBy = _UserManager.Users
                .Where(u => u.Id == e.CreateBy)
                .Select(u => u.FullName)
                .FirstOrDefault() ?? "System"
        });

        // Fetch all employees first (to map roles)
        var employeesList = await projectedQuery.ToListAsync();
        var userIds = employeesList.Select(e => e.UserId).Distinct().ToList();

        var users = await _UserManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var rolesMap = new Dictionary<string, List<string>>();
        foreach (var user in users)
        {
            var roles = await _UserManager.GetRolesAsync(user);
            rolesMap[user.Id] = roles.ToList(); // safe even if empty
        }

        var allRoles = await _RoleManager.Roles.ToListAsync();

        // Map roles safely
        foreach (var dto in employeesList)
        {
            var userRoles = rolesMap.GetValueOrDefault(dto.UserId!, new List<string>());
            dto.RolesName = userRoles;
            dto.RolesId = userRoles
                .Select(roleName => allRoles.FirstOrDefault(r => r.Name == roleName)?.Id)
                .Where(id => id != null)
                .ToList()!;
        }

        // Pagination manually after projection (safe)
        var pagedResult = employeesList
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        return new PagedList<EmployeeDTo>(
            pagedResult,
            employeesList.Count,
            paginationParams.PageNumber,
            paginationParams.PageSize);
    }
    //--------------------------------------------------------------------------
    public async Task<PagedList<EmployeeDTo>> GetAllActiveEmployeeAsync(PaginationParams paginationParams)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return new PagedList<EmployeeDTo>(new List<EmployeeDTo>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
        var query = _UnitOfWork.GetRepository<Employee, string>().GetQueryable()
                     .AsNoTracking()
                     .Include(e => e.User)
                     .ThenInclude(c => c.City)
                     .Include(e => e.Department)
                     .Where(e => e.IsDeleted == false);

        var result = query.Select(e => new EmployeeDTo
        {
            UserId = e.UserId,
            Email = e.User!.Email != null ? e.User.Email : "",
            FullName = e.User.FullName,
            Gender = e.User.Gender,
            PhoneNumber = e.User.PhoneNumber,
            EmployeeCode = e.EmployeeCode,
            Address = e.User.Address,
            AccountNumber = e.AccountNumber,
            AccountName = e.AccountName,
            SNO = e.SNO,
            OvertimeRatePerHour = e.OvertimeRatePerHour,
            DeductionRatePerHour = e.DeductionRatePerHour,
            DepartmentName = e.Department != null ? e.Department.Name : null,
            DepartmentID = e.DepartmentID,
            BirthDate = e.BirthDate,
            HireDate = e.HireDate,
            Salary = e.Salary,
            TimeIn = e.TimeIn,
            TimeOut = e.TimeOut,
            WeekHoliday1 = e.WeekHoliday1,
            WeekHoliday2 = e.WeekHoliday2,
            CityName = e.User.City != null ? e.User.City.Name : null,
            CreateBy = e.CreateBy,
            NameOfCreatedBy = _UserManager.Users
            .Where(u => u.Id == e.CreateBy)
            .Select(u => u.FullName)
            .FirstOrDefault() ?? "System",

        }).AsNoTracking();
        foreach (var emp in result)
        {
            var user = await _UserManager.FindByIdAsync(emp.UserId!);
            if (user != null)
            {
                var roles = await _UserManager.GetRolesAsync(user);
                emp.RolesName = roles.ToList();

                emp.RolesId = roles
                    .Select(roleName => _RoleManager.Roles
                        .Where(r => r.Name == roleName)
                        .Select(r => r.Id)
                        .FirstOrDefault())
                    .Where(id => id != null)
                    .ToList()!;
            }
        }
        var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
        return pagedResult;
    }
    //--------------------------------------------------------------------------
    public async Task<Result<string>> SoftDeleteEmployeeAsync(EmployeeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
        if (DTo == null)
            return Result<string>.Failure("Invalid Emplyee Data");
        if (string.IsNullOrWhiteSpace(DTo.EmployeeCode))
            return Result<string>.Failure("Invalid Employee Code.");

        var repo = _UnitOfWork.GetRepository<Employee, string>();
        var employee = await repo.FindAsync(e => e.EmployeeCode == DTo.EmployeeCode);
        if (employee == null)
            return Result<string>.Failure("Employee not found.", HttpStatusCode.NotFound);
        employee.IsDeleted = true;
        employee.DeleteBy = userId;
        employee.DeleteAt = DateTime.UtcNow;
        await repo.UpdateAsync(employee);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("Employee Soft Deleted Successfully.");
    }
    //--------------------------------------------------------------------------
    public async Task<Result<string>> RestoreEmployeeAsync(EmployeeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);
        if (DTo == null)
            return Result<string>.Failure("Invalid Emplyee Data");
        if (string.IsNullOrWhiteSpace(DTo.EmployeeCode))
            return Result<string>.Failure("Invalid Employee Code.");
        var repo = _UnitOfWork.GetRepository<Employee, string>();
        var employee = await repo.FindAsync(e => e.EmployeeCode == DTo.EmployeeCode);
        if (employee == null)
            return Result<string>.Failure("Employee not found.", HttpStatusCode.NotFound);
        employee.IsDeleted = false;
        employee.DeleteBy = null;
        employee.DeleteAt = null;
        employee.UpdateBy = userId;
        employee.UpdateAt = DateTime.UtcNow;
        await repo.UpdateAsync(employee);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("Employee Restored Successfully.");
    }
    //--------------------------------------------------------------------------
    public async Task<Result<string>> UpdateEmployeeAsync(EmployeeDTo DTo)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<string>.Failure("Unauthorized User.", HttpStatusCode.Unauthorized);

        if (DTo == null)
            return Result<string>.Failure("Invalid Emplyee Data");
        await _UnitOfWork.BeginTransactionAsync();
        try
        {
            var repo = _UnitOfWork.GetRepository<Employee, string>();
            var employee = await repo.GetQueryable()
                                      .Include(e => e.User)
                                      .FirstOrDefaultAsync(e => e.EmployeeCode == DTo.EmployeeCode);
            if (employee == null)
                return Result<string>.Failure("Employee not found.", HttpStatusCode.NotFound);

            var user = employee.User;
            if (user == null)
                return Result<string>.Failure("User not found for this employee.");

            //if(await _UserManager.Users.AnyAsync(u => u.Email == DTo.Email))
            //    return Result<string>.Failure("An Employee With The Same Email Already Exists.",HttpStatusCode.Conflict);
            bool emailExists = await _UserManager.Users
                .AnyAsync(u => u.Email == DTo.Email && u.Id != user.Id);

            if (emailExists)
                return Result<string>.Failure("An Employee With The Same Email Already Exists.", HttpStatusCode.Conflict);
            // Update user email if it has changed
            if(user.Email != DTo.Email)
            {
                var setEmailResult = await _UserManager.SetEmailAsync(user,DTo.Email);
                if(!setEmailResult.Succeeded)
                {
                    var errors = string.Join("; ",setEmailResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"فشل في تحديث ايميل للموظف: {errors}",HttpStatusCode.BadRequest);
                }
                var setUserNameResult = await _UserManager.SetUserNameAsync(user,DTo.Email);
                if(!setUserNameResult.Succeeded)
                {
                    var errors = string.Join("; ",setUserNameResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"فشل في تحديث اسم المستخدم للموظف: {errors}",HttpStatusCode.BadRequest);
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
            if (string.IsNullOrEmpty(user.SecurityStamp))
            {
                await _UserManager.UpdateSecurityStampAsync(user);
            }
            var updateUserResult = await _UserManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                var errors = string.Join("; ", updateUserResult.Errors.Select(e => e.Description));
                return Result<string>.Failure($"Failed to update user. Errors: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(DTo.RoleName))
            {
                var currentRoles = await _UserManager.GetRolesAsync(user);

                // Remove old roles
                await _UserManager.RemoveFromRolesAsync(user, currentRoles);

                // Add new role
                var roleAddResult = await _UserManager.AddToRoleAsync(user, DTo.RoleName);

                if (!roleAddResult.Succeeded)
                {
                    var errors = string.Join("; ", roleAddResult.Errors.Select(e => e.Description));
                    return Result<string>.Failure($"Failed to update role. Errors: {errors}");
                }
            }

            employee.AccountName = DTo.AccountName;
            employee.AccountNumber = DTo.AccountNumber;
            employee.SNO = DTo.SNO;
            employee.OvertimeRatePerHour = DTo.OvertimeRatePerHour;
            employee.DeductionRatePerHour = DTo.DeductionRatePerHour;
            employee.DepartmentID = DTo.DepartmentID;
            employee.BirthDate = DTo.BirthDate;
            employee.HireDate = DTo.HireDate;
            employee.Salary = DTo.Salary;
            employee.TimeIn = DTo.TimeIn;
            employee.TimeOut = DTo.TimeOut;
            employee.WeekHoliday1 = DTo.WeekHoliday1;
            employee.WeekHoliday2 = DTo.WeekHoliday2;
            employee.UpdateBy = userId;
            employee.UpdateAt = DateTime.UtcNow;

            await repo.UpdateAsync(employee);
            await _UnitOfWork.SaveChangesAsync();
            await _UnitOfWork.CommitAsync();

            return Result<string>.Success("Employee Updated Successfully.");
        }
        catch (Exception ex)
        {
            await _UnitOfWork.RollbackAsync();
            return Result<string>.Failure($"Update failed: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    //--------------------------------------------------------------------------
    public async Task<Result<EmployeeSalaryDTo>> GetEmployeeSalaryByYearAndMonth(string EmpCode, int? Month, int? Year)
    {
        var userId = _CurrentUserService.UserId;
        if (userId is null)
            return Result<EmployeeSalaryDTo>.Failure("يجب تسجيل الدخول اولا", HttpStatusCode.Unauthorized);

        // تحديد الشهر والسنة
        int selectedYear = Year ?? DateTime.Now.Year;
        int selectedMonth = Month ?? DateTime.Now.Month;

        // الحصول على الموظف مع جميع البيانات ذات الصلة
        var employee = await _UnitOfWork.GetRepository<Employee, string>()
            .GetQueryable()
            .Include(e => e.User)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeCode == EmpCode);

        if (employee == null)
            return Result<EmployeeSalaryDTo>.Failure("هذا الموظف غير موجود", HttpStatusCode.NotFound);

        // تحويل WeekDays إلى DayOfWeek
        var weekHoliday1 = ConvertWeekDaysToDayOfWeek(employee.WeekHoliday1);

        var weekHoliday2 = employee.WeekHoliday2.HasValue ?
            ConvertWeekDaysToDayOfWeek(employee.WeekHoliday2.Value) : (DayOfWeek?)null;

        if (weekHoliday1 == null && weekHoliday2 == null)
            return Result<EmployeeSalaryDTo>.Failure("يرجى تحديد العطلات الأسبوعية للموظف في ملفه الشخصي", HttpStatusCode.BadRequest);


        // الحصول على العطلات الرسمية للشهر
        var publicHolidays = await _UnitOfWork.GetRepository<PublicHoliday, int>()
            .GetQueryable()
            .AsNoTracking()
            .Where(ph => ph.Date.Year == selectedYear && ph.Date.Month == selectedMonth && !ph.IsDeleted)
            .ToListAsync();

        // حساب أيام العمل الإجمالية
        int totalWorkingDays = CalculateWorkingDays(
            selectedYear, selectedMonth, weekHoliday1, weekHoliday2, publicHolidays);

        // الحصول على سجلات الحضور للشهر
        var attendanceRecords = await _UnitOfWork.GetRepository<EmployeeAttendance, int>()
            .GetQueryable().Include(a => a.Employee).ThenInclude(e => e.Department).Include(a => a.Employee).ThenInclude(e => e.User)
            .Where(a => a.EmployeeCode == EmpCode &&
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
        TimeSpan scheduledDuration = employee.TimeOut - employee.TimeIn;
        // التأكد من أن الوقت المجدول صالح
        double scheduledHours = scheduledDuration.TotalHours;

        // حساب تفاصيل الحضور
        foreach (var record in attendanceRecords)
        {
            var detail = new AttendanceDetailDto
            {
                Date = record.AttendanceDate!.Value,
                CheckIn = record.CheckInTime,
                CheckOut = record.CheckOutTime,
                Status = record.AttendanceStatus.ToString() ?? "Unknown"
            };

            // حساب ساعات العمل
            if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue)
            {
                // حساب الفرق بين وقت الدخول والخروج
                TimeSpan actualDuration = record.CheckOutTime.Value - record.CheckInTime.Value;
                
                detail.WorkHours = actualDuration.TotalHours;

                // حساب الوقت الإضافي والخصم
                if (detail.WorkHours > scheduledHours)
                {
                    detail.OvertimeHours = detail.WorkHours - scheduledHours;
                    totalOvertimeHours += detail.OvertimeHours;
                }
                else if (detail.WorkHours < scheduledHours)
                {
                    detail.DeductionHours = scheduledHours - detail.WorkHours;
                    totalDeductionHours += detail.DeductionHours;
                }
            }

            // حساب الحالات
            switch (record.AttendanceStatus)
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
        var leaveDetails = await CalculateLeavesForMonth(EmpCode, selectedMonth, selectedYear,
            weekHoliday1, weekHoliday2, publicHolidays);

        // حساب أيام الإجازة المدفوعة وغير المدفوعة
        int paidLeaveDays = leaveDetails.Where(l => l.IsPaid).Sum(l => l.Days);
        int unpaidLeaveDays = leaveDetails.Where(l => !l.IsPaid).Sum(l => l.Days);

        // حساب الغياب الفعلي (بعد استبعاد الإجازات)
        int actualAbsentDays = Math.Max(0, totalWorkingDays - presentDays - paidLeaveDays - unpaidLeaveDays);

        // --- حساب القروض ---
        var loanDetails = await CalculateLoanDeductionsForMonth(EmpCode, selectedMonth, selectedYear);
        decimal loanDeduction = loanDetails.Sum(l => l.InstallmentAmount);

        // --- حساب العقوبات والخصومات ---
        var sanctionDetails = await CalculateSanctionDeductionsForMonth(EmpCode, selectedMonth, selectedYear);
        decimal sanctionAmount = sanctionDetails.Sum(s => s.Amount);

        // --- حساب المرتب الأساسي ---
        decimal basicSalary = employee.Salary;
        decimal salaryPerDay = totalWorkingDays > 0 ? basicSalary / totalWorkingDays : 0;
        decimal salaryPerHour = scheduledHours > 0 ? salaryPerDay / (decimal)scheduledHours : 0;

        // --- حساب المستحقات ---
        decimal totalOvertimePay = (decimal)totalOvertimeHours * employee.OvertimeRatePerHour * salaryPerHour;
        decimal timeDeductionAmount = (decimal)totalDeductionHours * employee.DeductionRatePerHour * salaryPerHour;
        decimal absentDeduction = actualAbsentDays * salaryPerDay;
        decimal unpaidLeaveDeduction = unpaidLeaveDays * salaryPerDay;

        decimal lateDeduction = lateDays * (salaryPerHour * 2); // مثال: خصم ساعتين لكل تأخير
        decimal earlyLeaveDeduction = earlyLeaveDays * (salaryPerHour * 1); // مثال: خصم ساعة لكل خروج مبكر

        // --- حساب الإجماليات ---
        decimal totalAdditions = totalOvertimePay;
        decimal totalDeductions = timeDeductionAmount + absentDeduction + unpaidLeaveDeduction +
                                 lateDeduction + earlyLeaveDeduction + loanDeduction + sanctionAmount;

        decimal totalDeductionsWithLoans = timeDeductionAmount + absentDeduction + unpaidLeaveDeduction +
                                 lateDeduction + earlyLeaveDeduction + loanDeduction + sanctionAmount;

        decimal totalDeductionsWithoutLoans = timeDeductionAmount + absentDeduction + unpaidLeaveDeduction +
                                     lateDeduction + earlyLeaveDeduction + sanctionAmount;



        // --- حساب صافي المرتب ---
        decimal netSalary = basicSalary + totalAdditions - totalDeductions;
        netSalary = Math.Max(netSalary, 0); // التأكد من أن المرتب لا يقل عن الصفر

        // حساب صافي المرتب مع القروض (للأرشيف فقط)
        //decimal netSalaryWithLoans = basicSalary + totalAdditions - totalDeductionsWithLoans;
        //netSalaryWithLoans = Math.Max(netSalaryWithLoans,0);

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

        // إنشاء DTO النهائي
        var employeeSalaryDTo = new EmployeeSalaryDTo
        {
            EmployeeId = employee.UserId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.User!.FullName,
            DepartmentName = employee.Department?.Name,
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
            OvertimeRatePerHour = employee.OvertimeRatePerHour,
            TotalOvertimePay = totalOvertimePay,

            DeductionHours = totalDeductionHours,
            DeductionRatePerHour = employee.DeductionRatePerHour,
            TimeDeductionAmount = timeDeductionAmount,
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

        return Result<EmployeeSalaryDTo>.Success(employeeSalaryDTo, HttpStatusCode.OK);


    }
    //--------------------------------------------------------------------------
   
    public async Task<Result<MonthlySalarySummaryDto>> GetMonthlySalarySummaryAsync(string empCode, int? month, int? year)
    {
        try
        {
            // الحصول على بيانات الراتب الأساسية
            var salaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, month, year);

            if (!salaryResult.IsSuccess)
                return Result<MonthlySalarySummaryDto>.Failure(salaryResult.Message!);

            var salary = salaryResult.Data;

            // حساب نسب التحليل المالي (مع تجنب القسمة على صفر)
            decimal totalAdditions = salary.BasicSalary + salary.TotalAdditions;
            totalAdditions = totalAdditions > 0 ? totalAdditions : 1;
            decimal basicSalaryForCalc = salary.BasicSalary > 0 ? salary.BasicSalary : 1;

            // التحليل المالي
            var financialAnalysis = new FinancialAnalysisDto
            {
                BasicSalary = salary.BasicSalary,
                TotalAdditions = salary.TotalAdditions,
                TotalDeductions = salary.TotalDeductions,
                NetSalary = salary.NetSalary,
                BasicSalaryPercentage = (salary.BasicSalary / totalAdditions) * 100,
                OvertimePercentage = (salary.TotalOvertimePay / totalAdditions) * 100,
                DeductionPercentage = (salary.TotalDeductions / basicSalaryForCalc) * 100,
                NetSalaryToBasicRatio = (salary.NetSalary / basicSalaryForCalc) * 100,
                DeductionBreakdown = CalculateDeductionBreakdown(salary),
                AdditionBreakdown = CalculateAdditionBreakdown(salary)
            };

            // التحليل الزمني
            var timeAnalysis = new TimeAnalysisDto
            {
                TotalWorkingDays = salary.TotalWorkingDays,
                PresentDays = salary.PresentDays,
                AbsentDays = salary.AbsentDays,
                PaidLeaveDays = salary.PaidLeaveDays,
                UnpaidLeaveDays = salary.UnpaidLeaveDays,
                OvertimeHours = salary.OvertimeHours,
                DeductionHours = salary.DeductionHours,
                AttendanceRate = salary.TotalWorkingDays > 0 ?
                    (salary.PresentDays * 100.0m) / salary.TotalWorkingDays : 0,
                AbsenceRate = salary.TotalWorkingDays > 0 ?
                    (salary.AbsentDays * 100.0m) / salary.TotalWorkingDays : 0,
                LeaveRate = salary.TotalWorkingDays > 0 ?
                    ((salary.PaidLeaveDays + salary.UnpaidLeaveDays) * 100.0m) / salary.TotalWorkingDays : 0,
                OvertimeRate = salary.TotalWorkingDays > 0 ?
                    (decimal)salary.OvertimeHours / (salary.TotalWorkingDays * 8) : 0,
                DailyBreakdown = await GetDailyAttendanceBreakdownAsync(empCode, month ?? DateTime.Now.Month, year ?? DateTime.Now.Year)
            };

            // تحليل الأداء
            var performanceAnalysis = CalculatePerformanceAnalysis(salary);

            // المؤشرات الرئيسية
            var keyMetrics = CalculateKeyMetrics(salary);

            // التوصيات
            var recommendations = GenerateSalaryRecommendations(salary);

            var summary = new MonthlySalarySummaryDto
            {
                EmployeeCode = salary.EmployeeCode!,
                EmployeeName = salary.EmployeeName!,
                Month = salary.SelectedMonth,
                Year = salary.SelectedYear,
                FinancialAnalysis = financialAnalysis,
                TimeAnalysis = timeAnalysis,
                PerformanceAnalysis = performanceAnalysis,
                KeyMetrics = keyMetrics,
                Recommendations = recommendations
            };

            return Result<MonthlySalarySummaryDto>.Success(summary);
        }
        catch (Exception ex)
        {
            return Result<MonthlySalarySummaryDto>.Failure($"حدث خطأ في إنشاء التقرير: {ex.Message}");
        }
    }
    //--------------------------------------------------------------------------
    public async Task<Result<MonthlyStatisticsDto>> GetMonthlyStatisticsAsync(string empCode, int? month, int? year)
    {
        try
        {
            // الحصول على بيانات الراتب الأساسية
            var salaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, month, year);

            if (!salaryResult.IsSuccess)
                return Result<MonthlyStatisticsDto>.Failure(salaryResult.Message!);

            var salary = salaryResult.Data;

            // الحصول على بيانات الموظف
            var employee = await _UnitOfWork.GetRepository<Employee, string>()
                .GetQueryable()
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeCode == empCode);

            if (employee == null)
                return Result<MonthlyStatisticsDto>.Failure("الموظف غير موجود");


            // حساب إحصائيات الراتب
            var salaryStats = new SalaryStatisticsDto
            {
                AverageDailySalary = salary!.SalaryPerDay,
                AverageHourlySalary = salary.SalaryPerHour,
                OvertimeCostPerHour = salary.OvertimeRatePerHour * salary.SalaryPerHour,
                DeductionCostPerHour = salary.DeductionRatePerHour * salary.SalaryPerHour,
                SalaryGrowthRate = await CalculateSalaryGrowthRateAsync(empCode, month, year) // دالة مساعدة جديدة
            };

            // حساب إحصائيات الحضور
            var attendanceStats = new AttendanceStatisticsDto
            {
                TotalScheduledDays = salary.TotalWorkingDays,
                ActualWorkingDays = salary.PresentDays,
                LateCount = salary.LateDays,
                EarlyLeaveCount = salary.EarlyLeaveDays,
                PunctualityIndex = CalculatePunctualityIndex(salary),
                AttendanceGrade = CalculateAttendanceGrade(salary)
            };

            // حساب إحصائيات الإجازات
            var leaveStats = new LeaveStatisticsDto
            {
                TotalLeaveDays = salary.PaidLeaveDays + salary.UnpaidLeaveDays,
                PaidLeaveDays = salary.PaidLeaveDays,
                UnpaidLeaveDays = salary.UnpaidLeaveDays,
                LeaveUtilizationRate = salary.TotalWorkingDays > 0 ?
                ((salary.PaidLeaveDays + salary.UnpaidLeaveDays) * 100.0m) / salary.TotalWorkingDays : 0,
                LeaveRequestsCount = salary.LeaveDetails.Count,
                MostUsedLeaveType = GetMostUsedLeaveType(salary.LeaveDetails)
            };

            // حساب إحصائيات الوقت الإضافي
            var overtimeStats = new OvertimeStatisticsDto
            {
                TotalOvertimeHours = salary.OvertimeHours,
                AverageDailyOvertime = salary.PresentDays > 0 ? salary.OvertimeHours / salary.PresentDays : 0,
                OvertimeCost = salary.TotalOvertimePay,
                OvertimePattern = AnalyzeOvertimePattern(salary.AttendanceDetails),
                IsExcessiveOvertime = salary.OvertimeHours > 40 // أكثر من 40 ساعة يعتبر مفرط
            };

            var monthlyStats = new MonthlyStatisticsDto
            {
                EmployeeCode = empCode,
                EmployeeName = employee.User!.FullName ?? "غير معروف",
                Month = salary.SelectedMonth,
                Year = salary.SelectedYear,
                SalaryStats = salaryStats,
                AttendanceStats = attendanceStats,
                LeaveStats = leaveStats,
                OvertimeStats = overtimeStats
            };

            return Result<MonthlyStatisticsDto>.Success(monthlyStats);
        }
        catch (Exception ex)
        {
            return Result<MonthlyStatisticsDto>.Failure($"حدث خطأ في حساب الإحصائيات: {ex.Message}");
        }
    }
    //--------------------------------------------------------------------------
    public async Task<Result<SalaryComparisonDto>> CompareMonthlySalariesAsync(
    string empCode, int baseMonth, int baseYear, int compareMonth, int compareYear)
    {
        try
        {
            // جلب راتب الشهر الأساسي
            var baseSalaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, baseMonth, baseYear);
            if (!baseSalaryResult.IsSuccess)
                return Result<SalaryComparisonDto>.Failure(baseSalaryResult.Message!);

            // جلب راتب الشهر المقارن
            var compareSalaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, compareMonth, compareYear);
            if (!compareSalaryResult.IsSuccess)
                return Result<SalaryComparisonDto>.Failure(compareSalaryResult.Message!);

            var baseSalary = baseSalaryResult.Data;
            var compareSalary = compareSalaryResult.Data;

            // الحصول على بيانات الموظف
            var employee = await _UnitOfWork.GetRepository<Employee, string>()
                .GetQueryable()
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeCode == empCode);

            if (employee == null)
                return Result<SalaryComparisonDto>.Failure("الموظف غير موجود");

            // حساب التغيرات
            decimal salaryChange = compareSalary!.NetSalary - baseSalary!.NetSalary;
            decimal salaryChangePercentage = baseSalary.NetSalary != 0 ?
                (salaryChange / baseSalary.NetSalary) * 100 : 0;

            decimal attendanceChange = compareSalary.PresentDays - baseSalary.PresentDays;
            decimal attendanceChangePercentage = baseSalary.PresentDays != 0 ?
                (attendanceChange / baseSalary.PresentDays) * 100 : 0;

            decimal overtimeChange = (decimal)compareSalary.OvertimeHours - (decimal)baseSalary.OvertimeHours;
            decimal overtimeChangePercentage = baseSalary.OvertimeHours != 0 ?
                (overtimeChange / (decimal)baseSalary.OvertimeHours) * 100 : 0;

            // تحديد الاتجاه العام
            string overallTrend = salaryChange > 0 ? "تحسن" : salaryChange < 0 ? "تراجع" : "مستقر";

            // تحديد تغير الأداء
            string performanceChange = CalculatePerformanceChange(baseSalary, compareSalary);

            // إنشاء قائمة بمقاييس التغير
            var metricChanges = new List<MetricChangeDto>
                {
                    new MetricChangeDto
                    {
                        MetricName = "صافي الراتب",
                        BaseValue = baseSalary.NetSalary,
                        CompareValue = compareSalary.NetSalary,
                        ChangePercentage = salaryChangePercentage,
                        IsImprovement = salaryChange > 0,
                        Status = salaryChange > 0 ? "زيادة" : salaryChange < 0 ? "نقصان" : "ثابت"
                    },
                      new MetricChangeDto
                    {
                        MetricName = "أيام الحضور",
                        BaseValue = baseSalary.PresentDays,
                        CompareValue = compareSalary.PresentDays,
                        ChangePercentage = attendanceChangePercentage,
                        IsImprovement = attendanceChange > 0,
                        Status = attendanceChange > 0 ? "زيادة" : attendanceChange < 0 ? "نقصان" : "ثابت"
                    },
                     new MetricChangeDto
                    {
                        MetricName = "ساعات العمل الإضافي",
                        BaseValue = (decimal)baseSalary.OvertimeHours,
                        CompareValue = (decimal)compareSalary.OvertimeHours,
                        ChangePercentage = overtimeChangePercentage,
                        IsImprovement = overtimeChange > 0,
                        Status = overtimeChange > 0 ? "زيادة" : overtimeChange < 0 ? "نقصان" : "ثابت"
                    },
                    new MetricChangeDto
                    {
                        MetricName = "أيام الغياب",
                        BaseValue = baseSalary.AbsentDays,
                        CompareValue = compareSalary.AbsentDays,
                        ChangePercentage = baseSalary.AbsentDays != 0 ?
                            ((decimal)(compareSalary.AbsentDays - baseSalary.AbsentDays) / baseSalary.AbsentDays) * 100 : 0,
                        IsImprovement = compareSalary.AbsentDays < baseSalary.AbsentDays,
                        Status = compareSalary.AbsentDays < baseSalary.AbsentDays ? "تحسن" :
                                 compareSalary.AbsentDays > baseSalary.AbsentDays ? "تراجع" : "ثابت"
                    }
                };

            // إنشاء ملخص المقارنة
            string summary = GenerateComparisonSummary(baseSalary, compareSalary, salaryChangePercentage);


            var comparison = new SalaryComparisonDto
            {
                EmployeeCode = empCode,
                EmployeeName = employee.User?.FullName ?? "غير معروف",
                BasePeriod = new SalaryPeriodDto
                {
                    Month = baseMonth,
                    Year = baseYear,
                    BasicSalary = baseSalary.BasicSalary,
                    NetSalary = baseSalary.NetSalary,
                    TotalAdditions = baseSalary.TotalAdditions,
                    TotalDeductions = baseSalary.TotalDeductions,
                    PresentDays = baseSalary.PresentDays,
                    AbsentDays = baseSalary.AbsentDays,
                    PaidLeaveDays = baseSalary.PaidLeaveDays,
                    UnpaidLeaveDays = baseSalary.UnpaidLeaveDays,
                    OvertimeHours = baseSalary.OvertimeHours,
                    DeductionHours = baseSalary.DeductionHours
                },
                ComparePeriod = new SalaryPeriodDto
                {
                    Month = compareMonth,
                    Year = compareYear,
                    BasicSalary = compareSalary.BasicSalary,
                    NetSalary = compareSalary.NetSalary,
                    TotalAdditions = compareSalary.TotalAdditions,
                    TotalDeductions = compareSalary.TotalDeductions,
                    PresentDays = compareSalary.PresentDays,
                    AbsentDays = compareSalary.AbsentDays,
                    PaidLeaveDays = compareSalary.PaidLeaveDays,
                    UnpaidLeaveDays = compareSalary.UnpaidLeaveDays,
                    OvertimeHours = compareSalary.OvertimeHours,
                    DeductionHours = compareSalary.DeductionHours
                },
                Comparison = new ComparisonResultDto
                {
                    SalaryChangePercentage = salaryChangePercentage,
                    AttendanceChangePercentage = attendanceChangePercentage,
                    OvertimeChangePercentage = overtimeChangePercentage,
                    OverallTrend = overallTrend,
                    PerformanceChange = performanceChange,
                    Summary = summary
                },
                MetricChanges = metricChanges
            };

            return Result<SalaryComparisonDto>.Success(comparison);
        }
        catch (Exception ex)
        {
            return Result<SalaryComparisonDto>.Failure($"حدث خطأ في المقارنة: {ex.Message}");
        }
    }

    //--------------------------------------------------------------------------
    public async Task<Result<SalaryHistoryDto>> GetSalaryHistoryAsync(string empCode, int? year)
    {
        try
        {
            int targetYear = year ?? DateTime.Now.Year;
            var monthlyRecords = new List<MonthlySalaryRecordDto>();

            // الحصول على بيانات الموظف
            var employee = await _UnitOfWork.GetRepository<Employee, string>()
                .GetQueryable()
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.EmployeeCode == empCode);

            if (employee == null)
                return Result<SalaryHistoryDto>.Failure("الموظف غير موجود");

            // جلب بيانات الراتب لكل شهر من السنة
            for (int month = 1; month <= 12; month++)
            {
                var salaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, month, targetYear);
                if (salaryResult.IsSuccess)
                {
                    var salary = salaryResult.Data;
                    monthlyRecords.Add(new MonthlySalaryRecordDto
                    {
                        Month = month,
                        MonthName = GetMonthName(month),
                        BasicSalary = salary!.BasicSalary,
                        NetSalary = salary.NetSalary,
                        TotalAdditions = salary.TotalAdditions,
                        TotalDeductions = salary.TotalDeductions,
                        PresentDays = salary.PresentDays,
                        AbsentDays = salary.AbsentDays,
                        PaidLeaveDays = salary.PaidLeaveDays,
                        UnpaidLeaveDays = salary.UnpaidLeaveDays,
                        OvertimeHours = salary.OvertimeHours,
                        DeductionHours = salary.DeductionHours,
                        PerformanceLevel = CalculatePerformanceLevel(salary)
                    });
                }
            }

            if (!monthlyRecords.Any())
                return Result<SalaryHistoryDto>.Failure($"لا توجد بيانات رواتب للموظف {empCode} في سنة {targetYear}");

            // حساب الإجماليات السنوية
            decimal totalNetSalary = monthlyRecords.Sum(m => m.NetSalary);
            decimal averageMonthlySalary = monthlyRecords.Count > 0 ? totalNetSalary / monthlyRecords.Count : 0;
            decimal averageMonthlyAdditions = monthlyRecords.Average(m => m.TotalAdditions);
            decimal averageMonthlyDeductions = monthlyRecords.Average(m => m.TotalDeductions);
            int totalPresentDays = monthlyRecords.Sum(m => m.PresentDays);
            int totalAbsentDays = monthlyRecords.Sum(m => m.AbsentDays);
            int totalPaidLeaveDays = monthlyRecords.Sum(m => m.PaidLeaveDays);
            int totalUnpaidLeaveDays = monthlyRecords.Sum(m => m.UnpaidLeaveDays);
            double totalOvertimeHours = monthlyRecords.Sum(m => m.OvertimeHours);
            double totalDeductionHours = monthlyRecords.Sum(m => m.DeductionHours);

            // تحديد أفضل وأسوأ شهر من حيث الأداء
            var bestMonth = monthlyRecords.OrderByDescending(m => m.NetSalary).FirstOrDefault();
            var worstMonth = monthlyRecords.OrderBy(m => m.NetSalary).FirstOrDefault();


            var yearlySummary = new YearlySummaryDto
            {
                TotalNetSalary = totalNetSalary,
                AverageMonthlySalary = averageMonthlySalary,
                AverageMonthlyAdditions = averageMonthlyAdditions,
                AverageMonthlyDeductions = averageMonthlyDeductions,
                TotalPresentDays = totalPresentDays,
                TotalAbsentDays = totalAbsentDays,
                TotalPaidLeaveDays = totalPaidLeaveDays,
                TotalUnpaidLeaveDays = totalUnpaidLeaveDays,
                TotalOvertimeHours = totalOvertimeHours,
                TotalDeductionHours = totalDeductionHours,
                BestPerformanceMonth = bestMonth != null ? bestMonth.MonthName : "غير متاح",
                WorstPerformanceMonth = worstMonth != null ? worstMonth.MonthName : "غير متاح",
                BestNetSalary = bestMonth?.NetSalary ?? 0,
                WorstNetSalary = worstMonth?.NetSalary ?? 0
            };

            var history = new SalaryHistoryDto
            {
                EmployeeCode = empCode,
                EmployeeName = employee.User!.FullName ?? "غير معروف",
                Year = targetYear,
                MonthlyRecords = monthlyRecords,
                YearlySummary = yearlySummary
            };

            return Result<SalaryHistoryDto>.Success(history);
        }
        catch (Exception ex)
        {
            return Result<SalaryHistoryDto>.Failure($"حدث خطأ في جلب تاريخ الرواتب: {ex.Message}");
        }
    }

    public async Task<Result<string>> RecordSalaryLoanDeductionAsync(int payrollId,string employeeCode,decimal deductionAmount,DateTime payrollMonth)
    {
        try
        {
            var userId = _CurrentUserService.UserId;
            if(string.IsNullOrEmpty(userId))
                return Result<string>.Failure("غير مصرح بالدخول");

            // الحصول على القروض النشطة للموظف لهذا الشهر
            var activeLoans = await _UnitOfWork.GetRepository<EmployeeLoan,int>()
                .GetQueryable()
                .Where(l => l.EmployeeCode == employeeCode &&
                           l.Status == LoanStatus.Active &&
                           !l.IsDeleted &&
                           !l.IsPaidOff
                           //&&
                           //IsInstallmentDueThisMonth(l,payrollMonth)
                           )
                .OrderBy(l => l.FirstInstallmentDate)
                .ToListAsync();

            if(!activeLoans.Any())
                return Result<string>.Success("لا توجد أقساط قروض مستحقة لهذا الشهر");

            decimal remainingDeduction = deductionAmount;
            var deductions = new List<EmployeeLoanDeduction>();

            foreach(var loan in activeLoans)
            {
                if(remainingDeduction <= 0)
                    break;

                decimal installmentAmount = loan.InstallmentAmount;
                decimal amountToDeduct = Math.Min(installmentAmount,remainingDeduction);

                // تسجيل الخصم
                var loanDeduction = new EmployeeLoanDeduction
                {
                    PayrollId = payrollId,
                    LoanId = loan.Id,
                    EmployeeCode = employeeCode,
                    DeductionDate = DateTime.UtcNow,
                    DeductionAmount = amountToDeduct,
                    CreateBy = userId,
                    CreatedAt = DateTime.UtcNow
                };

                deductions.Add(loanDeduction);

                // تحديث القرض
                loan.PaidAmount += amountToDeduct;
                loan.RemainingAmount -= amountToDeduct;
                loan.UpdateBy = userId;
                loan.UpdateAt = DateTime.UtcNow;

                if(loan.RemainingAmount <= 0)
                {
                    loan.IsPaidOff = true;
                    loan.Status = LoanStatus.PaidOff;
                    loan.ActualEndDate = DateTime.UtcNow;
                }

                remainingDeduction -= amountToDeduct;
            }

            // حفظ جميع الخصومات
            foreach(var deduction in deductions)
            {
                await _UnitOfWork.GetRepository<EmployeeLoanDeduction,int>().AddAsync(deduction);
            }

            await _UnitOfWork.SaveChangesAsync();

            return Result<string>.Success($"تم تسجيل {deductions.Count} خصم قرض من الراتب");
        }
        catch(Exception ex)
        {       
            return Result<string>.Failure($"خطأ في تسجيل خصم القروض: {ex.Message}");
        }
    }


    //--------------------------------------------------------------------------


    // الدوال المساعدة
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

        if (salary.LateDeduction > 0)
        {
            breakdown.Add(new DeductionBreakdownDto
            {
                DeductionType = "خصم تأخير",
                Amount = salary.LateDeduction,
                Percentage = (salary.LateDeduction / totalDeductions) * 100
            });
        }

        if (salary.LoanDeduction > 0)
        {
            breakdown.Add(new DeductionBreakdownDto
            {
                DeductionType = "أقساط قروض",
                Amount = salary.LoanDeduction,
                Percentage = (salary.LoanDeduction / totalDeductions) * 100
            });
        }

        if (salary.SanctionAmount > 0)
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

    private async Task<List<DailyAttendanceDto>> GetDailyAttendanceBreakdownAsync(string empCode, int month, int year)
    {
        var dailyBreakdown = new List<DailyAttendanceDto>();

        var attendanceRecords = await _UnitOfWork.GetRepository<EmployeeAttendance, int>()
            .GetQueryable()
            .Where(a => a.EmployeeCode == empCode &&
                       a.AttendanceDate.HasValue &&
                       a.AttendanceDate.Value.Year == year &&
                       a.AttendanceDate.Value.Month == month)
            .ToListAsync();

        var employee = await _UnitOfWork.GetRepository<Employee, string>()
            .GetQueryable()
            .FirstOrDefaultAsync(e => e.EmployeeCode == empCode);

        if (employee == null) return dailyBreakdown;

        // تحويل WeekDays إلى DayOfWeek
        var weekHoliday1 =ConvertWeekDaysToDayOfWeek(employee.WeekHoliday1);
        var weekHoliday2 = employee.WeekHoliday2.HasValue ?
            ConvertWeekDaysToDayOfWeek(employee.WeekHoliday2.Value) : (DayOfWeek?)null;

        // الحصول على العطلات الرسمية
        var publicHolidays = await _UnitOfWork.GetRepository<PublicHoliday, int>()
            .GetQueryable()
            .Where(ph => ph.Date.Year == year && ph.Date.Month == month && !ph.IsDeleted)
            .ToListAsync();

        // عدد أيام الشهر
        int daysInMonth = DateTime.DaysInMonth(year, month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var currentDate = new DateOnly(year, month, day);
            var attendance = attendanceRecords.FirstOrDefault(a => a.AttendanceDate == currentDate);

            var isHoliday = IsHoliday(currentDate.ToDateTime(TimeOnly.MinValue),
                weekHoliday1, weekHoliday2, publicHolidays);

            double workHours = 0;
            if (attendance?.CheckInTime.HasValue == true && attendance.CheckOutTime.HasValue == true)
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
        performance.PunctualityScore = Math.Max(0, 100 - (salary.LateDays * 10));

        // درجة الحضور (نسبة الحضور * 100)
        performance.AttendanceScore = salary.TotalWorkingDays > 0 ?
            (salary.PresentDays * 100m) / salary.TotalWorkingDays : 0;

        // درجة الإضافي (كل 5 ساعات إضافية = 10 نقاط، بحد أقصى 30)
        performance.OvertimeScore = Math.Min(30, (decimal)salary.OvertimeHours / 5 * 10);

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
        if (performance.PunctualityScore >= 90)
            performance.Strengths.Add("التزام عالي بالمواعيد");

        if (performance.AttendanceScore >= 95)
            performance.Strengths.Add("حضور ممتاز");

        if (performance.OvertimeScore >= 20)
            performance.Strengths.Add("تفاني في العمل");

        // مجالات التحسين
        if (salary.AbsentDays > 2)
            performance.AreasForImprovement.Add("تحسين معدل الحضور");

        if (salary.LateDays > 3)
            performance.AreasForImprovement.Add("الالتزام بالمواعيد");

        if (salary.DeductionHours > 10)
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

        if (salary.AbsentDays > 2)
            recommendations.Add($"مستوى الغياب مرتفع ({salary.AbsentDays} يوم). ينصح بمتابعة الموظف.");

        if (salary.LateDays > 3)
            recommendations.Add($"عدد أيام التأخير كبير ({salary.LateDays} يوم). ينصح باتخاذ إجراء.");

        if (salary.UnpaidLeaveDays > 0)
            recommendations.Add($"يوجد {salary.UnpaidLeaveDays} يوم إجازة بدون أجر.");

        if (salary.LoanDeduction > 0)
            recommendations.Add($"يتم خصم {salary.LoanDeduction:C} كأقساط قرض هذا الشهر.");

        if (salary.SanctionAmount > 0)
            recommendations.Add($"يوجد {salary.SanctionAmount:C} كخصومات عقوبات.");

        if (salary.OvertimeHours > 40)
            recommendations.Add($"عدد ساعات العمل الإضافي مرتفع ({salary.OvertimeHours:F2} ساعة). ينصح بمراجعة عبء العمل.");

        if (salary.DeductionHours > 10)
            recommendations.Add($"عدد ساعات الخصم مرتفع ({salary.DeductionHours:F2} ساعة). ينصح بمتابعة الدوام.");

        // توصيات إيجابية
        if (salary.AbsentDays == 0)
            recommendations.Add("الأداء ممتاز في الحضور - لم يغب الموظف طوال الشهر.");

        if (salary.LateDays == 0)
            recommendations.Add("التزام ممتاز بالمواعيد - لم يتأخر الموظف طوال الشهر.");

        if (salary.OvertimeHours > 0 && salary.OvertimeHours <= 20)
            recommendations.Add("الموظف يبذل جهداً إضافياً معقولاً.");

        if (salary.NetSalary / salary.BasicSalary >= 0.95m)
            recommendations.Add("المرتب قريب من الراتب الأساسي - أداء جيد.");

        return recommendations;
    }



    private int CalculateWorkingDays(int year, int month, DayOfWeek? weekHoliday1,
    DayOfWeek? weekHoliday2, List<PublicHoliday> publicHolidays)
    {
        int totalDays = DateTime.DaysInMonth(year, month);
        int workingDays = 0;

        for (int day = 1; day <= totalDays; day++)
        {
            DateTime currentDate = new DateTime(year, month, day);
            DayOfWeek currentDayOfWeek = currentDate.DayOfWeek;

            // التحقق إذا كان يوم عطلة أسبوعية
            if (currentDayOfWeek == weekHoliday1 || currentDayOfWeek == weekHoliday2)
                continue;

            // التحقق إذا كان يوم عطلة رسمية
            DateOnly dateOnly = DateOnly.FromDateTime(currentDate);
            if (publicHolidays.Any(ph => ph.Date == dateOnly))
                continue;

            workingDays++;
        }

        return workingDays;
    }

    private async Task<List<LeaveDetailDto>> CalculateLeavesForMonth(string empCode, int month, int year,
        DayOfWeek? weekHoliday1, DayOfWeek? weekHoliday2, List<PublicHoliday> publicHolidays)
    {
        var leaveDetails = new List<LeaveDetailDto>();

        var leaveRequests = await _UnitOfWork.GetRepository<EmployeeLeaveRequest, int>()
            .GetQueryable()
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.EmployeeCode == empCode &&
                        lr.Status == LeaveRequestStatus.Approved &&
                        !lr.IsDeleted &&
                        lr.FromDate <= new DateTime(year, month, DateTime.DaysInMonth(year, month)) &&
                        lr.ToDate >= new DateTime(year, month, 1))
            .ToListAsync();

        foreach (var request in leaveRequests)
        {
            DateTime startDate = request.FromDate > new DateTime(year, month, 1)
                ? request.FromDate
                : new DateTime(year, month, 1);

            DateTime endDate = request.ToDate < new DateTime(year, month, DateTime.DaysInMonth(year, month))
                ? request.ToDate
                : new DateTime(year, month, DateTime.DaysInMonth(year, month));

            // حساب أيام العمل في فترة الإجازة
            int workingDaysInLeave = 0;
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                DayOfWeek dayOfWeek = date.DayOfWeek;
                DateOnly dateOnly = DateOnly.FromDateTime(date);

                // استبعاد العطلات الأسبوعية
                if (dayOfWeek == weekHoliday1 || dayOfWeek == weekHoliday2)
                    continue;

                // استبعاد العطلات الرسمية
                if (publicHolidays.Any(ph => ph.Date == dateOnly))
                    continue;

                workingDaysInLeave++;
            }

            if (workingDaysInLeave > 0)
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

    private async Task<List<LoanDetailDto>> CalculateLoanDeductionsForMonth(string empCode, int month, int year)
    {
        var loanDetails = new List<LoanDetailDto>();

        var targetMonth = new DateTime(year, month, 1);

        // الحصول على القروض النشطة
        var activeLoans = await _UnitOfWork.GetRepository<EmployeeLoan, int>()
            .GetQueryable()
            .Include(l => l.Employee)
            .ThenInclude(e => e!.User)
            .Where(l => l.EmployeeCode == empCode &&
                       l.Status == LoanStatus.Active &&
                       !l.IsDeleted &&
                       !l.IsPaidOff)
            .ToListAsync();

        foreach (var loan in activeLoans)
        {
            // حساب عدد الأقساط المدفوعة
            int paidInstallments = (int)Math.Floor(loan.PaidAmount / loan.InstallmentAmount);

            // تاريخ القسط التالي
            DateTime nextDueDate = loan.FirstInstallmentDate.AddMonths(paidInstallments);

            // التحقق إذا كان القسط مستحق هذا الشهر
            if (nextDueDate.Year == year && nextDueDate.Month == month)
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
    private async Task<List<SanctionDetailDto>> CalculateSanctionDeductionsForMonth(string empCode, int month, int year)
    {
        var sanctionDetails = new List<SanctionDetailDto>();

        // الحصول على خصومات العقوبات للشهر
        var sanctions = await _UnitOfWork.GetRepository<PayrollDeductions, int>()
            .GetQueryable()
            .Where(d => d.EmployeeCode == empCode &&
                       d.DeductionDate.Year == year &&
                       d.DeductionDate.Month == month &&
                       !d.IsDeleted)
            .ToListAsync();

        foreach (var sanction in sanctions)
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
        switch (weekDay)
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

    private async Task<decimal> CalculateSalaryGrowthRateAsync(string empCode, int? month, int? year)
    {
        try
        {
            int currentMonth = month ?? DateTime.Now.Month;
            int currentYear = year ?? DateTime.Now.Year;

            // الحصول على الشهر السابق
            DateTime previousMonthDate = new DateTime(currentYear, currentMonth, 1).AddMonths(-1);
            int previousMonth = previousMonthDate.Month;
            int previousYear = previousMonthDate.Year;

            // جلب راتب الشهر الحالي
            var currentSalaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, currentMonth, currentYear);
            if (!currentSalaryResult.IsSuccess) return 0;

            // جلب راتب الشهر السابق
            var previousSalaryResult = await GetEmployeeSalaryByYearAndMonth(empCode, previousMonth, previousYear);
            if (!previousSalaryResult.IsSuccess) return 0;

            decimal currentNetSalary = currentSalaryResult.Data!.NetSalary;
            decimal previousNetSalary = previousSalaryResult.Data!.NetSalary;

            if (previousNetSalary == 0) return 0;

            return ((currentNetSalary - previousNetSalary) / previousNetSalary) * 100;
        }
        catch
        {
            return 0;
        }
    }

    private decimal CalculatePunctualityIndex(EmployeeSalaryDTo salary)
    {
        // مؤشر الالتزام بالمواعيد: 100 - (عدد أيام التأخير * 5) - (عدد أيام الخروج المبكر * 3)
        decimal index = 100 - (salary.LateDays * 5) - (salary.EarlyLeaveDays * 3);
        return Math.Max(index, 0);
    }

    private string CalculateAttendanceGrade(EmployeeSalaryDTo salary)
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
        if (!leaveDetails.Any()) return "لا توجد إجازات";

        var mostUsed = leaveDetails
            .GroupBy(l => l.LeaveType)
            .Select(g => new { LeaveType = g.Key, Days = g.Sum(x => x.Days) })
            .OrderByDescending(x => x.Days)
            .FirstOrDefault();

        return mostUsed?.LeaveType ?? "غير معروف";
    }

    private string AnalyzeOvertimePattern(List<AttendanceDetailDto> attendanceDetails)
    {
        if (!attendanceDetails.Any()) return "لا يوجد عمل إضافي";

        var overtimeDays = attendanceDetails.Count(a => a.OvertimeHours > 0);
        var totalDays = attendanceDetails.Count(a => a.WorkHours > 0);

        if (totalDays == 0) return "لا يوجد عمل إضافي";

        decimal overtimeFrequency = (decimal)overtimeDays / totalDays * 100;

        if (overtimeFrequency >= 80) return "منتظم";
        if (overtimeFrequency >= 50) return "موسمي";
        return "عشوائي";
    }
    private string CalculatePerformanceChange(EmployeeSalaryDTo baseSalary, EmployeeSalaryDTo compareSalary)
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

        if (change > 10)
            return "تحسن كبير";
        else if (change > 5)
            return "تحسن";
        else if (change < -10)
            return "تراجع كبير";
        else if (change < -5)
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

    private string CalculatePerformanceLevel(EmployeeSalaryDTo salary)
    {
        decimal attendanceRate = salary.TotalWorkingDays > 0 ?
            (salary.PresentDays * 100m) / salary.TotalWorkingDays : 0;

        if (attendanceRate >= 95 && salary.LateDays == 0 && salary.EarlyLeaveDays == 0)
            return "ممتاز";
        else if (attendanceRate >= 90 && salary.LateDays <= 2 && salary.EarlyLeaveDays <= 2)
            return "جيد جداً";
        else if (attendanceRate >= 80)
            return "جيد";
        else if (attendanceRate >= 70)
            return "مقبول";
        else
            return "ضعيف";
    }

    private string GenerateComparisonSummary(EmployeeSalaryDTo baseSalary, EmployeeSalaryDTo compareSalary, decimal salaryChangePercentage)
    {
        var summary = new StringBuilder();

        summary.AppendLine($"مقارنة بين {baseSalary.SelectedMonth}/{baseSalary.SelectedYear} و {compareSalary.SelectedMonth}/{compareSalary.SelectedYear}");
        summary.AppendLine();

        if (salaryChangePercentage > 0)
        {
            summary.AppendLine($"✅ زيادة في صافي الراتب بنسبة {salaryChangePercentage:F2}%");
            summary.AppendLine($"   - الراتب الأساسي: {baseSalary.NetSalary:C} → {compareSalary.NetSalary:C}");
        }
        else if (salaryChangePercentage < 0)
        {
            summary.AppendLine($"⚠️ نقصان في صافي الراتب بنسبة {Math.Abs(salaryChangePercentage):F2}%");
            summary.AppendLine($"   - الراتب الأساسي: {baseSalary.NetSalary:C} → {compareSalary.NetSalary:C}");
        }
        else
        {
            summary.AppendLine("⚖️ لا يوجد تغير في صافي الراتب");
        }

        // مقارنة الحضور
        if (compareSalary.PresentDays > baseSalary.PresentDays)
        {
            summary.AppendLine($"✅ تحسن في الحضور: +{compareSalary.PresentDays - baseSalary.PresentDays} يوم");
        }
        else if (compareSalary.PresentDays < baseSalary.PresentDays)
        {
            summary.AppendLine($"⚠️ تراجع في الحضور: -{baseSalary.PresentDays - compareSalary.PresentDays} يوم");
        }

        // مقارنة الوقت الإضافي
        if (compareSalary.OvertimeHours > baseSalary.OvertimeHours)
        {
            summary.AppendLine($"📈 زيادة في ساعات العمل الإضافي: +{(compareSalary.OvertimeHours - baseSalary.OvertimeHours):F2} ساعة");
        }
        else if (compareSalary.OvertimeHours < baseSalary.OvertimeHours)
        {
            summary.AppendLine($"📉 نقصان في ساعات العمل الإضافي: -{(baseSalary.OvertimeHours - compareSalary.OvertimeHours):F2} ساعة");
        }

        return summary.ToString();
    }
    private bool IsHoliday(
    DateTime date,
    DayOfWeek? weekHoliday1,
    DayOfWeek? weekHoliday2,
    List<PublicHoliday> publicHolidays)
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
