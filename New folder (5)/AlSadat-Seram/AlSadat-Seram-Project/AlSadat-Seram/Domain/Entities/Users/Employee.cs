using Domain.Entities.Commonitems;
using Domain.Entities.HR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Users
{
    public class Employee : BaseStaff
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? EmployeeCode { get; set; }
        public int? AccountNumber { get; set; }
        public string? AccountName { get; set; } = string.Empty;
        public string SNO { get; set; } = string.Empty;
        //قيمه الساعه الاضافيه
        [Precision(18,4)]
        public decimal OvertimeRatePerHour { get; set; }
        //قيمه الساعه الخصم
        [Precision(18,4)]
        public decimal DeductionRatePerHour { get; set; }

        //----------- Obj From Department and ForeignKey DepartmentId ---------------------------------
        [ForeignKey(nameof(Department))]
        public int? DepartmentID { get; set; }
        virtual public Department? Department { get; set; }

        // Navigation property to Attendance
        public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; } = new List<EmployeeAttendance>();
        
        [ForeignKey(nameof(Manager))]
        public string? ManagerEmployeeCode { get; set; }
        public virtual Employee? Manager { get; set; }
    }
}
