import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PayrollDeductionService } from '../../app/Services/payroll-deduction-service';
import { EmployeeService } from '../../app/Services/employee.service';
import { RepresentativeService } from '../../app/Services/representative-service';
import { DeductionDetailDto, EmployeeDeductionsSummaryDto } from '../../app/models/IPayrollDeduction';
import { forkJoin } from 'rxjs';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-employee-deductions-summary',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatTableModule, MatPaginatorModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule, MatFormFieldModule, MatInputModule, MatAutocompleteModule, MatSelectModule, FormsModule],
  templateUrl: './employee-deductions-summary.component.html',
  styleUrls: ['./employee-deductions-summary.component.css']
})
export class EmployeeDeductionsSummaryComponent implements OnInit {
  private service = inject(PayrollDeductionService);
  private employeeService = inject(EmployeeService);
  private representativeService = inject(RepresentativeService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  empCode = '';
  employeeName = '';
  selectedMonth: number | null = null;
  selectedYear: number | null = null;
  items: DeductionDetailDto[] = [];
  totals: any = {};
  isLoading = false;
  employeesLoading = false;
  pageNumber = 1;
  pageSize = 20;
  totalCount = 0;
  
  employees: any[] = [];
  employeeFilter = '';
  _selectedEmployee: any = null;
  
  // Generate available years
  availableYears = this.generateYears();
  months = [
    { value: 1, label: 'يناير' },
    { value: 2, label: 'فبراير' },
    { value: 3, label: 'مارس' },
    { value: 4, label: 'أبريل' },
    { value: 5, label: 'مايو' },
    { value: 6, label: 'يونيو' },
    { value: 7, label: 'يوليو' },
    { value: 8, label: 'أغسطس' },
    { value: 9, label: 'سبتمبر' },
    { value: 10, label: 'أكتوبر' },
    { value: 11, label: 'نوفمبر' },
    { value: 12, label: 'ديسمبر' }
  ];
  
  private generateYears(): number[] {
    const currentYear = new Date().getFullYear();
    const years = [];
    for (let i = currentYear - 5; i <= currentYear + 2; i++) {
      years.push(i);
    }
    return years.reverse();
  }

  ngOnInit(): void {
    this.empCode = this.route.snapshot.params['empCode'] || this.route.snapshot.queryParams['empCode'] || '';
    this.loadAllEmployees();
    // Only load if we have empCode, otherwise just stay on the page and let user navigate back
    if (this.empCode) {
      this.loadSummary();
    }
  }
  async loadAllEmployees(): Promise<void> {
    this.employeesLoading = true;
    const params = { pageNumber: 1, pageSize: 1000 };
    forkJoin({
      employees: this.employeeService.getEmployeesByFilter(params as any, {}),
      representatives: this.representativeService.getRepresentativesByFilter(params as any, { representativeCode: '', representativeName: '', cityName: '', isActive: true, representiveType: 0 } as any)
    }).subscribe({
      next: (res: any) => {
        let loadedEmployees: any[] = [];
        const emps = (res.employees?.items ?? res.employees?.data ?? res.employees) as any[] || [];
        const reps = (res.representatives?.items ?? res.representatives?.data ?? res.representatives) as any[] || [];
        emps.forEach(e => loadedEmployees.push({ code: e.employeeCode || e.code || '', fullName: e.fullName || e.name || '', raw: e, isRepresentative: false }));
        reps.forEach(r => loadedEmployees.push({ code: r.representativesCode || r.representativeCode || r.code || '', fullName: (r.user?.fullName || r.user?.FullName || r.fullName || r.name) || '', raw: r, isRepresentative: true }));
        this.employees = this.removeDuplicates(loadedEmployees);
        this.employeesLoading = false;
      },
      error: (err) => {
        this.employees = [];
        this.employeesLoading = false;
      }
    });
  }

  removeDuplicates(employees: any[]): any[] {
    const uniqueEmployees = [];
    const employeeMap = new Map();
    
    for (const employee of employees) {
      const key = employee.employeeCode || employee.code || employee.id;
      
      if (key && !employeeMap.has(key)) {
        employeeMap.set(key, true);
        uniqueEmployees.push(employee);
      }
    }
    
    return uniqueEmployees;
  }

  get filteredEmployees(): any[] {
    if (!this.employeeFilter || this.employeeFilter.trim().length === 0) {
      return this.employees.slice(0, 20);
    }
    
    const q = this.employeeFilter.toLowerCase().trim();
    
    return this.employees.filter((e: any) => {
      const name = (e.fullName || e.name || '').toLowerCase();
      const code = (e.code || e.employeeCode || '').toString().toLowerCase();
      return name.includes(q) || code.includes(q);
    }).slice(0, 20);
  }

  set selectedEmployee(value: any) {
    this._selectedEmployee = value;
    if (value) {
      this.empCode = value.code || value.employeeCode || '';
    }
  }
  
  get selectedEmployee() { 
    return this._selectedEmployee; 
  }

  onEmployeeFilterChange(q: string): void {
    this.employeeFilter = q;
    
    if (!q || q.trim().length === 0) {
      this.selectedEmployee = null;
      this.empCode = '';
    }
  }

  onEmployeeSelected(event: any): void {
    const selectedValue = event.option.value;
    const emp = this.employees.find(e => (e.code && e.code === selectedValue));
    
    if (emp) {
      this.selectedEmployee = emp;
      const empName = emp.fullName || emp.name || '';
      const empCode = emp.employeeCode || emp.code || '';
      this.employeeFilter = `${empName} (${empCode})`;
      this.empCode = empCode;
    }
  }

  onFilterChange(): void {
    if (this.empCode) {
      this.loadSummary();
    }
  }

  loadSummary() { 
    if (!this.empCode) {
      Swal.fire('تحذير', 'يرجى اختيار موظف', 'warning');
      return;
    }
    
    this.isLoading = true;
    this.service.getEmployeeDeductionsWithSummary(this.empCode, this.selectedMonth ?? undefined, this.selectedYear ?? undefined).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        // Backend wraps response in Result<EmployeeDeductionsSummaryDto>
        // Structure: { isSuccess: bool, data: EmployeeDeductionsSummaryDto, message: string }
        const summaryData = res?.data ?? res ?? {};
        this.items = summaryData?.deductions ?? [];
        this.totals = summaryData?.totals ?? {};
        this.totalCount = (this.items || []).length;
        this.employeeName = this.totals?.employeeName || '';
      },
      error: (e) => { 
        this.isLoading = false; 
        Swal.fire('خطأ', e?.error?.message ?? 'فشل التحميل', 'error'); 
      }
    });
  }

  restore(item: DeductionDetailDto) {
    Swal.fire({ 
      title: 'تأكيد الاستعادة', 
      text: 'هل تريد استعادة هذا الخصم؟',
      icon: 'warning',
      showCancelButton: true, 
      confirmButtonText: 'استعادة',
      cancelButtonText: 'إلغاء'
    }).then(r => { 
      if (r.isConfirmed) {
        this.service.restorePayrollDeduction(item.id).subscribe({ 
          next: (res: any) => { 
            Swal.fire('نجاح', res?.message ?? 'تمت الاستعادة بنجاح', 'success'); 
            this.loadSummary(); 
          }, 
          error: (e) => Swal.fire('خطأ', e?.error?.message ?? 'فشل الاستعادة', 'error') 
        });
      }
    });
  }
}

