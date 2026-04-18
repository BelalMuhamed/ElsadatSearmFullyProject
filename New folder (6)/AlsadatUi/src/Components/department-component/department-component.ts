import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCardModule } from '@angular/material/card';
import { DepartmentService } from '../../app/Services/department.service';
import { Department } from '../../app/models/IDepartment';
import { DepartmentDialogComponent } from './department-dialog/department-dialog.component';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-department',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatCardModule, MatSlideToggleModule, MatTooltipModule],
  templateUrl: './department-component.html',
  styleUrls: ['./department-component.css']
})
export class DepartmentComponent implements OnInit {
  private departmentService = inject(DepartmentService);
  private dialog = inject(MatDialog);

  departments: Department[] = [];
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  ngOnInit(): void {
    this.loadDepartments();
  }

  toggleDepartmentStatus(dept: Department): void {
    if (!dept) return;
    if (dept.isDeleted) {
      this.departmentService.restore(dept).subscribe({
        next: () => {
          dept.isDeleted = false;
          Swal.fire('تم', 'تم استعادة القسم', 'success');
          this.loadDepartments();
        },
        error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
      });
    } else {
      this.departmentService.softDelete(dept).subscribe({
        next: () => {
          dept.isDeleted = true;
          Swal.fire('تم', 'تم تعطيل القسم', 'success');
          this.loadDepartments();
        },
        error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
      });
    }
  }

  loadDepartments(): void {
    this.isLoading = true;
    this.departmentService.getAll(this.currentPage, this.pageSize).subscribe({
      next: (res: any) => {
        if (res?.items && Array.isArray(res.items)) {
          this.departments = res.items;
          this.totalCount = res.totalCount ?? res.items.length;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل تحميل الأقسام', 'error');
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  openAdd(): void {
    const ref = this.dialog.open(DepartmentDialogComponent, { data: null, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => {
      if (saved) this.loadDepartments();
    });
  }

  openEdit(dept: Department): void {
    const ref = this.dialog.open(DepartmentDialogComponent, { data: dept, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => {
      if (saved) this.loadDepartments();
    });
  }

  softDelete(dept: Department): void {
    Swal.fire({
      title: 'تأكيد',
      text: `هل تريد تعطيل القسم ${dept.name}?`,
      icon: 'warning',
      showCancelButton: true
    }).then((r) => {
      if (r.isConfirmed) {
        this.departmentService.softDelete(dept).subscribe({
          next: () => {
            Swal.fire('تم', 'تم تعطيل القسم', 'success');
            this.loadDepartments();
          },
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
        });
      }
    });
  }

  restore(dept: Department): void {
    Swal.fire({
      title: 'تأكيد',
      text: `هل تريد استعادة القسم ${dept.name}?`,
      icon: 'question',
      showCancelButton: true
    }).then((r) => {
      if (r.isConfirmed) {
        this.departmentService.restore(dept).subscribe({
          next: () => {
            Swal.fire('تم', 'تم استعادة القسم', 'success');
            this.loadDepartments();
          },
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
        });
      }
    });
  }

  hardDelete(dept: Department): void {
    Swal.fire({
      title: 'حذف نهائي',
      text: `هل تريد حذف القسم ${dept.name} نهائياً؟`,
      icon: 'warning',
      showCancelButton: true
    }).then((r) => {
      if (r.isConfirmed) {
        this.departmentService.hardDelete(dept).subscribe({
          next: () => {
            Swal.fire('تم', 'تم الحذف نهائياً', 'success');
            this.loadDepartments();
          },
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
        });
      }
    });
  }

  onPageChange(event: any): void {
    // PageEvent: {pageIndex, pageSize, length}
    this.pageSize = event.pageSize || this.pageSize;
    this.currentPage = (event.pageIndex != null) ? (event.pageIndex + 1) : this.currentPage;
    this.loadDepartments();
  }
}
