import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ICityDto, ICityFilteration } from '../models/Icity';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class CityServiceService {
apiUrl = environment.apiUrl;
 constructor(private http: HttpClient) {}
  getAllCities(filter: ICityFilteration): Observable<ApiResponse<ICityDto[]>> {

   let params = new HttpParams();

   if (filter.cityName !== null && filter.cityName !== undefined) {
     params = params.set('cityName', filter.cityName);
   }

   if (filter.governrateName !== null && filter.governrateName !== undefined) {
     params = params.set('governrateName', filter.governrateName);
   }
   if (filter.page !== null && filter.page !== undefined) {
     params = params.set('page', filter.page.toString());
   }

   if (filter.pageSize !== null && filter.pageSize !== undefined) {
     params = params.set('pageSize', filter.pageSize.toString());
   }

   return this.http.get<ApiResponse<ICityDto[]>>(
     `${this.apiUrl}City`,
     { params }
   );
 }
 addCity(dto: ICityDto): Observable<any> {
   return this.http.post(
     `${this.apiUrl}City`,
     dto
   );
 }
 updateCity(dto: ICityDto): Observable<any> {
   return this.http.put(
     `${this.apiUrl}City`,
     dto
   );
 }
 getCityById(id: number) {
   return this.http.get<ICityDto>(
     `${this.apiUrl}City/cities/${id}`
   );
 }
}
