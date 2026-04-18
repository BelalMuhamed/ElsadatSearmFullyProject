import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Department } from '../models/IDepartment';
import { ApiResponse } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private apiUrl = environment.apiUrl + 'Department/';

  constructor(private http: HttpClient) {}

  // Get all departments with pagination and optional search (returns paginated response with items property)
  getAll(pageNumber: number = 1, pageSize: number = 10, search?: string): Observable<any> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    if (search) params = params.set('search', search);

    return this.http.get<any>(this.apiUrl + 'GetAllDepartments', { params });
  }

  // Get all active (non-deleted) departments
  getAllActive(pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<Department[]>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<Department[]>>(this.apiUrl + 'GetAllActiveDepartment', { params });
  }

  // Get department by ID
  getById(id: number): Observable<ApiResponse<Department>> {
    return this.http.get<ApiResponse<Department>>(this.apiUrl + 'GetDepartmentById', {
      params: { id }
    });
  }

  // Get soft-deleted departments
  getSoftDeleted(pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<Department[]>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);

    return this.http.get<ApiResponse<Department[]>>(this.apiUrl + 'GetSoftDeleteDepartment', { params });
  }

  // Create new department
  create(department: Department): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(this.apiUrl + 'CreateDepartment', department);
  }

  // Update existing department
  update(department: Department): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(this.apiUrl + 'UpdateDepartment', department);
  }

  // Soft delete department
  softDelete(department: Department): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(this.apiUrl + 'SoftDeleteDepartment', department);
  }

  // Restore soft-deleted department
  restore(department: Department): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(this.apiUrl + 'RestoreDepartment', department);
  }

  // Permanently delete department
  hardDelete(department: Department): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(this.apiUrl + 'HardDeleteDepartment', {
      body: department
    });
  }
}
