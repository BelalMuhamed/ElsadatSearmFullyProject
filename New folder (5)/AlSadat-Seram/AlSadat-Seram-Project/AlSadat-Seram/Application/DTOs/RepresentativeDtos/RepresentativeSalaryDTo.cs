using Application.DTOs.EmployeeSalary;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.DTOs.RepresentativeDtos;
public class RepresentativeSalaryDTo
{
    // معلومات الموظف
    public string? RepresentativeId { get; set; }
    public string? RepresentativeCode { get; set; }
    public string? RepresentativeName { get; set; }

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

    public int PointsWallet { get; set; }
    public decimal MoneyOfPointInWallet { get; set; }

    [Column(TypeName = "money")]
    public decimal MoneyDeposit { get; set; }  // رصيد العهدة 
    public decimal TotalDeductionFromMoneyDeposit { get; set; }

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
