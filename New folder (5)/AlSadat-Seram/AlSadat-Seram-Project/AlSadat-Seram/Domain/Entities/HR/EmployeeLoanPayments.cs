using Domain.Entities.Commonitems;
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
    public class EmployeeLoanPayments : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string PaymentNumber { get; set; } = string.Empty;


        [ForeignKey(nameof(EmployeeLoan))]
        public int LoanId { get; set; }
        public virtual EmployeeLoan? EmployeeLoan { get; set; }


        [ForeignKey(nameof(Payroll))]
        public int? PayrollId { get; set; }
        public virtual Payroll? Payroll { get; set; }


        [Column(TypeName = "money")]
        public decimal PaymentAmount { get; set; }  // مبلغ الدفع
        public DateTime PaymentDate { get; set; }  // تاريخ الدفع
        [Column(TypeName = "money")]
        public decimal RemainingAmount { get; set; }  // المبلغ المتبقي بعد الدفع
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.SalaryDeduction;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreateBy { get; set; }
    }

}
