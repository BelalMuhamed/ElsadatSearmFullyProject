import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Result } from '../models/ApiReponse';
import {
  AgingReport, AgingReportReq, CashReport, CashReportReq, DateRangeReq,
  IncomeStatement, InventoryMovement, InventoryMovementReq,
  PartyBalancesReport, TrialBalance
} from '../models/Reports';

@Injectable({ providedIn: 'root' })
export class FinancialReportsService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}FinancialReports`;

  getCashReport(req: CashReportReq): Observable<Result<CashReport>> {
    return this.http.get<Result<CashReport>>(`${this.base}/cash`, { params: this.toParams(req) });
  }
  getCustomerBalances(req: DateRangeReq): Observable<Result<PartyBalancesReport>> {
    return this.http.get<Result<PartyBalancesReport>>(`${this.base}/customers/balances`, { params: this.toParams(req) });
  }
  getSupplierBalances(req: DateRangeReq): Observable<Result<PartyBalancesReport>> {
    return this.http.get<Result<PartyBalancesReport>>(`${this.base}/suppliers/balances`, { params: this.toParams(req) });
  }
  getReceivablesAging(req: AgingReportReq): Observable<Result<AgingReport>> {
    return this.http.get<Result<AgingReport>>(`${this.base}/customers/aging`, { params: this.toParams(req) });
  }
  getPayablesAging(req: AgingReportReq): Observable<Result<AgingReport>> {
    return this.http.get<Result<AgingReport>>(`${this.base}/suppliers/aging`, { params: this.toParams(req) });
  }
  getInventoryMovement(req: InventoryMovementReq): Observable<Result<InventoryMovement>> {
    return this.http.get<Result<InventoryMovement>>(`${this.base}/inventory/movement`, { params: this.toParams(req) });
  }
  getTrialBalance(req: DateRangeReq): Observable<Result<TrialBalance>> {
    return this.http.get<Result<TrialBalance>>(`${this.base}/trial-balance`, { params: this.toParams(req) });
  }
  getIncomeStatement(req: DateRangeReq): Observable<Result<IncomeStatement>> {
    return this.http.get<Result<IncomeStatement>>(`${this.base}/income-statement`, { params: this.toParams(req) });
  }

  private toParams(req: object): HttpParams {
    let params = new HttpParams();
    Object.entries(req).forEach(([k, v]) => {
      if (v !== null && v !== undefined && v !== '') params = params.append(k, String(v));
    });
    return params;
  }
}
