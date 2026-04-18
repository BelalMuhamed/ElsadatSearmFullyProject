import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CopounRespDto } from '../models/Icopoun';

@Injectable({
  providedIn: 'root'
})
export class CopounService {
   apiUrl = environment.apiUrl;
 constructor(private http: HttpClient) {}
 GetAllCopouns():Observable<CopounRespDto[]>
 {
  return this.http.get<CopounRespDto[]>(`${this.apiUrl}Copoun`)
 }

 addNewCopoun(dto: CopounRespDto): Observable<any> {
    return this.http.post(`${this.apiUrl}Copoun`, dto);
  }
  updateCopoun(dto: CopounRespDto) {
  return this.http.put(`${this.apiUrl}Copoun`, dto);
}
updateAllPoints(points: number) {
  return this.http.put(`${this.apiUrl}Copoun/UpdateAllPoints`, {
    pointsToCollectCopoun: points
  });
}

}
