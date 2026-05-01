import { HttpClient, HttpEvent, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { ApiResponse, Result } from '../models/ApiReponse';
import {
  PlumberDto,
  PlumberFilteration,
  PlumberImportResult,
  PlumberLookupDto,
  PlumberLookupFilter
} from '../models/IPlumberModels';

@Injectable({ providedIn: 'root' })
export class PlumberService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}Plumber`;

  // -----------------------------------------------------------
  // 1) Paginated list  — GET /api/Plumber
  // -----------------------------------------------------------
  getAllPlumbers(filters: PlumberFilteration): Observable<Result<ApiResponse<PlumberDto[]>>> {
    let params = this.buildFilterParams(filters);
    return this.http.get<Result<ApiResponse<PlumberDto[]>>>(this.baseUrl, { params });
  }

  // -----------------------------------------------------------
  // 2) Get by id  — GET /api/Plumber/Plumber/Details?id=5
  // -----------------------------------------------------------
  getById(id: number): Observable<Result<PlumberDto>> {
    const params = new HttpParams().set('id', String(id));
    return this.http.get<Result<PlumberDto>>(`${this.baseUrl}/Plumber/Details`, { params });
  }

  // -----------------------------------------------------------
  // 3) Lookup list  — GET /api/Plumber/lookups
  // -----------------------------------------------------------
  getLookups(filter: PlumberLookupFilter = {}): Observable<Result<PlumberLookupDto[]>> {
    let params = new HttpParams();
    if (filter.name)      params = params.set('name', filter.name);
    if (filter.specialty) params = params.set('specialty', filter.specialty);
    return this.http.get<Result<PlumberLookupDto[]>>(`${this.baseUrl}/lookups`, { params });
  }

  // -----------------------------------------------------------
  // 4) Create  — POST /api/Plumber
  // -----------------------------------------------------------
  addPlumber(dto: PlumberDto): Observable<Result<string>> {
    return this.http.post<Result<string>>(this.baseUrl, dto);
  }

  // -----------------------------------------------------------
  // 5) Update  — PUT /api/Plumber/Edit
  // -----------------------------------------------------------
  editPlumber(dto: PlumberDto): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.baseUrl}/Edit`, dto);
  }

  // -----------------------------------------------------------
  // 6) Toggle status  — PUT /api/Plumber/{id}/toggle-status
  // -----------------------------------------------------------
  toggleStatus(id: number): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.baseUrl}/${id}/toggle-status`, {});
  }

  // -----------------------------------------------------------
  // 7) Download Excel template  — GET /api/Plumber/import/template
  // -----------------------------------------------------------
  downloadTemplate(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/import/template`, { responseType: 'blob' });
  }

  // -----------------------------------------------------------
  // 8) Import from Excel  — POST /api/Plumber/import
  // -----------------------------------------------------------
  importFromExcel(file: File): Observable<HttpEvent<Result<PlumberImportResult>>> {
    const fd = new FormData();
    fd.append('file', file);
    return this.http.post<Result<PlumberImportResult>>(`${this.baseUrl}/import`, fd, {
      reportProgress: true,
      observe: 'events'
    });
  }

  // -----------------------------------------------------------
  // 9) Export to Excel  — GET /api/Plumber/export
  //    Honours the same filters as the list page so users get
  //    exactly what they're looking at on screen.
  // -----------------------------------------------------------
  exportToExcel(filters: PlumberFilteration): Observable<Blob> {
    const params = this.buildFilterParams(filters);
    return this.http.get(`${this.baseUrl}/export`, { params, responseType: 'blob' });
  }

  // -----------------------------------------------------------
  // Private helpers
  // -----------------------------------------------------------
  private buildFilterParams(f: PlumberFilteration): HttpParams {
    let p = new HttpParams();

    if (f.name?.trim())          p = p.set('name', f.name.trim());
    if (f.phoneNumbers?.trim())  p = p.set('phoneNumbers', f.phoneNumbers.trim());
    if (f.licenseNumber?.trim()) p = p.set('licenseNumber', f.licenseNumber.trim());
    if (f.specialty?.trim())     p = p.set('specialty', f.specialty.trim());

    if (f.isDeleted !== null && f.isDeleted !== undefined)
      p = p.set('isDeleted', String(f.isDeleted));

    if (f.page !== null && f.page !== undefined)
      p = p.set('page', String(f.page));

    if (f.pageSize !== null && f.pageSize !== undefined)
      p = p.set('pageSize', String(f.pageSize));

    return p;
  }
}
