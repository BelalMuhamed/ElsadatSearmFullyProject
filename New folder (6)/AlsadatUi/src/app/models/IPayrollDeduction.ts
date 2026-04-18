// DTO Models for Payroll Deductions - Matches Backend DTOs exactly

export interface PayrollDeductionsDto {
  id?: number;
  employeeCode?: string;
  representativeCode?: string;
  deductionDate: string | Date; // Backend expects YYYY-MM-DD format
  deductionAmount: number;
  moneyAmount?: number; // Calculated by backend
  deductionReason: string;
  createdAt?: string;
  isDeleted?: boolean;
}

export interface DeductionDetailDto {
  id: number;
  employeeCode?: string;
  representativeCode?: string;
  employeeName: string;
  deductionDate: string | Date;
  deductionAmount: number;
  monayAmount: number; // Note: Typo in backend (monay instead of money)
  deductionReason: string;
  createdAt: string;
  createBy: string;
  updateBy?: string;
  deleteBy?: string;
  updateAt?: string;
  isDeleted: boolean;
  deleteAt?: string;
}

export interface DeductionTotalsDto {
  totalMoneyAmount: number;
  totalDeductionHours: number;
  totalRecords: number;
  employeeCode?: string;
  representativeCode?: string;
  employeeName?: string;
  month?: number;
  year?: number;
}

export interface EmployeeDeductionsSummaryDto {
  deductions: DeductionDetailDto[];
  totals: DeductionTotalsDto;
  deductionsCount?: number;
}

export interface PayrollDeductionSearchDto {
  employeeCode?: string; // can be employee or representative code
  month?: number;
  year?: number;
  fromDate?: string | Date;
  toDate?: string | Date;
  includeDeleted?: boolean;
}

export interface ApiResponse<T> {
  isSuccess: boolean;
  data?: T;
  message?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}
