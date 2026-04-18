import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { EmployeeAttendanceService } from '../../app/Services/employee-attendance-service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-quick-attendance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatCardModule
  ],
  templateUrl: './quick-attendance-component.html',
  styleUrls: ['./quick-attendance-component.css']
})
export class QuickAttendanceComponent implements OnInit {
  private attendanceService = inject(EmployeeAttendanceService);

  // الوقت والتاريخ الحالي
  currentDate: string = '';
  currentTime: string = '';

  // نوع التسجيل: 'checkIn' أو 'checkOut'
  attendanceType: string = 'checkIn';

  // حالة الإرسال
  isSubmitting: boolean = false;

  ngOnInit(): void {
    this.initializeDateTime();
    // تحديث الوقت كل ثانية
    setInterval(() => {
      this.updateCurrentTime();
    }, 1000);
  }

  initializeDateTime(): void {
    const now = new Date();
    
    // تنسيق التاريخ: yyyy-MM-dd
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    this.currentDate = `${year}-${month}-${day}`;

    // تنسيق الوقت: HH:mm:ss
    this.updateCurrentTime();
  }

  updateCurrentTime(): void {
    const now = new Date();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    this.currentTime = `${hours}:${minutes}:${seconds}`;
  }

  submitAttendance(): void {
    if (this.isSubmitting) return;

    // التحقق من البيانات
    if (!this.currentDate || !this.currentTime) {
      Swal.fire('خطأ', 'تعذر الحصول على التاريخ والوقت', 'error');
      return;
    }

    // جلب إيميل المستخدم من localStorage
    let email: string | null = null;
    const rawEmail = localStorage.getItem('userEmail');
    const rawUser = localStorage.getItem('user');

    if (rawEmail) {
      email = rawEmail;
    } else if (rawUser) {
      try {
        const parsed = JSON.parse(rawUser);
        email = parsed?.userEmail || parsed?.email || parsed?.userEmailAddress || null;
        if (!email && typeof parsed === 'string' && parsed.includes('@')) email = parsed;
      } catch (e) {
        if (rawUser.includes('@')) email = rawUser;
      }
    }

    if (!email) {
      Swal.fire('خطأ', 'لم يتم العثور على إيميل المستخدم في الجلسة', 'error');
      return;
    }

    this.isSubmitting = true;

    const payload = {
      employeeEmail: email,
      date: this.currentDate,
      inputTime: this.currentTime
    };

    // quick attendance payload prepared (logging removed)

    const request = this.attendanceType === 'checkIn'
      ? this.attendanceService.checkInByEmployeeEmail({ employeeEmail: email, date: this.currentDate, inputTime: this.currentTime })
      : this.attendanceService.checkOutByEmployeeEmail({ employeeEmail: email, date: this.currentDate, inputTime: this.currentTime });

    request.subscribe({
      next: (res: any) => {
        this.isSubmitting = false;

        if (res?.isSuccess) {
          const message = this.attendanceType === 'checkIn'
            ? 'تم تسجيل الدخول بنجاح ✓'
            : 'تم تسجيل الخروج بنجاح ✓';

          Swal.fire({
            icon: 'success',
            title: 'نجح',
            text: message,
            timer: 2000,
            timerProgressBar: true
          });

          // إعادة تحميل البيانات/الصفحة بعد 2 ثانية
          setTimeout(() => {
            window.location.reload();
          }, 2000);
        } else {
          Swal.fire('خطأ', res?.message ?? 'حدث خطأ أثناء التسجيل', 'error');
        }
      },
      error: (err) => {
        this.isSubmitting = false;

        let errorMessage = 'حدث خطأ أثناء التسجيل';
        if (err?.error?.message) {
          errorMessage = err.error.message;
        } else if (err?.message) {
          errorMessage = err.message;
        }

        Swal.fire('خطأ', errorMessage, 'error');
      }
    });
  }

  getAttendanceTypeLabel(): string {
    return this.attendanceType === 'checkIn' ? 'حضور (Check-In)' : 'انصراف (Check-Out)';
  }

  getSubmitButtonLabel(): string {
    return this.attendanceType === 'checkIn' ? 'تسجيل الدخول' : 'تسجيل الخروج';
  }

  getSubmitButtonColor(): string {
    return this.attendanceType === 'checkIn' ? 'primary' : 'accent';
  }
}
