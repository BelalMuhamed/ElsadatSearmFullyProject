using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DTOs.EmployeeLoanPayments
{
    public class LoanPaymentsDTo
    {     
        public int LoanId { get; set; }  // رقم القرض
        [Column(TypeName = "money")]
        public decimal PaymentAmount { get; set; }  // مبلغ الدفع
        public DateTime PaymentDate { get; set; }  // تاريخ الدفع
        [Column(TypeName = "money")]
        public decimal RemainingAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.SalaryDeduction;
        public string? Notes { get; set; }
    }
    public class ApproveLoanDto
    {
        public int LoanId { get; set; }
        public string? Notes { get; set; }
    }
    public class RejectLoanDto
    {
        public int LoanId { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;
    }

}
