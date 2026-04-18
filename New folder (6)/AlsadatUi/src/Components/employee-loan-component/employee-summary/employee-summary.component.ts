import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { EmployeeLoanService } from '../../../app/Services/employee-loan.service';
import { LoanPaymentsComponent } from '../loan-payments/loan-payments.component';

@Component({
  selector: 'app-employee-summary',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatButtonModule],
  templateUrl: './employee-summary.component.html',
  styleUrls: ['./employee-summary.component.css']
})
export class EmployeeSummaryComponent implements OnInit {
  dialogRef = inject(MatDialogRef<boolean>);
  private loanService = inject(EmployeeLoanService);
  private dialog = inject(MatDialog);
  data: any = inject(MAT_DIALOG_DATA) || {};

  summary: any = null;
  loans: any[] = [];
  isLoading = false;

  ngOnInit(): void {
    const code = this.data?.employeeCode ?? this.data?.representativeCode ?? this.data?.employee?.code ?? this.data?.representative?.representativesCode;
    if (!code) return;
    this.loadSummary(code);
    this.loadLoans(code);
  }

  loadSummary(code: string) {
    this.isLoading = true;
    this.loanService.getEmployeeLoanSummary(code).subscribe({
      next: (res: any) => {
        this.summary = res?.data ?? res ?? null;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; this.summary = null; }
    });
  }

  loadLoans(code: string) {
    this.loanService.getEmployeeLoans(code,1,50).subscribe({
      next: (res: any) => {
        const payload = res?.data ?? res;
        this.loans = payload?.items ?? payload ?? [];
      },
      error: () => { this.loans = []; }
    });
  }

  openLoanPayments(loan: any) {
    const ref = this.dialog.open(LoanPaymentsComponent, { data: loan, width: '720px', maxHeight: '80vh' });
    ref.afterClosed().subscribe(() => {
      // refresh summary/loans after returning from payments
      const code = this.data?.employeeCode ?? this.data?.representativeCode ?? this.data?.employee?.code ?? this.data?.representative?.representativesCode;
      if (code) { this.loadSummary(code); this.loadLoans(code); }
    });
  }

  close() { this.dialogRef.close(false); }
}
