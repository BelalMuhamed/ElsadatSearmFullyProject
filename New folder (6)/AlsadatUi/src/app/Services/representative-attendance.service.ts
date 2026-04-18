import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { RepresentativeAttendanceDto, RepresentativeAttendanceFilteration, CheckInRequest } from '../models/IRepresentativeAttendance';

@Injectable({ providedIn: 'root' })
export class RepresentativeAttendanceService {
  private apiUrl = `${environment.apiUrl}RepresentativeAttendance/`;

  constructor(private http: HttpClient) {}

  // Get all representative attendance with filter
  getByFilter(filters: RepresentativeAttendanceFilteration): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}GetAllRepresentativeAttendance`, {
      params: {
        pageNumber: filters.page.toString(),
        pageSize: filters.pageSize.toString(),
        ...(filters.representativeCode && { representativeCode: filters.representativeCode }),
        ...(filters.representativeId && { representativeId: filters.representativeId }),
        ...(filters.representativeName && { representativeName: filters.representativeName }),
        ...(filters.selectedDate && { selectedDate: filters.selectedDate }),
        ...(filters.startDate && { startDate: filters.startDate.toISOString().split('T')[0] }),
        ...(filters.endDate && { endDate: filters.endDate.toISOString().split('T')[0] }),
        ...(filters.year && { year: filters.year.toString() }),
        ...(filters.month && { month: filters.month.toString() }),
        ...(filters.attendanceStatusID && { attendanceStatusID: filters.attendanceStatusID.toString() })
      }
    });
  }

  // Check in representative
  checkIn(request: CheckInRequest): Observable<any> {
    const body = {
      representativeCode: request.representativeCode,
      representativeEmail: request.representativeEmail,
      date: request.date,
      inputTime: request.inputTime,
      checkInLatitude: request.checkInLatitude,
      checkInLongitude: request.checkInLongitude,
      checkInLocation: request.checkInLocation
    };
    return this.http.post<any>(`${this.apiUrl}RepresentativeCheckIn`, body);
  }

  // Update attendance status
  changeStatus(attendance: RepresentativeAttendanceDto, status: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}UpdateRepresentativeAttendanceStatus?status=${status}`, attendance);
  }
}
