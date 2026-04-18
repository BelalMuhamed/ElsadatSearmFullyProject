import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { 
  EmployeeDTo, 
  EmployeeHelper, 
  EmployeeSalaryDTo,
  MonthlySalarySummaryDto,
  MonthlyStatisticsDto,
  SalaryComparisonDto,
  SalaryHistoryDto,
  PaginationParams, 
  PagedList 
} from '../models/IEmployee';
import { ApiResponse, Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Get all employees with pagination
  getAllEmployees(params: PaginationParams): Observable<PagedList<EmployeeDTo>> {
    let httpParams = new HttpParams();
    if (params.pageNumber != null) {
      httpParams = httpParams.set('pageNumber', String(params.pageNumber));
    }
    if (params.pageSize != null) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }
    return this.http.get<PagedList<EmployeeDTo>>(
      `${this.apiUrl}Employee/GetAllEmployees`,
      { params: httpParams }
    );
  }

  // Get all active employees with pagination
  getAllActiveEmployees(params: PaginationParams): Observable<PagedList<EmployeeDTo>> {
    let httpParams = new HttpParams();
    if (params.pageNumber != null) {
      httpParams = httpParams.set('pageNumber', String(params.pageNumber));
    }
    if (params.pageSize != null) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }
    return this.http.get<PagedList<EmployeeDTo>>(
      `${this.apiUrl}Employee/GetAllActiveEmployee`,
      { params: httpParams }
    );
  }

  // Get employees by filter
  getEmployeesByFilter(
    params: PaginationParams,
    filter: EmployeeHelper
  ): Observable<PagedList<EmployeeDTo>> {
    let httpParams = new HttpParams();
    if (params.pageNumber != null) {
      httpParams = httpParams.set('pageNumber', String(params.pageNumber));
    }
    if (params.pageSize != null) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }
    if (filter.empCode) {
      httpParams = httpParams.set('empCode', filter.empCode);
    }
    if (filter.departmentName) {
      httpParams = httpParams.set('departmentName', filter.departmentName);
    }
    if (filter.employeeName) {
      httpParams = httpParams.set('employeeName', filter.employeeName);
    }
    if (filter.cityName) {
      httpParams = httpParams.set('cityName', filter.cityName);
    }
    return this.http.get<PagedList<EmployeeDTo>>(
      `${this.apiUrl}Employee/GetEmployeeByFilterAsync`,
      { params: httpParams }
    );
  }

  // Get employee salary for a specific year and month
  getEmployeeSalary(
    empCode: string,
    month?: number,
    year?: number
  ): Observable<Result<EmployeeSalaryDTo>> {
    // backend expects query names: EmpCode, Month, Year
    let httpParams = new HttpParams().set('EmpCode', empCode);
    if (month != null) {
      httpParams = httpParams.set('Month', String(month));
    }
    if (year != null) {
      httpParams = httpParams.set('Year', String(year));
    }

    return this.http.get<Result<EmployeeSalaryDTo>>(
      `${this.apiUrl}Employee/GetEmployeeSalaryByYearAndMonth`,
      { params: httpParams }
    );
  }

  // Add new employee
  addEmployee(dto: EmployeeDTo): Observable<any> {
    // format DTO similarly to update: ensure DateOnly strings and SNO alias; backend returns plain text
    const body: any = { ...dto };

    function toDateOnlyString(d: any) {
      if (!d) return null;
      if (typeof d === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(d)) return d;
      const dt = new Date(d);
      if (!isNaN(dt.getTime())) {
        const yyyy = dt.getFullYear();
        const mm = String(dt.getMonth() + 1).padStart(2, '0');
        const dd = String(dt.getDate()).padStart(2, '0');
        return `${yyyy}-${mm}-${dd}`;
      }
      return null;
    }

    if (body.birthDate) body.birthDate = toDateOnlyString(body.birthDate);
    if (body.hireDate) body.hireDate = toDateOnlyString(body.hireDate);
    try { if (body.sno && !body.SNO) body.SNO = body.sno; } catch (e) { /* ignore */ }
    try { console.debug('[EmployeeService] addEmployee payload:', JSON.stringify(body)); } catch (e) { /* ignore */ }

    return this.http.post(
      `${this.apiUrl}Employee/AddNewEmployee`,
      body,
      { responseType: 'text' }
    );
  }

  // Update employee
  updateEmployee(dto: EmployeeDTo): Observable<any> {
    // Backend expects the EmployeeDTo at the request root (not wrapped).
    // Prepare a shallow copy of the dto, format DateOnly fields and mirror SNO casing.
    const body: any = { ...dto };

    function toDateOnlyString(d: any) {
      if (!d) return null;
      if (typeof d === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(d)) return d;
      const dt = new Date(d);
      if (!isNaN(dt.getTime())) {
        const yyyy = dt.getFullYear();
        const mm = String(dt.getMonth() + 1).padStart(2, '0');
        const dd = String(dt.getDate()).padStart(2, '0');
        return `${yyyy}-${mm}-${dd}`;
      }
      return null;
    }

    // format date fields to DateOnly strings on the root object
    if (body.birthDate) body.birthDate = toDateOnlyString(body.birthDate);
    if (body.hireDate) body.hireDate = toDateOnlyString(body.hireDate);

    // Mirror SNO uppercase if present (so JSON includes `SNO` key)
    try {
      if (body.sno && !body.SNO) body.SNO = body.sno;
    } catch (e) { /* ignore */ }

    // debug payload to help diagnose missing/incorrect fields (visible in console)
    try { console.debug('[EmployeeService] updateEmployee payload:', JSON.stringify(body)); } catch (e) { /* ignore */ }

    return this.http.put(
      `${this.apiUrl}Employee/UpdateEmployee`,
      body,
      { responseType: 'text' }
    );
  }

  // Soft delete employee
  softDeleteEmployee(dto: EmployeeDTo): Observable<any> {
    return this.http.put(
      `${this.apiUrl}Employee/SoftDeleteEmployee`,
      dto,
      { responseType: 'text' }
    );
  }

  // Restore employee
  restoreEmployee(dto: EmployeeDTo): Observable<any> {
    return this.http.put(
      `${this.apiUrl}Employee/RestoreEmployee`,
      dto,
      { responseType: 'text' }
    );
  }

  // Try to fetch available roles from backend. If endpoint doesn't exist or fails, caller can fallback.
  getRoles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}Role/GetAllRoles`).pipe(
      // don't transform here; let subscribers handle errors/fallback
    );
  }

  // Get monthly salary summary (Financial and Performance Analysis)
  getMonthlySalarySummary(
    empCode: string,
    month?: number,
    year?: number
  ): Observable<Result<MonthlySalarySummaryDto>> {
    let httpParams = new HttpParams().set('empCode', empCode);
    if (month != null) {
      httpParams = httpParams.set('month', String(month));
    }
    if (year != null) {
      httpParams = httpParams.set('year', String(year));
    }
    return this.http.get<Result<MonthlySalarySummaryDto>>(
      `${this.apiUrl}Employee/GetMonthlySalarySummary`,
      { params: httpParams }
    );
  }

  // Get monthly statistics (Salary, Attendance, Leave, Overtime stats)
  getMonthlyStatistics(
    empCode: string,
    month?: number,
    year?: number
  ): Observable<Result<MonthlyStatisticsDto>> {
    let httpParams = new HttpParams().set('empCode', empCode);
    if (month != null) {
      httpParams = httpParams.set('month', String(month));
    }
    if (year != null) {
      httpParams = httpParams.set('year', String(year));
    }
    return this.http.get<Result<MonthlyStatisticsDto>>(
      `${this.apiUrl}Employee/GetMonthlyStatistics`,
      { params: httpParams }
    );
  }

  // Compare salaries between two months
  compareMonthlySalaries(
    empCode: string,
    baseMonth: number,
    baseYear: number,
    compareMonth: number,
    compareYear: number
  ): Observable<Result<SalaryComparisonDto>> {
    let httpParams = new HttpParams()
      .set('empCode', empCode)
      .set('baseMonth', String(baseMonth))
      .set('baseYear', String(baseYear))
      .set('compareMonth', String(compareMonth))
      .set('compareYear', String(compareYear));

    return this.http.get<Result<SalaryComparisonDto>>(
      `${this.apiUrl}Employee/CompareMonthlySalaries`,
      { params: httpParams }
    );
  }

  // Get salary history for a specific year
  getSalaryHistory(
    empCode: string,
    year?: number
  ): Observable<Result<SalaryHistoryDto>> {
    let httpParams = new HttpParams().set('empCode', empCode);
    if (year != null) {
      httpParams = httpParams.set('year', String(year));
    }
    return this.http.get<Result<SalaryHistoryDto>>(
      `${this.apiUrl}Employee/GetSalaryHistory`,
      { params: httpParams }
    );
  }
}
