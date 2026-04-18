using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.EmployeeAttendance;
using Application.Helper;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.EmployeeAttendanceService;
using Domain.Common;
using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.UnitOfWork.Contract;
using ExcelDataReader;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net;

namespace Infrastructure.Services.EmployeeAttendanceServices
{
    internal class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ICurrentUserService _CurrentUserService;
        private readonly UserManager<ApplicationUser> _UserManager;

        public EmployeeAttendanceService(IUnitOfWork unitOfWork ,
            ICurrentUserService currentUserService ,
            UserManager<ApplicationUser> userManager)
        {
            _UnitOfWork = unitOfWork;
            _CurrentUserService = currentUserService;
            _UserManager = userManager;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<EmployeeAttendanceDTO>> GetAllAttendance(PaginationParams paginationParams)
        {
            var userId = _CurrentUserService.UserId;
            if(userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(),0,paginationParams.PageNumber,paginationParams.PageSize);
            var query = _UnitOfWork.GetRepository<EmployeeAttendance,int>()
                       .GetQueryable();
            if(query == null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(),0,paginationParams.PageNumber,paginationParams.PageSize);
            query = query.Include(a => a.Employee)
             .ThenInclude(e => e.Department)
             .Include(a => a.Employee)
             .ThenInclude(e => e.User).OrderBy(x=>x.AttendanceDate);

            var result = query.Select(a => new EmployeeAttendanceDTO(
                a.Id,
                a.EmployeeCode,
                a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
                a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
                a.Employee.Department != null ? a.Employee.Department.Id : 0,
                a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
                a.AttendanceDate!.Value,
                a.CheckInTime,
                a.CheckOutTime,
                a.CheckInMethod,
                a.AttendanceStatus
            ));

            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceByEmployeeCode(PaginationParams paginationParams, EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            if (string.IsNullOrEmpty(Pramter.EmployeeCode))
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);

            var query = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable().Include(a => a.Employee)
               .ThenInclude(e => e.Department)
               .Include(a => a.Employee)
               .ThenInclude(e => e.User).Where(x=>x.EmployeeCode == Pramter.EmployeeCode).OrderBy(x => x.AttendanceDate);
            var result = query.Select(a => new EmployeeAttendanceDTO(
               a.Id,
               a.EmployeeCode,
               a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
               a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
               a.Employee.Department != null ? a.Employee.Department.Id : 0,
               a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
               a.AttendanceDate!.Value,
               a.CheckInTime,
               a.CheckOutTime,
               a.CheckInMethod,
               a.AttendanceStatus
           ));

            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceByEmployeeId(PaginationParams paginationParams, EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            if (string.IsNullOrEmpty(Pramter.EmployeeId))
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);

            var query = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable().Include(a => a.Employee)
               .ThenInclude(e => e.Department)
               .Include(a => a.Employee)
               .ThenInclude(e => e.User).Where(a => a.Employee.UserId == Pramter.EmployeeId).OrderBy(x => x.AttendanceDate);
            var result = query.Select(a => new EmployeeAttendanceDTO(
               a.Id,
               a.EmployeeCode,
               a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
               a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
               a.Employee.Department != null ? a.Employee.Department.Id : 0,
               a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
               a.AttendanceDate!.Value,
               a.CheckInTime,
               a.CheckOutTime,
               a.CheckInMethod,
               a.AttendanceStatus
           ));

            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public Result<EmployeeAttendanceDTO> GetAttendanceForEmployeeByDate(EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<EmployeeAttendanceDTO>.Failure("Unauthorized", HttpStatusCode.Unauthorized);

            if(Pramter.SelectedDate is null)
                return Result<EmployeeAttendanceDTO>.Failure("Bad Data Entery ", HttpStatusCode.NoContent);
            var query = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable().Include(a => a.Employee)
               .ThenInclude(e => e.Department)
               .Include(a => a.Employee)
               .ThenInclude(e => e.User).Where(a => a.EmployeeCode == Pramter.EmployeeCode &&
                        a.AttendanceDate == Pramter.SelectedDate);
            var result = query.Select(a => new EmployeeAttendanceDTO(
               a.Id,
               a.EmployeeCode,
               a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
               a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
               a.Employee.Department != null ? a.Employee.Department.Id : 0,
               a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
               a.AttendanceDate!.Value,
               a.CheckInTime,
               a.CheckOutTime,
               a.CheckInMethod,
               a.AttendanceStatus
           ));
            return Result<EmployeeAttendanceDTO>.Success(result.FirstOrDefault()!, HttpStatusCode.OK);

        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceForEmployeeByYearAndMonth(PaginationParams paginationParams, EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            if (Pramter.Year==null || Pramter.Month==null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);

            var query = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable().Include(a => a.Employee)
               .ThenInclude(e => e.Department)
               .Include(a => a.Employee)
               .ThenInclude(e => e.User).Where(a =>
                a.EmployeeCode == Pramter.EmployeeCode &&
                a.AttendanceDate.HasValue &&
                a.AttendanceDate.Value.Year == Pramter.Year &&
                a.AttendanceDate.Value.Month == Pramter.Month).OrderBy(x => x.AttendanceDate);
            var result = query.Select(a => new EmployeeAttendanceDTO(
              a.Id,
              a.EmployeeCode,
              a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
              a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
              a.Employee.Department != null ? a.Employee.Department.Id : 0,
              a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
              a.AttendanceDate!.Value,
              a.CheckInTime,
              a.CheckOutTime,
              a.CheckInMethod,
              a.AttendanceStatus
          ));
            var pagedResult =await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> UpdateAttendanceStatus(EmployeeAttendanceDTO employeeAttendanceDTO, AttendanceStatus status)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);
            //var employeeAttendance = _UnitOfWork.GetRepository<EmployeeAttendance,int>()
            //    .GetByIdAsync(employeeAttendanceDTO.Id);

            //if(employeeAttendance==null)
            //    return Result<string>.Failure("Bad Data Entery ", HttpStatusCode.NoContent);

            var record = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable()
                .FirstOrDefault(a => a.Id == employeeAttendanceDTO.Id);
            if (record is null)
                return Result<string>.Failure("Record Not Found", HttpStatusCode.NotFound);
            record.AttendanceStatus = status;
            await _UnitOfWork.GetRepository<EmployeeAttendance, int>().UpdateAsync(record);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Attendance Status Updated Successfully", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> CheckIn(EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);
            if(!string.IsNullOrEmpty(Pramter.employeeEmail))
            {
                var userExists = await _UserManager.Users.SingleOrDefaultAsync(a => a.Email == Pramter.employeeEmail);
                if(userExists!=null)
                {
                    var emp = await _UnitOfWork.GetRepository<Employee,string>().GetQueryable()
                        .FirstOrDefaultAsync(e => e.UserId == userExists.Id);
                    if(emp != null)
                        Pramter.EmployeeCode = emp.EmployeeCode;
                }
            }
            if (string.IsNullOrEmpty(Pramter.EmployeeCode) || Pramter.Date is null || Pramter.InputTime is null)
                return Result<string>.Failure("Bad Data Entery ", HttpStatusCode.NoContent);

            var Emp = _UnitOfWork.GetRepository<Employee, string>().GetQueryable()
                .FirstOrDefault(e => e.EmployeeCode == Pramter.EmployeeCode);
            if (Emp is null)
                return Result<string>.Failure("Employee Not Found", HttpStatusCode.NotFound);
            var existingAttendance = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable()
                .FirstOrDefaultAsync(a => a.EmployeeCode == Pramter.EmployeeCode &&
                a.AttendanceDate == Pramter.Date);
            if (existingAttendance.Result != null)
                return Result<string>.Failure("Attendance Already Checked In for Today", HttpStatusCode.Conflict);
            AttendanceStatus attendanceStatus;
            if(Pramter.InputTime <= Emp.TimeIn)
            {
                attendanceStatus = AttendanceStatus.Present;
            }
            else if(Pramter.InputTime > Emp.TimeIn && Pramter.InputTime <= Emp.TimeOut)
            {
                attendanceStatus = AttendanceStatus.Late;
            }
            else
            {
                attendanceStatus = AttendanceStatus.Absent;
            }          
            var newAttendance = new EmployeeAttendance
            {
                EmployeeCode = Pramter.EmployeeCode,
                AttendanceDate = Pramter.Date,
                CheckInTime = Pramter.InputTime,
                CheckOutTime = null,
                CheckInMethod = CheckInMethod.Manual,
                AttendanceStatus = attendanceStatus
            };
            await _UnitOfWork.GetRepository<EmployeeAttendance, int>().AddAsync(newAttendance);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Check-In Recorded Successful", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------------
        public async Task<Result<string>> CheckOut(EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return Result<string>.Failure("Unauthorized", HttpStatusCode.Unauthorized);
            if(!string.IsNullOrEmpty(Pramter.employeeEmail))
            {
                var userExists = await _UserManager.Users.SingleOrDefaultAsync(a => a.Email == Pramter.employeeEmail);
                if(userExists != null)
                {
                    var emp = await _UnitOfWork.GetRepository<Employee,string>().GetQueryable()
                        .FirstOrDefaultAsync(e => e.UserId == userExists.Id);
                    if(emp != null)
                        Pramter.EmployeeCode = emp.EmployeeCode;
                }
            }

            if (string.IsNullOrEmpty(Pramter.EmployeeCode) || Pramter.Date is null || Pramter.InputTime is null)
                return Result<string>.Failure("Bad Data Entery ", HttpStatusCode.NoContent);

            var Emp = _UnitOfWork.GetRepository<Employee, string>().GetQueryable()
                .FirstOrDefault(e => e.EmployeeCode == Pramter.EmployeeCode);
            if (Emp is null)
                return Result<string>.Failure("Employee Not Found", HttpStatusCode.NotFound);

            var existingAttendance = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable()
                .FirstOrDefaultAsync(a => a.EmployeeCode == Pramter.EmployeeCode &&
                a.AttendanceDate == Pramter.Date);
            if (existingAttendance.Result == null)
                return Result<string>.Failure("No Check-In Record Found for Today", HttpStatusCode.NotFound);
            existingAttendance.Result.CheckOutTime = Pramter.InputTime;
            await _UnitOfWork.GetRepository<EmployeeAttendance, int>().UpdateAsync(existingAttendance.Result);
            await _UnitOfWork.SaveChangesAsync();
            return Result<string>.Success("Check-Out Recorded Successful", HttpStatusCode.OK);
        }
        //------------------------------------------------------------------------------
        public async Task<Result<ExcelImportResultDTo>> ImportFromExcelAsync(Stream fileStream)
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);

            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            };

            var ds = reader.AsDataSet(conf);
            var table = ds.Tables[0];

            var result = new ExcelImportResultDTo();

            var attendanceRepo = _UnitOfWork.GetRepository<EmployeeAttendance,int>();
            var empRepo = _UnitOfWork.GetRepository<Employee,int>();

            int rowIndex = 1;

            foreach(DataRow row in table.Rows)
            {
                rowIndex++;

                try
                {
                    var dateTimeStr = row["Date/Time"]?.ToString();
                    var empCode = row["No."]?.ToString();
                    var status = row["Status"]?.ToString();

                    if(string.IsNullOrWhiteSpace(empCode))
                    {
                        result.ErrorRecords.Add(new ExcelErrorRecord
                        {
                            RowNumber = rowIndex,
                            EmployeeCode = null,
                            ErrorMessage = "Employee code is empty."
                        });
                        continue;
                    }

                    if(!DateTime.TryParse(dateTimeStr,out var parsedDateTime))
                    {
                        result.ErrorRecords.Add(new ExcelErrorRecord
                        {
                            RowNumber = rowIndex,
                            EmployeeCode = empCode,
                            ErrorMessage = "Invalid Date/Time format."
                        });
                        continue;
                    }

                    var date = DateOnly.FromDateTime(parsedDateTime);
                    var time = TimeOnly.FromDateTime(parsedDateTime);

                    // CHECK EMPLOYEE
                    var emp = empRepo.GetQueryable().FirstOrDefault(e => e.EmployeeCode == empCode);
                    if(emp == null)
                    {
                        result.ErrorRecords.Add(new ExcelErrorRecord
                        {
                            RowNumber = rowIndex,
                            EmployeeCode = empCode,
                            ErrorMessage = $"Employee {empCode} not found in database."
                        });
                        continue;
                    }

                    // CHECK IF ATTENDANCE EXISTS
                    var attendance = await attendanceRepo.GetQueryable()
                        .FirstOrDefaultAsync(a => a.EmployeeCode == empCode &&
                                                  a.AttendanceDate == date);

                    bool isCheckIn = status.Contains("C/In",StringComparison.OrdinalIgnoreCase);
                    bool isCheckOut = status.Contains("C/Out",StringComparison.OrdinalIgnoreCase);

                    // -------- CHECK IN --------
                    if(isCheckIn)
                    {
                        if(attendance != null && attendance.CheckInTime != null)
                        {
                            result.ErrorRecords.Add(new ExcelErrorRecord
                            {
                                RowNumber = rowIndex,
                                EmployeeCode = empCode,
                                ErrorMessage = "Check-In already exists for this date."
                            });
                            continue;
                        }

                        if(attendance == null)
                        {
                            attendance = new EmployeeAttendance
                            {
                                EmployeeCode = empCode,
                                AttendanceDate = date,
                                CheckInTime = time,
                                CheckInMethod = CheckInMethod.FingerPrint,
                                AttendanceStatus = time <= emp.TimeIn ? AttendanceStatus.Present : AttendanceStatus.Late
                            };
                            await attendanceRepo.AddAsync(attendance);
                        }
                        else
                        {
                            attendance.CheckInTime = time;
                            attendance.AttendanceStatus =
                                time <= emp.TimeIn ? AttendanceStatus.Present : AttendanceStatus.Late;
                        }

                        result.SuccessMessages.Add($"Row {rowIndex}: Check-In registered.");
                        continue;
                    }

                    // -------- CHECK OUT --------
                    if(isCheckOut)
                    {
                        if(attendance == null)
                        {
                            result.ErrorRecords.Add(new ExcelErrorRecord
                            {
                                RowNumber = rowIndex,
                                EmployeeCode = empCode,
                                ErrorMessage = "Check-Out cannot be registered before Check-In."
                            });
                            continue;
                        }

                        if(attendance.CheckOutTime != null)
                        {
                            result.ErrorRecords.Add(new ExcelErrorRecord
                            {
                                RowNumber = rowIndex,
                                EmployeeCode = empCode,
                                ErrorMessage = "Check-Out already exists."
                            });
                            continue;
                        }

                        attendance.CheckOutTime = time;

                        result.SuccessMessages.Add($"Row {rowIndex}: Check-Out registered.");
                    }
                }
                catch(Exception ex)
                {
                    result.ErrorRecords.Add(new ExcelErrorRecord
                    {
                        RowNumber = rowIndex,
                        ErrorMessage = $"Unexpected error: {ex.Message}"
                    });
                }
            }

            await _UnitOfWork.SaveChangesAsync();

            return Result<ExcelImportResultDTo>.Success(result);
        }


        //------------------------------------------------------------------------------

        public async Task<PagedList<EmployeeAttendanceDTO>> GetTodayRecord(PaginationParams paginationParams)
        {
            var Day = DateOnly.FromDateTime(DateTime.Now);
            var userId = _CurrentUserService.UserId;
            if(userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(),0,paginationParams.PageNumber,paginationParams.PageSize);
            var query = _UnitOfWork.GetRepository<EmployeeAttendance,int>()
                       .GetQueryable();
            if(query == null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(),0,paginationParams.PageNumber,paginationParams.PageSize);
            query = query.Include(a => a.Employee)
             .ThenInclude(e => e.Department)
             .Include(a => a.Employee)
             .ThenInclude(e => e.User).Where(a => a.AttendanceDate == Day).OrderBy(x => x.AttendanceDate);

            var result = query.Select(a => new EmployeeAttendanceDTO(
               a.Id,
               a.EmployeeCode,
               a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
               a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
               a.Employee.Department != null ? a.Employee.Department.Id : 0,
               a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
               a.AttendanceDate!.Value,
               a.CheckInTime,
               a.CheckOutTime,
               a.CheckInMethod,
               a.AttendanceStatus
           ));
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
            return pagedResult;
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceByDateRange(PaginationParams paginationParams, EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if (userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            if(Pramter.StartDate == null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(), 0, paginationParams.PageNumber, paginationParams.PageSize);
            if (Pramter.EndDate.HasValue)
            {
                var query = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable().Include(a => a.Employee)
                   .ThenInclude(e => e.Department)
                   .Include(a => a.Employee)
                   .ThenInclude(e => e.User).Where(a =>
                    a.AttendanceDate.HasValue &&
                    a.AttendanceDate.Value >= DateOnly.FromDateTime(Pramter.StartDate.Value) &&
                    a.AttendanceDate.Value <= DateOnly.FromDateTime(Pramter.EndDate.Value)).OrderBy(x => x.AttendanceDate);
                var result = query.Select(a => new EmployeeAttendanceDTO(
                   a.Id,
                   a.EmployeeCode,
                   a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
                   a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
                   a.Employee.Department != null ? a.Employee.Department.Id : 0,
                   a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
                   a.AttendanceDate!.Value,
                   a.CheckInTime,
                   a.CheckOutTime,
                   a.CheckInMethod,
                   a.AttendanceStatus
               ));
                var pagedResult =await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
                return pagedResult;

            }
            else
            {
                Pramter.EndDate = DateTime.Now;
                var query = _UnitOfWork.GetRepository<EmployeeAttendance, int>().GetQueryable().Include(a => a.Employee)
                   .ThenInclude(e => e.Department)
                   .Include(a => a.Employee)
                   .ThenInclude(e => e.User).Where(a =>
                    a.AttendanceDate.HasValue &&
                    a.AttendanceDate.Value >= DateOnly.FromDateTime(Pramter.StartDate.Value) &&
                    a.AttendanceDate.Value <= DateOnly.FromDateTime(Pramter.EndDate.Value));
                var result = query.Select(a => new EmployeeAttendanceDTO(
                   a.Id,
                   a.EmployeeCode,
                   a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
                   a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
                   a.Employee.Department != null ? a.Employee.Department.Id : 0,
                   a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
                   a.AttendanceDate!.Value,
                   a.CheckInTime,
                   a.CheckOutTime,
                   a.CheckInMethod,
                   a.AttendanceStatus
               ));
                var pagedResult =await result.ToPagedListAsync(paginationParams.PageNumber, paginationParams.PageSize);
                return pagedResult;
            }
        }
        //------------------------------------------------------------------------------
        public async Task<PagedList<EmployeeAttendanceDTO>> GetAttendanceWithFilter(PaginationParams paginationParams,EmpAttendanceHelper Pramter)
        {
            var userId = _CurrentUserService.UserId;
            if(userId is null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(),0,paginationParams.PageNumber,paginationParams.PageSize);
            var query = _UnitOfWork.GetRepository<EmployeeAttendance,int>()
                       .GetQueryable();
            if(query == null)
                return new PagedList<EmployeeAttendanceDTO>(new List<EmployeeAttendanceDTO>(),0,paginationParams.PageNumber,paginationParams.PageSize);

            if(!string.IsNullOrEmpty(Pramter.EmployeeCode))
            {
                query = query.Where(a => a.EmployeeCode != null &&
                             a.EmployeeCode.Contains(Pramter.EmployeeCode));
            }

            if(!string.IsNullOrEmpty(Pramter.EmployeeId))
            {
                query = query.Where(a => a.Employee != null &&
                           a.Employee.UserId != null &&
                           a.Employee.UserId.Contains(Pramter.EmployeeId));
            }

            if(Pramter.SelectedDate != null )
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

            if(!string.IsNullOrEmpty(Pramter.DepartmentName))
            {
                query = query.Where(a => a.Employee != null &&
                         a.Employee.Department != null &&
                         a.Employee.Department.Name != null &&
                         a.Employee.Department.Name.Contains(Pramter.DepartmentName));
            }

            if(Pramter.StartDate != null)
            {
                var endDate = Pramter.EndDate ?? DateTime.Now;
                query = query.Where(a => a.AttendanceDate.HasValue &&
                                         a.AttendanceDate.Value >= DateOnly.FromDateTime(Pramter.StartDate.Value) &&
                                         a.AttendanceDate.Value <= DateOnly.FromDateTime(endDate));
            }

            if(!string.IsNullOrEmpty(Pramter.EmployeeName))
            {
                query = query.Where(a => a.Employee != null &&
                    (a.Employee.User != null && a.Employee.User.FullName != null && a.Employee.User.FullName.Contains(Pramter.EmployeeName)));
            }
            query = query
                .Include(a => a.Employee.Department)
                .Include(a => a.Employee.User)
                .AsSplitQuery();

            //query = query.OrderByDescending(a => a.Id);

            var result = query.Select(a => new EmployeeAttendanceDTO(
                  a.Id,
                  a.EmployeeCode,
                  a.Employee.User != null ? a.Employee.User.FullName ?? "Unknown" : "Unknown",
                  a.Employee.User != null ? a.Employee.User.Id ?? string.Empty : string.Empty,
                  a.Employee.Department != null ? a.Employee.Department.Id : 0,
                  a.Employee.Department != null ? a.Employee.Department.Name ?? "Unknown" : "Unknown",
                  a.AttendanceDate!.Value,
                  a.CheckInTime,
                  a.CheckOutTime,
                  a.CheckInMethod,
                  a.AttendanceStatus
              ));
            var pagedResult = await result.ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);
            return pagedResult;
        }
    }
}
