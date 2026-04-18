import { Component, ViewChild, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { EmployeeDetailsDialog } from '../employee-details-dialog/employee-details-dialog';
import { EmployeeEditDialog } from '../employee-edit-dialog/employee-edit-dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { EmployeeService } from '../../app/Services/employee.service';
import { EmployeeDTo, EmployeeHelper, PaginationParams } from '../../app/models/IEmployee';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-employees-list',
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
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    RouterModule
  ],
  templateUrl: './employees-list-component.html',
  styleUrls: ['./employees-list-component.css']
})
export class EmployeesListComponent implements OnInit {
  private employeeService = inject(EmployeeService);
  private router = inject(Router);
  private dialog = inject(MatDialog);

  columns = [
    { key: 'employeeCode', label: 'كود الموظف', type: 'string' },
    { key: 'fullName', label: 'اسم الموظف', type: 'string' },
    { key: 'email', label: 'البريد الإلكتروني', type: 'string' },
    { key: 'departmentName', label: 'القسم', type: 'string' },
    { key: 'salary', label: 'الراتب', type: 'number' },
    { key: 'phoneNumber', label: 'رقم الهاتف', type: 'string' },
    { key: 'cityName', label: 'المدينة', type: 'string' }
  ];

  displayedColumnKeys = this.columns.map(c => c.key);
  // add a dedicated status column before actions
  allColumnKeys = [...this.displayedColumnKeys, 'status', 'actions'];

  dataSource = new MatTableDataSource<EmployeeDTo>([]);
  totalCount = 0;
  isLoading = false;

  filters: EmployeeHelper = {
    empCode: null,
    departmentName: null,
    employeeName: null,
    cityName: null
  };

  paginationParams: PaginationParams = {
    pageNumber: 1,
    pageSize: 10
  };

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.getEmployees();
  }

  getEmployees(): void {
    this.isLoading = true;

    this.employeeService.getEmployeesByFilter(this.paginationParams, this.filters).subscribe({
      next: (res: any) => {
        // server response received (debug logging removed)

        // Normalize response
        let items: EmployeeDTo[] = [];
        let totalCount = 0;
        let serverPage = this.paginationParams.pageNumber || 1;
        let serverPageSize = this.paginationParams.pageSize || 10;

        // Extract items from multiple possible shapes
        if (Array.isArray(res)) {
          items = res as EmployeeDTo[];
        } else if (Array.isArray(res?.items)) {
          items = res.items;
        } else if (Array.isArray(res?.Items)) {
          items = res.Items;
        } else if (Array.isArray(res?.data)) {
          items = res.data;
        }

        // Extract total count
        totalCount = Number(res?.totalCount ?? res?.TotalCount ?? res?.total ?? items.length ?? 0) || 0;

        // Extract paging info
        serverPage = Number(res?.currentPage ?? res?.CurrentPage ?? res?.pageNumber ?? res?.PageNumber ?? this.paginationParams.pageNumber) || this.paginationParams.pageNumber || 1;
        serverPageSize = Number(res?.pageSize ?? res?.PageSize ?? this.paginationParams.pageSize) || this.paginationParams.pageSize || 10;

        // parsed items count available

        // Update table
        this.dataSource = new MatTableDataSource<EmployeeDTo>(items || []);
        if (this.paginator) {
          this.dataSource.paginator = this.paginator;
          setTimeout(() => {
            try {
              this.paginator.pageIndex = Math.max((serverPage - 1), 0);
              this.paginator.length = totalCount;
              this.paginator.pageSize = serverPageSize;
            } catch (e) { /* ignore */ }
          }, 0);
        }

        this.totalCount = totalCount || (items?.length ?? 0);
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ في تحميل البيانات', 'error');
      }
    });
  }

  // open details dialog (read-only)
  openDetailsDialog(employee: EmployeeDTo) {
    this.dialog.open(EmployeeDetailsDialog, { data: { employee }, width: '720px' });
  }

  // open edit dialog and refresh on success
  openEditDialog(employee: EmployeeDTo) {
    const ref = this.dialog.open(EmployeeEditDialog, { data: { employee }, width: '720px' });
    ref.afterClosed().subscribe((res: any) => {
      if (res && (res.saved || res.success)) this.getEmployees();
    });
  }

  // toggle soft-delete / restore
  toggleEmployeeStatus(employee: EmployeeDTo) {
    if (!employee.employeeCode) return;
    // disable toggle until server responds could be added, for now optimistic update after success
    if (employee.isDeleted) {
      this.employeeService.restoreEmployee(employee).subscribe({
        next: (res: any) => {
          const serverMsg = typeof res === 'string' ? res : (res?.message ?? null);
          const msg = serverMsg ? `${serverMsg}` : `تم تفعيل الموظف ${employee.fullName}`;
          // update local state to show correct toggle immediately
          employee.isDeleted = false;
          Swal.fire('تم', msg, 'success');
        },
        error: (err) => { Swal.fire('خطأ', (err?.error ?? err?.message) ?? 'فشل العملية', 'error'); }
      });
    } else {
      this.employeeService.softDeleteEmployee(employee).subscribe({
        next: (res: any) => {
          const serverMsg = typeof res === 'string' ? res : (res?.message ?? null);
          const msg = serverMsg ? `${serverMsg}` : `تم إيقاف الموظف ${employee.fullName}`;
          // update local state to show correct toggle immediately
          employee.isDeleted = true;
          Swal.fire('تم', msg, 'success');
        },
        error: (err) => { Swal.fire('خطأ', (err?.error ?? err?.message) ?? 'فشل العملية', 'error'); }
      });
    }
  }

  onPageChange(event: PageEvent): void {
    this.paginationParams.pageNumber = event.pageIndex + 1;
    this.paginationParams.pageSize = event.pageSize;
    this.getEmployees();
  }

  onFilter(): void {
    if (this.filters.empCode) {
      this.filters.empCode = this.filters.empCode.trim();
    }
    this.paginationParams.pageNumber = 1;
    this.getEmployees();
  }

  onReset(): void {
    this.filters = {
      empCode: null,
      departmentName: null,
      employeeName: null,
      cityName: null
    };
    this.paginationParams.pageNumber = 1;
    this.getEmployees();
  }

  viewSalary(employee: EmployeeDTo): void {
    if (!employee.employeeCode) {
      Swal.fire('خطأ', 'كود الموظف غير متوفر', 'error');
      return;
    }
    this.router.navigate(['/hr/employee-salary', employee.employeeCode]);
  }

  deleteEmployee(employee: EmployeeDTo): void {
    if (!employee.employeeCode) {
      Swal.fire('خطأ', 'كود الموظف غير متوفر', 'error');
      return;
    }

    Swal.fire({
      title: 'تأكيد الحذف',
      text: `هل أنت متأكد من حذف الموظف ${employee.fullName}؟`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'حذف',
      cancelButtonText: 'إلغاء'
    }).then((result) => {
      if (result.isConfirmed) {
        this.employeeService.softDeleteEmployee(employee).subscribe({
          next: () => {
            Swal.fire('تم', 'تم حذف الموظف بنجاح', 'success');
            this.getEmployees();
          },
          error: (err) => {
            Swal.fire('خطأ', err?.error?.message ?? 'فشل حذف الموظف', 'error');
          }
        });
      }
    });
  }

  formatSalary(salary: number): string {
    return salary ? salary.toLocaleString('ar-EG', { style: 'currency', currency: 'EGP' }) : '-';
  }

  addNewEmployee(): void {
    this.router.navigate(['/hr/employees/add']);
  }
}
