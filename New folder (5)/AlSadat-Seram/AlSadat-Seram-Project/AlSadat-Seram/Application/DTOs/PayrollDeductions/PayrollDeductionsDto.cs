using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DTOs.PayrollDeductions
{
    public class PayrollDeductionsDto
    {
        public string? EmployeeCode { get; set; }
        public string? RepresentativeCode { get; set; }
        public DateTime DeductionDate { get; set; }  
        public decimal DeductionAmount { get; set; }  
        [Column(TypeName = "money")]
        public decimal MoneyAmount { get; set; }
        public string DeductionReason { get; set; } = string.Empty;  
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
    }
    public class DeductionDetailDto
    {
        public int Id { get; set; }
        public string? EmployeeCode { get; set; } 
        public string? EmployeeName { get; set; }
        public string? RepresentativeCode { get; set; }
        public DateTime DeductionDate { get; set; }
        public decimal DeductionAmount { get; set; }
        [Column(TypeName = "money")]
        public decimal MonayAmount { get; set; }
        public string DeductionReason { get; set; }= string.Empty;
        public DateTime CreatedAt { get; set; }
        [Column(TypeName = "money")]
        public decimal? TotalMonayAmount { get; set; }
        public decimal? TotalDeductionHours { get; set; }
        public string? CreateBy { get; set; }
        public string? UpdateBy { get; set; }
        public string? DeleteBy { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteAt { get; set; }
    }

    public class EmployeeDeductionsSummaryDto
    {
        public List<DeductionDetailDto> Deductions { get; set; } = new List<DeductionDetailDto>();
        public DeductionTotalsDto Totals { get; set; } = new DeductionTotalsDto();
        public int DeductionsCount => Deductions?.Count ?? 0;
    }
    public class PayrollDeductionSearchDto
    {
        public string? EmployeeCode { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IncludeDeleted { get; set; } = false;
    }
    public class DeductionTotalsDto
    {
        public decimal TotalMoneyAmount { get; set; } 
        public decimal TotalDeductionHours { get; set; } 
        public int TotalRecords { get; set; }
        public string? EmployeeCode { get; set; } 
        public string? EmployeeName { get; set; } 
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

}
