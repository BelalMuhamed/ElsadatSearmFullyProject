import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, NativeDateAdapter, DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE, MAT_NATIVE_DATE_FORMATS } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { PublicHoliday } from '../../../app/models/IPublicHoliday';
import { PublicHolidayService } from '../../../app/Services/public-holiday.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-public-holiday-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule, MatCardModule],
  providers: [
    { provide: DateAdapter, useClass: NativeDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS }
  ],
  templateUrl: './public-holiday-dialog.component.html',
  styleUrls: ['./public-holiday-dialog.component.css']
})
export class PublicHolidayDialogComponent implements OnInit {
  holiday: PublicHoliday = {
    id: 0,
    name: '',
    date: '',
    createdAt: new Date(),
    isDeleted: false
  };
  selectedDate: Date | null = null;
  isLoading = false;
  isEdit = false;

  constructor(
    public dialogRef: MatDialogRef<PublicHolidayDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PublicHoliday | null,
    private holidayService: PublicHolidayService
  ) {
    if (data) {
      this.holiday = { ...this.holiday, ...data };
      if ((data as any).date) {
        try {
          this.selectedDate = new Date((data as any).date);
        } catch (e) {
          this.selectedDate = null;
        }
      }
      this.isEdit = !!((data as any).id && Number((data as any).id) > 0);
    }
  }

  ngOnInit(): void {}

  save(): void {
    if (!this.holiday.name || this.holiday.name.trim() === '') {
      Swal.fire('خطأ', 'الرجاء إدخال اسم العطلة', 'error');
      return;
    }

    if (!this.selectedDate) {
      Swal.fire('خطأ', 'الرجاء اختيار التاريخ', 'error');
      return;
    }

    // Format date as YYYY-MM-DD
    const year = this.selectedDate.getFullYear();
    const month = String(this.selectedDate.getMonth() + 1).padStart(2, '0');
    const day = String(this.selectedDate.getDate()).padStart(2, '0');
    this.holiday.date = `${year}-${month}-${day}`;

    this.isLoading = true;

    // Save invoked (debug logging removed)

    const request = this.isEdit
      ? this.holidayService.update(this.holiday)
      : this.holidayService.create(this.holiday);

    request.subscribe({
      next: (res: any) => {
        this.isLoading = false;
        if (res?.isSuccess) {
          Swal.fire('نجح', res?.message ?? 'تم الحفظ بنجاح', 'success');
          this.dialogRef.close(true);
        } else {
          Swal.fire('خطأ', res?.message ?? 'حدث خطأ', 'error');
        }
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire('خطأ', err?.error?.message ?? 'حدث خطأ', 'error');
      }
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
