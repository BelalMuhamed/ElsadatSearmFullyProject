// Payroll DTOs - تطابق الـ Backend DTOs

export interface GeneratePayrollRequestDto {
  employeeCode?: string | null;
  representativeCode?: string | null;
  month: number;
  year: number;
  payLoansFromSalary?: boolean;
  paymentMethodForLoans?: string;
}

export interface GenerateBulkPayrollRequestDto {
  month: number;
  year: number;
  userCodes?: string[] | null; // محدد من السيرفر ليشمل موظفين ومندوبين
  employeeCodes?: string[] | null; // null = كل الموظفين
  includeActiveOnly?: boolean;
  autoPostToAccounting?: boolean;
  paymentMethod?: string;
  payLoansFromSalary?: boolean;
  confirmLoans?: boolean;
}

export interface PayrollGenerationDetailDto {
  employeeCode: string;
  representativeCode?: string;
  employeeName: string;
  department: string;
  basicSalary: number;
  overtimePay: number;
  deductions: number;
  netSalary: number;
  status: string; // Success, Failed
  message?: string;
  payrollId?: number;
}

export interface BulkPayrollResultDto {
  totalEmployees: number;
  processedSuccessfully: number;
  failed: number;
  totalNetSalary: number;
  totalDeductions: number;
  totalOvertime: number;
  accountingEntryNumber?: string;
  details: PayrollGenerationDetailDto[];
  generatedAt: Date;
  generatedBy: string;
}

export interface PayrollFilterDto {
  month?: number;
  year?: number;
  departments?: string[];
  status?: string; // "Created", "Approved", "Paid", etc.
  fromDate?: Date;
  toDate?: Date;
  minNetSalary?: number;
  maxNetSalary?: number;
}

export interface PayrollResponseDto {
  id: number;
  employeeCode: string;
  representativeCode?: string | null;
  employeeName: string;
  department: string;
  payPeriod: Date;
  basicSalary: number;
  grossSalary: number;
  overtimePay: number;
  totalDeductions: number;
  timeDeductions: number;
  absentDeductions: number;
  leaveDeductions: number;
  lateDeductions: number;
  earlyLeaveDeductions: number;
  sanctionDeductions: number;
  hasPendingLoans: boolean;
  pendingLoanAmount: number;
  loanInstallmentsCount: number;
  netSalary: number;
  // Backend returns status as string: "Created", "Approved", "Paid", "Rejected", "Cancelled"
  status: string;
  paymentMethod?: string;
  accountingEntryNumber?: string;
  postedToAccountingDate?: Date;
  paidDate?: Date;
  paidBy?: string;
  createdAt: Date;
}

// Status constants - match backend enum ToString() values
export const PayrollStatus = {
  Created: 'Created',
  Approved: 'Approved', 
  Paid: 'Paid',
  Rejected: 'Rejected',
  Cancelled: 'Cancelled'
} as const;

export interface PayrollExportDto {
  fileContent: Uint8Array;
  fileName: string;
  contentType: string;
}

export interface PayrollSummaryDto {
  month: number;
  year: number;
  totalEmployees: number;
  totalBasicSalary: number;
  totalOvertime: number;
  totalDeductions: number;
  totalNetSalary: number;
  generatedCount: number;
  postedCount: number;
  paidCount: number;
}

export interface MarkPayrollPaidDto {
  payrollIds: number[];
  paymentMethod: string;
  paymentReference?: string;
}

export interface PayrollFilterRequest {
  month?: number;
  year?: number;
  departments?: string[];
  status?: string;
  fromDate?: string;
  toDate?: string;
  minNetSalary?: number;
  maxNetSalary?: number;
}

// DTO for Payroll Preview (before confirmation)
export interface PayrollPreviewDto {
  employeeCode?: string | null;
  representativeCode?: string | null;
  employeeName: string;
  department: string;
  month: number;
  year: number;
  basicSalary: number;
  grossSalary: number;
  overtimePay: number;
  totalDeductions: number;
  timeDeductions: number;
  absentDeductions: number;
  leaveDeductions: number;
  lateDeductions: number;
  earlyLeaveDeductions: number;
  sanctionDeductions: number;
  hasPendingLoans: boolean;
  pendingLoanAmount: number;
  loanInstallmentsCount: number;
  dueInstallments: LoanInstallmentDto[];
  netSalaryBeforeLoan: number;
  netSalaryAfterLoan: number;
  deductLoan: boolean;
}

export interface LoanInstallmentDto {
  loanId: number;
  loanNumber: string;
  installmentAmount: number;
  dueDate: Date;
}

// DTO for Bulk Payroll Preview
export interface PreviewBulkPayrollDto {
  totalEmployees: number;
  successCount: number;
  failedCount: number;
  totalNetSalary: number;
  totalDeductions: number;
  totalOvertime: number;
  totalBasicSalary: number;
  successPreviews: PayrollPreviewDto[];
  failedPreviews: FailedPreviewDto[];
  previewedAt: Date;
  previewedBy: string;
}

export interface FailedPreviewDto {
  employeeCode: string;
  employeeName: string;
  department: string;
  status: string; // Existing, Error
  message: string;
  existingPayrollId?: number;
}
