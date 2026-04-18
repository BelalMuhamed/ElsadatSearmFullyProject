import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

@Injectable({ providedIn: 'root' })
export class CoponCollectionRepresentiveRateService {
  private api = `${environment.apiUrl}CoponCollectionRepresentiveRate/`;
  constructor(private http: HttpClient) {}

  getAll(p: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.api}GetAllCoponCollectionRepresentiveRate?pageNumber=${p}&pageSize=${pageSize}`);
  }

  getActive(p: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.api}GetAllActiveCoponCollectionRepresentiveRate?pageNumber=${p}&pageSize=${pageSize}`);
  }

  getSoftDeleted(p: number, pageSize: number): Observable<any> {
    return this.http.get<any>(`${this.api}GetSoftDeleteCoponCollectionRepresentiveRate?pageNumber=${p}&pageSize=${pageSize}`);
  }

  create(model: any): Observable<any> {
    return this.http.post<any>(`${this.api}CreateCoponCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  update(model: any): Observable<any> {
    return this.http.put<any>(`${this.api}UpdateCoponCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  softDelete(model: any): Observable<any> {
    return this.http.put<any>(`${this.api}SoftDeleteCoponCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  restore(model: any): Observable<any> {
    return this.http.put<any>(`${this.api}RestoreCoponCollectionRepresentiveRate`, model, { responseType: 'text' as 'json' });
  }

  hardDelete(model: any): Observable<any> {
    return this.http.delete<any>(`${this.api}HardDeleteCoponCollectionRepresentiveRate`, { body: model });
  }
}
