using Application.DTOs.EmployeeSalary;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DTOs.Payroll
{
    public class GeneratePayrollRequestDto
    {
        public string? EmployeeCode { get; set; }
        public string? RepresentativeCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        //--------------------------------------
        public bool PayLoansFromSalary { get; set; } = false;
        public string? PaymentMethodForLoans { get; set; } = "Cash";
    }

    // طلب إنشاء كشوف مرتبات جماعي
    public class GenerateBulkPayrollRequestDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public List<string>? UserCodes { get; set; }
        public List<string>? EmployeeCodes { get; set; } // لو null يبقى كل الموظفين
        public bool IncludeActiveOnly { get; set; } = true; // الموظفين النشطين فقط
        public bool AutoPostToAccounting { get; set; } = false; // تسجيل تلقائي في المحاسبة
        public bool PayLoansFromSalary { get; set; } = false;
        public bool ConfirmLoans { get; set; } = false;
    }

    // نتيجة عملية الإنشاء الجماعي
    public class BulkPayrollResultDto
    {
        public int TotalEmployees { get; set; }
        public int ProcessedSuccessfully { get; set; }
        public int Failed { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalOvertime { get; set; }
        public string? AccountingEntryNumber { get; set; }
        public List<PayrollGenerationDetailDto> Details { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
    }
    public class PayrollGenerationDetailDto
    {
        public string? EmployeeCode { get; set; }
        public string? RepresentativeCode { get; set; }
        public string? EmployeeName { get; set; } 
        public string? Department { get; set; } 
        public decimal BasicSalary { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty; // Success, Failed
        public string? Message { get; set; }
        public int? PayrollId { get; set; }
    }

    public class PreviewBulkPayrollDto
    {
        public int TotalEmployees { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalOvertime { get; set; }
        public decimal TotalBasicSalary { get; set; }
        public List<PayrollPreviewDto> SuccessPreviews { get; set; } = new();
        public List<FailedPreviewDto> FailedPreviews { get; set; } = new();
        public DateTime PreviewedAt { get; set; }
        public string PreviewedBy { get; set; } = string.Empty;
    }
    public class FailedPreviewDto
    {
        public string? EmployeeCode { get; set; }
        public string? RepresentativeCode { get; set; }
        public string? EmployeeName { get; set; } 
        public string? Department { get; set; } 
        public string? Status { get; set; } 
        public string? Message { get; set; } 
        public int? ExistingPayrollId { get; set; }
    }
    

    // فلترة لعرض كشوف الرواتب
    public class PayrollFilterDto
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public List<string>? Departments { get; set; }
        public PayrollStatus? Status { get; set; } 
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinNetSalary { get; set; }
        public decimal? MaxNetSalary { get; set; }
    }

    // تصدير Excel
    public class PayrollExportDto
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }

    public class PayrollSummaryDto
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalEmployees { get; set; }
        public decimal TotalBasicSalary { get; set; }
        public decimal TotalOvertime { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public int GeneratedCount { get; set; }
        public int PostedCount { get; set; }
        public int PaidCount { get; set; }
    }

    // DTO لعرض كشف الراتب
    public class PayrollResponseDto
    {
        public int Id { get; set; }
        public string? EmployeeCode { get; set; }
        public string? RepresentativeCode { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime PayPeriod { get; set; }

        // التفاصيل المالية
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; } // إجمالي قبل الخصومات
        public decimal OvertimePay { get; set; }

        // الخصومات
        public decimal TotalDeductions { get; set; } // إجمالي الخصومات بدون قروض
        public decimal TimeDeductions { get; set; }
        public decimal AbsentDeductions { get; set; }
        public decimal LeaveDeductions { get; set; }
        public decimal LateDeductions { get; set; }
        public decimal EarlyLeaveDeductions { get; set; }
        public decimal SanctionDeductions { get; set; }

        // القروض (للإشارة فقط)
        public bool HasPendingLoans { get; set; }
        public decimal PendingLoanAmount { get; set; }
        public int LoanInstallmentsCount { get; set; }

        // النتيجة النهائية
        public decimal NetSalary { get; set; }

        // الحالة
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? AccountingEntryNumber { get; set; }
        public DateTime? PostedToAccountingDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PaidBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PayrollPreviewDto
    {
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? RepresentativeCode { get; set; }
        public string Department { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }

        // الأساسيات
        public decimal BasicSalary { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal OvertimePay { get; set; }

        // الخصومات
        public decimal TotalDeductions { get; set; }
        public decimal TimeDeductions { get; set; }
        public decimal AbsentDeductions { get; set; }
        public decimal LeaveDeductions { get; set; }
        public decimal LateDeductions { get; set; }
        public decimal EarlyLeaveDeductions { get; set; }
        public decimal SanctionDeductions { get; set; }

        // القروض
        public bool HasPendingLoans { get; set; }
        public decimal PendingLoanAmount { get; set; }
        public int LoanInstallmentsCount { get; set; }
        public List<LoanInstallmentDto> DueInstallments { get; set; } = new();

        // النتيجة
        public decimal NetSalaryBeforeLoan { get; set; }
        public decimal NetSalaryAfterLoan { get; set; }

        // خيارات
        public bool DeductLoan { get; set; } = false;
    }
    public class LoanInstallmentDto
    {
        public int LoanId { get; set; }
        public string LoanNumber { get; set; } = string.Empty;
        public decimal InstallmentAmount { get; set; }
        public DateTime DueDate { get; set; }
    }

    // DTO لتحديث حالة الدفع
    public class MarkPayrollPaidDto
    {
        public List<int> PayrollIds { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
    }

    public class BulkPaymentResultDto
    {
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PaymentReference { get; set; }
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public List<PaymentDetailDto> Details { get; set; } = new();
    }

    public class PaymentDetailDto
    {
        public int PayrollId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal NetSalary { get; set; }
        public string? AccountingEntryNumber { get; set; }
        public string? ErrorMessage { get; set; }
    }


    public class SalaryCalculationResult 
    {
        // معلومات الموظف
        public string? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }

        // معلومات المندوب
        public string? RepresentativeId { get; set; }
        public string? RepresentativeCode { get; set; }
        public string? RepresentativeName { get; set; }
        public int? PointsWallet { get; set; }
        public decimal? MoneyOfPointInWallet { get; set; }
        public decimal? TotalDeductionFromMoneyDeposit { get; set; }
        [Column(TypeName = "money")]
        public decimal? MoneyDeposit { get; set; }

        // الفترة
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }

        // الأيام
        public int TotalWorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int PaidLeaveDays { get; set; }
        public int UnpaidLeaveDays { get; set; }
        public int LateDays { get; set; }
        public int EarlyLeaveDays { get; set; }

        // المرتب الأساسي
        public decimal BasicSalary { get; set; }
        public decimal SalaryPerDay { get; set; }
        public decimal SalaryPerHour { get; set; }

        public decimal GrossSalary { get; set; } // الراتب الإجمالي قبل خصومات الوقت والغياب
        public decimal NetSalaryBeforeLoans { get; set; } // صافي الراتب قبل خصم القروض
        public decimal NetSalary
        {
            get => NetSalaryBeforeLoans;
            set => NetSalaryBeforeLoans = value;
        }

        public bool HasPendingLoans => LoanDeduction > 0;
        // الوقت الإضافي
        public double OvertimeHours { get; set; }
        public decimal OvertimeRatePerHour { get; set; }
        public decimal TotalOvertimePay { get; set; }

        // الخصومات
        public double DeductionHours { get; set; }
        public decimal DeductionRatePerHour { get; set; }
        public decimal TimeDeductionAmount { get; set; }
        public decimal AbsentDeduction { get; set; }
        public decimal UnpaidLeaveDeduction { get; set; }
        public decimal LateDeduction { get; set; }
        public decimal EarlyLeaveDeduction { get; set; }

        // القروض
        public decimal LoanDeduction { get; set; }
        public int LoanInstallmentsCount { get; set; }

        // العقوبات والخصومات الأخرى
        public decimal SanctionAmount { get; set; }
        public int SanctionsCount { get; set; }

        // الإجماليات
        public decimal TotalAdditions { get; set; }
        public decimal TotalDeductions { get; set; }

        // ملخص
        public string Summary { get; set; } = string.Empty;

        // تفاصيل إضافية
        public List<LeaveDetailDto> LeaveDetails { get; set; } = new();
        public List<LoanDetailDto> LoanDetails { get; set; } = new();
        public List<SanctionDetailDto> SanctionDetails { get; set; } = new();
        public List<AttendanceDetailDto> AttendanceDetails { get; set; } = new();
    }

}
