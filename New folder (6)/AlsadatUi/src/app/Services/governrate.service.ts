import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { GovernrateDto, GovernrateFilteration } from '../models/IGovernrateVM';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class GovernrateService {

  apiUrl = environment.apiUrl;
 constructor(private http: HttpClient) {}
 getAllGovernrates(filter: GovernrateFilteration): Observable<ApiResponse<GovernrateDto[]>> {

  let params = new HttpParams();

  if (filter.name !== null && filter.name !== undefined) {
    params = params.set('name', filter.name);
  }

  if (filter.page !== null && filter.page !== undefined) {
    params = params.set('page', filter.page.toString());
  }

  if (filter.pageSize !== null && filter.pageSize !== undefined) {
    params = params.set('pageSize', filter.pageSize.toString());
  }

  return this.http.get<ApiResponse<GovernrateDto[]>>(
    `${this.apiUrl}Governrate`,
    { params }
  );
}
addGovernrate(dto: GovernrateDto): Observable<any> {
  return this.http.post(
    `${this.apiUrl}Governrate`,
    dto
  );
}
updateGovernrate(dto: GovernrateDto): Observable<any> {
  return this.http.put(
    `${this.apiUrl}Governrate`,
    dto
  );
}
getGovernrateById(id: number) {
  return this.http.get<GovernrateDto>(
    `${this.apiUrl}Governrate/Governrates/${id}`
  );
}

}
