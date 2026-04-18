export interface EmployeeDTo {
  userId?: string;
  email: string;
  password?: string;
  fullName: string;
  phoneNumber?: string;
  address?: string;
  gender: number; // 0 = Male, 1 = Female
  createBy?: string;
  createAt?: Date;
  updateBy?: string;
  updateAt?: Date;
  isDeleted?: boolean;
  deleteBy?: string;
  deleteAt?: Date;
  cityID?: number;
  cityName?: string;
  employeeCode?: string;
  accountNumber?: number;
  accountName?: string;
  sno: string;
  overtimeRatePerHour: number;
  deductionRatePerHour: number;
  departmentID?: number;
  departmentName?: string;
  birthDate: Date;
  hireDate: Date;
  salary: number;
  timeIn: string; // TimeOnly format "HH:mm:ss"
  timeOut: string; // TimeOnly format "HH:mm:ss"
  weekHoliday1: number; // WeekDays enum
  weekHoliday2?: number; // WeekDays enum
  nameOfCreatedBy?: string;
  roleName?: string;
  roleId?: string;
  rolesName?: string[];
  rolesId?: string[];
}

export interface EmployeeHelper {
  empCode?: string | null;
  departmentName?: string | null;
  employeeName?: string | null;
  cityName?: string | null;
}

export interface EmployeeSalaryDTo {
  employeeId?: string;
  employeeCode?: string;
  employeeName?: string;
  departmentName?: string;
  selectedMonth: number;
  selectedYear: number;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  lateDays: number;
  earlyLeaveDays: number;
  basicSalary: number;
  salaryPerDay: number;
  salaryPerHour: number;
  overtimeHours: number;
  overtimeRatePerHour: number;
  totalOvertimePay: number;
  deductionHours: number;
  deductionRatePerHour: number;
  timeDeductionAmount: number;
  absentDeduction: number;
  unpaidLeaveDeduction: number;
  lateDeduction: number;
  earlyLeaveDeduction: number;
  loanDeduction: number;
  loanInstallmentsCount: number;
  sanctionAmount: number;
  sanctionsCount: number;
  totalAdditions: number;
  totalDeductions: number;
  netSalary: number;
  summary?: string;
  leaveDetails?: LeaveDetailDto[];
  loanDetails?: LoanDetailDto[];
  sanctionDetails?: SanctionDetailDto[];
  attendanceDetails?: AttendanceDetailDto[];
}

export interface LeaveDetailDto {
  leaveType: string;
  fromDate: Date;
  toDate: Date;
  days: number;
  isPaid: boolean;
  status: string;
}

export interface LoanDetailDto {
  loanNumber: string;
  loanAmount: number;
  installmentAmount: number;
  dueDate: Date;
  isPaid: boolean;
  status: string;
}

export interface SanctionDetailDto {
  date: Date;
  reason: string;
  amount: number;
  type: string;
  deductionAmount: number;
}

export interface AttendanceDetailDto {
  date: string;
  checkIn?: string;
  checkOut?: string;
  status: string;
  workHours: number;
  overtimeHours: number;
  deductionHours: number;
}

export interface MonthlySalarySummaryDto {
  employeeCode: string;
  employeeName: string;
  month: number;
  year: number;
  financialAnalysis: FinancialAnalysisDto;
  timeAnalysis: TimeAnalysisDto;
  performanceAnalysis: PerformanceAnalysisDto;
  keyMetrics: KeyMetricDto[];
  recommendations: string[];
}

export interface FinancialAnalysisDto {
  basicSalary: number;
  totalAdditions: number;
  totalDeductions: number;
  netSalary: number;
  basicSalaryPercentage: number;
  overtimePercentage: number;
  deductionPercentage: number;
  netSalaryToBasicRatio: number;
  deductionBreakdown: DeductionBreakdownDto[];
  additionBreakdown: AdditionBreakdownDto[];
}

export interface DeductionBreakdownDto {
  deductionType: string;
  amount: number;
  percentage: number;
}

export interface AdditionBreakdownDto {
  additionType: string;
  amount: number;
  percentage: number;
}

export interface TimeAnalysisDto {
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  overtimeHours: number;
  deductionHours: number;
  attendanceRate: number;
  absenceRate: number;
  leaveRate: number;
  overtimeRate: number;
  dailyBreakdown: DailyAttendanceDto[];
}

export interface DailyAttendanceDto {
  date: string;
  dayOfWeek: string;
  status: string;
  workHours: number;
  isHoliday: boolean;
}

export interface PerformanceAnalysisDto {
  punctualityScore: number;
  attendanceScore: number;
  overtimeScore: number;
  productivityScore: number;
  performanceLevel: string;
  strengths: string[];
  areasForImprovement: string[];
}

export interface KeyMetricDto {
  metricName: string;
  value: number;
  target: number;
  status: string;
  trend: string;
}

export interface MonthlyStatisticsDto {
  employeeCode: string;
  employeeName: string;
  month: number;
  year: number;
  salaryStats: SalaryStatisticsDto;
  attendanceStats: AttendanceStatisticsDto;
  leaveStats: LeaveStatisticsDto;
  overtimeStats: OvertimeStatisticsDto;
}

export interface SalaryStatisticsDto {
  averageDailySalary: number;
  averageHourlySalary: number;
  overtimeCostPerHour: number;
  deductionCostPerHour: number;
  salaryGrowthRate: number;
}

export interface AttendanceStatisticsDto {
  totalScheduledDays: number;
  actualWorkingDays: number;
  lateCount: number;
  earlyLeaveCount: number;
  punctualityIndex: number;
  attendanceGrade: string;
}

export interface LeaveStatisticsDto {
  totalLeaveDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  leaveUtilizationRate: number;
  leaveRequestsCount: number;
  mostUsedLeaveType: string;
}

export interface OvertimeStatisticsDto {
  totalOvertimeHours: number;
  averageDailyOvertime: number;
  overtimeCost: number;
  overtimePattern: string;
  isExcessiveOvertime: boolean;
}

export interface SalaryComparisonDto {
  employeeCode: string;
  employeeName: string;
  basePeriod: SalaryPeriodDto;
  comparePeriod: SalaryPeriodDto;
  comparison: ComparisonResultDto;
  metricChanges: MetricChangeDto[];
}

export interface SalaryPeriodDto {
  month: number;
  year: number;
  basicSalary: number;
  netSalary: number;
  totalAdditions: number;
  totalDeductions: number;
  presentDays: number;
  absentDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  overtimeHours: number;
  deductionHours: number;
}

export interface ComparisonResultDto {
  salaryChangePercentage: number;
  attendanceChangePercentage: number;
  overtimeChangePercentage: number;
  overallTrend: string;
  performanceChange: string;
  summary: string;
}

export interface MetricChangeDto {
  metricName: string;
  baseValue: number;
  compareValue: number;
  changePercentage: number;
  isImprovement: boolean;
  status: string;
}

export interface SalaryHistoryDto {
  employeeCode: string;
  employeeName: string;
  year: number;
  monthlyRecords: MonthlySalaryRecordDto[];
  yearlySummary: YearlySummaryDto;
}

export interface MonthlySalaryRecordDto {
  month: number;
  empName: string;
  monthName: string;
  basicSalary: number;
  netSalary: number;
  totalAdditions: number;
  totalDeductions: number;
  presentDays: number;
  absentDays: number;
  paidLeaveDays: number;
  unpaidLeaveDays: number;
  overtimeHours: number;
  deductionHours: number;
  performanceLevel: string;
}

export interface YearlySummaryDto {
  totalNetSalary: number;
  averageMonthlySalary: number;
  averageMonthlyAdditions: number;
  averageMonthlyDeductions: number;
  totalPresentDays: number;
  totalAbsentDays: number;
  totalPaidLeaveDays: number;
  totalUnpaidLeaveDays: number;
  totalOvertimeHours: number;
  totalDeductionHours: number;
  bestPerformanceMonth: string;
  worstPerformanceMonth: string;
  bestNetSalary: number;
  worstNetSalary: number;
}

export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
}

export interface PagedList<T> {
  items: T[];
  currentPage: number;
  totalPages: number;
  pageSize: number;
  totalCount: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
