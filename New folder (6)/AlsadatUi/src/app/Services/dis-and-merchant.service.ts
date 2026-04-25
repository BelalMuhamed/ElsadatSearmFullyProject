import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.development';
import { HttpClient, HttpEvent, HttpParams } from '@angular/common/http';
import { DistributorsAndMerchantsDto, DistributorsAndMerchantsFilters } from '../models/IDisAndMercDto';
import { ApiResponse, Result } from '../models/ApiReponse';
import { Observable } from 'rxjs';
import { ExcelImportResult } from '../models/IExcelDtos';

@Injectable({
  providedIn: 'root'
})
export class DisAndMerchantService {

 apiUrl = environment.apiUrl;
  constructor(private http: HttpClient) {}
  getAllDisAndMerch(filter: DistributorsAndMerchantsFilters): Observable<ApiResponse<DistributorsAndMerchantsDto[]>> {

    let params = new HttpParams();

    if (filter.cityName !== null && filter.cityName !== undefined) {
      params = params.set('cityName', filter.cityName);
    }

    if (filter.fullName !== null && filter.fullName !== undefined) {
      params = params.set('fullName', filter.fullName);
    }

     if (filter.isDeleted !== null && filter.isDeleted !== undefined) {
      params = params.set('isDeleted', filter.isDeleted);
    }
    if (filter.phoneNumber !== null && filter.phoneNumber !== undefined) {
      params = params.set('phoneNumber', filter.phoneNumber);
    }
      if (filter.type !== null && filter.type !== undefined) {
      params = params.set('type', filter.type);
    }

    if (filter.page !== null && filter.page !== undefined) {
      params = params.set('page', filter.page.toString());
    }

    if (filter.pageSize !== null && filter.pageSize !== undefined) {
      params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<ApiResponse<DistributorsAndMerchantsDto[]>>(
      `${this.apiUrl}DistAndMerch/list`,
      { params }
    );
  }
  EditDisOrMerchant(req:DistributorsAndMerchantsDto):Observable<Result<any>>
  {
 return this.http.put<Result<any>>(`${this.apiUrl}DistAndMerch/edit/${req.userId}`, req);
  }

  AddDisOrMerchant(dto:DistributorsAndMerchantsDto):Observable<Result<any>>
  {
 return this.http.post<Result<any>>(`${this.apiUrl}DistAndMerch/add`, dto);

  }
  getById(userId: string): Observable<Result<DistributorsAndMerchantsDto>> {
  return this.http.get<Result<DistributorsAndMerchantsDto>>(
    `${this.apiUrl}DistAndMerch/get/${userId}`
  );
}
// -----------------------------------------------------------
  // 7) Download Excel template  — GET /api/Supplier/import/template
  //   Returns a binary .xlsx as Blob so the browser can save it.
  // -----------------------------------------------------------

   downloadImportTemplate(): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}DistAndMerch/import/template`,
      { responseType: 'blob' }
    );
  }


  // -----------------------------------------------------------
  // 8) Import from Excel  — POST /api/Supplier/import
  //   `reportProgress:true` lets the dialog show a live progress bar.
  //   Caller receives HttpEvent<T> — use the `Response` event to read the body.
  // -----------------------------------------------------------

  importFromExcel(
    file: File,

  ): Observable<Result<ExcelImportResult<DistributorsAndMerchantsDto>>> {

    const formData = new FormData();
    formData.append('file', file);


    return this.http.post<
      Result<ExcelImportResult<DistributorsAndMerchantsDto>>>(
      `${this.apiUrl}DistAndMerch/import`,
      formData
    );
  }
}
