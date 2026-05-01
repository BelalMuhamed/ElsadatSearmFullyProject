import { CommonModule } from '@angular/common';
import { HttpEventType } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';

import { PlumberService } from '../../app/Services/plumber.service';
import { SwalService } from '../../app/Services/swal.service';
import {
  PlumberImportResult,
  PlumberImportRowError
} from '../../app/models/IPlumberModels';

type Step = 'select' | 'uploading' | 'result';

/**
 * PlumberImportDialogComponent
 * ------------------------------------------------------------------
 * Three-step import flow (matches the supplier dialog):
 *   1. select    — user picks a .xlsx (≤ 5 MB)
 *   2. uploading — determinate progress via HttpClient events
 *   3. result    — summary + per-row error table + "export errors" helper
 *
 * Closes with `true` if at least one row was imported, so the parent reloads.
 */
@Component({
  selector: 'app-plumber-import-dialog',
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
  templateUrl: './plumber-import-dialog.component.html',
  styleUrls: ['./plumber-import-dialog.component.css']
})
export class PlumberImportDialogComponent {
  private readonly plumberService = inject(PlumberService);
  private readonly alert = inject(SwalService);
  private readonly dialogRef = inject(MatDialogRef<PlumberImportDialogComponent>);
  private readonly destroyRef = inject(DestroyRef);

  // ---- Constants ----
  private readonly MAX_SIZE_BYTES = 5 * 1024 * 1024;
  private readonly ALLOWED_EXT = ['.xlsx', '.xls'];

  // ---- State ----
  readonly step = signal<Step>('select');
  readonly progressPercent = signal(0);
  readonly selectedFile = signal<File | null>(null);
  readonly isDragging = signal(false);
  readonly result = signal<PlumberImportResult | null>(null);

  readonly errorColumns = ['rowNumber', 'plumberName', 'column', 'message'];

  readonly hasErrors = computed(() => (this.result()?.errors?.length ?? 0) > 0);
  readonly hasImported = computed(() => (this.result()?.successCount ?? 0) > 0);

  // ---------------------------------------------------------------
  // File selection
  // ---------------------------------------------------------------
  onFilePicked(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.acceptFile(file);
    // reset input so picking the same file again triggers (change)
    input.value = '';
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
    const file = event.dataTransfer?.files?.[0] ?? null;
    this.acceptFile(file);
  }

  private acceptFile(file: File | null): void {
    if (!file) return;

    const ext = '.' + (file.name.split('.').pop() ?? '').toLowerCase();
    if (!this.ALLOWED_EXT.includes(ext)) {
      this.alert.error('الصيغة غير مدعومة — استخدم ملف Excel بصيغة .xlsx أو .xls');
      return;
    }
    if (file.size > this.MAX_SIZE_BYTES) {
      this.alert.error('حجم الملف يتجاوز الحد المسموح به (5 ميجابايت)');
      return;
    }

    this.selectedFile.set(file);
  }

  removeFile(): void {
    this.selectedFile.set(null);
  }

  // ---------------------------------------------------------------
  // Upload
  // ---------------------------------------------------------------
  startImport(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.step.set('uploading');
    this.progressPercent.set(0);

    this.plumberService.importFromExcel(file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: event => {
          if (event.type === HttpEventType.UploadProgress && event.total) {
            this.progressPercent.set(Math.round((event.loaded / event.total) * 100));
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
          this.alert.error(err?.error?.message ?? 'حدث خطأ أثناء رفع الملف');
          this.step.set('select');
        }
      });
  }

  // ---------------------------------------------------------------
  // Errors export
  // ---------------------------------------------------------------
  exportErrors(): void {
    const errors = this.result()?.errors ?? [];
    if (errors.length === 0) return;

    const wsData = [
      ['Row', 'Name', 'Column', 'Message'],
      ...errors.map((e: PlumberImportRowError) => [
        e.rowNumber,
        e.plumberName ?? '',
        e.column,
        e.message
      ])
    ];

    const ws = XLSX.utils.aoa_to_sheet(wsData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Errors');
    const blob = new Blob(
      [XLSX.write(wb, { bookType: 'xlsx', type: 'array' })],
      { type: 'application/octet-stream' }
    );
    saveAs(blob, 'PlumberImport_Errors.xlsx');
  }

  // ---------------------------------------------------------------
  // Close
  // ---------------------------------------------------------------
  close(): void {
    this.dialogRef.close(this.hasImported());
  }
}
