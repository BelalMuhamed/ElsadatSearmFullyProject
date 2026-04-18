import { Component, ViewChild, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { RepresentativeService } from '../../app/Services/representative-service';
import {
  RepresentativeDTo,
  RepresentativeHelper,
  PaginationParams,
  RepresentiveType
} from '../../app/models/IRepresentative';
import Swal from 'sweetalert2';
import { RepresentativeDetailsDialog } from '../representative-details-dialog/representative-details-dialog';
import { RepresentativeEditDialog } from '../representative-edit-dialog/representative-edit-dialog';

@Component({
  selector: 'app-representatives-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSlideToggleModule,
    MatTooltipModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    RouterModule
  ],
  templateUrl: './representatives-list-component.html',
  styleUrls: ['./representatives-list-component.css']
})
export class RepresentativesListComponent implements OnInit {
  private representativeService = inject(RepresentativeService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  columns = [
    { key: 'representativeCode', label: 'كود المندوب', type: 'string' },
    { key: 'fullName', label: 'اسم المندوب', type: 'string' },
    { key: 'email', label: 'البريد الإلكتروني', type: 'string' },
    { key: 'representiveType', label: 'نوع المندوب', type: 'string' },
    { key: 'salary', label: 'الراتب', type: 'number' },
    { key: 'phoneNumber', label: 'رقم الهاتف', type: 'string' },
    { key: 'cityName', label: 'المدينة', type: 'string' }
  ];

  displayedColumnKeys = this.columns.map((c) => c.key);
  // add status and actions columns
  allColumnKeys = [...this.displayedColumnKeys, 'status', 'actions'];

  dataSource = new MatTableDataSource<RepresentativeDTo>([]);
  totalCount = 0;
  isLoading = false;
  showDeletedOnly = false;

  filters: RepresentativeHelper = {
    representativeCode: undefined,
    representativeName: undefined,
    cityName: undefined,
    isActive: true,
    representiveType: RepresentiveType.None
  };

  paginationParams: PaginationParams = {
    pageNumber: 1,
    pageSize: 10
  };

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.getRepresentatives();
  }

  getRepresentatives(): void {
    this.isLoading = true;

    // Apply soft-delete filter based on toggle
    this.filters.isActive = !this.showDeletedOnly;

    this.representativeService
      .getRepresentativesByFilter(this.paginationParams, this.filters)
      .subscribe({
        next: (res: any) => {
          // Extract data from response
          let items: RepresentativeDTo[] = [];
          let totalCount = 0;
          let serverPage = this.paginationParams.pageNumber || 1;
          let serverPageSize = this.paginationParams.pageSize || 10;

          // Handle different response formats
          if (Array.isArray(res)) {
            items = res;
          } else if (Array.isArray(res?.items)) {
            items = res.items;
          } else if (Array.isArray(res?.Items)) {
            items = res.Items;
          } else if (Array.isArray(res?.data)) {
            items = res.data;
          }

          // Extract total count
          totalCount =
            Number(
              res?.totalCount ??
                res?.TotalCount ??
                res?.total ??
                items.length ??
                0
            ) || 0;

          // Extract pagination info
          serverPage =
            Number(
              res?.currentPage ??
                res?.CurrentPage ??
                res?.pageNumber ??
                res?.PageNumber ??
                this.paginationParams.pageNumber
            ) || this.paginationParams.pageNumber || 1;

          serverPageSize =
            Number(res?.pageSize ?? res?.PageSize ?? this.paginationParams.pageSize) ||
            this.paginationParams.pageSize ||
            10;

          // Update table
          this.dataSource = new MatTableDataSource<RepresentativeDTo>(
            items || []
          );

          if (this.paginator) {
            this.dataSource.paginator = this.paginator;
            setTimeout(() => {
              try {
                this.paginator.pageIndex = Math.max(serverPage - 1, 0);
                this.paginator.length = totalCount;
                this.paginator.pageSize = serverPageSize;
              } catch (e) {
                /* ignore */
              }
            }, 0);
          }

          this.totalCount = totalCount || (items?.length ?? 0);
          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
          Swal.fire(
            'خطأ',
            err?.error?.message ?? 'حدث خطأ في تحميل البيانات',
            'error'
          );
        }
      });
  }

  openDetailsDialog(representative: RepresentativeDTo) {
    this.dialog.open(RepresentativeDetailsDialog, {
      data: { representative },
      width: '720px'
    });
  }

  openEditDialog(representative: RepresentativeDTo) {
    const ref = this.dialog.open(RepresentativeEditDialog, {
      data: { representative },
      width: '720px'
    });
    ref.afterClosed().subscribe((res: any) => {
      if (res?.success) {
        this.getRepresentatives();
      }
    });
  }

  addNewRepresentative() {
    this.router.navigate(['/sales/representatives/add']);
  }

  toggleRepresentativeStatus(representative: RepresentativeDTo) {
    const action = representative.isDeleted ? 'استعادة' : 'حذف';
    Swal.fire({
      title: `تأكيد ${action}`,
      text: `هل تريد ${action} المندوب ${representative.fullName}؟`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'نعم',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        const dto: RepresentativeDTo = { ...representative };

        const call = representative.isDeleted
          ? this.representativeService.restoreRepresentative(dto)
          : this.representativeService.softDeleteRepresentative(dto);

        call.subscribe({
          next: () => {
            const msg = representative.isDeleted
              ? 'تم استعادة المندوب بنجاح'
              : 'تم حذف المندوب بنجاح';
            Swal.fire('نجح', msg, 'success');
            this.getRepresentatives();
          },
          error: (err) => {
            Swal.fire(
              'خطأ',
              err?.error?.message ?? 'حدث خطأ في تعديل الحالة',
              'error'
            );
          }
        });
      } else {
        representative.isDeleted = !representative.isDeleted;
      }
    });
  }

  formatSalary(salary: number | undefined): string {
    if (!salary) return '0.00';
    return new Intl.NumberFormat('ar-EG', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(salary);
  }

  formatRepresentativeType(type: RepresentiveType | string | number | any): string {
    // Handle both number and string types
    const typeValue = typeof type === 'string' ? parseInt(type, 10) : type;
    
    switch (typeValue) {
      case RepresentiveType.Inspection:
      case 1:
        return 'معاينات';
      case RepresentiveType.Collection:
      case 2:
        return 'تحصيل';
      case RepresentiveType.Mixed:
      case 3:
        return 'مختلط';
      default:
        return 'غير معروف';
    }
  }

  onFilter(): void {
    this.paginationParams.pageNumber = 1;
    this.getRepresentatives();
  }

  onReset(): void {
    this.filters = {
      representativeCode: undefined,
      representativeName: undefined,
      cityName: undefined,
      isActive: true,
      representiveType: RepresentiveType.None
    };
    this.paginationParams.pageNumber = 1;
    this.getRepresentatives();
  }

  onPageChange(event: PageEvent): void {
    this.paginationParams.pageNumber = event.pageIndex + 1;
    this.paginationParams.pageSize = event.pageSize;
    this.getRepresentatives();
  }
}
