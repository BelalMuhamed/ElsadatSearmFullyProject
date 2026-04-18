import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { SupplierDto, SupplierFilteration } from '../models/ISupplierModels';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
apiUrl = environment.apiUrl;
constructor(private http: HttpClient) {}

private readonly baseUrl = `${environment.apiUrl}Supplier`;
getAllSuppliers(
  filters: SupplierFilteration
): Observable<ApiResponse<SupplierDto[]>> {

  let params = new HttpParams();

  if (filters.name !== null && filters.name !== undefined) {
    params = params.set('name', filters.name);
  }

  if (filters.phoneNumbers !== null && filters.phoneNumbers !== undefined) {
    params = params.set('phoneNumbers', filters.phoneNumbers);
  }

  if (filters.page !== null && filters.page !== undefined) {
    params = params.set('page', filters.page.toString());
  }
if (filters.isDeleted !== null&& filters.isDeleted !== undefined)
    params = params.set('isDeleted', filters.isDeleted.toString());
  if (filters.pageSize !== null && filters.pageSize !== undefined) {
    params = params.set('pageSize', filters.pageSize.toString());
  }

  return this.http.get<ApiResponse<SupplierDto[]>>(
    `${this.apiUrl}Supplier`,
    { params }
  );
}

 toggleStatus(supplier: SupplierDto): Observable<any> {
    return this.http.put(
      `${this.baseUrl}/Edit`,
      supplier
    );
  }

  // 🔹 GET: api/Supplier/Supplier/Details?id=5
    getById(id: number): Observable<SupplierDto> {
      return this.http.get<SupplierDto>(
        `${this.baseUrl}/Supplier/Details`,
        { params: { id } }
      );
    }

    // 🔹 POST: api/Supplier
    addSupplier(dto: SupplierDto): Observable<any> {
      return this.http.post(this.baseUrl, dto);
    }
    ditSupplier(dto: SupplierDto): Observable<any> {
  return this.http.put(`${this.baseUrl}/Edit`, dto);
}
}
