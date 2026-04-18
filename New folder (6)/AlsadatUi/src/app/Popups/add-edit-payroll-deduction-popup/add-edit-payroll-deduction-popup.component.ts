import { Component, inject, Inject, OnInit, ViewChild } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import Swal from 'sweetalert2';
import { EmployeeService } from '../../Services/employee.service';
import { RepresentativeService } from '../../Services/representative-service';
import { PayrollDeductionsDto, DeductionDetailDto } from '../../models/IPayrollDeduction';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-add-edit-payroll-deduction-popup',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    FormsModule, 
    MatDialogModule, 
    MatFormFieldModule, 
    MatInputModule, 
    MatButtonModule, 
    MatDatepickerModule, 
    MatNativeDateModule, 
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    MatCardModule
  ],
  templateUrl:'./add-edit-payroll-deduction-popup.component.html',
  styleUrls: ['./add-edit-payroll-deduction-popup.component.css']
})
export class AddEditPayrollDeductionPopupComponent implements OnInit {
  private fb = inject(FormBuilder);
  private empService = inject(EmployeeService);
  private representativeService = inject(RepresentativeService);
  constructor(private dialogRef: MatDialogRef<AddEditPayrollDeductionPopupComponent>, @Inject(MAT_DIALOG_DATA) public data: DeductionDetailDto | null) {}

  form!: FormGroup;
  employees: any[] = [];
  employeeFilter = '';
  employeesLoading = false;
  isViewMode = false;
  isSaving = false;
  _selectedEmployee: any = null;
  
  @ViewChild(MatAutocompleteTrigger) autoTrigger?: MatAutocompleteTrigger;

  ngOnInit(): void {
    this.initData();
  }

  async initData(): Promise<void> {
    this.data == this.data || {};
    this.isViewMode = false; // للخصومات نسمح بالتعديل دائماً

    // تحميل الموظفين أولاً
    await this.loadAllEmployees();
    
    // ثم معالجة بيانات الخصم
    this.initForm();
    this.processDeductionData();
  }

  async loadAllEmployees(): Promise<void> {
    return new Promise((resolve) => {
      this.employeesLoading = true;
      const params = { pageNumber: 1, pageSize: 1000 };

      forkJoin({
        employees: this.empService.getEmployeesByFilter(params as any, {}),
        representatives: this.representativeService.getRepresentativesByFilter(params as any, { representativeCode: '', representativeName: '', cityName: '', isActive: true, representiveType: 0 } as any)
      }).subscribe({
        next: (res: any) => {
          let loadedEmployees: any[] = [];
          const emps = (res.employees?.items ?? res.employees?.data ?? res.employees) as any[] || [];
          const reps = (res.representatives?.items ?? res.representatives?.data ?? res.representatives) as any[] || [];

          emps.forEach(e => loadedEmployees.push({ code: e.employeeCode || e.code || '', fullName: e.fullName || e.name || '', raw: e, isRepresentative: false }));
          reps.forEach(r => loadedEmployees.push({ code: r.representativesCode || r.representativeCode || r.code || '', fullName: (r.user?.fullName || r.user?.FullName || r.fullName || r.name) || '', raw: r, isRepresentative: true }));

          // إزالة التكرارات
          this.employees = this.removeDuplicates(loadedEmployees);

          this.employeesLoading = false;
          resolve();
        },
        error: (err) => {
          this.employees = [];
          this.employeesLoading = false;
          resolve();
        }
      });
    });
  }

  // دالة لإزالة التكرارات من قائمة الموظفين
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

  initForm() {
    this.form = this.fb.group({
      id: [this.data?.id ?? null],
      employeeCode: [this.data?.employeeCode || '', Validators.required],
      deductionDate: [this.data?.deductionDate ? new Date(this.data.deductionDate) : new Date(), Validators.required],
      deductionAmount: [this.data?.deductionAmount ?? 0, [Validators.required, Validators.min(0.01)]],
      deductionReason: [this.data?.deductionReason || '', Validators.required]
    });
  }

  processDeductionData(): void {
    const deductionData = this.data;
    
    if (deductionData) {
      this.form.patchValue({
        id: deductionData.id ?? null,
        employeeCode: deductionData.employeeCode ?? deductionData.representativeCode ?? '',
        deductionDate: deductionData.deductionDate ? new Date(deductionData.deductionDate) : new Date(),
        deductionAmount: deductionData.deductionAmount ?? 0,
        deductionReason: deductionData.deductionReason ?? ''
      });
      
      // البحث عن الموظف/المندوب المحدد وتعيينه
      const lookupCode = deductionData.employeeCode ?? deductionData.representativeCode ?? null;
      if (lookupCode) {
        const foundEmp = this.employees.find((e: any) => e.code === lookupCode || e.employeeCode === lookupCode);
        if (foundEmp) {
          this.selectedEmployee = foundEmp;
        }
      }
      
      // تعيين قيمة حقل البحث للعرض
      if (this.selectedEmployee) {
        const empName = this.selectedEmployee.fullName || this.selectedEmployee.name || '';
        const empCode = this.selectedEmployee.employeeCode || this.selectedEmployee.code || '';
        this.employeeFilter = `${empName} (${empCode})`;
      } else if (deductionData.employeeCode) {
        this.employeeFilter = deductionData.employeeCode;
      }
    }
  }

  get filteredEmployees(): any[] {
    if (!this.employeeFilter || this.employeeFilter.trim().length === 0) {
      return this.employees.slice(0, 20);
    }
    
    const q = this.employeeFilter.toLowerCase().trim();
    
    return this.employees.filter((e: any) => {
      const name = (e.fullName || e.name || '').toLowerCase();
      const code = (e.employeeCode || e.code || '').toString().toLowerCase();
      
      return name.includes(q) || code.includes(q);
    }).slice(0, 20);
  }

  set selectedEmployee(value: any) {
    this._selectedEmployee = value;
    if (value) {
      this.form.patchValue({ employeeCode: value.code || value.employeeCode });
    }
  }
  
  get selectedEmployee() { 
    return this._selectedEmployee; 
  }

  close() { this.dialogRef.close(null); }

  submit() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    
    const v = this.form.value;
    
    // التحقق من وجود كود الموظف
    if (!v.employeeCode) {
      Swal.fire('تحذير', 'يرجى اختيار الموظف أو المندوب', 'warning');
      return;
    }
    
    // Convert date to YYYY-MM-DD format to match backend expectations
    let dateString = '';
    if (v.deductionDate instanceof Date) {
      const year = v.deductionDate.getFullYear();
      const month = String(v.deductionDate.getMonth() + 1).padStart(2, '0');
      const day = String(v.deductionDate.getDate()).padStart(2, '0');
      dateString = `${year}-${month}-${day}`;
    } else {
      dateString = v.deductionDate;
    }
    
    const dto: PayrollDeductionsDto = {
      deductionDate: dateString,
      deductionAmount: Number(v.deductionAmount),
      deductionReason: v.deductionReason
    };

    // set either employeeCode or representativeCode depending on selection
    if (this.selectedEmployee?.isRepresentative) {
      dto.representativeCode = this.selectedEmployee.code || v.employeeCode;
    } else {
      dto.employeeCode = this.selectedEmployee?.code || v.employeeCode;
    }
    
    // Only add ID if editing existing record
    if (v.id) {
      dto.id = v.id;
    }
    
    this.dialogRef.close(dto);
  }

  onEmployeeFilterChange(q: string): void {
    this.employeeFilter = q;
    
    if (!q || q.trim().length === 0) {
      this.selectedEmployee = null;
      this.form.patchValue({ employeeCode: '' });
    }
  }

  onEmployeeSelected(event: any): void {
    const selectedValue = event.option.value;
    
    // البحث عن الموظف في القائمة
    const emp = this.employees.find(e => 
      (e.employeeCode && e.employeeCode === selectedValue) || 
      (e.code && e.code === selectedValue)
    );
    
    if (emp) {
      this.selectedEmployee = emp;
      const empName = emp.fullName || emp.name || '';
      const empCode = emp.employeeCode || emp.code || '';
      this.employeeFilter = `${empName} (${empCode})`;
      this.form.patchValue({ employeeCode: emp.employeeCode || emp.code });
    }
  }
}
