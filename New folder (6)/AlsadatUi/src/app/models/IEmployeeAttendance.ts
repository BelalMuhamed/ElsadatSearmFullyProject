export interface EmployeeAttendanceDto {
  id: number;
  employeeCode: string;
  employeeName?: string | null;
  employeeId?: string | null;
  departmentId: number;
  departmentName?: string | null;
  attendanceDate: string; // ISO date (DateOnly in backend)
  checkInTime?: string | null; // HH:mm:ss or null
  checkOutTime?: string | null;
  checkInMethod?: string | null;
  attendanceStatus?: number | null;
}

export interface EmployeeAttendanceFilteration {
  page: number;
  pageSize: number;
  employeeCode?: string | null;
  employeeId?: string | null;
  selectedDate?: string | null; // ISO date string
  startDate?: string | null;
  endDate?: string | null;
  employeeName?: string | null;
  departmentName?: string | null;
  year?: number | null;
  month?: number | null;
  attendanceStatusID?: number | null;
}
