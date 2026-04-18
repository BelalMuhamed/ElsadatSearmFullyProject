// Leave balance DTOs for frontend
export interface LeaveTypeBalanceDto {
  leaveTypeId: number;
  leaveTypeName: string;
  isPaid: boolean;
  openingBalance: number;
  accrued: number;
  used: number;
  remaining: number;
  pending: number;
}

export interface LeaveBalanceSummaryDto {
  employeeCode: string;
  employeeName: string;
  year: number;
  balances: LeaveTypeBalanceDto[];
  totalRemaining?: number;
  totalUsed?: number;
  totalPending?: number;
}

export interface EmployeeLeaveBalanceDto {
  employeeCode: string;
  employeeName?: string;
  leaveTypeId: number;
  leaveTypeName?: string;
  year: number;
  openingBalance: number;
  accrued: number;
  used: number;
  remaining: number;
  pendingRequests?: number;
}

export interface LeaveBalanceItemDto {
  leaveTypeId: number;
  openingBalance: number;
  accrued?: number | null;
}

export interface BulkLeaveBalanceRequestDto {
  employeeCode: string;
  year: number;
  balances: LeaveBalanceItemDto[];
}

export interface LeaveBalanceDetailDto {
  leaveTypeId: number;
  leaveTypeName: string;
  openingBalance: number;
  accrued: number;
  remaining: number;
}

export interface FailedBalanceDto {
  leaveTypeId: number;
  leaveTypeName?: string;
  reason: string;
}

export interface BulkLeaveBalanceResultDto {
  employeeCode: string;
  employeeName: string;
  year: number;
  createdBalances: LeaveBalanceDetailDto[];
  failedBalances: FailedBalanceDto[];
  totalCreated?: number;
  totalFailed?: number;
}
