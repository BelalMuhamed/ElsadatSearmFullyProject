import { CommonModule, DatePipe } from '@angular/common';
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
import { MatOption } from '@angular/material/core';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import saveAs from 'file-saver';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';

import { AddEditMerchDisPopupComponent } from '../../app/Popups/add-edit-merch-dis-popup/add-edit-merch-dis-popup.component';
import { ImportExcelDialogComponent } from '../import-excel-dialog/import-excel-dialog.component';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { DisAndMerchantService } from '../../app/Services/dis-and-merchant.service';
import { SwalService } from '../../app/Services/swal.service';
import {
  DistributorsAndMerchantsDto,
  DistributorsAndMerchantsFilters
} from '../../app/models/IDisAndMercDto';

@Component({
  selector: 'app-dis-and-merchant',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatTooltipModule,
    MatOption,
    DatePipe
  ],
  templateUrl: './dis-and-merchant.component.html',
  styleUrl: './dis-and-merchant.component.css'
})
export class DisAndMerchantComponent implements OnInit, OnDestroy {

  // ───────── state ─────────
  Searchform!: FormGroup;
  isLoading = true;
  isSavingRow = false;

  filters: DistributorsAndMerchantsFilters = {
    page: 1,
    pageSize: 10,
    cityName: null,
    fullName: null,
    phoneNumber: null,
    type: null,
    isDeleted: null
  };

  columns: ColumnDef[] = [
    { key: 'fullName',    label: 'الاسم بالكامل',  type: 'text' },
    { key: 'address',     label: 'العنوان',        type: 'text' },
    { key: 'type',        label: 'النوع',          type: 'DisOrMecrhant' },
    { key: 'phoneNumber', label: 'رقم الهاتف',     type: 'text' },
    { key: 'cityName',    label: 'المدينة',        type: 'text' },
    { key: 'createdAt',   label: 'تاريخ الإنشاء',   type: 'date' },
    { key: 'createdBy',   label: 'أنشئ بواسطة',    type: 'text' },
    { key: 'updatedAt',   label: 'تاريخ التعديل',   type: 'date' },
    { key: 'updatedBy',   label: 'عُدّل بواسطة',    type: 'text' },
    { key: 'isDelted',    label: 'حالة الحذف',     type: 'boolean' },
    { key: 'deletedAt',   label: 'تاريخ الحذف',    type: 'date' },
    { key: 'deletedBy',   label: 'حُذف بواسطة',     type: 'text' },
    { key: 'actions',     label: 'الإجراءات',      type: 'actions' }
  ];

  displayedColumnKeys = this.columns.map(c => c.key);
  dataSource = new MatTableDataSource<DistributorsAndMerchantsDto>([]);
  totalCount = 0;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  // ───────── deps ─────────
  private readonly subs = new Subscription();
  private readonly fb = inject(FormBuilder);
  private readonly disMerchService = inject(DisAndMerchantService);
  private readonly swal = inject(SwalService);
  private readonly dialog = inject(MatDialog);

  // ───────── lifecycle ─────────

  ngOnInit(): void {
    this.initSearchForm();
    this.loadList();
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // ───────── data loading ─────────

  loadList(): void {
    this.isLoading = true;
    this.subs.add(
      this.disMerchService.getAllDisAndMerch(this.filters).subscribe({
        next: res => {
          this.dataSource.data = res.data ?? [];
          this.totalCount = res.totalCount ?? 0;
          this.isLoading = false;
        },
        error: err => {
          this.isLoading = false;
          this.swal.error(err?.error?.message ?? 'حدثت مشكلة أثناء تحميل البيانات');
        }
      })
    );
  }

  // ───────── pagination ─────────

  PageChange(event: PageEvent): void {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.loadList();
  }

  // ───────── search ─────────

  private initSearchForm(): void {
    this.Searchform = this.fb.group({
      phoneNumber: [''],
      fullName:    [''],
      cityName:    [''],
      type:        [null],
      isDeleted:   [false]
    });
  }

  onSearch(): void {
    if (this.Searchform.invalid) return;

    const v = this.Searchform.value;
    this.filters = {
      ...this.filters,
      page: 1,                             // ✅ reset to page 1 on filter change
      fullName:    v.fullName    || null,
      phoneNumber: v.phoneNumber || null,
      cityName:    v.cityName    || null,
      type:        v.type        ?? null,
      isDeleted:   v.isDeleted   ?? null
    };
    this.loadList();
  }

  ReAsign(): void {
    this.filters = {
      ...this.filters,
      page: 1,
      fullName: null,
      phoneNumber: null,
      cityName: null,
      type: null,
      isDeleted: null
    };
    this.initSearchForm();
    this.loadList();
  }

  // ───────── add / edit ─────────

  openAddPopup(): void {
    const ref = this.dialog.open(AddEditMerchDisPopupComponent, {
      width: '60%',
      maxWidth: '95vw',
      height: 'auto',
      maxHeight: '92vh',
      data: null,
      panelClass: 'custom-popup-panel',
      disableClose: true,
      autoFocus: 'first-tabbable',
      restoreFocus: true
    });

    this.subs.add(
      ref.afterClosed().subscribe((payload: DistributorsAndMerchantsDto | null) => {
        if (!payload) return;
        this.subs.add(
          this.disMerchService.AddDisOrMerchant(payload).subscribe({
            next: () => {
              this.swal.success('تمت الإضافة بنجاح');
              this.loadList();
            },
            error: err => {
              this.swal.error(err?.error?.message ?? 'هناك مشكلة في الخادم');
            }
          })
        );
      })
    );
  }

  openEditPopup(row: DistributorsAndMerchantsDto): void {
    const ref = this.dialog.open(AddEditMerchDisPopupComponent, {
      width: '60%',
      maxWidth: '95vw',
      height: 'auto',
      maxHeight: '92vh',
      data: { ...row },                    // pass a COPY — avoid in-place mutation
      panelClass: 'custom-popup-panel',
      disableClose: true,
      autoFocus: 'first-tabbable',
      restoreFocus: true
    });

    this.subs.add(
      ref.afterClosed().subscribe((payload: DistributorsAndMerchantsDto | null) => {
        if (!payload) return;
        this.subs.add(
          this.disMerchService.EditDisOrMerchant(payload).subscribe({
            next: () => {
              this.swal.success('تم التعديل بنجاح');
              this.loadList();
            },
            error: err => {
              this.swal.error(err?.error?.message ?? 'هناك مشكلة في الخادم');
            }
          })
        );
      })
    );
  }

  // ───────── soft-delete toggle ─────────

  ToggleCategoryStatus(row: DistributorsAndMerchantsDto, checked: boolean): void {
    // Optimistic UI: send a copy with the new flag — revert UI on error.
    const payload: DistributorsAndMerchantsDto = {
      ...row,
      isDelted: !checked
    };

    this.subs.add(
      this.disMerchService.EditDisOrMerchant(payload).subscribe({
        next: () => {
          this.swal.success(checked ? 'تم التفعيل بنجاح' : 'تم التعطيل بنجاح');
          this.loadList();
        },
        error: err => {
          row.isDelted = !row.isDelted;    // revert
          this.swal.error(err?.error?.message ?? 'تعذّر تغيير الحالة');
        }
      })
    );
  }

  // ───────── details popup ─────────

  GetDisDetailsByUserId(userId: string): void {
    this.subs.add(
      this.disMerchService.getById(userId).subscribe({
        next: res => {
          const data = res.data;
          const cashBalance = Number(data?.cashBalance ?? 0);
          const pointsBalance = Number(data?.pointsBalance ?? 0);

          Swal.fire({
            title: 'تفاصيل الحساب',
            html: `
              <div class="swal-details">
                <div class="row"><span>الاسم:</span><strong>${data?.fullName ?? '-'}</strong></div>
                <div class="row"><span>رصيد النقاط:</span><strong class="points">${pointsBalance}</strong></div>
                <div class="row">
                  <span>الرصيد المالي:</span>
                  <strong class="${cashBalance >= 0 ? 'positive' : 'negative'}">
                    ${cashBalance <= 0 ? cashBalance.toFixed(2) : (-cashBalance).toFixed(2)}
                  </strong>
                </div>
              </div>`,
            confirmButtonText: 'إغلاق',
            background: document.body.classList.contains('dark-mode') ? '#1a1a1a' : '#fff',
            color: document.body.classList.contains('dark-mode') ? '#fff' : '#000'
          });
        },
        error: err => {
          this.swal.error(err?.error?.message ?? 'حدثت مشكلة أثناء الاتصال');
        }
      })
    );
  }

  // ───────── Excel ─────────

  downloadTemplate(): void {
    this.subs.add(
      this.disMerchService.downloadImportTemplate().subscribe({
        next: blob => saveAs(blob, 'DistributorsAndMerchants_Template.xlsx'),
        error: () => this.swal.error('تعذر تحميل القالب')
      })
    );
  }

  openDistributorMerchantImport(): void {
    const ref = this.dialog.open(
      ImportExcelDialogComponent<DistributorsAndMerchantsDto>,
      {
        width: '860px',
        maxWidth: '95vw',
        disableClose: true,
        data: {
          title: 'استيراد موزعين / تجار / وكلاء',
          fileHint: 'يجب أن يحتوي الملف على البيانات حسب القالب المحدد',
          templateName: 'تحميل قالب الموزعين والتجار',
          importFn: (file: File) => this.disMerchService.importFromExcel(file),
          columns: ['fullName', 'address', 'phoneNumber', 'type']
        }
      }
    );

    this.subs.add(
      ref.afterClosed().subscribe(() => this.loadList())
    );
  }
}
