import { Component, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { EmployeeDTo } from '../../app/models/IEmployee';
import { EmployeeService } from '../../app/Services/employee.service';
import { CityServiceService } from '../../app/Services/city-service.service';
import { AuthService } from '../../app/Services/auth-service';
import { ICityDto } from '../../app/models/Icity';
import { DepartmentService } from '../../app/Services/department.service';
import { Department } from '../../app/models/IDepartment';
import Swal from 'sweetalert2';
import { EmployeeService as EmpService } from '../../app/Services/employee.service';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-employee-add',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatAutocompleteModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCardModule
  ],
  templateUrl: './employee-add-component.html',
  styleUrls: ['./employee-add-component.css']
})
export class EmployeeAddComponent {
  private employeeService = inject(EmployeeService);
  private cityService = inject(CityServiceService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private departmentService = inject(DepartmentService);

  // Reuse the interface instead of re-declaring every field here.
  // Minimal defaults are set in the constructor so template bindings work.
  model: EmployeeDTo = {} as EmployeeDTo;

  cities: ICityDto[] = [];
  filteredCities: ICityDto[] = [];
  departments: Department[] = [];
  filteredDepartments: Department[] = [];
  cityFilter: string = '';
  deptFilter: string = '';
  roleFilter: string = '';
  isSaving = false;
  @ViewChild('deptAuto', { read: MatAutocompleteTrigger }) deptTrigger!: MatAutocompleteTrigger;
  @ViewChild('cityAuto', { read: MatAutocompleteTrigger }) cityTrigger!: MatAutocompleteTrigger;
  showPassword = false;
  roles: string[] = [];
  filteredRoles: string[] = [];
  rolesRaw: any[] = [];

  constructor() {
    this.loadCities();
    this.loadDepartments();
    // Ensure a few minimal defaults at runtime so template bindings work
    this.model.birthDate = this.model.birthDate ?? new Date();
    this.model.hireDate = this.model.hireDate ?? new Date();
    this.model.timeIn = this.model.timeIn ?? '09:00:00';
    this.model.timeOut = this.model.timeOut ?? '17:00:00';
    this.model.overtimeRatePerHour = this.model.overtimeRatePerHour ?? 0;
    this.model.deductionRatePerHour = this.model.deductionRatePerHour ?? 0;
    this.model.weekHoliday1 = this.model.weekHoliday1 ?? 1;
    // Ensure some string fields are empty to avoid browser autofill showing unexpected values
    this.model.accountName = this.model.accountName ?? '';
    this.model.password = this.model.password ?? '';
    this.model.employeeCode = this.model.employeeCode ?? '';

    // Load active roles from backend; fallback to common roles
    this.authService.getAllRoles().pipe(
      catchError(() => of({ isSuccess: true, data: [{ roleID: 'default', roleName: 'Employee', createdAt: null, isDeleted: false }] }))
    ).subscribe({
      next: (res: any) => {
        const list = (res?.isSuccess ? res.data : res) || [];
        const items = Array.isArray(list) ? list.filter((x: any) => !x.isDeleted) : [];
        this.rolesRaw = items;
        this.roles = items.map((x: any) => x.roleName);
        this.filteredRoles = [...this.roles];
      },
      error: () => { this.roles = ['Employee', 'Admin']; }
    });
  }

  onDepartmentSelected(deptName: string | null) {
    if (!deptName) return;
    const name = deptName as string;
    this.model.departmentName = name;
    const found = this.departments.find(d => (d.name ?? '').toLowerCase() === name.toLowerCase());
    if (found) this.model.departmentID = (found as any).id ?? undefined;
  }

  openDeptPanel() {
    try { this.deptTrigger.openPanel(); } catch { }
  }

  openCityPanel() {
    try { this.cityTrigger.openPanel(); } catch { }
  }

  onRoleSelected(roleName: string | null) {
    // roleName might be a string (role name) or an object (role DTO) depending on option value
    if (!roleName) { this.model.roleName = ''; this.model.roleId = undefined; return; }
    if (typeof roleName === 'string') {
      this.model.roleName = roleName;
      const found = this.rolesRaw.find(r => r.roleName === roleName || r.role === roleName);
      if (found) this.model.roleId = found.roleID ?? found.roleId ?? undefined;
    } else if (typeof roleName === 'object') {
      const obj: any = roleName as any;
      this.model.roleName = obj.roleName ?? obj.role ?? '';
      this.model.roleId = obj.roleID ?? obj.roleId ?? undefined;
    }
  }

  onCitySelected(cityName: string | null) {
    if (!cityName) { this.model.cityID = undefined; this.model.cityName = '' ; return; }
    const name = cityName as string;
    const c = this.cities.find(x => (x.cityName ?? '').toLowerCase() === name.toLowerCase());
    if (c) { this.model.cityID = c.id ?? undefined; this.model.cityName = c.cityName ?? ''; }
  }

  loadDepartments() {
    this.departmentService.getAll(1,1000).subscribe({ next: (res: any) => {
      const list = res?.items ?? res?.data ?? [];
      this.departments = Array.isArray(list) ? list.filter((d: any) => !d.isDeleted) : [];
      this.filteredDepartments = [...this.departments];
    }, error: (e) => { console.warn('Failed to load departments', e); this.departments = []; this.filteredDepartments = []; } });
  }

  filterDepartments(q: string) {
    const s = (q || '').trim().toLowerCase();
    if (!s) { this.filteredDepartments = [...this.departments]; return; }
    this.filteredDepartments = this.departments.filter(d => (d.name || '').toLowerCase().includes(s));
  }

  filterRoles(q: string) {
    const s = (q || '').trim().toLowerCase();
    if (!s) { this.filteredRoles = [...this.roles]; return; }
    this.filteredRoles = this.roles.filter(r => (r || '').toLowerCase().includes(s));
  }


  loadCities() {
    this.cityService.getAllCities({ page: 1, pageSize: 1000, cityName: null, governrateName: null }).subscribe({
      next: (res: any) => {
        const list = res?.data ?? [];
        this.cities = list;
        this.filteredCities = [...this.cities];
      },
      error: (err) => console.error('Failed to load cities', err)
    });
  }

  // helper: map week day options
  weekDays = [
    { id: 0, name: 'الأحد' },
    { id: 1, name: 'الاثنين' },
    { id: 2, name: 'الثلاثاء' },
    { id: 3, name: 'الأربعاء' },
    { id: 4, name: 'الخميس' },
    { id: 5, name: 'الجمعة' },
    { id: 6, name: 'السبت' }
  ];

  filterCities(q: string) {
    const s = (q || '').trim().toLowerCase();
    if (!s) { this.filteredCities = [...this.cities]; return; }
    this.filteredCities = this.cities.filter(c => (c.cityName ?? '').toLowerCase().includes(s) || (c.governrateName ?? '').toLowerCase().includes(s));
  }

  cancel() {
    this.router.navigate(['/hr/employees']);
  }

  goBack() {
    this.router.navigate(['/hr/employees']);
  }

  save() {
    this.isSaving = true;

    // basic required checks
    if (!this.model.email || !this.model.fullName || !this.model.sno) {
      this.isSaving = false;
      Swal.fire('خطأ', 'الرجاء ملء الحقول المطلوبة: البريد، الاسم، الرقم التأسيسي (SNO).', 'error');
      return;
    }

    // ensure cityName copied for backend
    if (this.model.cityID) {
      const city = this.cities.find(c => c.id === this.model.cityID);
      if (city) this.model.cityName = city.cityName ?? '';
    }

    this.employeeService.addEmployee(this.model).subscribe({
      next: (res: any) => {
        this.isSaving = false;
        const message = typeof res === 'string' ? res : (res?.message ?? 'تم الإنشاء بنجاح');
        Swal.fire('نجاح', message, 'success').then(() => this.router.navigate(['/hr/employees']));
      },
      error: (err) => {
        this.isSaving = false;
        console.error('Add employee error', err);
        const msg = err?.error ?? err?.message ?? 'حدث خطأ أثناء الإضافة';
        Swal.fire('خطأ', msg, 'error');
      }
    });
  }

  toggleShowPassword() {
    this.showPassword = !this.showPassword;
  }

}
