// DTOs for employee leave requests (frontend representation)
export interface EmployeeLeaveRequestDto {
  id: number;
  employeeCode: string;
  representativeCode?: string;
  employeeName?: string;
  representativeName?: string;
  leaveTypeId: number;
  leaveTypeName?: string;
  fromDate: string | Date;
  toDate: string | Date;
  daysRequested: number;
  status: number;
  statusName?: string;
  approvedBy?: string;
  approvedAt?: string | Date | null;
  rejectedBy?: string | null;
  rejectedAt?: string | Date | null;
  rejectionReason?: string | null;
  notes?: string | null;
  createdAt: string | Date;
  createdBy?: string | null;
}

export interface ApproveRejectLeaveDto {
  leaveRequestId: number;
  reason?: string | null;
}

export interface CreateLeaveRequestDto {
  employeeEmail?: string;
  employeeCode?: string;
  representativeCode?: string;
  leaveTypeId: number;
  fromDate: string | Date;
  toDate: string | Date;
  notes?: string | null;
  isEmergency?: boolean;
  contactDuringLeave?: string | null;
}

export interface LeaveRequestFilterDto {
  employeeCode?: string | null;
  representativeCode?: string | null;
  leaveTypeId?: number | null;
  status?: number | null;
  fromDate?: string | Date | null;
  toDate?: string | Date | null;
  sortBy?: string | null;
  pageNumber?: number;
  pageSize?: number;
  sortDescending?: boolean;
}
