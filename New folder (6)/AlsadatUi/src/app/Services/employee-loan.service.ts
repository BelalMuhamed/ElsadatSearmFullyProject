import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({
  providedIn: 'root'
})
export class EmployeeLoanService {
  private apiUrl = environment.apiUrl + 'EmployeeLoan/';

  constructor(private http: HttpClient) {}

  create(dto: any): Observable<any> {
    return this.http.post<any>(this.apiUrl + 'CreateLoan', dto);
  }

  approve(dto: any): Observable<any> {
    return this.http.post<any>(this.apiUrl + 'ApproveLoan', dto);
  }

  reject(dto: any): Observable<any> {
    return this.http.post<any>(this.apiUrl + 'RejectLoan', dto);
  }

  makePayment(dto: any): Observable<any> {
    return this.http.post<any>(this.apiUrl + 'MakePayment', dto);
  }

  getEmployeeLoans(employeeCode: string, pageNumber: number = 1, pageSize: number = 10): Observable<any> {
    const params = new HttpParams()
      .set('employeeCode', employeeCode)
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<any>(this.apiUrl + 'GetEmployeeLoans', { params });
  }

  getAllLoans(pageNumber: number = 1, pageSize: number = 10, filter?: any): Observable<any> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (filter) {
      if (filter.employeeCode) params = params.set('employeeCode', filter.employeeCode);
      if (filter.employeeName) params = params.set('employeeName', filter.employeeName);
      // Important: check for zero explicitly since 0 is falsy in JavaScript
      if (filter.status !== undefined && filter.status !== null) params = params.set('status', filter.status);
      if (filter.fromDate) params = params.set('fromDate', filter.fromDate);
      if (filter.toDate) params = params.set('toDate', filter.toDate);
    }
    return this.http.get<any>(this.apiUrl + 'GetAllLoans', { params });
  }

  getById(id: number): Observable<any> {
    const params = new HttpParams().set('id', id);
    return this.http.get<any>(this.apiUrl + 'GetLoanById', { params });
  }

  getLoanByNumber(loanNumber: string): Observable<any> {
    const params = new HttpParams().set('loanNumber', loanNumber);
    return this.http.get<any>(this.apiUrl + 'GetLoanByNumber', { params });
  }

  getLoanPayments(loanId: number): Observable<any> {
    const params = new HttpParams().set('loanId', loanId);
    return this.http.get<any>(this.apiUrl + 'GetLoanPayments', { params });
  }

  getEmployeeLoanSummary(employeeCode: string): Observable<any> {
    const params = new HttpParams().set('employeeCode', employeeCode);
    return this.http.get<any>(this.apiUrl + 'GetEmployeeLoanSummary', { params });
  }

  calculateMonthlyDeduction(employeeCode: string, month?: string): Observable<any> {
    let params = new HttpParams().set('employeeCode', employeeCode);
    if (month) params = params.set('month', month);
    return this.http.get<any>(this.apiUrl + 'CalculateMonthlyDeduction', { params });
  }
}
