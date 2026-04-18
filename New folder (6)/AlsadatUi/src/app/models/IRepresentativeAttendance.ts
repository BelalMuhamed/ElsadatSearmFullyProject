export enum CheckInMethod {
  FingerPrint = 0,
  Manual = 1,
  MobileApp = 2
}

export enum AttendanceStatus {
  Present = 1,
  Absent = 2,
  Late = 3,
  Excused = 4,
  Vacation = 5,
  SickLeave = 6,
  EarlyLeave = 7
}

export interface RepresentativeAttendanceDto {
  id: number;
  representativeCode: string;
  representativeName: string;
  representativeId: string;
  attendanceDate: string | Date;
  checkInTime?: string | null;
  checkOutTime?: string | null;
  checkInMethod: CheckInMethod;
  attendanceStatus: AttendanceStatus;
  checkInLatitude?: number | null;
  checkInLongitude?: number | null;
  checkInLocation: string;
}

export interface RepresentativeAttendanceFilteration {
  page: number;
  pageSize: number;
  representativeCode?: string | null;
  representativeId?: string | null;
  representativeName?: string | null;
  selectedDate?: string | null;
  startDate?: Date | null;
  endDate?: Date | null;
  year?: number | null;
  month?: number | null;
  attendanceStatusID?: number | null;
  representativeEmail?: string | null;
  checkInLatitude?: number | null;
  checkInLongitude?: number | null;
  checkInLocation?: string | null;
}

export interface CheckInRequest {
  representativeCode?: string;
  representativeEmail?: string;
  date: string;
  inputTime: string;
  checkInLatitude?: number;
  checkInLongitude?: number;
  checkInLocation?: string;
}
