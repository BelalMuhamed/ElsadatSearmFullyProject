import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { saveAs } from 'file-saver';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import Swal from 'sweetalert2';

import {
  ProductInventoryRow,
  StockHealth,
  WarehouseInventoryFilter,
  WarehouseInventoryMatrix,
} from '../../app/models/IWarehouseInventoryVM';
import { WarehouseInventoryService } from '../../app/Services/warehouse-inventory.service';

/**
 * المخزون page — product-centric inventory matrix.
 *
 * Responsibilities:
 *  - Render the matrix returned by the API (no business calculations here).
 *  - Map server-side health enum to CSS classes.
 *  - Drive filtering, pagination, and Excel export.
 */
@Component({
  selector: 'app-warehouse-inventory',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatCheckboxModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
  ],
  templateUrl: './warehouse-inventory.component.html',
  styleUrl: './warehouse-inventory.component.css',
})
export class WarehouseInventoryComponent implements OnInit {
  private readonly fb              = inject(FormBuilder);
  private readonly inventorySvc    = inject(WarehouseInventoryService);
  private readonly destroyRef      = inject(DestroyRef);

  /** Signals — the only state for this view. */
  protected readonly matrix       = signal<WarehouseInventoryMatrix | null>(null);
  protected readonly isLoading    = signal<boolean>(false);
  protected readonly isExporting  = signal<boolean>(false);
  protected readonly totalCount   = signal<number>(0);

  /** Derived: whether the table has anything to show. */
  protected readonly hasData = computed(() => (this.matrix()?.products?.length ?? 0) > 0);

  protected readonly StockHealth = StockHealth;

  /** Lightweight reactive form — debounced. */
  searchForm!: FormGroup;

  filter: WarehouseInventoryFilter = {
    productName: null,
    productCode: null,
    storeId: null,
    lowStockOnly: false,
    excludeDeletedProducts: true,
    excludeDeletedWarehouses: true,
    page: 1,
    pageSize: 25,
  };

  ngOnInit(): void {
    this.initForm();
    this.loadMatrix();
  }

  // ---------------------------------------------------------------------------
  private initForm(): void {
    this.searchForm = this.fb.group({
      productName:  [''],
      productCode:  [''],
      lowStockOnly: [false],
    });

    this.searchForm.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((v) => {
        this.filter = {
          ...this.filter,
          productName:  v.productName  || null,
          productCode:  v.productCode  || null,
          lowStockOnly: !!v.lowStockOnly,
          page: 1,
        };
        this.loadMatrix();
      });
  }

  // ---------------------------------------------------------------------------
  loadMatrix(): void {
    this.isLoading.set(true);
    this.inventorySvc.getMatrix(this.filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res) => {
          this.matrix.set(res?.data ?? null);
          this.totalCount.set(res?.data?.products?.length ?? 0);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: err?.error?.message ?? 'تعذر تحميل بيانات المخزون',
          });
        },
      });
  }

  // ---------------------------------------------------------------------------
  onPageChange(e: PageEvent): void {
    this.filter = { ...this.filter, page: e.pageIndex + 1, pageSize: e.pageSize };
    this.loadMatrix();
  }

  // ---------------------------------------------------------------------------
  exportToExcel(): void {
    this.isExporting.set(true);
    this.inventorySvc.exportMatrix(this.filter)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          const stamp = new Date().toISOString().slice(0, 16).replace(/[T:]/g, '-');
          saveAs(blob, `warehouse-inventory-${stamp}.xlsx`);
          this.isExporting.set(false);
        },
        error: (err) => {
          this.isExporting.set(false);
          Swal.fire({
            icon: 'error',
            title: 'خطأ في التصدير',
            text: err?.error?.message ?? 'تعذر إنشاء ملف الإكسل',
          });
        },
      });
  }

  // ---------------------------------------------------------------------------
  // Pure helpers — no state, easy to unit-test.
  // ---------------------------------------------------------------------------

  /** Quantity for a (row, store) cell. Backend always emits 0 for missing — defensive fallback. */
  qtyFor(row: ProductInventoryRow, storeId: number): number {
    return row.quantities?.[storeId] ?? 0;
  }

  /** Footer total per warehouse. */
  warehouseTotal(storeId: number): number {
    return this.matrix()?.warehouseTotals?.find((t) => t.storeId === storeId)?.totalQuantity ?? 0;
  }

  /** Map server-side enum to a CSS class. ONE place. */
  healthClass(h: StockHealth | undefined | null): string {
    switch (h) {
      case StockHealth.OutOfStock: return 'health-out';
      case StockHealth.Critical:   return 'health-critical';
      case StockHealth.Warning:    return 'health-warning';
      case StockHealth.Healthy:    return 'health-healthy';
      default:                     return '';
    }
  }

  healthLabel(h: StockHealth | undefined | null): string {
    switch (h) {
      case StockHealth.OutOfStock: return 'نفد المخزون';
      case StockHealth.Critical:   return 'حرج';
      case StockHealth.Warning:    return 'منخفض';
      case StockHealth.Healthy:    return 'آمن';
      default:                     return '';
    }
  }

  /**
   * Health for the FOOTER per-warehouse total.
   * We don't have a per-warehouse threshold, so we derive a simple rule:
   *   total <= 0 → out, otherwise neutral (default styling).
   * Per-product health is the authoritative one (server-computed).
   */
  warehouseTotalClass(storeId: number): string {
    const t = this.warehouseTotal(storeId);
    if (t <= 0) return 'health-out';
    return '';
  }

  trackProduct = (_: number, p: ProductInventoryRow) => p.productId;
  trackStore   = (_: number, s: { storeId: number }) => s.storeId;
}
