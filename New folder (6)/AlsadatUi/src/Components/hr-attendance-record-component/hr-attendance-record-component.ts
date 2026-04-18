import { Component, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatPaginator } from '@angular/material/paginator';
import { EmployeeAttendanceService } from '../../app/Services/employee-attendance-service';
import Swal from 'sweetalert2';
import { Router } from '@angular/router';

@Component({
  selector: 'app-hr-attendance-record',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule, MatSelectModule, MatTableModule, MatPaginatorModule],
  templateUrl: './hr-attendance-record-component.html',
  styleUrls: ['./hr-attendance-record-component.css']
})
export class HrAttendanceRecordComponent {
  // paginator for today's records (client-side paging)
  dataSource = new MatTableDataSource<any>([]);
  displayedColumns = ['employeeCode','employeeName','attendanceDate','checkInTime','checkOutTime','attendanceStatus','actions'];
  pageSizeOptions = [5,10,20];
  @ViewChild('timeInput') timeInput!: ElementRef;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  employeeCode: string | null = null;
  selectedDateObj: Date | null = new Date();
  selectedTime: string = '';
  isProcessing = false;

  private attendanceService = inject(EmployeeAttendanceService);
  private router = inject(Router);
  todayRecords: any[] = [];
  isLoadingToday = false;
  selectedType: 'checkin' | 'checkout' = 'checkin';
  
  ngOnDestroy() {}
  ngOnInit() {
    this.loadTodayRecords();
  }

  loadTodayRecords() {
    this.isLoadingToday = true;
    // Send today's date as filter if backend expects it
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const todayStr = `${yyyy}-${mm}-${dd}`;
    // If backend expects selectedDate, add it to filters
    this.attendanceService.getByFilter({
      page: 1,
      pageSize: 100,
      selectedDate: todayStr
    } as any).subscribe({
      next: (res: any) => {
        // backend may return paged wrapper with items or data
        if (Array.isArray(res.items)) {
          this.todayRecords = res.items;
        } else if (Array.isArray(res.data)) {
          this.todayRecords = res.data;
        } else if (Array.isArray(res?.data?.items)) {
          this.todayRecords = res.data.items;
        } else {
          this.todayRecords = [];
        }
        // populate table datasource and hook paginator
        this.dataSource.data = this.todayRecords;
        try { this.dataSource.paginator = this.paginator; } catch { }
        this.isLoadingToday = false;
      },
      error: () => {
        this.todayRecords = [];
        this.isLoadingToday = false;
      }
    });
  }

  private formatDate(d: Date) {
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }

  onManualRecord() {
    const code = (this.employeeCode ?? '').trim();
    if (!code) {
      Swal.fire('خطأ', 'برجاء إدخال كود الموظف', 'error');
      return;
    }
    if (!this.selectedDateObj) {
      Swal.fire('خطأ', 'برجاء اختيار تاريخ', 'error');
      return;
    }
    if (!this.selectedTime) {
      Swal.fire('خطأ', 'برجاء اختيار الوقت', 'error');
      return;
    }
    const date = this.formatDate(this.selectedDateObj);
    const time = this.selectedTime;
    this.isProcessing = true;
    const method = this.selectedType === 'checkin' ? this.attendanceService.checkIn : this.attendanceService.checkOut;
    method.call(this.attendanceService, { employeeCode: code, date, inputTime: time }).subscribe({
      next: (res) => {
        if (res?.isSuccess) {
          Swal.fire('تم', res.message ?? 'تم التسجيل بنجاح', 'success');
          this.loadTodayRecords();
        } else {
          Swal.fire('خطأ', res?.message ?? 'لم يتم التسجيل', 'error');
        }
        this.isProcessing = false;
      },
      error: (err) => {
        this.isProcessing = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ أثناء التسجيل', 'error');
      }
    });
  }
  onFileSelected(event: any) {
    const file: File | null = event?.target?.files?.[0] ?? null;
    if (!file) return;
    Swal.fire({ title: 'جاري رفع الملف...', didOpen: () => { Swal.showLoading(); } });
    this.attendanceService.uploadFromFile(file).subscribe({
      next: (res) => {
        Swal.close();
        if (res?.isSuccess) {
          // backend returns data: { successMessages:[], errorRecords:[] }
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
            // build a small table of errors
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
            if (errorRecords.length > 200) html += `<p>... ${errorRecords.length - 200} المزيد</p>`;
          }

          html += `</div>`;

          // Show summary modal
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

          // refresh today's list to reflect newly added records
          this.loadTodayRecords();
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
  goBack() {
    this.router.navigate(['/hr/attendance']);
  }

  // Map backend AttendanceStatus enum -> display text
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

  openStatusDialog(row: any) {
    Swal.fire({
      title: 'تغيير حالة التسجيل',
      input: 'select',
      inputOptions: {
        '1': 'حاضر',
        '2': 'غائب',
        '3': 'متأخر',
        '4': 'معتذر',
        '5': 'إجازة',
        '6': 'مرضية'
      },
      inputValue: row.attendanceStatus ? String(row.attendanceStatus) : '1',
      showCancelButton: true,
      confirmButtonText: 'تغيير',
      cancelButtonText: 'إلغاء',
      preConfirm: (v) => v
    }).then(res => {
      if (res.isConfirmed && res.value) {
        this.changeAttendanceStatus(row, Number(res.value));
      }
    });
  }

  changeAttendanceStatus(row: any, newStatus: number) {
    // Prepare DTO matching backend EmployeeAttendanceDTO
    const dto: any = {
      Id: row.id,
      EmployeeCode: row.employeeCode ?? '',
      EmployeeName: row.employeeName ?? null,
      EmployeeId: row.employeeId ?? null,
      DepartmentId: row.departmentId ?? 0,
      DepartmentName: row.departmentName ?? null,
      AttendanceDate: row.attendanceDate ?? this.formatDate(new Date()),
      CheckInTime: row.checkInTime ?? null,
      CheckOutTime: row.checkOutTime ?? null,
      CheckInMethod: row.checkInMethod ?? null,
      AttendanceStatus: row.attendanceStatus ?? null
    };

    this.attendanceService.changeStatus(dto as any, newStatus).subscribe({
      next: (res) => {
        if (res?.isSuccess) {
          Swal.fire('تم', res.message ?? 'تم تغيير الحالة', 'success');
          this.loadTodayRecords();
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل تغيير الحالة', 'error');
        }
      },
      error: (err) => {
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ أثناء تغيير الحالة', 'error');
      }
    });
  }

  // Format time strings like "09:00:00" into Arabic human-readable labels.
  // Examples:
  //  - "09:00:00" -> "الساعة 9 صباحاً"
  //  - "18:34:28" -> "الساعة 6:34 مساءً"
  formatTimeToArabicLabel(timeStr?: string | null): string {
    if (!timeStr) return '-';
    const t = String(timeStr).trim();
    if (!t) return '-';

    const parts = t.split(':');
    if (parts.length < 1) return '-';

    let hh = Number(parts[0]);
    if (isNaN(hh)) return '-';
    const mm = parts.length >= 2 ? Number(parts[1]) : 0;
    const minutesStr = !isNaN(mm) ? String(mm).padStart(2, '0') : '00';

    const period = hh >= 12 ? 'مساءً' : 'صباحاً';
    const hh12 = hh % 12 === 0 ? 12 : hh % 12;

    if (minutesStr === '00') {
      return ` ${hh12} ${period}`;
    }
    return ` ${hh12}:${minutesStr} ${period}`;
  }

  onReset() {
    this.employeeCode = null;
    this.selectedDateObj = new Date();
    this.selectedTime = '';
  }

  openTimePicker() {
    if (this.timeInput && this.timeInput.nativeElement) {
      const input = this.timeInput.nativeElement as any;
      if (input.showPicker) {
        input.showPicker();
      } else {
        input.focus();
        input.click();
      }
    }
  }
}
