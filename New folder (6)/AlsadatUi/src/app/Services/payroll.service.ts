import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import {
  GeneratePayrollRequestDto,
  GenerateBulkPayrollRequestDto,
  PayrollFilterDto,
  PayrollResponseDto,
  BulkPayrollResultDto,
  PayrollSummaryDto,
  PayrollExportDto,
  MarkPayrollPaidDto,
  PayrollPreviewDto,
  PreviewBulkPayrollDto
} from '../models/IPayroll';
import { Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class PayrollService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // الحصول على معاينة كشف الراتب (قبل التأكيد)
  previewPayroll(request: GeneratePayrollRequestDto): Observable<Result<PayrollPreviewDto>> {
    return this.http.get<Result<PayrollPreviewDto>>(
      `${this.apiUrl}Payroll/PreviewPayroll`,
      { params: this.payrollRequestToParams(request) }
    );
  }

  // الحصول على معاينة كشوف رواتب جماعي (قبل التأكيد)
  previewBulkPayroll(request: GenerateBulkPayrollRequestDto): Observable<Result<PreviewBulkPayrollDto>> {
    return this.http.get<Result<PreviewBulkPayrollDto>>(
      `${this.apiUrl}Payroll/PreviewBulkPayroll`,
      { params: this.bulkPayrollRequestToParams(request) }
    );
  }

  // إنشاء كشف راتب فردي
  generatePayroll(request: GeneratePayrollRequestDto): Observable<Result<PayrollResponseDto>> {
    return this.http.post<Result<PayrollResponseDto>>(
      `${this.apiUrl}Payroll/GeneratePayroll`,
      request
    );
  }

  // إنشاء كشوف رواتب جماعي
  generateBulkPayroll(request: GenerateBulkPayrollRequestDto): Observable<Result<BulkPayrollResultDto>> {
    return this.http.post<Result<BulkPayrollResultDto>>(
      `${this.apiUrl}Payroll/GenerateBulkPayroll`,
      request
    );
  }

  // تسجيل كشف راتب فردي في المحاسبة
  postToAccounting(payrollId: number, confirmLoans: boolean = false): Observable<Result<string>> {
    let httpParams = new HttpParams()
      .set('payrollId', String(payrollId))
      .set('confirmLoans', String(confirmLoans));
    return this.http.post<Result<string>>(
      `${this.apiUrl}Payroll/PostToAccounting`,
      {}, // Send empty object instead of null
      { params: httpParams }
    );
  }

  // تسجيل جماعي في المحاسبة
  postBulkToAccounting(payrollIds: number[], confirmLoans: boolean = false): Observable<Result<string>> {
    let httpParams = new HttpParams().set('confirmLoans', String(confirmLoans));
    return this.http.post<Result<string>>(
      `${this.apiUrl}Payroll/PostBulkToAccounting`,
      payrollIds,
      { params: httpParams }
    );
  }

  // تعيين كشف راتب كمدفوع (فردي)
  markAsPaid(payrollId: number, paymentMethod: string, paymentReference?: string): Observable<Result<string>> {
    let httpParams = new HttpParams()
      .set('payrollId', String(payrollId))
      .set('paymentMethod', paymentMethod);
    
    if (paymentReference) {
      httpParams = httpParams.set('paymentReference', paymentReference);
    }

    return this.http.put<Result<string>>(
      `${this.apiUrl}Payroll/MarkAsPaid`,
      null,
      { params: httpParams }
    );
  }

  // تعيين جماعي كمدفوع
  markBulkAsPaid(dto: MarkPayrollPaidDto): Observable<Result<string>> {
    return this.http.put<Result<string>>(
      `${this.apiUrl}Payroll/MarkBulkAsPaid`,
      dto
    );
  }

  // حذف كشف راتب
  deletePayroll(payrollId: number): Observable<Result<string>> {
    let httpParams = new HttpParams().set('PayrollID', String(payrollId));
    return this.http.delete<Result<string>>(
      `${this.apiUrl}Payroll/DeletePayroll`,
      { params: httpParams }
    );
  }

  // الحصول على كشوف الرواتب مع الفلترة
  getPayrolls(filter: PayrollFilterDto): Observable<Result<PayrollResponseDto[]>> {
    let httpParams = new HttpParams();
    
    if (filter.month != null) {
      httpParams = httpParams.set('month', String(filter.month));
    }
    if (filter.year != null) {
      httpParams = httpParams.set('year', String(filter.year));
    }
    if (filter.status) {
      httpParams = httpParams.set('status', filter.status);
    }
    if (filter.fromDate) {
      httpParams = httpParams.set('fromDate', new Date(filter.fromDate).toISOString());
    }
    if (filter.toDate) {
      httpParams = httpParams.set('toDate', new Date(filter.toDate).toISOString());
    }
    if (filter.minNetSalary != null) {
      httpParams = httpParams.set('minNetSalary', String(filter.minNetSalary));
    }
    if (filter.maxNetSalary != null) {
      httpParams = httpParams.set('maxNetSalary', String(filter.maxNetSalary));
    }
    if (filter.departments && filter.departments.length > 0) {
      filter.departments.forEach(dept => {
        httpParams = httpParams.append('departments', dept);
      });
    }

    return this.http.get<Result<PayrollResponseDto[]>>(
      `${this.apiUrl}Payroll/GetPayrolls`,
      { params: httpParams }
    );
  }

  // تصدير كشوف الرواتب إلى Excel
  exportToExcel(filter: PayrollFilterDto): Observable<Blob> {
    let httpParams = new HttpParams();
    
    if (filter.month != null) {
      httpParams = httpParams.set('month', String(filter.month));
    }
    if (filter.year != null) {
      httpParams = httpParams.set('year', String(filter.year));
    }
    if (filter.status) {
      httpParams = httpParams.set('status', filter.status);
    }
    if (filter.departments && filter.departments.length > 0) {
      filter.departments.forEach(dept => {
        httpParams = httpParams.append('departments', dept);
      });
    }

    return this.http.get(
      `${this.apiUrl}Payroll/ExportToExcel`,
      { params: httpParams, responseType: 'blob' }
    );
  }

  // الحصول على ملخص الرواتب
  getSummary(month: number, year: number): Observable<Result<PayrollSummaryDto>> {
    let httpParams = new HttpParams()
      .set('month', String(month))
      .set('year', String(year));

    return this.http.get<Result<PayrollSummaryDto>>(
      `${this.apiUrl}Payroll/GetSummary`,
      { params: httpParams }
    );
  }

  // الحصول على كشف راتب محدد
  getPayrollById(id: number): Observable<Result<PayrollResponseDto>> {
    let httpParams = new HttpParams().set('id', String(id));
    
    return this.http.get<Result<PayrollResponseDto>>(
      `${this.apiUrl}Payroll/GetPayrollById`,
      { params: httpParams }
    );
  }

  // الحصول على كشوف رواتب موظف محدد
  getEmployeePayrolls(employeeCode: string, year?: number): Observable<Result<PayrollResponseDto[]>> {
    let httpParams = new HttpParams().set('employeeCode', employeeCode);
    
    if (year != null) {
      httpParams = httpParams.set('year', String(year));
    }

    return this.http.get<Result<PayrollResponseDto[]>>(
      `${this.apiUrl}Payroll/GetEmployeePayrolls`,
      { params: httpParams }
    );
  }

  // Helper method to convert PayrollRequest to HttpParams
  private payrollRequestToParams(request: GeneratePayrollRequestDto): HttpParams {
    let params = new HttpParams()
      .set('month', String(request.month))
      .set('year', String(request.year));
    // optional employee or representative
    if (request.employeeCode) {
      params = params.set('employeeCode', request.employeeCode!);
    }
    if ((request as any).representativeCode) {
      params = params.set('representativeCode', (request as any).representativeCode);
    }
    if (request.payLoansFromSalary !== undefined) {
      params = params.set('payLoansFromSalary', String(request.payLoansFromSalary));
    }
    if (request.paymentMethodForLoans) {
      params = params.set('paymentMethodForLoans', request.paymentMethodForLoans);
    }

    return params;
  }

  // Helper method to convert BulkPayrollRequest to HttpParams
  private bulkPayrollRequestToParams(request: GenerateBulkPayrollRequestDto): HttpParams {
    let params = new HttpParams()
      .set('month', String(request.month))
      .set('year', String(request.year))
      .set('includeActiveOnly', String(request.includeActiveOnly ?? true));

    // Support userCodes (mix of employees and representatives)
    if ((request as any).userCodes && (request as any).userCodes.length > 0) {
      (request as any).userCodes.forEach((code: string) => {
        params = params.append('userCodes', code);
      });
    }

    if (request.employeeCodes && request.employeeCodes.length > 0) {
      request.employeeCodes.forEach(code => {
        params = params.append('employeeCodes', code);
      });
    }

    if (request.autoPostToAccounting !== undefined) {
      params = params.set('autoPostToAccounting', String(request.autoPostToAccounting));
    }
    if (request.payLoansFromSalary !== undefined) {
      params = params.set('payLoansFromSalary', String(request.payLoansFromSalary));
    }
    if (request.confirmLoans !== undefined) {
      params = params.set('confirmLoans', String(request.confirmLoans));
    }
    if (request.paymentMethod) {
      params = params.set('paymentMethod', request.paymentMethod);
    }

    return params;
  }
}

