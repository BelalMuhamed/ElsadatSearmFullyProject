import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Result } from '../models/ApiReponse';
import {
  ChangePasswordRequest,
  ProfileDto,
  UpdateProfileRequest,
  UpdateProfileResponse
} from '../models/IProfileModels';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}Profile`;

  /** GET /api/Profile — current user snapshot. */
  getProfile(): Observable<Result<ProfileDto>> {
    return this.http.get<Result<ProfileDto>>(this.baseUrl);
  }

  /** PUT /api/Profile — update phone / email / username. */
  updateProfile(request: UpdateProfileRequest): Observable<Result<UpdateProfileResponse>> {
    return this.http.put<Result<UpdateProfileResponse>>(this.baseUrl, request);
  }

  /** PUT /api/Profile/change-password */
  changePassword(request: ChangePasswordRequest): Observable<Result<boolean>> {
    return this.http.put<Result<boolean>>(`${this.baseUrl}/change-password`, request);
  }
}
