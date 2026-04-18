using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class EmployeeLeaveBalance : BaseEntity
    {
        [ForeignKey(nameof(Employee))]
        [StringLength(150)]
        public string? EmployeeCode { get; set; } = string.Empty;
        public virtual Employee? Employee { get; set; }

        [ForeignKey(nameof(Representative))]
        public string? RepresentativeCode { get; set; } = string.Empty;
        public virtual Representatives? Representative { get; set; }


        [Required]
        [ForeignKey(nameof(LeaveType))]
        public int LeaveTypeId { get; set; }
        public virtual LeaveType? LeaveType { get; set; }

        public int Year { get; set; } // السنة
        public decimal OpeningBalance { get; set; } // الرصيد الافتتاحي في بداية السنة
        public decimal Accrued { get; set; } // الرصيد المكتسب طوال السنة
        public decimal Used { get; set; } // الرصيد المستخدم
        public decimal Remaining { get; set; } // الرصيد المتبقي بعد الخصم
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
