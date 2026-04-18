import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductStockDto, StockDto, StockFilterations } from '../models/IStockVM';
import { ApiResponse, Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class StockService {
apiUrl = environment.apiUrl;
constructor(private http: HttpClient) {}
 getAllStocks(filters: StockFilterations): Observable<ApiResponse<StockDto[]>> {
  let params = new HttpParams();
   if (filters.storeName !== null && filters.storeName !== undefined) {
    params = params.set('storeName', filters.storeName);
  }


    params = params.set('page', filters.page.toString());



    params = params.set('pageSize', filters.pageSize.toString());


    return this.http.get<ApiResponse<StockDto[]>>(`${this.apiUrl}Stock`, { params });
  }


  getStoreStockById(id: number|null): Observable<ApiResponse<StockDto>> {
    return this.http.get<ApiResponse<StockDto>>(`${this.apiUrl}Stock/${id}`);
  }


  getStockByProductId(id: number | null): Observable<Result<ProductStockDto>> {
    return this.http.get<Result<ProductStockDto>>(
      `${this.apiUrl}Stock/product/${id}`
    );
  }
}
