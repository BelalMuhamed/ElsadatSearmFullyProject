import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { PublicHoliday } from '../models/IPublicHoliday';
import { ApiResponse } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class PublicHolidayService {
  private apiUrl = environment.apiUrl + 'PublicHoliday/';

  constructor(private http: HttpClient) {}

  // Get all public holidays with pagination (returns paginated response with items property)
  getAll(pageNumber: number = 1, pageSize: number = 10): Observable<any> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<any>(this.apiUrl + 'GetAllPublicHoliday', { params });
  }

  // Get all active (non-deleted) public holidays
  getAllActive(pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<PublicHoliday[]>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<PublicHoliday[]>>(this.apiUrl + 'GetAllActivePublicHoliday', { params });
  }

  // Get public holiday by ID
  getById(id: number): Observable<ApiResponse<PublicHoliday>> {
    return this.http.get<ApiResponse<PublicHoliday>>(this.apiUrl + `GetPublicHolidayByID/${id}`);
  }

  // Get soft-deleted public holidays
  getSoftDeleted(pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<PublicHoliday[]>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<PublicHoliday[]>>(this.apiUrl + 'GetSoftDeletePublicHoliday', { params });
  }

  // Create new public holiday
  create(holiday: PublicHoliday): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(this.apiUrl + 'CreatePublicHoliday', holiday);
  }

  // Update existing public holiday
  update(holiday: PublicHoliday): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(this.apiUrl + 'UpdatePublicHoliday', holiday);
  }

  // Soft delete public holiday
  softDelete(holiday: PublicHoliday): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(this.apiUrl + 'SoftDeletePublicHoliday', holiday);
  }

  // Restore soft-deleted public holiday
  restore(holiday: PublicHoliday): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(this.apiUrl + 'RestorePublicHoliday', holiday);
  }

  // Permanently delete public holiday
  hardDelete(holiday: PublicHoliday): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(this.apiUrl + 'HardDeletePublicHoliday', {
      body: holiday
    });
  }
}
