import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment.development';
import {
  RepresentativeDTo,
  RepresentativeHelper,
  PaginationParams
} from '../models/IRepresentative';

@Injectable({
  providedIn: 'root'
})
export class RepresentativeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  constructor() {}

  /**
   * Get all representatives with optional filtering and pagination
   */
  getRepresentativesByFilter(
    paginationParams: PaginationParams,
    filters: RepresentativeHelper
  ) {
    const url = `${this.apiUrl}Representive/GetAllRepresentives`;
    const params: any = {
      pageNumber: paginationParams.pageNumber.toString(),
      pageSize: paginationParams.pageSize.toString(),
      representativeCode: filters.representativeCode || '',
      representativeName: filters.representativeName || '',
      cityName: filters.cityName || '',
      isActive: filters.isActive.toString()
    };
    
    // Only add representiveType if it's not 0 (None/All)
    if (filters.representiveType !== 0) {
      params.representiveType = filters.representiveType.toString();
    }
    
    return this.http.get<any>(url, { params });
  }

  /**
   * Add a new representative
   */
  addRepresentative(dto: RepresentativeDTo) {
    const url = `${this.apiUrl}Representive/AddNewRepresentive`;
    return this.http.post<any>(url, dto, { responseType: 'text' as 'json' });
  }

  /**
   * Update an existing representative
   */
  updateRepresentative(dto: RepresentativeDTo) {
    const url = `${this.apiUrl}Representive/UpdateRepresentive`;
    return this.http.put<any>(url, dto, { responseType: 'text' as 'json' });
  }

  /**
   * Soft delete a representative
   */
  softDeleteRepresentative(dto: RepresentativeDTo) {
    const url = `${this.apiUrl}Representive/SoftDeleteRepresentive`;
    return this.http.put<any>(url, dto, { responseType: 'text' as 'json' });
  }

  /**
   * Restore a deleted representative
   */
  restoreRepresentative(dto: RepresentativeDTo) {
    const url = `${this.apiUrl}Representive/RestoreRepresentive`;
    return this.http.put<any>(url, dto, { responseType: 'text' as 'json' });
  }
}
