using Domain.Entities.HR;
using Domain.Entities.Users;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Commonitems
{
    public class  BaseStaff
    {
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        [Column(TypeName = "money")]
        public decimal Salary { get; set; }
        public TimeOnly TimeIn { get; set; }
        public TimeOnly TimeOut { get; set; }
        public WeekDays WeekHoliday1 { get; set; }
        public WeekDays? WeekHoliday2 { get; set; }
        [NotMapped]
        public decimal MaxLoanAmount => Salary * 60;

        public string? CreateBy { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.UtcNow;
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }


        //----------- Obj From User and ForeignKey UserID ---------------------------------
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }
        //----------- Collection From EmployeeLoan  ---------------------------------
        public virtual ICollection<EmployeeLoan> EmployeeLoans { get; set; } = [];

    }

}
