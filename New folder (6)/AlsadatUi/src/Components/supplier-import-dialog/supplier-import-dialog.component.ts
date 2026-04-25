import { CommonModule } from '@angular/common';
import { HttpEventType } from '@angular/common/http';
import { Component, OnDestroy, inject, signal, computed } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subscription } from 'rxjs';
import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';

import { SupplierService } from '../../app/Services/supplier.service';
import { SwalService } from '../../app/Services/swal.service';
import {
  SupplierImportResult,
  SupplierImportRowError
} from '../../app/models/ISupplierModels';

type Step = 'select' | 'uploading' | 'result';

/**
 * SupplierImportDialogComponent
 * ------------------------------------------------------------------
 * Three-step flow:
 *   1. select    — user picks a .xlsx (≤ 5 MB)
 *   2. uploading — determinate progress via HttpClient events
 *   3. result    — summary + per-row error table with "export errors" helper
 *
 * Closes with `true` if at least one row was imported (parent reloads).
 */
@Component({
  selector: 'app-supplier-import-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatTableModule,
    MatTooltipModule
  ],
  templateUrl: './supplier-import-dialog.component.html',
  styleUrls: ['./supplier-import-dialog.component.css']
})
export class SupplierImportDialogComponent implements OnDestroy {
  private readonly supplierService = inject(SupplierService);
  private readonly alert = inject(SwalService);
  private readonly dialogRef = inject(MatDialogRef<SupplierImportDialogComponent>);
  private readonly subs = new Subscription();

  // ---- Constants ----
  private readonly MAX_SIZE_BYTES = 5 * 1024 * 1024;
  private readonly ALLOWED_EXT = ['.xlsx', '.xls'];

  // ---- State ----
  readonly step = signal<Step>('select');
  readonly progressPercent = signal(0);
  readonly selectedFile = signal<File | null>(null);
  readonly isDragging = signal(false);
  readonly result = signal<SupplierImportResult | null>(null);

  /** Columns for the errors table (template uses these). */
  readonly errorColumns = ['rowNumber', 'supplierName', 'column', 'message'];

  // ---- Derived (computed) values ----
  readonly hasErrors = computed(() => (this.result()?.errors?.length ?? 0) > 0);
  readonly hasImported = computed(() => (this.result()?.successCount ?? 0) > 0);

  readonly resultBannerKind = computed<'success' | 'warning' | 'error'>(() => {
    const r = this.result();
    if (!r) return 'error';
    if (r.successCount > 0 && r.failedCount === 0) return 'success';
    if (r.successCount > 0 && r.failedCount > 0) return 'warning';
    return 'error';
  });

  readonly resultBannerText = computed(() => {
    const r = this.result();
    if (!r) return '';
    if (r.successCount > 0 && r.failedCount === 0)
      return `تم استيراد ${r.successCount} مورد بنجاح`;
    if (r.successCount > 0 && r.failedCount > 0)
      return `تم استيراد ${r.successCount} بنجاح، وفشل ${r.failedCount} من أصل ${r.totalRows}`;
    return `فشل استيراد جميع الصفوف (${r.failedCount} صف)`;
  });

  // -----------------------------------------------------------
  // Lifecycle
  // -----------------------------------------------------------
  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // -----------------------------------------------------------
  // File selection (browse + drag-drop)
  // -----------------------------------------------------------
  onBrowse(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.setFile(file);
    input.value = ''; // allow re-picking the same file
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file) this.setFile(file);
  }

  clearFile(): void {
    this.selectedFile.set(null);
  }

  private setFile(file: File): void {
    // Client-side validation
    const ext = this.getExtension(file.name).toLowerCase();
    if (!this.ALLOWED_EXT.includes(ext)) {
      this.alert.error('نوع الملف غير مدعوم — استخدم .xlsx أو .xls');
      return;
    }
    if (file.size === 0) {
      this.alert.error('الملف فارغ');
      return;
    }
    if (file.size > this.MAX_SIZE_BYTES) {
      this.alert.error('حجم الملف أكبر من المسموح (5 ميجابايت)');
      return;
    }
    this.selectedFile.set(file);
  }

  // -----------------------------------------------------------
  // Upload
  // -----------------------------------------------------------
  upload(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.step.set('uploading');
    this.progressPercent.set(0);

    this.subs.add(
      this.supplierService.importFromExcel(file).subscribe({
        next: event => {
          if (event.type === HttpEventType.UploadProgress && event.total) {
            const pct = Math.round((event.loaded / event.total) * 100);
            this.progressPercent.set(pct);
          } else if (event.type === HttpEventType.Response) {
            const body = event.body;
            if (body?.isSuccess && body.data) {
              this.result.set(body.data);
              this.step.set('result');
            } else {
              this.alert.error(body?.message ?? 'تعذر استيراد الملف');
              this.step.set('select');
            }
          }
        },
        error: err => {
          this.step.set('select');
          this.alert.error(err?.error?.message ?? 'حدث خطأ أثناء الاستيراد');
        }
      })
    );
  }

  // -----------------------------------------------------------
  // Export errors to a new Excel file — lets the user fix rows offline
  // -----------------------------------------------------------
  exportErrors(): void {
    const errors = this.result()?.errors ?? [];
    if (errors.length === 0) return;

    const rows = errors.map(e => ({
      'رقم الصف':      e.rowNumber,
      'اسم المورد':    e.supplierName ?? '',
      'العمود':        e.column,
      'رسالة الخطأ':   e.message
    }));

    const ws = XLSX.utils.json_to_sheet(rows);
    // Auto width-ish (optional polish)
    ws['!cols'] = [{ wch: 10 }, { wch: 28 }, { wch: 18 }, { wch: 50 }];

    const wb: XLSX.WorkBook = { Sheets: { 'Errors': ws }, SheetNames: ['Errors'] };
    const buffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });

    const blob = new Blob([buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });
    saveAs(blob, 'Supplier_Import_Errors.xlsx');
  }

  // -----------------------------------------------------------
  // Navigation & close
  // -----------------------------------------------------------
  backToSelect(): void {
    this.step.set('select');
    this.progressPercent.set(0);
    this.result.set(null);
    this.selectedFile.set(null);
  }

  close(): void {
    const imported = this.hasImported();
    this.dialogRef.close(imported);
  }

  // -----------------------------------------------------------
  // Helpers
  // -----------------------------------------------------------
  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
  }

  private getExtension(fileName: string): string {
    const idx = fileName.lastIndexOf('.');
    return idx >= 0 ? fileName.substring(idx) : '';
  }

  // Trackers for *ngFor / @for performance
  trackByErrorRow(_index: number, err: SupplierImportRowError): number {
    return err.rowNumber;
  }
}
