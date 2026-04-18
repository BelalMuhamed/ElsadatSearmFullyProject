using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.RepresentativeAttendanceDtos;
using Application.Helper;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.RepresentativeAttendanceService;
using Domain.Common;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Infrastructure.Services.RepresentativeAttendanceServices;
internal class RepresentativeAttendanceService:IRepresentativeAttendanceService
{
    private readonly IUnitOfWork _UnitOfWork;
    private readonly ICurrentUserService _CurrentUserService;
    private readonly UserManager<ApplicationUser> _UserManager;

    public RepresentativeAttendanceService(IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        UserManager<ApplicationUser> userManager)
    {
        _UnitOfWork = unitOfWork;
        _CurrentUserService = currentUserService;
        _UserManager = userManager;
    } 

    public async Task<PagedList<RepresentativeAttendanceDto>> GetRepresentativeAttendanceWithFilter(PaginationParams paginationParams,RepresentativeAttendanceHelper Pramter)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return new PagedList<RepresentativeAttendanceDto>(new List<RepresentativeAttendanceDto>(),0,paginationParams.PageNumber,paginationParams.PageSize);

        var query = _UnitOfWork.GetRepository<RepresentativeAttendance,int>()
                      .GetQueryable();
        if(query == null)
            return new PagedList<RepresentativeAttendanceDto>(new List<RepresentativeAttendanceDto>(),0,paginationParams.PageNumber,paginationParams.PageSize);

        if(!string.IsNullOrEmpty(Pramter.RepresentativeCode))
        {
            query = query.Where(a => a.RepresentativeCode != null &&
                         a.RepresentativeCode.Contains(Pramter.RepresentativeCode));
        }
        if(!string.IsNullOrEmpty(Pramter.RepresentativeId))
        {
            query = query.Where(a => a.Representatives != null &&
                       a.Representatives.UserId != null &&
                       a.Representatives.UserId.Contains(Pramter.RepresentativeId));
        }
        if(Pramter.SelectedDate != null)
        {
            query = query.Where(a => a.AttendanceDate.HasValue &&
                         a.AttendanceDate.Value == Pramter.SelectedDate);
        }
        if(Pramter.Year != null && Pramter.Month != null)
        {
            query = query.Where(a => a.AttendanceDate.HasValue &&
                         a.AttendanceDate.Value.Year == Pramter.Year &&
                         a.AttendanceDate.Value.Month == Pramter.Month);
        }
        if(Pramter.StartDate.HasValue && Pramter.EndDate.HasValue)
        {
            query = query.Where(a => a.AttendanceDate >= DateOnly.FromDateTime(Pramter.StartDate.Value) &&
                         a.AttendanceDate <= DateOnly.FromDateTime(Pramter.EndDate.Value));
        }
        if(Pramter.StartDate != null && Pramter.EndDate == null)
        {
            var endDate = Pramter.EndDate ?? DateTime.Now;
            query = query.Where(a => a.AttendanceDate.HasValue &&
                                     a.AttendanceDate.Value >= DateOnly.FromDateTime(Pramter.StartDate.Value) &&
                                     a.AttendanceDate.Value <= DateOnly.FromDateTime(endDate));
        }
        if(Pramter.EndDate != null && Pramter.StartDate == null)
        {
            var startDate = Pramter.StartDate ?? DateTime.MinValue;
            query = query.Where(a => a.AttendanceDate.HasValue &&
                                     a.AttendanceDate.Value >= DateOnly.FromDateTime(startDate) &&
                                     a.AttendanceDate.Value <= DateOnly.FromDateTime(Pramter.EndDate.Value));
        }
        if(!string.IsNullOrEmpty(Pramter.RepresentativeName))
        {
            query = query.Where(a => a.Representatives != null &&
                (a.Representatives.User != null && a.Representatives.User.FullName != null 
                && a.Representatives.User.FullName.Contains(Pramter.RepresentativeName)));
        }
        query = query
                .Include(a => a.Representatives!.User).OrderDescending();

        var result = query.Select(a => new RepresentativeAttendanceDto
        {
            Id = a.Id,
            RepresentativeCode = a.RepresentativeCode ?? string.Empty,
            RepresentativeName = a.Representatives != null && a.Representatives.User != null
                        ? a.Representatives.User.FullName ?? "Unknown"
                        : "Unknown",
            RepresentativeId = a.Representatives != null && a.Representatives.User != null
                        ? a.Representatives.User.Id.ToString()   
                        : string.Empty,
            AttendanceDate = a.AttendanceDate!.Value,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            CheckInMethod = a.CheckInMethod,
            AttendanceStatus = a.AttendanceStatus,
            CheckInLatitude = a.CheckInLatitude,
            CheckInLongitude = a.CheckInLongitude,
            CheckInLocation = a.CheckInLocation
        });
        var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);
        return pagedResult;
    }
    //-------------------------------------------------------------------------------------------
    public async Task<Result<string>> UpdateRepresentativeAttendanceStatus(RepresentativeAttendanceDto representativeAttendanceDto,AttendanceStatus status)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);

        var record = _UnitOfWork.GetRepository<RepresentativeAttendance,int>().GetQueryable()
               .FirstOrDefault(a => a.Id == representativeAttendanceDto.Id);
        if(record is null)
            return Result<string>.Failure("هذا التسجيل غير موجود في قاعدة البيانات",HttpStatusCode.NotFound);

        record.AttendanceStatus = status;
        await _UnitOfWork.GetRepository<RepresentativeAttendance,int>().UpdateAsync(record);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("تم تحديث حاله التسجيل بنجاح",HttpStatusCode.OK);

    }
    public async Task<Result<string>> RepresentativeCheckIn(RepresentativeAttendanceHelper Pramter)
    {
        var userId = _CurrentUserService.UserId;
        if(userId is null)
            return Result<string>.Failure("الرجاء تسجيل الدخول اولا",HttpStatusCode.Unauthorized);

        if(!string.IsNullOrEmpty(Pramter.RepresentativeEmail))
        {
            var userExists = await _UserManager.Users.SingleOrDefaultAsync(a => a.Email == Pramter.RepresentativeEmail);
            if(userExists != null)
            {
                var emp = await _UnitOfWork.GetRepository<Representatives,string>().GetQueryable()
                    .FirstOrDefaultAsync(e => e.UserId == userExists.Id);
                if(emp != null)
                    Pramter.RepresentativeCode = emp.RepresentativesCode;
            }
        }
        if(string.IsNullOrEmpty(Pramter.RepresentativeCode) || Pramter.Date is null || Pramter.InputTime is null)
            return Result<string>.Failure("يرجا مراجعه البيانات المدخله ",HttpStatusCode.NoContent);

        var Representative = _UnitOfWork.GetRepository<Representatives,string>().GetQueryable()
               .FirstOrDefault(e => e.RepresentativesCode == Pramter.RepresentativeCode);
        if(Representative is null)
            return Result<string>.Failure("المندوب غير موجود في قاعدة البيانات",HttpStatusCode.NotFound);

        var existingAttendance = _UnitOfWork.GetRepository<RepresentativeAttendance,int>().GetQueryable()
                .FirstOrDefaultAsync(a => a.RepresentativeCode == Pramter.RepresentativeCode &&
                a.AttendanceDate == Pramter.Date);
        if(existingAttendance.Result != null)
            return Result<string>.Failure("يوجد اثبات حضور لنفس هذا اليوم مسبقا",HttpStatusCode.Conflict);
        AttendanceStatus attendanceStatus;
        if(Pramter.InputTime <= Representative.TimeIn)
        {
            attendanceStatus = AttendanceStatus.Present;
        }
        else if(Pramter.InputTime > Representative.TimeIn &&Pramter.InputTime <= Representative.TimeOut)
        {
            attendanceStatus = AttendanceStatus.Late;
        }
        else
        {
            attendanceStatus = AttendanceStatus.Absent;
        }
        var newAttendance = new RepresentativeAttendance
        {
            RepresentativeCode = Pramter.RepresentativeCode,
            AttendanceDate = Pramter.Date,
            CheckInTime = Pramter.InputTime,
            CheckOutTime = Representative.TimeOut,
            CheckInMethod = CheckInMethod.MobileApp,
            AttendanceStatus = attendanceStatus,
            CheckInLatitude = Pramter.CheckInLatitude,
            CheckInLongitude = Pramter.CheckInLongitude,
            CheckInLocation = Pramter.CheckInLocation

        };
        await _UnitOfWork.GetRepository<RepresentativeAttendance,int>().AddAsync(newAttendance);
        await _UnitOfWork.SaveChangesAsync();
        return Result<string>.Success("تم اثبات الحضور بنجاح",HttpStatusCode.OK);
    }
}
