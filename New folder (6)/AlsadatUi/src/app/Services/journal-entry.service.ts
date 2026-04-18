import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { JournalEntriesDto, JournalEntryFilterationReq } from '../models/JournalEntryVM';
import { Observable } from 'rxjs';
import { ApiResponse, Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class JournalEntryService {
  private baseUrl = environment.apiUrl + 'JournalEntry';
 constructor(private http: HttpClient) {}

  getAllJournalEntries(filter: JournalEntryFilterationReq): Observable<Result<ApiResponse<JournalEntriesDto[]>>>
  {

    let params = new HttpParams()
      .set('page', filter.page)
      .set('pageSize', filter.pageSize);

    if (filter.entryDate)
      params = params.set('entryDate', filter.entryDate);

    if (filter.referenceType !== null)
      params = params.set('referenceType', filter.referenceType);

    if (filter.referenceNo)
      params = params.set('referenceNo', filter.referenceNo);

    if (filter.isPosted !== null)
      params = params.set('isPosted', filter.isPosted);

    if (filter.postedDate)
      params = params.set('postedDate', filter.postedDate);

    return this.http.get<Result<ApiResponse<JournalEntriesDto[]>>>(
      `${this.baseUrl}/journal-entries`,
      { params }
    );
  }
   addNewEntry(model: JournalEntriesDto)
    : Observable<Result<string>> {

    return this.http.post<Result<string>>(
      `${this.baseUrl}/add-new-entry`,
      model
    );
  }
    editEntry(model: JournalEntriesDto)
    : Observable<Result<string>> {

    return this.http.put<Result<string>>(
      `${this.baseUrl}/edit-entry`,
      model
    );
  }
   postEntry(id: number)
    : Observable<string> {

    return this.http.patch<string>(
      `${this.baseUrl}/${id}/post`,
      {} // PATCH لازم body حتى لو فاضي
    );
  }
  getById(id: number): Observable<Result<JournalEntriesDto>> {
  return this.http.get<Result<JournalEntriesDto>>(
    `${this.baseUrl}/${id}`
  );
}
deleteEntry(id: number): Observable<Result<string>> {
  return this.http.delete<Result<string>>(`${this.baseUrl}/delete-entry/${id}`);
}
}
