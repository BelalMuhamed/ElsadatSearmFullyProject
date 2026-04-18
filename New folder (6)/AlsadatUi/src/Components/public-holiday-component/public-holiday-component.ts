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
import { PublicHolidayService } from '../../app/Services/public-holiday.service';
import { PublicHoliday } from '../../app/models/IPublicHoliday';
import { PublicHolidayDialogComponent } from './public-holiday-dialog/public-holiday-dialog.component';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-public-holiday',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatCardModule, MatSlideToggleModule, MatTooltipModule],
  templateUrl: './public-holiday-component.html',
  styleUrls: ['./public-holiday-component.css']
})
export class PublicHolidayComponent implements OnInit {
  private holidayService = inject(PublicHolidayService);
  private dialog = inject(MatDialog);

  holidays: PublicHoliday[] = [];
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  ngOnInit(): void {
    this.loadHolidays();
  }

  toggleHolidayStatus(holiday: PublicHoliday): void {
    if (!holiday) return;
    if (holiday.isDeleted) {
      this.holidayService.restore(holiday).subscribe({
        next: () => {
          holiday.isDeleted = false;
          Swal.fire('تم', 'تم استعادة العطلة', 'success');
          this.loadHolidays();
        },
        error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
      });
    } else {
      this.holidayService.softDelete(holiday).subscribe({
        next: () => {
          holiday.isDeleted = true;
          Swal.fire('تم', 'تم تعطيل العطلة', 'success');
          this.loadHolidays();
        },
        error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
      });
    }
  }

  loadHolidays(): void {
    this.isLoading = true;
    this.holidayService.getAll(this.currentPage, this.pageSize).subscribe({
      next: (res: any) => {
        if (res?.items && Array.isArray(res.items)) {
          this.holidays = res.items;
          this.totalCount = res.totalCount ?? res.items.length;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل تحميل العطل', 'error');
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        // error handled silently
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  openAdd(): void {
    const ref = this.dialog.open(PublicHolidayDialogComponent, { data: null, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => {
      if (saved) this.loadHolidays();
    });
  }

  openEdit(holiday: PublicHoliday): void {
    const ref = this.dialog.open(PublicHolidayDialogComponent, { data: holiday, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => {
      if (saved) this.loadHolidays();
    });
  }

  softDelete(holiday: PublicHoliday): void {
    Swal.fire({
      title: 'تأكيد',
      text: `هل تريد تعطيل العطلة ${holiday.name}?`,
      icon: 'warning',
      showCancelButton: true
    }).then((r) => {
      if (r.isConfirmed) {
        this.holidayService.softDelete(holiday).subscribe({
          next: () => {
            Swal.fire('تم', 'تم تعطيل العطلة', 'success');
            this.loadHolidays();
          },
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
        });
      }
    });
  }

  restore(holiday: PublicHoliday): void {
    Swal.fire({
      title: 'تأكيد',
      text: `هل تريد استعادة العطلة ${holiday.name}?`,
      icon: 'question',
      showCancelButton: true
    }).then((r) => {
      if (r.isConfirmed) {
        this.holidayService.restore(holiday).subscribe({
          next: () => {
            Swal.fire('تم', 'تم استعادة العطلة', 'success');
            this.loadHolidays();
          },
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
        });
      }
    });
  }

  hardDelete(holiday: PublicHoliday): void {
    Swal.fire({
      title: 'حذف نهائي',
      text: `هل تريد حذف العطلة ${holiday.name} نهائياً؟`,
      icon: 'warning',
      showCancelButton: true
    }).then((r) => {
      if (r.isConfirmed) {
        this.holidayService.hardDelete(holiday).subscribe({
          next: () => {
            Swal.fire('تم', 'تم الحذف نهائياً', 'success');
            this.loadHolidays();
          },
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل', 'error')
        });
      }
    });
  }

  onPageChange(event: any): void {
    this.pageSize = event.pageSize || this.pageSize;
    this.currentPage = (event.pageIndex != null) ? (event.pageIndex + 1) : this.currentPage;
    this.loadHolidays();
  }
}
