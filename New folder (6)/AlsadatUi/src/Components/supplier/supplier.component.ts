import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
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
import { Subscription } from 'rxjs';
import { saveAs } from 'file-saver';

import { SupplierService } from '../../app/Services/supplier.service';
import { SwalService } from '../../app/Services/swal.service';
import { SupplierDto, SupplierFilteration } from '../../app/models/ISupplierModels';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { SupplierImportDialogComponent } from '../supplier-import-dialog/supplier-import-dialog.component';

/**
 * SupplierComponent — the suppliers list page.
 *
 * Updated in this iteration:
 *  - 3-state isDeleted filter (All / Active / Archived) (B-10)
 *  - Pessimistic toggle with server confirmation — mutate AFTER success (B-11, Option A)
 *  - Confirmation dialog before deactivation (not before reactivation)
 *  - Import/Template buttons in the toolbar
 *  - Dedicated toggle endpoint (no more full-DTO PUT)
 *  - Uses SwalService, not direct Swal.fire
 */
@Component({
  selector: 'app-supplier',
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
  templateUrl: './supplier.component.html',
  styleUrls: ['./supplier.component.css']
})
export class SupplierComponent implements OnInit, OnDestroy {
  private readonly supplierService = inject(SupplierService);
  private readonly dialog = inject(MatDialog);
  private readonly fb = inject(FormBuilder);
  private readonly alert = inject(SwalService);

  private readonly subs = new Subscription();

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  readonly columns: ColumnDef[] = [
    { key: 'name',         label: 'اسم المورد' },
    { key: 'phoneNumbers', label: 'رقم الهاتف' },
    { key: 'address',      label: 'العنوان' },
    { key: 'cityName',     label: 'المدينة' },
    { key: 'isDeleted',    label: 'الحالة', type: 'boolean' },
    { key: 'actions',      label: 'الإجراءات', type: 'actions' }
  ];

  readonly displayedColumnKeys = this.columns.map(c => c.key);

  filters: SupplierFilteration = {
    name: null,
    phoneNumbers: null,
    isDeleted: null,          // null = all by default
    page: 1,
    pageSize: 10
  };

  form!: FormGroup;
  dataSource = new MatTableDataSource<SupplierDto>([]);
  totalCount = 0;
  isLoaded = false;

  // -----------------------------------------------------------
  // Lifecycle
  // -----------------------------------------------------------
  ngOnInit(): void {
    this.initForm();
    this.loadSuppliers();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  private initForm(): void {
    this.form = this.fb.group({
      name:         [''],
      phoneNumbers: [''],
      isDeleted:    [null]   // null = all, false = active, true = archived
    });
  }

  // -----------------------------------------------------------
  // Data loading
  // -----------------------------------------------------------
  private loadSuppliers(): void {
    this.isLoaded = false;
    this.subs.add(
      this.supplierService.getAllSuppliers(this.filters).subscribe({
        next: res => {
          this.isLoaded = true;
          if (res.isSuccess && res.data) {
            this.dataSource.data = res.data.data ?? [];
            this.totalCount = res.data.totalCount ?? 0;
          } else {
            this.alert.error(res.message ?? 'تعذر تحميل الموردين');
          }
        },
        error: err => {
          this.isLoaded = true;
          this.alert.error(err?.error?.message ?? 'حدث خطأ أثناء تحميل الموردين');
        }
      })
    );
  }

  // -----------------------------------------------------------
  // Filters
  // -----------------------------------------------------------
  applyFilters(): void {
    const v = this.form.value;
    this.filters = {
      ...this.filters,
      name: v.name?.trim() || null,
      phoneNumbers: v.phoneNumbers?.trim() || null,
      isDeleted: v.isDeleted,  // null | false | true — matches the select options
      page: 1,
      pageSize: this.filters.pageSize ?? 10
    };
    if (this.paginator) this.paginator.firstPage();
    this.loadSuppliers();
  }

  resetFilters(): void {
    this.form.reset({ name: '', phoneNumbers: '', isDeleted: null });
    this.filters = {
      name: null,
      phoneNumbers: null,
      isDeleted: null,
      page: 1,
      pageSize: 10
    };
    if (this.paginator) this.paginator.firstPage();
    this.loadSuppliers();
  }

  // -----------------------------------------------------------
  // Pagination
  // -----------------------------------------------------------
  onPageChange(event: PageEvent): void {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadSuppliers();
  }

  // -----------------------------------------------------------
  // Toggle status — pessimistic (Option A)
  //   UI flips only after the server confirms success.
  //   If the user is about to DEACTIVATE, we ask for confirmation first.
  // -----------------------------------------------------------
  async toggleSupplierStatus(supplier: SupplierDto, nextActive: boolean): Promise<void> {
    // `nextActive` = the value the toggle will have after the click.
    // The "row is active" when !isDeleted, so deactivation = nextActive === false.
    const willDeactivate = !nextActive;

    if (!supplier.id) return;

    if (willDeactivate) {
      const result = await this.alert.confirm(
        `هل أنت متأكد من إيقاف المورد "${supplier.name}"؟`
      );
      if (!result.isConfirmed) {
        // Because the slide-toggle already flipped visually, force it back.
        // We achieve that by re-assigning the array reference — Angular re-renders
        // from the unchanged `isDeleted` on the row.
        this.dataSource.data = [...this.dataSource.data];
        return;
      }
    }

    this.subs.add(
      this.supplierService.toggleStatus(supplier.id).subscribe({
        next: res => {
          if (res.isSuccess) {
            supplier.isDeleted = !nextActive;   // <-- mutate only after success (B-11)
            this.dataSource.data = [...this.dataSource.data]; // force re-render
            this.alert.success(res.message ?? 'تم تحديث حالة المورد');
          } else {
            this.dataSource.data = [...this.dataSource.data]; // rollback UI
            this.alert.error(res.message ?? 'تعذر تحديث حالة المورد');
          }
        },
        error: err => {
          this.dataSource.data = [...this.dataSource.data]; // rollback UI
          this.alert.error(err?.error?.message ?? 'تعذر تحديث حالة المورد');
        }
      })
    );
  }

  // -----------------------------------------------------------
  // Excel: template download
  // -----------------------------------------------------------
  downloadTemplate(): void {
    this.subs.add(
      this.supplierService.downloadTemplate().subscribe({
        next: blob => {
          saveAs(blob, 'Suppliers_Template.xlsx');
        },
        error: () => this.alert.error('تعذر تحميل القالب')
      })
    );
  }

  // -----------------------------------------------------------
  // Excel: open import dialog
  // -----------------------------------------------------------
  openImportDialog(): void {
    const ref = this.dialog.open(SupplierImportDialogComponent, {
      width: '860px',
      maxWidth: '95vw',
      disableClose: true,
      autoFocus: true,
      panelClass: 'custom-popup-panel'
    });

    ref.afterClosed().subscribe((imported: boolean) => {
      if (imported) this.loadSuppliers();
    });
  }
}
