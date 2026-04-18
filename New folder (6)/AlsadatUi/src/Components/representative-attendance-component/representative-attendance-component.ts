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
import { RepresentativeAttendanceService } from '../../app/Services/representative-attendance.service';
import { RepresentativeAttendanceDto, RepresentativeAttendanceFilteration, AttendanceStatus } from '../../app/models/IRepresentativeAttendance';
import Swal from 'sweetalert2';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-representative-attendance',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatPaginatorModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule, MatSelectModule, RouterModule],
  templateUrl: './representative-attendance-component.html',
  styleUrls: ['./representative-attendance-component.css']
})
export class RepresentativeAttendanceComponent {
  selectedDateObj: Date | null = null;
  private attendanceService = inject(RepresentativeAttendanceService);

  columns = [
    { key: 'representativeCode', label: 'كود المندوب', type: 'string' },
    { key: 'representativeName', label: 'اسم المندوب', type: 'string' },
    { key: 'attendanceDate', label: 'التاريخ', type: 'date' },
    { key: 'checkInTime', label: 'دخول', type: 'string' },
    { key: 'checkOutTime', label: 'انصراف', type: 'string' },
    { key: 'checkInMethod', label: 'طريقة الدخول', type: 'number' },
    { key: 'checkInLocation', label: 'موقع الدخول', type: 'string' },
    { key: 'attendanceStatus', label: 'الحالة', type: 'number' }
  ];

  displayedColumnKeys = this.columns.map(c => c.key);
  allColumnKeys = [...this.displayedColumnKeys, 'actions'];

  dataSource = new MatTableDataSource<RepresentativeAttendanceDto>([]);
  totalCount = 0;
  isLoading = false;

  filters: RepresentativeAttendanceFilteration = {
    page: 1,
    pageSize: 10,
    representativeCode: null,
    representativeId: null,
    selectedDate: null,
    startDate: null,
    endDate: null,
    representativeName: null,
    year: null,
    month: null,
    attendanceStatusID: null
  };

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  private dialog = inject(MatDialog);

  ngOnInit(): void {
    this.getAttendance();
  }

  // Dialog for changing attendance status
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
      preConfirm: (value) => value
    }).then(result => {
      if (result.isConfirmed && result.value) {
        this.changeAttendanceStatus(row, Number(result.value));
      }
    });
  }

  changeAttendanceStatus(row: RepresentativeAttendanceDto, newStatus: number) {
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

  getAttendance() {
    this.isLoading = true;
    this.attendanceService.getByFilter(this.filters).subscribe({
      next: (res: any) => {
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
    if (this.filters.representativeCode) {
      this.filters.representativeCode = this.filters.representativeCode.trim();
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
    this.filters = {
      ...this.filters,
      representativeCode: null,
      representativeId: null,
      selectedDate: null,
      representativeName: null,
      year: null,
      month: null,
      attendanceStatusID: null,
      page: 1
    };
    this.selectedDateObj = null;
    this.getAttendance();
  }

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
      case 7:
        return 'انصراف مبكر';
      default:
        return '-';
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
      return ` ${hh12}  ${period}`;
    }
    return ` ${hh12}:${minutesStr}  ${period}`;
  }

  getCheckInMethodText(method?: number | null) {
    switch (method) {
      case 0:
        return 'بصمة';
      case 1:
        return 'يدوي';
      case 2:
        return 'تطبيق الهاتف';
      default:
        return '-';
    }
  }
}
