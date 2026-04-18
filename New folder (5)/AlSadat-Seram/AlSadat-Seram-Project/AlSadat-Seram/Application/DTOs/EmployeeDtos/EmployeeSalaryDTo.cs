namespace Application.DTOs.EmployeeSalary;

public class EmployeeSalaryDTo
{
    // معلومات الموظف
    public string? EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }

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

// DTOs مساعدة
public class LeaveDetailDto
{
    public string LeaveType { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int Days { get; set; }
    public bool IsPaid { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LoanDetailDto
{
    public string LoanNumber { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsPaid { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SanctionDetailDto
{
    public DateTime Date { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal DeductionAmount { get; set; }
}

public class AttendanceDetailDto
{
    public DateOnly Date { get; set; }
    public TimeOnly? CheckIn { get; set; }
    public TimeOnly? CheckOut { get; set; }
    public string Status { get; set; } = string.Empty;
    public double WorkHours { get; set; }
    public double OvertimeHours { get; set; }
    public double DeductionHours { get; set; }
}
// DTOs للتقرير التحليلي
public class MonthlySalarySummaryDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public FinancialAnalysisDto FinancialAnalysis { get; set; } = new FinancialAnalysisDto();
    public TimeAnalysisDto TimeAnalysis { get; set; } = new TimeAnalysisDto();
    public PerformanceAnalysisDto PerformanceAnalysis { get; set; } = new PerformanceAnalysisDto();
    public List<string> Recommendations { get; set; } = new List<string>();
    public List<KeyMetricDto> KeyMetrics { get; set; } = new List<KeyMetricDto>();
}

public class FinancialAnalysisDto
{
    public decimal BasicSalary { get; set; }
    public decimal TotalAdditions { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal BasicSalaryPercentage { get; set; } // نسبة الراتب الأساسي من إجمالي المستحقات
    public decimal OvertimePercentage { get; set; }    // نسبة الوقت الإضافي من إجمالي المستحقات
    public decimal DeductionPercentage { get; set; }   // نسبة الخصومات من الراتب الأساسي
    public decimal NetSalaryToBasicRatio { get; set; } // نسبة صافي الراتب إلى الراتب الأساسي
    public List<DeductionBreakdownDto> DeductionBreakdown { get; set; } = new();
    public List<AdditionBreakdownDto> AdditionBreakdown { get; set; } = new();
}

public class DeductionBreakdownDto
{
    public string DeductionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class AdditionBreakdownDto
{
    public string AdditionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class TimeAnalysisDto
{
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public double OvertimeHours { get; set; }
    public double DeductionHours { get; set; }
    public decimal AttendanceRate { get; set; }    // نسبة الحضور
    public decimal AbsenceRate { get; set; }       // نسبة الغياب
    public decimal LeaveRate { get; set; }         // نسبة الإجازات
    public decimal OvertimeRate { get; set; }      // نسبة الوقت الإضافي (ساعات إضافية لكل يوم عمل)
    public List<DailyAttendanceDto> DailyBreakdown { get; set; } = new();
}

public class DailyAttendanceDto
{
    public DateOnly Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double WorkHours { get; set; }
    public bool IsHoliday { get; set; }
}

public class PerformanceAnalysisDto
{
    public decimal PunctualityScore { get; set; }      // درجة الالتزام بالمواعيد
    public decimal AttendanceScore { get; set; }       // درجة الحضور
    public decimal OvertimeScore { get; set; }         // درجة الإضافي
    public decimal ProductivityScore { get; set; }     // درجة الإنتاجية
    public string PerformanceLevel { get; set; } = string.Empty; // ممتاز/جيد جدا/جيد/مقبول/ضعيف
    public List<string> Strengths { get; set; } = new();
    public List<string> AreasForImprovement { get; set; } = new();
}

public class KeyMetricDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal Target { get; set; }
    public string Status { get; set; } = string.Empty; // جيد/متوسط/ضعيف
    public string Trend { get; set; } = string.Empty;  // متزايد/مستقر/متناقص
}

// DTOs للإحصائيات الشهرية
public class MonthlyStatisticsDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public SalaryStatisticsDto SalaryStats { get; set; } = new();
    public AttendanceStatisticsDto AttendanceStats { get; set; } = new();
    public LeaveStatisticsDto LeaveStats { get; set; } = new();
    public OvertimeStatisticsDto OvertimeStats { get; set; } = new();
}

public class SalaryStatisticsDto
{
    public decimal AverageDailySalary { get; set; }
    public decimal AverageHourlySalary { get; set; }
    public decimal OvertimeCostPerHour { get; set; }
    public decimal DeductionCostPerHour { get; set; }
    public decimal SalaryGrowthRate { get; set; } // نسبة نمو الراتب مقارنة بالشهر السابق
}

public class AttendanceStatisticsDto
{
    public int TotalScheduledDays { get; set; }
    public int ActualWorkingDays { get; set; }
    public int LateCount { get; set; }
    public int EarlyLeaveCount { get; set; }
    public decimal PunctualityIndex { get; set; }
    public string AttendanceGrade { get; set; } = string.Empty;
}

public class LeaveStatisticsDto
{
    public int TotalLeaveDays { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public decimal LeaveUtilizationRate { get; set; }
    public int LeaveRequestsCount { get; set; }
    public string MostUsedLeaveType { get; set; } = string.Empty;
}

public class OvertimeStatisticsDto
{
    public double TotalOvertimeHours { get; set; }
    public double AverageDailyOvertime { get; set; }
    public decimal OvertimeCost { get; set; }
    public string OvertimePattern { get; set; } = string.Empty; // منتظم/موسمي/عشوائي
    public bool IsExcessiveOvertime { get; set; }
}

public class SalaryComparisonDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public SalaryPeriodDto BasePeriod { get; set; } = new();
    public SalaryPeriodDto ComparePeriod { get; set; } = new();
    public ComparisonResultDto Comparison { get; set; } = new();
    public List<MetricChangeDto> MetricChanges { get; set; } = new();
}

public class SalaryPeriodDto
{
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalAdditions { get; set; }
    public decimal TotalDeductions { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public double OvertimeHours { get; set; }
    public double DeductionHours { get; set; }
}

public class ComparisonResultDto
{
    public decimal SalaryChangePercentage { get; set; }
    public decimal AttendanceChangePercentage { get; set; }
    public decimal OvertimeChangePercentage { get; set; }
    public string OverallTrend { get; set; } = string.Empty;
    public string PerformanceChange { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public class MetricChangeDto
{
    public string MetricName { get; set; } = string.Empty;
    public decimal BaseValue { get; set; }
    public decimal CompareValue { get; set; }
    public decimal ChangePercentage { get; set; }
    public bool IsImprovement { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class SalaryHistoryDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public int Year { get; set; }
    public List<MonthlySalaryRecordDto> MonthlyRecords { get; set; } = new();
    public YearlySummaryDto YearlySummary { get; set; } = new();
}

public class MonthlySalaryRecordDto
{
    public int Month { get; set; }
    public string EmpName { get; set; } = string.Empty;
    public string MonthName { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalAdditions { get; set; }
    public decimal TotalDeductions { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int PaidLeaveDays { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public double OvertimeHours { get; set; }
    public double DeductionHours { get; set; }
    public string PerformanceLevel { get; set; } = string.Empty;
}

public class YearlySummaryDto
{
    public decimal TotalNetSalary { get; set; }
    public decimal AverageMonthlySalary { get; set; }
    public decimal AverageMonthlyAdditions { get; set; }
    public decimal AverageMonthlyDeductions { get; set; }
    public int TotalPresentDays { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalPaidLeaveDays { get; set; }
    public int TotalUnpaidLeaveDays { get; set; }
    public double TotalOvertimeHours { get; set; }
    public double TotalDeductionHours { get; set; }
    public string BestPerformanceMonth { get; set; } = string.Empty;
    public string WorstPerformanceMonth { get; set; } = string.Empty;
    public decimal BestNetSalary { get; set; }
    public decimal WorstNetSalary { get; set; }
}