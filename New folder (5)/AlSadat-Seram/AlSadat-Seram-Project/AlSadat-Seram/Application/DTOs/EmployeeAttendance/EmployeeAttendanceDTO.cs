using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.EmployeeAttendance
{
    public record EmployeeAttendanceDTO
    (
         int Id ,
         string EmployeeCode ,
         string? EmployeeName ,
         string? EmployeeId ,
         int DepartmentId ,
         string? DepartmentName ,
         DateOnly AttendanceDate ,
         TimeOnly? CheckInTime ,
         TimeOnly? CheckOutTime ,
         CheckInMethod? CheckInMethod ,
         AttendanceStatus? AttendanceStatus 
    );
}
