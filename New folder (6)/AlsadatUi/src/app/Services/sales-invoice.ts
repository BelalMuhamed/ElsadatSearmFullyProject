import { InvoiceChangeStatusReq, SalesInvoiceDetails, SalesInvoiceFilters, salesInvoiceItemsDetails, SalesInvoiceItemsResp } from './../models/IsalesInvoice';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {  SalesInvoicesResponse } from '../models/IsalesInvoice';
import { ApiResponse, Result } from '../models/ApiReponse';
import { ProductDto } from '../models/IProductVM';
import { FormArray } from '@angular/forms';
import { invoiceProductsStock } from '../models/IStockVM';

@Injectable({
  providedIn: 'root'
})
export class SalesInvoice {
   apiUrl = environment.apiUrl;
 constructor(private http: HttpClient) {}

 getAllSalesInvoices(filters: SalesInvoiceFilters | null): Observable<Result<ApiResponse<SalesInvoicesResponse[]>>> {
  let params = new HttpParams();

  if (filters != null) {

    if (filters.createAt != null) {
      const d = new Date(filters.createAt);
d.setHours(12, 0, 0, 0); // منتصف اليوم
params = params.set('createAt', d.toISOString());
    }
    if (filters.customerId)
      params = params.set('customerId', filters.customerId);

      if (filters.invoiceNumber)
      params = params.set('invoiceNumber', filters.invoiceNumber);

      if (filters.deleteStatus !== null && filters.deleteStatus !== undefined)
      params = params.set('deleteStatus', filters.deleteStatus);



    if (filters.page)
      params = params.set('page', filters.page);

    if (filters.pageSize)
      params = params.set('pageSize', filters.pageSize);
  }

  return this.http.get<Result<ApiResponse<SalesInvoicesResponse[]>>>(
    `${this.apiUrl}SalesInvoices`,
    { params }
  );
}
GetSalesInvoiceItems(id:number):Observable<ApiResponse<SalesInvoiceItemsResp[]>>
{
return this.http.get<ApiResponse<SalesInvoiceItemsResp[]>>(`${this.apiUrl}SalesInvoiceItems/get-sales-invoice-items?id=${id}`)
}

 GetSalesInvoiceById(id: number): Observable<Result<SalesInvoicesResponse>> {
    return this.http.get<Result<SalesInvoicesResponse>>(`${this.apiUrl}SalesInvoices/${id}`);
  }

 addInvoice(dto: SalesInvoicesResponse): Observable<Result<string>> {
    const url = `${this.apiUrl}SalesInvoices`;
    return this.http.post<Result<string>>(url, dto);
  }

    // ================= Edit =================
    updateInvoice(
      id: number,
      dto: SalesInvoicesResponse
    ): Observable<Result<string>> {
      return this.http.put<Result<string>>(`${this.apiUrl}SalesInvoices/${id}`, dto);
    }



  changeInvoiceStatus(req: InvoiceChangeStatusReq): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.apiUrl}SalesInvoices/change-status`, req);
  }


  deleteInvoice(id: number): Observable<Result<string>> {
    const url = `${this.apiUrl}SalesInvoices/${id}`;
    return this.http.delete<Result<string>>(url);
  }

  confirmInvoice(

  dto: invoiceProductsStock
): Observable<Result<string>> {
  return this.http.post<Result<string>>(
    `${this.apiUrl}SalesInvoices/confirm`,
    dto
  );
}

GetInvoiceDetails(id:number):Observable<Result<SalesInvoiceDetails>>
{
 return this.http.get<Result<SalesInvoiceDetails>>(`${this.apiUrl}SalesInvoices/${id}/details`);
}
askToReverseInvoice(id: number): Observable<Result<string>> {
  const url = `${this.apiUrl}SalesInvoices/${id}/ask-to-reverse`;
  return this.http.patch<Result<string>>(url, {});
}
RefusedReverseInvoice(id: number): Observable<Result<string>> {
  const url = `${this.apiUrl}SalesInvoices/${id}/refused-reverse`;
  return this.http.patch<Result<string>>(url, {});
}
reverseInvoice(id: number): Observable<Result<string>> {
  return this.http.post<Result<string>>(
    `${this.apiUrl}SalesInvoices/${id}/reverse`,
    {}
  );
}
}
