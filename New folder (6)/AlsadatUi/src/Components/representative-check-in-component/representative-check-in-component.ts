import { Component, inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource } from '@angular/material/table';
import { RouterModule } from '@angular/router';
import Swal from 'sweetalert2';
import { RepresentativeAttendanceService } from '../../app/Services/representative-attendance.service';
import { RepresentativeAttendanceDto } from '../../app/models/IRepresentativeAttendance';

@Component({
  selector: 'app-representative-check-in',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, RouterModule, MatTableModule, MatPaginatorModule],
  templateUrl: './representative-check-in-component.html',
  styleUrls: ['./representative-check-in-component.css']
})
export class RepresentativeCheckInComponent implements OnInit {
  private attendanceService = inject(RepresentativeAttendanceService);

  // Paginator for today's records
  dataSource = new MatTableDataSource<RepresentativeAttendanceDto>([]);
  displayedColumns = ['representativeCode', 'representativeName', 'attendanceDate', 'checkInTime', 'checkOutTime', 'attendanceStatus', 'actions'];
  pageSizeOptions = [5, 10, 20];
  @ViewChild('timeInput') timeInput!: ElementRef;
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  // Form inputs for manual record
  representativeCode: string | null = null;
  selectedDateObj: Date | null = new Date();
  selectedTime: string = '';
  isProcessing = false;

  todayRecords: RepresentativeAttendanceDto[] = [];
  isLoadingToday = false;

  ngOnInit(): void {
    this.loadTodayRecords();
  }

  loadTodayRecords() {
    this.isLoadingToday = true;
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const todayStr = `${yyyy}-${mm}-${dd}`;

    this.attendanceService.getByFilter({
      page: 1,
      pageSize: 100,
      selectedDate: todayStr
    } as any).subscribe({
      next: (res: any) => {
        if (Array.isArray(res.items)) {
          this.todayRecords = res.items;
        } else if (Array.isArray(res.data)) {
          this.todayRecords = res.data;
        } else if (Array.isArray(res?.data?.items)) {
          this.todayRecords = res.data.items;
        } else {
          this.todayRecords = [];
        }
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
    const code = (this.representativeCode ?? '').trim();
    if (!code) {
      Swal.fire('خطأ', 'برجاء إدخال كود المندوب', 'error');
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

    const request = {
      representativeCode: code,
      representativeEmail: '',
      date: date,
      inputTime: time,
      checkInLatitude: undefined,
      checkInLongitude: undefined,
      checkInLocation: ''
    };

    this.attendanceService.checkIn(request).subscribe({
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

  openStatusDialog(row: RepresentativeAttendanceDto) {
    Swal.fire({
      title: 'تغيير حالة التسجيل',
      input: 'select',
      inputOptions: {
        '1': 'حاضر',
        '2': 'غائب',
        '3': 'متأخر',
        '4': 'معتذر',
        '5': 'إجازة',
        '6': 'إجازة مرضية',
        '7': 'انصراف مبكر'
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

  changeAttendanceStatus(row: RepresentativeAttendanceDto, newStatus: number) {
    this.attendanceService.changeStatus(row, newStatus).subscribe({
      next: (res) => {
        if (res?.isSuccess) {
          Swal.fire('تم', 'تم تحديث الحالة بنجاح', 'success');
          this.loadTodayRecords();
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل التحديث', 'error');
        }
      },
      error: (err) => {
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ أثناء تغيير الحالة', 'error');
      }
    });
  }

  getStatusText(status?: number | null) {
    switch (status) {
      case 1: return 'حاضر';
      case 2: return 'غائب';
      case 3: return 'متأخر';
      case 4: return 'معتذر';
      case 5: return 'إجازة';
      case 6: return 'إجازة مرضية';
      case 7: return 'انصراف مبكر';
      default: return '-';
    }
  }

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
    this.representativeCode = null;
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
