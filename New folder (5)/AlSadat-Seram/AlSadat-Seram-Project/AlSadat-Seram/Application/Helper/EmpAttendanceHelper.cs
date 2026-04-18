using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helper
{
    public class EmpAttendanceHelper
    {
        public string? EmployeeCode { get; set; }
        public string? EmployeeId { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public DateOnly? Date { get; set; }
        public TimeOnly? InputTime { get; set; }
        public DateOnly? SelectedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? AttendanceStatusID { get; set; }
        public string? DepartmentName { get; set; } 
        public string? EmployeeName { get; set; }
        public  string? employeeEmail { get; set; }
    }
}
