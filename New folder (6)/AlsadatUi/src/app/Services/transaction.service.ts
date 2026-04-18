import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { StoreTransactionDto, StoreTransactionFilters, StoreTransactionProductsDto } from '../models/ITransactionVM';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {

 apiUrl = environment.apiUrl;
  constructor(private http: HttpClient) {}
  getAllTransaction(filters: StoreTransactionFilters | null): Observable<ApiResponse<StoreTransactionDto[]>> {
    let params = new HttpParams();

    if (filters != null) {
if (filters.createdAt) {
  const dateOnly = new Date(filters.createdAt);
  const formatted = dateOnly.toISOString().substring(0, 10); // YYYY-MM-DD
  params = params.set('createdAt', formatted);
}
      if (filters.sourceName)
        params = params.set('sourceName', filters.sourceName);

      if (filters.destenationName)
        params = params.set('destenationName', filters.destenationName);



      if (filters.page)
        params = params.set('page', filters.page);

      if (filters.pageSize)
        params = params.set('pageSize', filters.pageSize);
    }

    return this.http.get<ApiResponse<StoreTransactionDto[]>>(
      `${this.apiUrl}Transaction`,
      { params }
    );
  }
 getTransactionProducts(transactionId: number): Observable<StoreTransactionProductsDto[]> {
    return this.http.get<StoreTransactionProductsDto[]>(`${this.apiUrl}Transaction/${transactionId}/products`);
  }
  addNewTransaction(
  transaction: StoreTransactionDto
): Observable<{ message: string }> {

  return this.http.post<{ message: string }>(
    `${this.apiUrl}Transaction`,
    transaction
  );
}

}
