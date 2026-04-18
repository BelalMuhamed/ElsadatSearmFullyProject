import { PurchaseInvoiceFilteration } from './../models/IPurchaseInvoiceVMs';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PurchaseInvoiceDtos } from '../models/IPurchaseInvoiceVMs';
import { Observable } from 'rxjs';
import { ApiResponse, Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class PurchaseInvoiceService {

  private apiUrl = environment.apiUrl + 'PurchaseInvoice/';

    constructor(private http: HttpClient ) {}


  addPurchaseInvoice(dto: PurchaseInvoiceDtos): Observable<Result<string>> {
    return this.http.post<any>(this.apiUrl, dto);
  }
  GetAllPurchaseInvoices(filters: PurchaseInvoiceFilteration): Observable<ApiResponse<PurchaseInvoiceDtos[]>> {
    let params = new HttpParams();

     if (filters.invoiceNumber !== null && filters.invoiceNumber !== undefined) {
       params = params.set('invoiceNumber', filters.invoiceNumber);
     }

     if (filters.supplierId !== null && filters.supplierId !== undefined) {
       params = params.set('supplierId', filters.supplierId.toString());
     }

     if (filters.settledStatus !== null && filters.settledStatus !== undefined) {
       params = params.set('settledStatus', filters.settledStatus.toString());
     }

     if (filters.deleteStatus !== null && filters.deleteStatus !== undefined) {
       params = params.set('deleteStatus', filters.deleteStatus.toString());
     }

     if (filters.page !== null && filters.page !== undefined) {
       params = params.set('page', filters.page.toString());
     }

     if (filters.pageSize !== null && filters.pageSize !== undefined) {
       params = params.set('pageSize', filters.pageSize.toString());
     }

     return this.http.get<ApiResponse<PurchaseInvoiceDtos[]>>(
       `${this.apiUrl}`,
       { params }
     );
  }
   // ================= Get By Id =================
  getPurchaseInvoiceById(id: number): Observable<Result<PurchaseInvoiceDtos>> {
    return this.http.get<Result<PurchaseInvoiceDtos>>(`${this.apiUrl}${id}`);
  }
  // ================= Edit =================
  editPurchaseInvoice(
    id: number,
    dto: PurchaseInvoiceDtos
  ): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.apiUrl}${id}`, dto);
  }
  // ================= Delete =================
deletePurchaseInvoice(id: number): Observable<Result<string>> {
  return this.http.delete<Result<string>>(`${this.apiUrl}${id}`);
}
downloadInvoicePdf(id: number, type: 'full' | 'simple'): Observable<Blob> {
  return this.http.get(`${this.apiUrl}${id}/pdf/${type}`, {
    responseType: 'blob'
  });
}

}
