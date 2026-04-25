import { HttpClient, HttpEvent, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { ApiResponse, Result } from '../models/ApiReponse';
import {
  SupplierDto,
  SupplierFilteration,
  SupplierImportResult,
  SupplierLookupDto,
  SupplierLookupFilter
} from '../models/ISupplierModels';

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}Supplier`;

  // -----------------------------------------------------------
  // 1) Paginated list  — GET /api/Supplier
  // -----------------------------------------------------------
  getAllSuppliers(filters: SupplierFilteration): Observable<Result<ApiResponse<SupplierDto[]>>> {
    let params = new HttpParams();

    if (filters.name !== null && filters.name !== undefined && filters.name !== '') {
      params = params.set('name', filters.name);
    }
    if (filters.phoneNumbers !== null && filters.phoneNumbers !== undefined && filters.phoneNumbers !== '') {
      params = params.set('phoneNumbers', filters.phoneNumbers);
    }
    if (filters.isDeleted !== null && filters.isDeleted !== undefined) {
      params = params.set('isDeleted', String(filters.isDeleted));
    }
    if (filters.page !== null && filters.page !== undefined) {
      params = params.set('page', String(filters.page));
    }
    if (filters.pageSize !== null && filters.pageSize !== undefined) {
      params = params.set('pageSize', String(filters.pageSize));
    }

    return this.http.get<Result<ApiResponse<SupplierDto[]>>>(this.baseUrl, { params });
  }

  // -----------------------------------------------------------
  // 2) Get by id  — GET /api/Supplier/Supplier/Details?id=5
  // -----------------------------------------------------------
  getById(id: number): Observable<Result<SupplierDto>> {
    const params = new HttpParams().set('id', String(id));
    return this.http.get<Result<SupplierDto>>(`${this.baseUrl}/Supplier/Details`, { params });
  }

  // -----------------------------------------------------------
  // 3) Lookup list for select-boxes  — GET /api/Supplier/lookups
  // -----------------------------------------------------------
  getLookups(filter: SupplierLookupFilter = {}): Observable<Result<SupplierLookupDto[]>> {
    let params = new HttpParams();
    if (filter.name) {
      params = params.set('name', filter.name);
    }
    return this.http.get<Result<SupplierLookupDto[]>>(`${this.baseUrl}/lookups`, { params });
  }

  // -----------------------------------------------------------
  // 4) Create  — POST /api/Supplier
  // -----------------------------------------------------------
  addSupplier(dto: SupplierDto): Observable<Result<string>> {
    return this.http.post<Result<string>>(this.baseUrl, dto);
  }

  // -----------------------------------------------------------
  // 5) Update  — PUT /api/Supplier/Edit
  //   Kept as `ditSupplier` per your explicit decision (item 13).
  // -----------------------------------------------------------
  ditSupplier(dto: SupplierDto): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.baseUrl}/Edit`, dto);
  }

  // -----------------------------------------------------------
  // 6) Toggle status  — PUT /api/Supplier/{id}/toggle-status
  //   Dedicated endpoint, no body required.
  // -----------------------------------------------------------
  toggleStatus(id: number): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.baseUrl}/${id}/toggle-status`, {});
  }

  // -----------------------------------------------------------
  // 7) Download Excel template  — GET /api/Supplier/import/template
  //   Returns a binary .xlsx as Blob so the browser can save it.
  // -----------------------------------------------------------
  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/import/template`, { responseType: 'blob' });
  }

  // -----------------------------------------------------------
  // 8) Import from Excel  — POST /api/Supplier/import
  //   `reportProgress:true` lets the dialog show a live progress bar.
  //   Caller receives HttpEvent<T> — use the `Response` event to read the body.
  // -----------------------------------------------------------
  importFromExcel(file: File): Observable<HttpEvent<Result<SupplierImportResult>>> {
    const fd = new FormData();
    fd.append('file', file);

    return this.http.post<Result<SupplierImportResult>>(`${this.baseUrl}/import`, fd, {
      reportProgress: true,
      observe: 'events'
    });
  }
}
