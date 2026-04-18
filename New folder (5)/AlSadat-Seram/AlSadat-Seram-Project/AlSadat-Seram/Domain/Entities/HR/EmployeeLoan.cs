using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR
{
    public class EmployeeLoan : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string LoanNumber { get; set; } = string.Empty;


        [ForeignKey(nameof(Employee))]
        [StringLength(150)]
        public string? EmployeeCode { get; set; } 
        public virtual Employee? Employee { get; set; }


        [ForeignKey(nameof(Representative))]
        public string? RepresentativeCode { get; set; } 
        public virtual Representatives? Representative { get; set; }


        [Column(TypeName = "money")]
        public decimal LoanAmount { get; set; }  // قيمة القرض
        public int InstallmentsCount { get; set; }  // عدد الأقساط
        [Column(TypeName = "money")]
        public decimal InstallmentAmount { get; set; }  // قيمة القسط الشهري
        [Column(TypeName = "money")]
        public decimal RemainingAmount { get; set; }  // المبلغ المتبقي من القرض
        [Column(TypeName = "money")]
        public decimal PaidAmount { get; set; } = 0;
        public bool IsPaidOff { get; set; }  // حالة القرض (مدفوع بالكامل أم لا)
        
        public LoanStatus Status { get; set; } = LoanStatus.PendingApproval;
        
        public DateTime LoanDate { get; set; }
        public DateTime FirstInstallmentDate { get; set; }  // تاريخ بدء القرض
        public DateTime? ExpectedEndDate { get; set; }  // تاريخ سداد القرض بالكامل
        public DateTime? ActualEndDate { get; set; }
        
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }

        public virtual ICollection<EmployeeLoanPayments> Payments { get; set; } = [];
    }

}
