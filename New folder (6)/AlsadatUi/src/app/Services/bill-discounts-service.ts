import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BillDiscount } from '../models/IBillDiscount';

@Injectable({
  providedIn: 'root'
})
export class BillDiscountsService {
  apiUrl = environment.apiUrl;
 constructor(private http: HttpClient) {}
   getBillDiscounts(): Observable<BillDiscount> {
    return this.http.get<BillDiscount>(`${this.apiUrl}GeneralSetting`);
  }

   updateBillDiscount(req: BillDiscount): Observable<any> {
    return this.http.put(`${this.apiUrl}GeneralSetting`, req);
  }
}
