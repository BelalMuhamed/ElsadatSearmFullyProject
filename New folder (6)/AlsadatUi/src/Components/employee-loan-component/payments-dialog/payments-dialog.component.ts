import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { EmployeeLoanService } from '../../../app/Services/employee-loan.service';
import Swal from 'sweetalert2';
import { MatCard } from "@angular/material/card";

@Component({
  selector: 'app-payments-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatCard],
  templateUrl: './payments-dialog.component.html',
  styleUrls: ['./payments-dialog.component.css']
})
export class PaymentsDialogComponent {
  dialogRef = inject(MatDialogRef<boolean>);
  private loanService = inject(EmployeeLoanService);
  data: any = inject(MAT_DIALOG_DATA) || {};

  model: any = {
    loanId: 0,
    paymentAmount: 0,
    paymentDate: new Date().toISOString().substring(0,10),
    remainingAmount: 0,
    paymentMethod: 0,
    notes: ''
  };

  isSaving = false;

  constructor() {
    if (this.data) {
      this.model.loanId = this.data.id ?? this.data.loanId ?? 0;
    }
  }

  save(): void {
    this.isSaving = true;
    this.loanService.makePayment(this.model).subscribe({
      next: (res: any) => {
        this.isSaving = false;
        // close dialog first so parent can refresh immediately
        this.dialogRef.close(true);
        // show a non-blocking toast notification
        Swal.fire({
          toast: true,
          position: 'top-end',
          icon: 'success',
          title: res?.message ?? 'تم تسجيل الدفعة',
          showConfirmButton: false,
          timer: 1500
        });
      },
      error: (e) => {
        this.isSaving = false;
        const msg = e?.error?.message || e?.error?.Message || e?.message || 'فشل';
        Swal.fire('خطأ', msg, 'error'); // changed: normalized message
      }
    });
  }

  close(): void { this.dialogRef.close(false); } // changed: enable closing
}