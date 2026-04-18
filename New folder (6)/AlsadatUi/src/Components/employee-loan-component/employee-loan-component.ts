import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import Swal from 'sweetalert2';
import { EmployeeLoanService } from '../../app/Services/employee-loan.service';
import { RepresentativeService } from '../../app/Services/representative-service';
import { LoanDialogComponent } from './loan-dialog/loan-dialog.component';
import { PaymentsDialogComponent } from './payments-dialog/payments-dialog.component';
import { EmployeeSummaryComponent } from './employee-summary/employee-summary.component';
import { LoanPaymentsComponent } from './loan-payments/loan-payments.component';
import { of } from 'rxjs';
import { Router } from '@angular/router';
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-employee-loan',
  standalone: true,
  imports: [CommonModule, FormsModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatDialogModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatDatepickerModule, MatNativeDateModule, MatProgressSpinner, MatTooltipModule],
  templateUrl: './employee-loan-component.html',
  styleUrls: ['./employee-loan-component.css']
})
export class EmployeeLoanComponent implements OnInit {
  private loanService = inject(EmployeeLoanService);
  private representativeService = inject(RepresentativeService);
  private dialog = inject(MatDialog);
  private router = inject(Router);

  loans: any[] = [];
  isLoading = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;

  // change default for status to empty (means "all")
  filters: any = {
    q: '',
    status: '', // '' == All (do not send to API)
    fromDate: '',
    toDate: ''
  };

  ngOnInit(): void {
    // keep only data initialization here; theme is global (navbar)
    this.loadLoans();
  }

  // New helper to normalize error messages from HttpErrorResponse
  private getErrorMessage(err: any): string {
    return err?.error?.message || err?.error?.Message || err?.message || 'حدث خطأ';
  }

  // helper: normalize various status representations to numeric code
  private normalizeItemStatus(it: any): number | null {
    if (it == null) return null;
    const s = it.status;
    if (typeof s === 'number') return s;
    // if server returns numeric string
    const maybeNum = Number(s);
    if (!isNaN(maybeNum)) return maybeNum;
    // map textual enum names to numeric codes
    const map: Record<string, number> = {
      'PendingApproval': 0,
      'Active': 1,
      'PaidOff': 2,
      'Rejected': 3,
      'Overdue': 4,
      'Defaulted': 5
    };
    return map[s] ?? null;
  }

  loadLoans(): void {
    this.isLoading = true;

    // build a clean filters object for the API (only for status and dates, NOT for general search)
    const reqFilters: any = {};
    
    // الحالة - ترسل للـ Backend
    if (this.filters.status !== '' && this.filters.status != null) {
      const sNum = Number(this.filters.status);
      if (!isNaN(sNum)) reqFilters.status = sNum;
    }
    
    // التواريخ - ترسل للـ Backend (normalize to ISO date yyyy-MM-dd)
    if (this.filters.fromDate) {
      const d = this.filters.fromDate;
      const fromStr = (d instanceof Date) ? d.toISOString().split('T')[0] : (typeof d === 'string' && d ? new Date(d).toISOString().split('T')[0] : d);
      reqFilters.fromDate = fromStr;
    }
    if (this.filters.toDate) {
      const d2 = this.filters.toDate;
      const toStr = (d2 instanceof Date) ? d2.toISOString().split('T')[0] : (typeof d2 === 'string' && d2 ? new Date(d2).toISOString().split('T')[0] : d2);
      reqFilters.toDate = toStr;
    }

    (this.loanService as any).getAllLoans(this.currentPage, this.pageSize, reqFilters).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        const payload = res?.data ?? res;
        if (payload?.items && Array.isArray(payload.items)) {
          let items = payload.items;
          
          // البحث العام - الفلترة المحلية فقط
          if (this.filters.q) {
            const q = this.filters.q.toString().toLowerCase().trim();
            items = items.filter((it: any) => {
              const name = (it.employeeName || it.representativeName || it.employee?.fullName || it.representative?.fullName || '').toLowerCase();
              const code = (it.employeeCode || it.representativeCode || it.code || '').toString().toLowerCase();
              const loanNum = (it.loanNumber || '').toString().toLowerCase();
              return name.includes(q) || code.includes(q) || loanNum.includes(q);
            });
          }
          
          this.loans = items;
          this.totalCount = items.length;
        } else if (res?.success === true) {
          this.loans = [];
          this.totalCount = payload?.totalCount ?? 0;
        } else {
          Swal.fire('خطأ', res?.message ?? 'فشل تحميل القروض', 'error');
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        Swal.fire('خطأ', this.getErrorMessage(err), 'error');
      }
    });
  }

  // new: apply and clear filters
  applyFilters(): void {
    this.currentPage = 1;
    this.loadLoans();
  }

  clearFilters(): void {
    this.filters = { q: '', status: '', fromDate: '', toDate: '' };
    this.currentPage = 1;
    this.loadLoans();
  }

  // translate status to Arabic (uses normalizeItemStatus)
  getStatusText(item: any): string {
    const code = this.normalizeItemStatus(item);
    const map: Record<number,string> = {
      0: 'قيد الموافقة',
      1: 'نشط',
      2: 'مسدد',
      3: 'مرفوض',
      4: 'متأخر',
      5: 'متعثر'
    };
    return (code != null && map[code]) ? map[code] : (item?.status ?? '-');
  }

  // get numeric status code (or null)
  getStatusCode(item: any): number | null {
    return this.normalizeItemStatus(item);
  }

  // try to fetch employees (best-effort). Uses loanService.searchEmployees if present; otherwise returns empty list.
  private loadEmployees() {
    try {
      const svc = (this.loanService as any);
      if (svc && typeof svc.searchEmployees === 'function') {
        return svc.searchEmployees(''); // observable expected
      }
    } catch (e) { /* continue to fallback */ }
    return of([]);
  }

  openAdd(): void {
    this.loadEmployees().subscribe((emps: any[]) => {
      const pagination = { pageNumber: 1, pageSize: 1000 } as any;
      this.representativeService.getRepresentativesByFilter(pagination, { representativeCode: '', representativeName: '', cityName: '', isActive: true, representiveType: 0 }).subscribe({
        next: (res: any) => {
          const payload = res?.data ?? res ?? [];
          const reps = payload?.items ?? payload ?? [];
          const ref = this.dialog.open(LoanDialogComponent, { data: { loan: null, employees: emps, representatives: reps }, width: '520px' });
          ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadLoans(); });
        },
        error: () => {
          const ref = this.dialog.open(LoanDialogComponent, { data: { loan: null, employees: emps, representatives: [] }, width: '520px' });
          ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadLoans(); });
        }
      });
    });
  }

  openEdit(loan: any): void {
    this.loadEmployees().subscribe((emps: any[]) => {
      const pagination = { pageNumber: 1, pageSize: 1000 } as any;
      this.representativeService.getRepresentativesByFilter(pagination, { representativeCode: '', representativeName: '', cityName: '', isActive: true, representiveType: 0 }).subscribe({
        next: (res: any) => {
          const payload = res?.data ?? res ?? [];
          const reps = payload?.items ?? payload ?? [];
          const ref = this.dialog.open(LoanDialogComponent, { data: { loan, employees: emps, representatives: reps }, width: '520px' });
          ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadLoans(); });
        },
        error: () => {
          const ref = this.dialog.open(LoanDialogComponent, { data: { loan, employees: emps, representatives: [] }, width: '520px' });
          ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadLoans(); });
        }
      });
    });
  }

  openPayments(loan: any): void {
    // open a reasonably sized dialog for payments (not full-screen)
    const ref = this.dialog.open(LoanPaymentsComponent, { data: loan, width: '720px', maxHeight: '80vh' });
    ref.afterClosed().subscribe((saved: any) => { if (saved) this.loadLoans(); });
  }

  openSummary(employeeCode: string): void {
    if (!employeeCode) { Swal.fire('خطأ', 'لا يوجد كود موظف', 'error'); return; }
    // navigate to the dedicated summary page (keeps history and allows direct linking)
    this.router.navigate(['/hr/employee-loan-summary'], { queryParams: { employeeCode } });
  }

  approveLoan(loan: any): void {
    Swal.fire({ title: 'تأكيد', text: `هل تريد الموافقة على القرض ${loan.loanNumber}?`, icon: 'question', showCancelButton: true })
      .then((r) => {
        if (r.isConfirmed) {
          this.loanService.approve({ loanId: loan.id }).subscribe({
            next: () => { Swal.fire('تم', 'تمت الموافقة على القرض', 'success'); this.loadLoans(); },
            error: (e) => Swal.fire('خطأ', this.getErrorMessage(e), 'error') // changed
          });
        }
      });
  }

  rejectLoan(loan: any): void {
    Swal.fire({ title: 'رفض القرض', input: 'text', inputLabel: 'السبب', showCancelButton: true }).then((r) => {
      if (r.isConfirmed) {
        this.loanService.reject({ loanId: loan.id, reason: r.value }).subscribe({
          next: () => { Swal.fire('تم', 'تم رفض القرض', 'success'); this.loadLoans(); },
          error: (e) => Swal.fire('خطأ', this.getErrorMessage(e), 'error') // changed
        });
      }
    });
  }

  onPageChange(event: any): void {
    this.pageSize = event.pageSize || this.pageSize;
    this.currentPage = (event.pageIndex != null) ? (event.pageIndex + 1) : this.currentPage;
    this.loadLoans();
  }
}
