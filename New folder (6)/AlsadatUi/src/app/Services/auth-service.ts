import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { AuthResponse, loginDto, RefreshTokenDto } from '../models/IAuthModels';
import { RoleDTO, CreateRoleRequestDTO } from '../models/IAuthModels';
import { tap } from 'rxjs/operators';
import { Observable, throwError } from 'rxjs';
import { Result } from '../models/ApiReponse';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Router } from '@angular/router';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
    apiUrl = environment.apiUrl;

    constructor(private http: HttpClient, private router: Router) {}
  login(dto:loginDto):Observable<Result<AuthResponse>> {
    return this.http.post<Result<AuthResponse>>(`${this.apiUrl}Auth/login`, dto);
  }
  refreshToken() {
    const refreshToken = localStorage.getItem('refreshToken');


  if (!refreshToken) return throwError(() => new Error('No refresh token'));
 const dto :RefreshTokenDto={
      token:refreshToken,
      ipAddress:refreshToken
    };
    return this.http.post<AuthResponse>(`${this.apiUrl}Auth/refresh`, dto).pipe(
      tap(res => {
        localStorage.setItem('accessToken', res.accessToken);
        localStorage.setItem('refreshToken', res.refreshToken);
      })
    );
  }
   getToken() {
    return localStorage.getItem('accessToken');
  }
    logout() {
    const refreshToken = localStorage.getItem('refreshToken');

    // Call backend API
    return this.http.post(`${this.apiUrl}Auth/logout`, {
      refreshToken: refreshToken
    }).subscribe({
      next: () => {
        this.clearStorageAndRedirect();
      },
      error: () => {
        // حتى لو فشل، اعمل logout محلي
        this.clearStorageAndRedirect();
      }
    });
  }

  private clearStorageAndRedirect() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('userName');
    localStorage.removeItem('userEmail');

    this.router.navigate(['/login']);
  }

  // Roles API
  getAllRoles(): Observable<Result<RoleDTO[]>> {
    return this.http.get<Result<RoleDTO[]>>(`${this.apiUrl}Auth/roles`);
  }

  getInactiveRoles(): Observable<Result<RoleDTO[]>> {
    return this.http.get<Result<RoleDTO[]>>(`${this.apiUrl}Auth/roles/inactive`);
  }

  getSoftDeletedRoles(): Observable<Result<RoleDTO[]>> {
    return this.http.get<Result<RoleDTO[]>>(`${this.apiUrl}Auth/roles/deleted`);
  }

  getRoleById(id: string): Observable<Result<RoleDTO>> {
    return this.http.get<Result<RoleDTO>>(`${this.apiUrl}Auth/roles/${id}`);
  }

  createRole(dto: CreateRoleRequestDTO): Observable<Result<string>> {
    return this.http.post<Result<string>>(`${this.apiUrl}Auth/roles`, dto);
  }

  updateRole(id: string, dto: CreateRoleRequestDTO): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.apiUrl}Auth/roles/${id}`, dto);
  }

  softDeleteRole(id: string): Observable<Result<string>> {
    return this.http.delete<Result<string>>(`${this.apiUrl}Auth/roles/${id}`);
  }

  restoreRole(id: string): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.apiUrl}Auth/roles/restore/${id}`, {} as any);
  }

  hardDeleteRole(id: string): Observable<Result<string>> {
    return this.http.delete<Result<string>>(`${this.apiUrl}Auth/roles/hard/${id}`);
  }
  getRoles(): string[] {
  return JSON.parse(localStorage.getItem('roles') || '[]');
}
}
