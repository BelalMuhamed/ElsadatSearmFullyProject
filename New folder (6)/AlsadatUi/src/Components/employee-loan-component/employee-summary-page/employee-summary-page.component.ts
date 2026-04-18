import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router, ActivatedRoute } from '@angular/router';
import { EmployeeService } from '../../../app/Services/employee.service';
import { EmployeeLoanService } from '../../../app/Services/employee-loan.service';
import { RepresentativeService } from '../../../app/Services/representative-service';
import { LoanPaymentsComponent } from '../loan-payments/loan-payments.component';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { LoanDialogComponent } from '../loan-dialog/loan-dialog.component';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-employee-summary-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    MatButtonToggleModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './employee-summary-page.component.html',
  styleUrls: ['./employee-summary-page.component.css']
})
export class EmployeeSummaryPageComponent implements OnInit, OnDestroy {
  private employeeService = inject(EmployeeService);
  private loanService = inject(EmployeeLoanService);
  private representativeService = inject(RepresentativeService);
  private dialog = inject(MatDialog);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  employeeFilter = '';
  employees: any[] = [];
  representatives: any[] = [];
  employeesLoading = false;
  representativesLoading = false;
  userType: 'employee' | 'representative' = 'employee';

  selectedEmployee: any = null;
  summary: any = null;
  loans: any[] = [];
  isLoadingSummary = false;

  private subs: Subscription[] = [];

  ngOnInit(): void {
    this.loadAllEmployees();
    this.loadAllRepresentatives();

    const sub = this.route.queryParams.subscribe(params => {
      const code = params['employeeCode'];
      if (code) {
        this.employeeFilter = code;
        // try to select after employees loaded
        const trySelect = () => {
          const found = this.employees.find(e => (e.employeeCode || e.code) === code);
          if (found) this.selectEmployee(found);
        };
        if (this.employees.length) trySelect(); else {
          const check = setInterval(() => { if (this.employees.length) { clearInterval(check); trySelect(); } }, 200);
        }
      }
    });
    this.subs.push(sub);
  }

  ngOnDestroy(): void { this.subs.forEach(s => s.unsubscribe()); }

  loadAllEmployees(): void {
    this.employeesLoading = true;
    const params = { pageNumber: 1, pageSize: 1000 };
    this.employeeService.getEmployeesByFilter(params as any, {}).subscribe({
      next: (res: any) => {
        const payload = res?.data ?? res ?? [];
        const list = payload?.items ?? payload ?? [];
        // remove duplicates by code
        const map = new Map<string, any>();
        for (const e of list) {
          const key = (e.employeeCode || e.code || e.id || '').toString();
          if (key && !map.has(key)) map.set(key, e);
        }
        this.employees = Array.from(map.values());
        this.employeesLoading = false;
      },
      error: () => { this.employees = []; this.employeesLoading = false; }
    });
  }

  loadAllRepresentatives(): void {
    this.representativesLoading = true;
    const pagination = { pageNumber: 1, pageSize: 1000 } as any;
    this.representativeService.getRepresentativesByFilter(pagination, { representativeCode: '', representativeName: '', cityName: '', isActive: true, representiveType: 0 }).subscribe({
      next: (res: any) => {
        const payload = res?.data ?? res ?? [];
        const list = payload?.items ?? payload ?? [];
        const map = new Map<string, any>();
        for (const e of list) {
          const key = (e.representativesCode || e.representativeCode || e.code || e.id || '').toString();
          if (key && !map.has(key)) map.set(key, e);
        }
        this.representatives = Array.from(map.values());
        this.representativesLoading = false;
      },
      error: () => { this.representatives = []; this.representativesLoading = false; }
    });
  }

  get filteredEmployees(): any[] {
    const q = (this.employeeFilter || '').toString().trim().toLowerCase();
    const list = this.userType === 'employee' ? this.employees : this.representatives;
    if (!q) return list.slice(0, 30);
    return list.filter((e:any) => {
      const name = (e.fullName || e.name || e.user?.fullName || '').toString().toLowerCase();
      const code = (e.employeeCode || e.code || e.representativesCode || e.representativeCode || '').toString().toLowerCase();
      return name.includes(q) || code.includes(q);
    }).slice(0, 30);
  }

  onEmployeeFilterChange(q: string) {
    this.employeeFilter = q;
    // do not hit server here, filtering is local
  }

  onEmployeeSelected(event: any) {
    const emp = event.option.value;
    // option value is whole object (employee or representative)
    this.selectEmployee(emp);
  }

  selectEmployee(emp: any) {
    if (!emp) return;
    this.selectedEmployee = emp;
    const code = this.userType === 'employee' ? (emp.employeeCode ?? emp.code ?? emp.id) : (emp.representativesCode ?? emp.representativeCode ?? emp.code ?? emp.id);
    // update URL
    this.router.navigate([], { queryParams: { employeeCode: code }, queryParamsHandling: 'merge' });
    this.loadSummary(code);
  }

  loadSummary(employeeCode: string) {
    if (!employeeCode) return;
    this.isLoadingSummary = true;
    this.summary = null;
    this.loans = [];
    this.loanService.getEmployeeLoanSummary(employeeCode).subscribe({
      next: (res:any) => { this.summary = res?.data ?? res ?? null; this.isLoadingSummary = false; },
      error: () => { this.summary = null; this.isLoadingSummary = false; }
    });
    this.loanService.getEmployeeLoans(employeeCode, 1, 100).subscribe({
      next: (res:any) => { const payload = res?.data ?? res; this.loans = payload?.items ?? payload ?? []; },
      error: () => { this.loans = []; }
    });
  }

  openLoanPayments(loan: any) {
    const ref = this.dialog.open(LoanPaymentsComponent, { data: loan, width: '720px', maxHeight: '80vh' });
    ref.afterClosed().subscribe(() => {
      if (this.selectedEmployee) {
        const code = this.selectedEmployee.employeeCode ?? this.selectedEmployee.code;
        this.loadSummary(code);
      }
    });
  }

  openAddLoan() {
    const ref = this.dialog.open(LoanDialogComponent, { data: { loan: null, employees: this.employees, representatives: this.representatives }, width: '520px' });
    ref.afterClosed().subscribe((saved:any) => {
      if (saved && this.selectedEmployee) {
        const code = this.selectedEmployee.employeeCode ?? this.selectedEmployee.code;
        this.loadSummary(code);
      }
    });
  }

  calculateMonthlyDeduction() {
    const code = this.userType === 'employee' ? (this.selectedEmployee?.employeeCode ?? this.selectedEmployee?.code) : (this.selectedEmployee?.representativesCode ?? this.selectedEmployee?.representativeCode ?? this.selectedEmployee?.code);
    if (!code) { Swal.fire('تحذير','اختر موظف أولاً','warning'); return; }
    this.loanService.calculateMonthlyDeduction(code).subscribe({
      next: (res:any) => {
        const payload = res?.data ?? res;
        const amount = payload ?? (res?.message ? res.message : null);
        Swal.fire('الخصم الشهري', `المبلغ: ${amount ?? 'غير متوفر'}`, 'info');
      },
      error: (e:any) => Swal.fire('خطأ', e?.error?.message || e?.message || 'فشل', 'error')
    });
  }

  // status translation helper
  private normalizeItemStatus(it: any): number | null {
    if (it == null) return null;
    const s = it.status ?? it;
    if (typeof s === 'number') return s;
    const maybeNum = Number(s);
    if (!isNaN(maybeNum)) return maybeNum;
    const map: Record<string, number> = {
      'pendingapproval': 0,
      'pending': 0,
      'active': 1,
      'paidoff': 2,
      'rejected': 3,
      'overdue': 4,
      'defaulted': 5,
      'موافق': 1,
      'مسدد': 2,
      'مرفوض': 3,
      'قيد المراجعة': 0
    };
    return map[(s + '').toLowerCase()] ?? null;
  }

  getStatusText(item: any): string {
    const code = this.normalizeItemStatus(item);
    const map: Record<number,string> = {
      0: 'قيد المراجعة',
      1: 'موافق',
      2: 'مسدد',
      3: 'مرفوض',
      4: 'متأخر',
      5: 'متعثر'
    };
    return (code != null && map[code]) ? map[code] : (item?.status ?? '-');
  }
}
