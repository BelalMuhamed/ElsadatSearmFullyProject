import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import {
  EmployeeLeaveRequestDto,
  CreateLeaveRequestDto,
  LeaveRequestFilterDto,
  ApproveRejectLeaveDto
} from '../models/leave/employee-leave-request.model';
import {
  LeaveBalanceSummaryDto,
  EmployeeLeaveBalanceDto,
  BulkLeaveBalanceRequestDto,
  BulkLeaveBalanceResultDto
} from '../models/leave/leave-balance.model';
import { PaginationParams, PagedList } from '../models/IEmployee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeLeaveService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getMyLeaveRequests(params: PaginationParams = {}, employeeEmail?: string): Observable<PagedList<EmployeeLeaveRequestDto>> {
    let httpParams = new HttpParams();
    if (params.pageNumber != null) httpParams = httpParams.set('pageNumber', String(params.pageNumber));
    if (params.pageSize != null) httpParams = httpParams.set('pageSize', String(params.pageSize));
    if (employeeEmail) httpParams = httpParams.set('employeeEmail', employeeEmail);
    return this.http.get<PagedList<EmployeeLeaveRequestDto>>(
      `${this.apiUrl}EmployeeLeave/GetMyLeaveRequests`,
      { params: httpParams }
    );
  }

  getLeaveRequestById(id: number): Observable<EmployeeLeaveRequestDto> {
    const params = new HttpParams().set('id', String(id));
    return this.http.get<EmployeeLeaveRequestDto>(`${this.apiUrl}EmployeeLeave/GetLeaveRequestById`, { params });
  }

  createLeaveRequest(dto: CreateLeaveRequestDto): Observable<any> {
    return this.http.post(`${this.apiUrl}EmployeeLeave/CreateLeaveRequest`, dto);
  }

  cancelLeaveRequest(id: number, employeeCode: string): Observable<any> {
    let params = new HttpParams().set('id', String(id));
    if (employeeCode) params = params.set('employeeCode', employeeCode);
    return this.http.put(`${this.apiUrl}EmployeeLeave/CancelLeaveRequest`, null, { params, responseType: 'text' as 'json' });
  }

  searchLeaveRequests(filter: LeaveRequestFilterDto): Observable<PagedList<EmployeeLeaveRequestDto>> {
    let params = new HttpParams();
    if (filter.pageNumber != null) params = params.set('pageNumber', String(filter.pageNumber));
    if (filter.pageSize != null) params = params.set('pageSize', String(filter.pageSize));
    if (filter.employeeCode) params = params.set('employeeCode', filter.employeeCode);
    if ((filter as any).representativeCode) params = params.set('representativeCode', (filter as any).representativeCode);
    if (filter.leaveTypeId != null) params = params.set('leaveTypeId', String(filter.leaveTypeId));
    if (filter.status != null) params = params.set('status', String(filter.status));
    if (filter.fromDate) params = params.set('fromDate', String(filter.fromDate));
    if (filter.toDate) params = params.set('toDate', String(filter.toDate));
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);
    if (filter.sortDescending != null) params = params.set('sortDescending', String(filter.sortDescending));

    return this.http.get<PagedList<EmployeeLeaveRequestDto>>(`${this.apiUrl}EmployeeLeave/SearchLeaveRequests`, { params });
  }

  approveLeaveRequest(id: number, dto: ApproveRejectLeaveDto, employeeCode: string): Observable<any> {
    let params = new HttpParams().set('id', String(id));
    if (employeeCode) params = params.set('employeeCode', employeeCode);
    return this.http.put(`${this.apiUrl}EmployeeLeave/ApproveLeaveRequest`, dto, { params, responseType: 'text' as 'json' });
  }

  rejectLeaveRequest(id: number, dto: ApproveRejectLeaveDto, employeeCode: string): Observable<any> {
    let params = new HttpParams().set('id', String(id));
    if (employeeCode) params = params.set('employeeCode', employeeCode);
    return this.http.put(`${this.apiUrl}EmployeeLeave/RejectLeaveRequest`, dto, { params, responseType: 'text' as 'json' });
  }

  getPendingRequests(): Observable<EmployeeLeaveRequestDto[]> {
    return this.http.get<EmployeeLeaveRequestDto[]>(`${this.apiUrl}EmployeeLeave/GetPendingRequests`);
  }

  bulkApproveRequests(requestIds: number[], employeeCode: string): Observable<any> {
    let params = new HttpParams();
    if (employeeCode) params = params.set('employeeCode', employeeCode);
    return this.http.post(`${this.apiUrl}EmployeeLeave/BulkApproveRequests`, requestIds, { params, responseType: 'text' as 'json' });
  }

  getMyLeaveBalance(employeeCode: string, year?: number): Observable<LeaveBalanceSummaryDto> {
    let params = new HttpParams().set('employeeCode', employeeCode || '');
    if (year != null) params = params.set('year', String(year));
    return this.http.get<LeaveBalanceSummaryDto>(`${this.apiUrl}EmployeeLeave/GetMyLeaveBalance`, { params });
  }

  initializeLeaveBalance(request: { employeeCode: string; year: number }) {
    return this.http.post(`${this.apiUrl}EmployeeLeave/InitializeLeaveBalance`, request, { responseType: 'text' as 'json' });
  }

  createMultipleLeaveBalances(request: BulkLeaveBalanceRequestDto) {
    return this.http.post<BulkLeaveBalanceResultDto>(`${this.apiUrl}EmployeeLeave/CreateMultipleLeaveBalances`, request);
  }

  updateLeaveBalance(dto: EmployeeLeaveBalanceDto) {
    return this.http.put(`${this.apiUrl}EmployeeLeave/UpdateLeaveBalance`, dto, { responseType: 'text' as 'json' });
  }

  getLeaveBalanceByType(employeeCode: string, leaveTypeId: number, year?: number) {
    let params = new HttpParams().set('employeeCode', employeeCode || '');
    params = params.set('leaveTypeId', String(leaveTypeId));
    if (year != null) params = params.set('year', String(year));
    return this.http.get<EmployeeLeaveBalanceDto>(`${this.apiUrl}EmployeeLeave/GetLeaveBalanceByType`, { params });
  }

  getEmployeeLeaveBalance(employeeName: string, year?: number) {
    let params = new HttpParams().set('employeeName', employeeName || '');
    if (year != null) params = params.set('year', String(year));
    return this.http.get<LeaveBalanceSummaryDto>(`${this.apiUrl}EmployeeLeave/GetEmplyeeLeaveBalance`, { params });
  }

  setCustomLeaveBalance(employeeCode: string, leaveTypeId: number, opening: number) {
    const payload = { EmployeeCode: employeeCode, LeaveTypeId: leaveTypeId, OpeningBalance: opening };
    return this.http.post(`${this.apiUrl}EmployeeLeave/SetCustomLeaveBalance`, payload, { responseType: 'text' as 'json' });
  }
}
