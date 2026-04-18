using Domain.Entities.Commonitems;
using Domain.Entities.Users;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.HR
{
    public class Payroll : BaseEntity
    {
        [ForeignKey(nameof(Employee))]
        public string? EmployeeCode { get; set; }
        public virtual Employee? Employee { get; set; }


        [ForeignKey(nameof(Representative))]
        public string? RepresentativeCode { get; set; } 
        public virtual Representatives? Representative { get; set; }

        [ForeignKey(nameof(LoanPayment))]
        public int? LoanPaymentId { get; set; } // ربط بسجل دفع القرض
        public virtual EmployeeLoanPayments? LoanPayment { get; set; }

        public DateTime PayPeriod { get; set; }  // فترة الراتب (أول يوم في الشهر)
        public DateTime GenerationDate { get; set; } // تاريخ إنشاء الكشف
        public DateTime? PaymentDate { get; set; } // تاريخ الدفع الفعلي

        // الأساسيات
        [Column(TypeName = "money")]
        public decimal BasicSalary { get; set; }
        [Column(TypeName = "money")]
        public decimal GrossSalary { get; set; } // إجمالي الراتب قبل الخصومات (BasicSalary + OvertimePay)
        [Column(TypeName = "money")]
        public decimal OvertimePay { get; set; }
        [Column(TypeName = "money")]
        public decimal LoanDeduction { get; set; } = 0; // قيمة القسط المخصوم في هذا الراتب


        // إجمالي الخصومات بدون قروض
        [Column(TypeName = "money")]
        public decimal TotalDeductions { get; set; }
        [Column(TypeName = "money")]
        public decimal TimeDeductions { get; set; }
        [Column(TypeName = "money")]
        public decimal AbsentDeductions { get; set; }
        [Column(TypeName = "money")]
        public decimal LeaveDeductions { get; set; }
        [Column(TypeName = "money")]
        public decimal LateDeductions { get; set; }
        [Column(TypeName = "money")]
        public decimal EarlyLeaveDeductions { get; set; }
        [Column(TypeName = "money")]
        public decimal SanctionDeductions { get; set; }


        // صافي الراتب بعد خصم  قروض
        [Column(TypeName = "money")]
        public decimal NetSalary { get; set; }

        [Column(TypeName = "money")]
        public decimal NetSalaryBeforeLoan { get; set; } // صافي الراتب قبل خصم القرض

        // معلومات القروض (للإشارة فقط)
        public bool IsLoanDeducted { get; set; } = false; // هل تم خصم قسط القرض؟
        public bool HasPendingLoans { get; set; }

        [Column(TypeName = "money")]
        public decimal PendingLoanAmount { get; set; }
        public int LoanInstallmentsCount { get; set; }

        // الحالة
        public PayrollStatus Status { get; set; } = PayrollStatus.Created;

        // طريقة الدفع
        public string? PaymentMethod { get; set; } // Cash, BankTransfer, Check        

        // المحاسبة
        public string? AccountingEntryNumber { get; set; }
        public DateTime? PostedToAccountingDate { get; set; }
        public bool IsPostedToAccounting { get; set; } = false;
        public int? AccountingEntryId { get; set; }

        // تاريخيات
        public DateTime PayDate { get; set; }
        public string? PaymentReference { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PaidBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
