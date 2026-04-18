import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class CollectionRepresentiveRateService {
  private api = `${environment.apiUrl}CollectionRepresentiveRate/`;
  constructor(private http: HttpClient) {}

  getAll(p: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.api}GetAllCollectionRepresentiveRate?pageNumber=${p}&pageSize=${pageSize}`);
  }

  getActive(p: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.api}GetAllActiveCollectionRepresentiveRate?pageNumber=${p}&pageSize=${pageSize}`);
  }

  getSoftDeleted(p: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.api}GetSoftDeleteCollectionRepresentiveRate?pageNumber=${p}&pageSize=${pageSize}`);
  }

  create(model: any): Observable<any> {
    return this.http.post<any>(`${this.api}CreateCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  update(model: any): Observable<any> {
    return this.http.put<any>(`${this.api}UpdateCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  softDelete(model: any): Observable<any> {
    return this.http.put<any>(`${this.api}SoftDeleteCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  restore(model: any): Observable<any> {
    return this.http.put<any>(`${this.api}RestoreCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  hardDelete(model: any): Observable<any> {
    return this.http.delete<any>(`${this.api}HardDeleteCollectionRepresentiveRate`, { body: model });
  }
}
