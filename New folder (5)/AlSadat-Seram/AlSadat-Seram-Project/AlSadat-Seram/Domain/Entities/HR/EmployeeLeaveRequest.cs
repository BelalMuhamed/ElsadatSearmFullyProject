using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.HR
{
    public class EmployeeLeaveRequest : BaseEntity
    {
        [ForeignKey(nameof(Employee))]
        [StringLength(150)]
        public string? EmployeeCode { get; set; } 
        public virtual Employee? Employee { get; set; }


        [ForeignKey(nameof(Representative))]
        public string? RepresentativeCode { get; set; } 
        public virtual Representatives? Representative { get; set; }

        [Required]
        [ForeignKey(nameof(LeaveType))]
        public int LeaveTypeId { get; set; }
        public virtual LeaveType? LeaveType { get; set; }

        public DateTime FromDate { get; set; } // من تاريخ
        public DateTime ToDate { get; set; } // إلى تاريخ
        public decimal DaysRequested { get; set; } // عدد الأيام المطلوبة
        public LeaveRequestStatus Status { get; set; } // حالة الطلب (معلق، معتمد، مرفوض)

        public string ApprovedBy { get; set; } = string.Empty; // من قام بالموافقة
        public DateTime? ApprovedAt { get; set; } // تاريخ الموافقة

        public string? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }

        public string? Notes { get; set; } // ملاحظات إضافية

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }

}
