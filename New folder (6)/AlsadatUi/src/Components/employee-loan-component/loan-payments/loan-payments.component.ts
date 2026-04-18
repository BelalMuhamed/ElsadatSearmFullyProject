import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { EmployeeLoanService } from '../../../app/Services/employee-loan.service';
import { PaymentsDialogComponent } from '../payments-dialog/payments-dialog.component';

@Component({
  selector: 'app-loan-payments',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule, MatIconModule],
  templateUrl: './loan-payments.component.html',
  styleUrls: ['./loan-payments.component.css']
})
export class LoanPaymentsComponent implements OnInit {
  dialogRef = inject(MatDialogRef<boolean>);
  private loanService = inject(EmployeeLoanService);
  private dialog = inject(MatDialog);
  data: any = inject(MAT_DIALOG_DATA) || {};

  payments: any[] = [];
  isLoading = false;
  totalPaid = 0;
  loanInfo: any = null;
  detailsOpen: { [k: number]: boolean } = {};

  ngOnInit(): void {
    const loanId = this.data?.id ?? this.data?.loanId;
    this.loanInfo = this.data;
    if (!loanId) { this.payments = []; return; }
    this.loadPayments(loanId);
  }

  loadPayments(loanId: number) {
    this.isLoading = true;
    this.loanService.getLoanPayments(loanId).subscribe({
      next: (res: any) => {
        // support both { success:true, data: ... } and bare arrays
        const payload = res?.data ?? res;
        this.payments = payload?.items ?? payload ?? [];
        this.totalPaid = (this.payments || []).reduce((s, p) => s + (Number(p.paymentAmount) || 0), 0);
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; this.payments = []; }
    });
  }

  toggleDetails(idx: number) {
    this.detailsOpen[idx] = !this.detailsOpen[idx];
  }

  isActiveLoan(): boolean {
    if (!this.loanInfo) return false;
    const s = this.loanInfo.status;
    if (s == null) return false;
    if (typeof s === 'number') return s === 1;
    const n = Number(s);
    if (!isNaN(n)) return n === 1;
    const str = (s + '').toLowerCase();
    return str.includes('active') || str.includes('موافق') || str.includes('نشط') || str.includes('1');
  }

  openAddPayment() {
    const loanId = this.data?.id ?? this.data?.loanId;
    const ref = this.dialog.open(PaymentsDialogComponent, { data: { loanId }, width: '520px' });
    ref.afterClosed().subscribe((saved: any) => {
      if (saved) {
        this.loadPayments(loanId);
      }
    });
  }

  close() { this.dialogRef.close(false); }
}
