import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountDetailsDto, AccountDetailsDtoReq, AccountDto, DisAndMerchAccountDto, FilterationAccountsDto, TreeAccountDto } from '../models/TreeAccountDto';
import { Result } from '../models/ApiReponse';

@Injectable({
  providedIn: 'root'
})
export class TreeAccountsService {
apiUrl = environment.apiUrl;
constructor(private http: HttpClient) {}
 getTreeAccounts(): Observable<TreeAccountDto[]> {
    return this.http.get<TreeAccountDto[]>(`${this.apiUrl}TreeAccounts`)
  }

  addAccount(account: AccountDto): Observable<Result<string>> {
    return this.http.post<Result<string>>(`${this.apiUrl}TreeAccounts`, account)
  }

  updateAccount(account: AccountDto): Observable<Result<string>> {
    return this.http.put<Result<string>>(`${this.apiUrl}TreeAccounts`, account)
  }
  getAccounts(filter: FilterationAccountsDto):Observable<Result<AccountDto[]>> {

    let params = new HttpParams();

    Object.keys(filter).forEach((key) => {
      const value = (filter as any)[key];

      if (value !== null && value !== undefined) {
        params = params.append(key, value);
      }
    });

    return this.http.get<Result<AccountDto[]>>(`${this.apiUrl}TreeAccounts/accounts`, { params });
  }
  deleteAccount(id: number): Observable<Result<string>> {
    return this.http.delete<Result<string>>(`${this.apiUrl}TreeAccounts/${id}`)
  }
  getByUserId(id:string):Observable<Result<DisAndMerchAccountDto>>
  {
return this.http.get<Result<DisAndMerchAccountDto>>(`${this.apiUrl}TreeAccounts/by-user/${id}`)
  }
  getByAccountId(id: number): Observable<Result<AccountDto>> {
    return this.http.get<Result<AccountDto>>(
      `${this.apiUrl}TreeAccounts/${id}`
    );
  }
getAccountJournalEntryDetails(req: AccountDetailsDtoReq)
  : Observable<Result<AccountDetailsDto>> {

  let params: any = {
    accountId: req.accountId,
    page: req.page,
    pageSize: req.pageSize
  };

  if (req.entryId)
    params.entryId = req.entryId;

  if (req.entryDate)
    params.entryDate = req.entryDate;

  return this.http.get<Result<AccountDetailsDto>>(
    `${this.apiUrl}JournalEntry`,
    { params }
  );
}
}
