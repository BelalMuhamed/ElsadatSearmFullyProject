import { Component, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, PageEvent, MatPaginatorModule } from '@angular/material/paginator';
import { FormsModule } from '@angular/forms';
import { MatFormField, MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { EmployeeAttendanceService } from '../../app/Services/employee-attendance-service';
import { EmployeeAttendanceDto, EmployeeAttendanceFilteration } from '../../app/models/IEmployeeAttendance';
import Swal from 'sweetalert2';
import { MatDialog } from '@angular/material/dialog';
import { HrAttendanceRecordComponent } from '../hr-attendance-record-component/hr-attendance-record-component';

@Component({
  selector: 'app-hr-attendance',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule, MatSelectModule, RouterModule],
  templateUrl: './hr-attendance-component.html',
  styleUrls: ['./hr-attendance-component.css']
})
export class HrAttendanceComponent {
  selectedDateObj: Date | null = null;
  private attendanceService = inject(EmployeeAttendanceService);

  columns = [
    { key: 'employeeCode', label: 'كود الموظف', type: 'string' },
    { key: 'employeeName', label: 'اسم الموظف', type: 'string' },
    { key: 'departmentName', label: 'القسم', type: 'string' },
    { key: 'attendanceDate', label: 'التاريخ', type: 'date' },
    { key: 'checkInTime', label: 'دخول', type: 'string' },
    { key: 'checkOutTime', label: 'انصراف', type: 'string' },
    { key: 'attendanceStatus', label: 'الحالة', type: 'number' }
    // actions column is added in template only
  ];

  displayedColumnKeys = this.columns.map(c => c.key);
  allColumnKeys = [...this.displayedColumnKeys, 'actions'];

  dataSource = new MatTableDataSource<EmployeeAttendanceDto>([]);
  totalCount = 0;
  isLoading = false;

  filters: EmployeeAttendanceFilteration = {
    page: 1,
    pageSize: 10,
    employeeCode: null,
    employeeId: null,
    selectedDate: null,
    startDate: null,
    endDate: null,
    employeeName: null,
    departmentName: null,
    year: null,
    month: null,
    attendanceStatusID: null
  };

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  private dialog = inject(MatDialog);
  
  // Dialog logic for changing status
  openStatusDialog(row: EmployeeAttendanceDto) {
    // For now, just show a simple prompt. You can replace with a proper dialog later.
    Swal.fire({
      title: 'تغيير حالة التسجيل',
      input: 'select',
      inputOptions: {
        '1': 'حاضر',
        '2': 'غائب',
        '3': 'متأخر',
        '4': 'مستأذن',
        '5': 'إجازة',
        '6': 'مرضية'
      },
      inputValue: row.attendanceStatus ? String(row.attendanceStatus) : '1',
      showCancelButton: true,
      confirmButtonText: 'تغيير',
      cancelButtonText: 'إلغاء',
      preConfirm: (value) => value
    }).then(result => {
      if (result.isConfirmed && result.value) {
        this.changeAttendanceStatus(row, Number(result.value));
      }
    });
  }

  changeAttendanceStatus(row: EmployeeAttendanceDto, newStatus: number) {
    // Call backend API to update status (PUT) - send full DTO
    this.attendanceService.changeStatus(row, newStatus).subscribe({
      next: (res) => {
        if (res?.isSuccess) {
          Swal.fire('تم التغيير', res.message ?? 'تم تغيير حالة التسجيل بنجاح', 'success');
          this.getAttendance();
        } else {
          Swal.fire('خطأ', res?.message ?? 'لم يتم تغيير الحالة', 'error');
        }
      },
      error: (err) => {
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ أثناء تغيير الحالة', 'error');
      }
    });
  }

  ngOnInit(): void {
    this.getAttendance();
  }


  onFileSelected(event: any) {
    const file: File | null = event?.target?.files?.[0] ?? null;
    if (!file) return;
    Swal.fire({ title: 'جاري رفع الملف...', didOpen: () => { Swal.showLoading(); } });
    this.attendanceService.uploadFromFile(file).subscribe({
      next: (res) => {
        Swal.close();
        if (res?.isSuccess) {
          const data = res.data as any;
          const successMessages: string[] = data?.successMessages ?? [];
          const errorRecords: any[] = data?.errorRecords ?? [];

          let html = `<div style="text-align:right; direction:rtl; font-family: 'Cairo', sans-serif; color:var(--text-color); background:var(--sidenav-bg); padding:12px; border-radius:8px;">
            <p style="color:var(--gold); margin:0 0 8px 0;"><strong>تم استيراد السجلات:</strong> <span style=\"color:var(--text-color)\">${successMessages.length}</span></p>`;

          if (successMessages.length > 0) {
            html += `<ul style="margin:0 0 8px 0; padding-inline-start: 18px;">`;
            successMessages.slice(0, 20).forEach(m => html += `<li style=\"color:var(--text-color)\">${m}</li>`);
            if (successMessages.length > 20) html += `<li style=\"color:var(--text-color)\">... ${successMessages.length - 20} المزيد</li>`;
            html += `</ul>`;
          }

          html += `<p style="margin-top:8px; color:var(--text-color);"><strong style=\"color:var(--gold)\">المشكلات:</strong> <span style=\"color:var(--gold)\">${errorRecords.length}</span></p>`;

          if (errorRecords.length > 0) {
            html += `<div style="max-height:300px; overflow:auto; text-align:right; border:1px solid rgba(0,0,0,0.06); padding:8px; border-radius:6px; background:var(--sidenav-bg);">
              <table style="width:100%; border-collapse:collapse; direction:rtl;">
                <thead><tr style="text-align:right; font-weight:700;"><th style=\"color:var(--gold)\">رقم السطر</th><th style=\"color:var(--gold)\">كود الموظف</th><th style=\"color:var(--gold)\">الخطأ</th></tr></thead>
                <tbody>`;
            errorRecords.slice(0, 200).forEach((er: any) => {
              const row = er.rowNumber ?? '-';
              const code = er.employeeCode ?? '-';
              const msg = er.errorMessage ?? er.error ?? '-';
              html += `<tr style="border-top:1px solid rgba(0,0,0,0.04)"><td style="padding:6px; color:var(--text-color)">${row}</td><td style="padding:6px; color:var(--text-color)">${code}</td><td style="padding:6px; color:var(--gold)">${msg}</td></tr>`;
            });
            html += `</tbody></table></div>`;
            if (errorRecords.length > 200) html += `<p style=\"color:var(--text-color)\">... ${errorRecords.length - 200} المزيد</p>`;
          }

          html += `</div>`;

          Swal.fire({
            title: 'نتيجة الاستيراد',
            html,
            icon: errorRecords.length === 0 ? 'success' : 'warning',
            width: '720px',
            confirmButtonText: 'حسناً',
            customClass: {
              popup: 'my-swal-popup',
              title: 'my-swal-title',
              htmlContainer: 'my-swal-text',
              confirmButton: 'my-swal-btn'
            }
          });

          this.getAttendance();
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل استيراد الملف', 'error');
        }
      },
      error: (err) => {
        Swal.close();
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ أثناء رفع الملف', 'error');
      }
    });
  }

  getAttendance() {
    this.isLoading = true;
    // debug logging removed for HR attendance filters
    this.attendanceService.getByFilter(this.filters).subscribe({
      next: (res: any) => {
        // server response handling (debug logging removed)
        const items = (res && (res.items || res.Items)) || (res && res.data && Array.isArray(res.data) ? res.data : (res && res.data && (res.data.items || res.data.Items))) || null;

        if (Array.isArray(items)) {
          this.dataSource.data = items;
          this.totalCount = (res && (res.totalCount || res.TotalCount)) || (res && res.data && (res.data.totalCount || res.data.TotalCount)) || items.length || 0;
        } else if (Array.isArray(res)) {
          this.dataSource.data = res;
          this.totalCount = res.length || 0;
        } else {
          this.dataSource.data = [];
          this.totalCount = 0;
        }

        const serverPage = (res && (res.page || res.Page || res.currentPage || res.CurrentPage || res.pageNumber || res.PageNumber));
        const serverPageSize = (res && (res.pageSize || res.PageSize)) || (res && res.data && (res.data.pageSize || res.data.PageSize));
        if (serverPage != null) this.filters.page = Number(serverPage);
        if (serverPageSize != null) this.filters.pageSize = Number(serverPageSize);
        setTimeout(() => {
          try {
            if (this.paginator) {
              this.paginator.pageIndex = (this.filters.page || 1) - 1;
              this.paginator.length = this.totalCount || 0;
              this.paginator.pageSize = this.filters.pageSize || this.paginator.pageSize;
            }
          } catch (e) {
          }
        }, 0);

        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', (err && err.error && err.error.message) || 'حدث خطأ في السيرفر', 'error');
      }
    });
  }

  onPageChange(event: PageEvent) {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.getAttendance();
  }

  onFilter() {
    if (this.filters.employeeCode) {
      this.filters.employeeCode = this.filters.employeeCode.trim();
    }
    if (this.selectedDateObj) {
      const d = this.selectedDateObj;
      const yyyy = d.getFullYear();
      const mm = String(d.getMonth() + 1).padStart(2, '0');
      const dd = String(d.getDate()).padStart(2, '0');
      this.filters.selectedDate = `${yyyy}-${mm}-${dd}`;
    } else {
      this.filters.selectedDate = null;
    }
    this.filters.page = 1;
    this.getAttendance();
  }

  onReset() {
    this.filters = { ...this.filters, employeeCode: null, employeeId: null, selectedDate: null, employeeName: null, departmentName: null, year: null, month: null, attendanceStatusID: null, page: 1 };
    this.selectedDateObj = null;
    this.getAttendance();
  }

  // Map backend AttendanceStatus enum (from backend):
  // Present = 1, Absent = 2, Late = 3, Excused = 4, Vacation = 5, SickLeave = 6
  getStatusText(status?: number | null) {
    switch (status) {
      case 1:
        return 'حاضر';
      case 2:
        return 'غائب';
      case 3:
        return 'متأخر';
      case 4:
        return 'معتذر';
      case 5:
        return 'إجازة';
      case 6:
        return 'إجازة مرضية';
      default:
        return '-';
    }
  }

  // Format time strings like "09:00:00" into Arabic human-readable labels.
  // Examples:
  //  - "09:00:00" -> "الساعة 9 صباحاً"
  //  - "18:34:28" -> "الساعة 6:34 مساءً"
  formatTimeToArabicLabel(timeStr?: string | null): string {
    if (!timeStr) return '-';
    const t = String(timeStr).trim();
    if (!t) return '-';

    // Accept formats like HH:mm:ss or HH:mm
    const parts = t.split(':');
    if (parts.length < 1) return '-';

    let hh = Number(parts[0]);
    if (isNaN(hh)) return '-';
    const mm = parts.length >= 2 ? Number(parts[1]) : 0;
    const minutesStr = !isNaN(mm) ? String(mm).padStart(2, '0') : '00';

    const period = hh >= 12 ? 'مساءً' : 'صباحاً';
    const hh12 = hh % 12 === 0 ? 12 : hh % 12;

    if (minutesStr === '00') {
      return ` ${hh12}  ${period}`;
    }
    return ` ${hh12}:${minutesStr}  ${period}`;
  }

  goToDetails(row: EmployeeAttendanceDto) {
    // Placeholder: user can expand to open details dialog or route
    Swal.fire('تفاصيل', `${row.employeeName ?? row.employeeCode} - ${row.attendanceDate}`, 'info');
  }

  performCheckIn(employeeCode?: string) {
    if (!employeeCode) {
      Swal.fire('خطأ', 'لا يوجد كود موظف', 'error');
      return;
    }
    const now = new Date();
    const date = now.toISOString().split('T')[0];
    const time = now.toTimeString().split(' ')[0];
    this.attendanceService.checkIn({ employeeCode, date, inputTime: time }).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          Swal.fire('تم', res.message ?? 'تم تسجيل الحضور', 'success');
          this.getAttendance();
        } else {
          Swal.fire('خطأ', res.message ?? 'لم يتم تسجيل الحضور', 'error');
        }
      },
      error: (err) => {
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  performCheckOut(employeeCode?: string) {
    if (!employeeCode) {
      Swal.fire('خطأ', 'لا يوجد كود موظف', 'error');
      return;
    }
    const now = new Date();
    const date = now.toISOString().split('T')[0];
    const time = now.toTimeString().split(' ')[0];
    this.attendanceService.checkOut({ employeeCode, date, inputTime: time }).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          Swal.fire('تم', res.message ?? 'تم تسجيل الانصراف', 'success');
          this.getAttendance();
        } else {
          Swal.fire('خطأ', res.message ?? 'لم يتم تسجيل الانصراف', 'error');
        }
      },
      error: (err) => {
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }
}
