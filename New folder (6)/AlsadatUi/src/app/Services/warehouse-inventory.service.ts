import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';
import { Result } from '../models/ApiReponse';
import {
  WarehouseInventoryFilter,
  WarehouseInventoryMatrix,
} from '../models/IWarehouseInventoryVM';

/**
 * Thin transport layer for the warehouse inventory report.
 * - No business logic (totals/health are computed server-side).
 * - No subscriptions here — components own their RxJS lifecycle.
 */
@Injectable({ providedIn: 'root' })
export class WarehouseInventoryService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}warehouse-inventory`;

  getMatrix(filter: WarehouseInventoryFilter): Observable<Result<WarehouseInventoryMatrix>> {
    return this.http.get<Result<WarehouseInventoryMatrix>>(
      `${this.baseUrl}/matrix`,
      { params: this.toParams(filter) }
    );
  }

  /** Returns the raw .xlsx blob — the component handles download with file-saver. */
  exportMatrix(filter: WarehouseInventoryFilter): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/matrix/export`, {
      params: this.toParams(filter),
      responseType: 'blob',
    });
  }

  // -------------------------------------------------------------------------
  private toParams(f: WarehouseInventoryFilter): HttpParams {
    let p = new HttpParams()
      .set('page', String(f.page ?? 1))
      .set('pageSize', String(f.pageSize ?? 50));

    if (f.productName)            p = p.set('productName', f.productName);
    if (f.productCode)            p = p.set('productCode', f.productCode);
    if (f.storeId != null)        p = p.set('storeId', String(f.storeId));
    if (f.lowStockOnly != null)   p = p.set('lowStockOnly', String(f.lowStockOnly));
    if (f.excludeDeletedProducts != null)
      p = p.set('excludeDeletedProducts', String(f.excludeDeletedProducts));
    if (f.excludeDeletedWarehouses != null)
      p = p.set('excludeDeletedWarehouses', String(f.excludeDeletedWarehouses));

    return p;
  }
}
