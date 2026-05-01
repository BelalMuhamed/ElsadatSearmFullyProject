import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule
} from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';
import { saveAs } from 'file-saver';

import { PlumberService } from '../../app/Services/plumber.service';
import { SwalService } from '../../app/Services/swal.service';
import {
  PlumberDto,
  PlumberFilteration,
  PLUMBER_SPECIALTY_OPTIONS
} from '../../app/models/IPlumberModels';
import { PlumberImportDialogComponent } from '../plumber-import-dialog/plumber-import-dialog.component';

interface ColumnDef {
  key: keyof PlumberDto | 'actions' | 'isDeleted';
  label: string;
  type?: 'text' | 'boolean' | 'actions';
}

/**
 * PlumberComponent — the plumbers list page.
 *
 * Mirrors SupplierComponent in shape, with these additions:
 *  - License + Specialty filter fields
 *  - Specialty column in the table
 *  - Export-to-Excel button (honours active filters)
 */
@Component({
  selector: 'app-plumber',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatTableModule,
    MatIconModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatPaginatorModule,
    MatSelectModule,
    MatTooltipModule,
    RouterLink
  ],
  templateUrl: './plumber.component.html',
  styleUrls: ['./plumber.component.css']
})
export class PlumberComponent implements OnInit {
  private readonly plumberService = inject(PlumberService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly alert = inject(SwalService);
  private readonly destroyRef = inject(DestroyRef);

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly specialtyOptions = PLUMBER_SPECIALTY_OPTIONS;

  isLoaded = false;
  totalCount = 0;
  isExporting = false;

  filters: PlumberFilteration = {
    name: null,
    phoneNumbers: null,
    licenseNumber: null,
    specialty: null,
    isDeleted: false,
    page: 1,
    pageSize: 10
  };

  form!: FormGroup;
  dataSource = new MatTableDataSource<PlumberDto>([]);

  readonly columns: ColumnDef[] = [
    { key: 'name',          label: 'الاسم',         type: 'text' },
    { key: 'phoneNumbers',  label: 'رقم الهاتف',    type: 'text' },
    { key: 'licenseNumber', label: 'رقم الرخصة',    type: 'text' },
    { key: 'specialty',     label: 'التخصص',        type: 'text' },
    { key: 'cityName',      label: 'المدينة',       type: 'text' },
    { key: 'isDeleted',     label: 'الحالة',        type: 'boolean' },
    { key: 'actions',       label: 'إجراءات',       type: 'actions' }
  ];
  readonly displayedColumnKeys = this.columns.map(c => c.key as string);

  // ------------------------------------------------------------
  // Lifecycle
  // ------------------------------------------------------------
  ngOnInit(): void {
    this.form = this.fb.group({
      name: '',
      phoneNumbers: '',
      licenseNumber: '',
      specialty: '',
      isDeleted: false   // default to "active only"
    });

    this.loadPlumbers();
  }

  // ------------------------------------------------------------
  // Data
  // ------------------------------------------------------------
  private loadPlumbers(): void {
    this.isLoaded = false;

    this.plumberService.getAllPlumbers(this.filters)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isLoaded = true;
          if (res.isSuccess && res.data) {
            this.dataSource.data = res.data.data ?? [];
            this.totalCount = res.data.totalCount ?? 0;
          } else {
            this.alert.error(res.message ?? 'حدث خطأ أثناء تحميل السباكين');
          }
        },
        error: err => {
          this.isLoaded = true;
          this.alert.error(err?.error?.message ?? 'حدث خطأ أثناء تحميل السباكين');
        }
      });
  }

  // ------------------------------------------------------------
  // Filters
  // ------------------------------------------------------------
  applyFilters(): void {
    const v = this.form.value;
    this.filters = {
      ...this.filters,
      name: v.name?.trim() || null,
      phoneNumbers: v.phoneNumbers?.trim() || null,
      licenseNumber: v.licenseNumber?.trim() || null,
      specialty: v.specialty?.trim() || null,
      isDeleted: v.isDeleted,
      page: 1,
      pageSize: this.filters.pageSize ?? 10
    };
    if (this.paginator) this.paginator.firstPage();
    this.loadPlumbers();
  }

  resetFilters(): void {
    this.form.reset({
      name: '', phoneNumbers: '', licenseNumber: '', specialty: '', isDeleted: null
    });
    this.filters = {
      name: null,
      phoneNumbers: null,
      licenseNumber: null,
      specialty: null,
      isDeleted: null,
      page: 1,
      pageSize: 10
    };
    if (this.paginator) this.paginator.firstPage();
    this.loadPlumbers();
  }

  // ------------------------------------------------------------
  // Pagination
  // ------------------------------------------------------------
  onPageChange(event: PageEvent): void {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadPlumbers();
  }

  // ------------------------------------------------------------
  // Toggle status — pessimistic (mutate after server confirms)
  // ------------------------------------------------------------
  async togglePlumberStatus(plumber: PlumberDto, nextActive: boolean): Promise<void> {
    if (!plumber.id) return;

    const willDeactivate = !nextActive;
    if (willDeactivate) {
      const result = await this.alert.confirm(
        `هل أنت متأكد من إيقاف السباك "${plumber.name}"؟`
      );
      if (!result.isConfirmed) {
        this.dataSource.data = [...this.dataSource.data];
        return;
      }
    }

    this.plumberService.toggleStatus(plumber.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          if (res.isSuccess) {
            plumber.isDeleted = !nextActive;
            this.dataSource.data = [...this.dataSource.data];
            this.alert.success(res.message ?? 'تم تحديث حالة السباك');
          } else {
            this.dataSource.data = [...this.dataSource.data];
            this.alert.error(res.message ?? 'تعذر تحديث الحالة');
          }
        },
        error: err => {
          this.dataSource.data = [...this.dataSource.data];
          this.alert.error(err?.error?.message ?? 'تعذر تحديث الحالة');
        }
      });
  }

  // ------------------------------------------------------------
  // Excel actions
  // ------------------------------------------------------------
  downloadTemplate(): void {
    this.plumberService.downloadTemplate()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => saveAs(blob, 'Plumbers_Template.xlsx'),
        error: () => this.alert.error('تعذر تحميل القالب')
      });
  }

  openImportDialog(): void {
    const ref = this.dialog.open(PlumberImportDialogComponent, {
      width: '860px',
      maxWidth: '95vw',
      disableClose: true,
      autoFocus: true,
      panelClass: 'custom-popup-panel'
    });
    ref.afterClosed().subscribe((imported: boolean) => {
      if (imported) this.loadPlumbers();
    });
  }

  exportToExcel(): void {
    if (this.isExporting) return;
    this.isExporting = true;

    this.plumberService.exportToExcel(this.filters)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: blob => {
          this.isExporting = false;
          const ts = new Date().toISOString().slice(0, 19).replace(/[T:]/g, '-');
          saveAs(blob, `Plumbers_${ts}.xlsx`);
        },
        error: () => {
          this.isExporting = false;
          this.alert.error('تعذر تصدير البيانات');
        }
      });
  }
}
