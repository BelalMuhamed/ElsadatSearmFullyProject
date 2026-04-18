using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.EmployeeLoan
{
    public class CreateEmployeeLoanDto
    {
        public string? EmployeeCode { get; set; }
        public string? RepresentativeCode { get; set; }


        [Required]
        [Range(1000,1000000)]
        public decimal LoanAmount { get; set; }

        [Required]
        [Range(1,60)]
        public int InstallmentsCount { get; set; }

        [Required]
        public DateTime FirstInstallmentDate { get; set; }

        public string? Purpose { get; set; }
    }
    public class UpdateEmployeeLoanDto
    {
        public string? Purpose { get; set; }
        public string? Notes { get; set; }
    }
    public class EmployeeLoanDto
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public string? EmployeeCode { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public string? RepresentativeCode { get; set; }
        public decimal LoanAmount { get; set; }
        public int InstallmentsCount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public bool IsPaidOff { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime LoanDate { get; set; }
        public DateTime FirstInstallmentDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public string? Purpose { get; set; }
    }
    public class EmployeeLoanSummaryDto
    {
        public string? EmployeeCode { get; set; } 
        public string? EmployeeName { get; set; }
        public string? RepresentativeCode { get; set; }
        public string? EmployeeDepartment { get; set; } 
        public int TotalLoansCount { get; set; }
        public int ActiveLoansCount { get; set; }
        public int PaidLoansCount { get; set; }
        public int PendingLoansCount { get; set; }
        public int RejectedLoansCount { get; set; }
        public decimal TotalBorrowed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalRemaining { get; set; }
        public decimal CurrentMonthDeduction { get; set; }
        public decimal MaxLoanAmount { get; set; }
        public decimal AvailableLoanAmount { get; set; }
        public List<LoanDetailDto> LoanDetails { get; set; } = new();
    }

    public class LoanDetailDto
    {
        public int LoanId { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public int InstallmentsCount { get; set; }
        public int InstallmentsPaid { get; set; }
        public int InstallmentsRemaining { get; set; }
        public DateTime NextDueDate { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime FirstInstallmentDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPaidOff { get; set; }
    }

    public class LoanFilterDto
    {
        public string? UserCode { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? RepresentativeCode { get; set; }
        public LoanStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsPaidOff { get; set; }
    }

}
