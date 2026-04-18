import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { 
  PayrollDeductionsDto, 
  DeductionDetailDto, 
  EmployeeDeductionsSummaryDto, 
  PayrollDeductionSearchDto,
  ApiResponse,
  PagedResponse
} from '../models/IPayrollDeduction';

@Injectable({ providedIn: 'root' })
export class PayrollDeductionService {
  private apiUrl = environment.apiUrl;
  constructor(private http: HttpClient) {}

  addPayrollDeduction(dto: PayrollDeductionsDto): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}PayrollDeduction/AddPayrollDeduction`, dto);
  }

  updatePayrollDeduction(dto: PayrollDeductionsDto): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.apiUrl}PayrollDeduction/UpdatePayrollDeduction`, dto);
  }

  softDeletePayrollDeduction(id: number): Observable<ApiResponse<string>> {
    const params = new HttpParams().set('id', String(id));
    return this.http.delete<ApiResponse<string>>(`${this.apiUrl}PayrollDeduction/SoftDeletePayrollDeduction`, { params });
  }

  restorePayrollDeduction(id: number): Observable<ApiResponse<string>> {
    const params = new HttpParams().set('id', String(id));
    return this.http.put<ApiResponse<string>>(`${this.apiUrl}PayrollDeduction/RestorePayrollDeduction`, null, { params });
  }

  getPayrollDeductionById(id: number): Observable<ApiResponse<DeductionDetailDto>> {
    const params = new HttpParams().set('id', String(id));
    return this.http.get<ApiResponse<DeductionDetailDto>>(`${this.apiUrl}PayrollDeduction/GetPayrollDeductionById`, { params });
  }

  getAllPayrollDeductions(pageNumber = 1, pageSize = 20): Observable<ApiResponse<PagedResponse<DeductionDetailDto>>> {
    let params = new HttpParams().set('pageNumber', String(pageNumber)).set('pageSize', String(pageSize));
    return this.http.get<ApiResponse<PagedResponse<DeductionDetailDto>>>(`${this.apiUrl}PayrollDeduction/GetAllPayrollDeductions`, { params });
  }

  searchPayrollDeductions(searchDto: PayrollDeductionSearchDto, pageNumber = 1, pageSize = 20): Observable<ApiResponse<PagedResponse<DeductionDetailDto>>> {
    let params = new HttpParams().set('pageNumber', String(pageNumber)).set('pageSize', String(pageSize));
    return this.http.post<ApiResponse<PagedResponse<DeductionDetailDto>>>(`${this.apiUrl}PayrollDeduction/SearchPayrollDeductions`, searchDto, { params });
  }

  getEmployeeDeductionsWithSummary(empCode: string, month?: number, year?: number): Observable<ApiResponse<EmployeeDeductionsSummaryDto>> {
    let params = new HttpParams().set('empCode', empCode || '');
    if (month != null) params = params.set('month', String(month));
    if (year != null) params = params.set('year', String(year));
    return this.http.get<ApiResponse<EmployeeDeductionsSummaryDto>>(`${this.apiUrl}PayrollDeduction/GetEmployeeDeductionsWithSummary`, { params });
  }
}
