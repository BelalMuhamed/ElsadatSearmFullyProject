import { CommonModule } from '@angular/common';
import { Component, computed, inject, input, OnDestroy, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SwalService } from '../../app/Services/swal.service';
import { Subscription } from 'rxjs';
import { ExcelImportResult, ImportExcelConfig } from '../../app/models/IExcelDtos';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-import-excel-dialog',
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
  templateUrl: './import-excel-dialog.component.html',
  styleUrls: ['./import-excel-dialog.component.css']
})
export class ImportExcelDialogComponent<T> implements OnDestroy {
readonly isDragging = signal(false);
  private readonly dialogRef = inject(MatDialogRef<ImportExcelDialogComponent<T>>);
  private readonly alert = inject(SwalService);
  private readonly subs = new Subscription();

  // ⚙️ injected config

config = inject(MAT_DIALOG_DATA) as ImportExcelConfig<T>;
  // ---- State ----
  step = signal<'select' | 'uploading' | 'result'>('select');
  progress = signal(0);
  file = signal<File | null>(null);
  result = signal<ExcelImportResult<T> | null>(null);

  readonly errorColumns = ['rowNumber', 'column', 'message'];

  // ---- computed ----
  hasErrors = computed(() => (this.result()?.errors?.length ?? 0) > 0);

  upload(): void {
    const file = this.file();
    if (!file) return;

    this.step.set('uploading');
    this.progress.set(0);

    this.subs.add(
      this.config.importFn(file).subscribe({
        next: res => {
          if (res.isSuccess && res.data) {
            this.result.set(res.data);
            this.step.set('result');
          } else {
            this.alert.error(res.message ?? 'Upload failed');
            this.step.set('select');
          }
        },
        error: () => {
          this.alert.error('Upload error');
          this.step.set('select');
        }
      })
    );
  }

  exportErrors(): void {
    const errors = this.result()?.errors ?? [];
    if (!errors.length) return;

    const rows = errors.map(e => ({
      Row: e.rowNumber,
      Column: e.column,
      Message: e.message
    }));

    const ws = XLSX.utils.json_to_sheet(rows);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Errors');

    const buffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
    saveAs(new Blob([buffer]), 'Import_Errors.xlsx');
  }
onBrowse(event: Event): void {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];

  if (file) {
    this.file.set(file);
  }

  input.value = '';
}
  close(): void {
    this.dialogRef.close(this.hasErrors());
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
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
  if (file) {
    this.file.set(file);
  }
}
formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
}
}
