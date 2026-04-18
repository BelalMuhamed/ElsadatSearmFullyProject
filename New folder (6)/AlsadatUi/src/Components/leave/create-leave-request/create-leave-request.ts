import { Component, inject, OnInit, Inject, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { NativeDateAdapter, DateAdapter, MAT_DATE_FORMATS, MAT_DATE_LOCALE, MAT_NATIVE_DATE_FORMATS } from '@angular/material/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { EmployeeLeaveService } from '../../../app/Services/employee-leave.service';
import { CreateLeaveRequestDto } from '../../../app/models/leave/employee-leave-request.model';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-create-leave',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatCardModule, MatIconModule, MatDialogModule],
  providers: [
    { provide: DateAdapter, useClass: NativeDateAdapter, deps: [MAT_DATE_LOCALE] },
    { provide: MAT_DATE_FORMATS, useValue: MAT_NATIVE_DATE_FORMATS }
  ],
  templateUrl: './create-leave-request.html',
  styleUrls: ['./create-leave-request.css']
})
export class CreateLeaveRequestComponent implements OnInit {
  private http = inject(HttpClient);
  private leaveService = inject(EmployeeLeaveService);

  dto: CreateLeaveRequestDto = { employeeCode: '', leaveTypeId: 0, fromDate: new Date(), toDate: new Date(), notes: '', employeeEmail: '' };
  leaveTypes: Array<{ id: number; name: string; isPaid: boolean }> = [];

  // dialog data may contain employeeEmail
  constructor(
    @Optional() @Inject(MAT_DIALOG_DATA) public data: any,
    @Optional() public dialogRef?: MatDialogRef<CreateLeaveRequestComponent>
  ) {}

  ngOnInit(): void { this.loadLeaveTypes(); this.dto.employeeEmail = this.data?.employeeEmail ?? (localStorage.getItem('userEmail') || ''); }

  loadLeaveTypes() {
    this.http.get<any>(`${environment.apiUrl}LeaveType/GetActiveLeaveTypes`).subscribe({
      next: (res) => { this.leaveTypes = res?.data ?? res ?? []; },
      error: () => { this.leaveTypes = []; }
    });
  }

  // Normalize a date to UTC-midnight for the selected calendar date to avoid
  // the ISO string showing the previous day due to local timezone offset.
  private normalizeDate(d: Date | string | null | undefined): Date | null {
    if (!d) return null;
    const dt = new Date(d as any);
    return new Date(Date.UTC(dt.getFullYear(), dt.getMonth(), dt.getDate(), 0, 0, 0));
  }

  submit() {
    // ensure employeeEmail is set
    this.dto.employeeEmail = this.dto.employeeEmail || this.data?.employeeEmail || (localStorage.getItem('userEmail') || '');

    // Create a payload copy with normalized dates (UTC-midnight)
    const payload: CreateLeaveRequestDto = {
      ...this.dto,
      fromDate: this.normalizeDate(this.dto.fromDate) as any,
      toDate: this.normalizeDate(this.dto.toDate) as any
    };

    this.leaveService.createLeaveRequest(payload).subscribe({
      next: () => {
        if (this.dialogRef) {
          this.dialogRef.close('saved');
        } else {
          Swal.fire('تم', 'تم إرسال طلب الإجازة', 'success');
          this.dto = { employeeCode: '', leaveTypeId: 0, fromDate: new Date(), toDate: new Date(), notes: '', employeeEmail: '' };
        }
      },
      error: (err) => Swal.fire('خطأ', err?.error?.message ?? 'فشل الإرسال','error')
    });
  }
}
