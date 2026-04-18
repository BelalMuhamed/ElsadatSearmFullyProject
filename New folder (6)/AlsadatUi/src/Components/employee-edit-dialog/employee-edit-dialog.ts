import { Component, Inject, OnInit, inject, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
// removed autocomplete import (using searchable selects instead)
import { AuthService } from '../../app/Services/auth-service';
import { EmployeeDTo } from '../../app/models/IEmployee';
import { EmployeeService } from '../../app/Services/employee.service';
import { CityServiceService } from '../../app/Services/city-service.service';
import { ICityDto } from '../../app/models/Icity';
import { DepartmentService } from '../../app/Services/department.service';
import { Department } from '../../app/models/IDepartment';
import Swal from 'sweetalert2';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-employee-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
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
  templateUrl: './employee-edit-dialog.html',
  styleUrls: ['./employee-edit-dialog.css']
})
export class EmployeeEditDialog implements OnInit {
  private employeeService = inject(EmployeeService);
  private cityService = inject(CityServiceService);
  private authService = inject(AuthService);
  private departmentService = inject(DepartmentService);

  model: EmployeeDTo = {} as EmployeeDTo;
  cities: ICityDto[] = [];
  filteredCities: ICityDto[] = [];
  departments: Department[] = [];
  filteredDepartments: Department[] = [];
  deptFilter: string = '';
  roleFilter: string = '';
  rolesRaw: any[] = [];
  roles: string[] = [];
  filteredRoles: string[] = [];
  cityFilter: string = '';
  saving = false;
  @ViewChild('deptAuto', { read: MatAutocompleteTrigger }) deptTrigger!: MatAutocompleteTrigger;
  @ViewChild('cityAuto', { read: MatAutocompleteTrigger }) cityTrigger!: MatAutocompleteTrigger;
  @ViewChild('roleAuto', { read: MatAutocompleteTrigger }) roleTrigger!: MatAutocompleteTrigger;

  constructor(
    public dialogRef: MatDialogRef<EmployeeEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { employee: EmployeeDTo }
    
  ) {}

  ngOnInit(): void {
    // make a shallow copy so editing doesn't immediately mutate parent object
    this.model = { ...this.data.employee } as EmployeeDTo;

    // convert ISO strings to Date for datepickers if needed
    try {
      if (this.model.birthDate && typeof this.model.birthDate === 'string') {
        this.model.birthDate = new Date(this.model.birthDate as any) as any;
      }
      if (this.model.hireDate && typeof this.model.hireDate === 'string') {
        this.model.hireDate = new Date(this.model.hireDate as any) as any;
      }
    } catch (e) {
      // ignore parse errors
    }

    this.loadCities();
    this.loadDepartments();
    // load roles (active only)
    this.authService.getAllRoles().pipe(
      catchError(() => of({ isSuccess: true, data: [{ roleID: 'default', roleName: 'Employee', createdAt: null, isDeleted: false }] }))
    ).subscribe({
      next: (res: any) => {
        const list = (res?.isSuccess ? res.data : res) || [];
        const items = Array.isArray(list) ? list.filter((x: any) => !x.isDeleted) : [];
        this.rolesRaw = items;
        this.roles = items.map((x: any) => x.roleName);
        this.filteredRoles = [...this.roles];
        // If the model has a roleId but not a roleName, try to map it
        if (this.model.roleId && !this.model.roleName) {
          const found = items.find((x: any) => (x.roleID === this.model.roleId) || (x.roleId === this.model.roleId) || (x.roleName === this.model.roleName));
          if (found) this.model.roleName = found.roleName ?? '';
        }
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

      openRolePanel() {
        try { this.roleTrigger.openPanel(); } catch { }
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
        }, error: (e) => { this.departments = []; this.filteredDepartments = []; } });
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
          error: (err) => { /* failed to load cities */ }
        });
      }

      filterCities(value: string) {
        const q = (value || '').trim().toLowerCase();
        if (!q) {
          this.filteredCities = [...this.cities];
          return;
        }
        this.filteredCities = this.cities.filter(c => (c.cityName ?? '').toLowerCase().includes(q) || (c.governrateName ?? '').toLowerCase().includes(q));
      }

  /**
   * Validate that fields which must be chosen from the lists were actually selected
   * and map names -> ids when possible. Returns true if the model is valid for saving.
   */
  validateAndPrepareModel(): boolean {
    // City: if name provided but id missing, try to find it
    if (this.model.cityName && !this.model.cityID) {
      const found = this.cities.find(c => (c.cityName ?? '').toLowerCase() === (this.model.cityName ?? '').toLowerCase());
      if (found) {
        this.model.cityID = found.id ?? undefined;
      } else {
        Swal.fire('خطأ', 'الرجاء اختيار المدينة من القائمة', 'error');
        return false;
      }
    }
    // Department: same behavior
    if (this.model.departmentName && !this.model.departmentID) {
      const found = this.departments.find(d => (d.name ?? '').toLowerCase() === (this.model.departmentName ?? '').toLowerCase());
      if (found) {
        this.model.departmentID = (found as any).id ?? undefined;
      } else {
        Swal.fire('خطأ', 'الرجاء اختيار القسم من القائمة', 'error');
        return false;
      }
    }
    // Role: map roleName -> roleId
    if (this.model.roleName && !this.model.roleId) {
      const found = this.rolesRaw.find((r: any) => (r.roleName ?? '').toLowerCase() === (this.model.roleName ?? '').toLowerCase() || (r.role ?? '').toLowerCase() === (this.model.roleName ?? '').toLowerCase());
      if (found) {
        this.model.roleId = found.roleID ?? found.roleId ?? undefined;
      } else {
        Swal.fire('خطأ', 'الرجاء اختيار الدور من القائمة', 'error');
        return false;
      }
    }

    return true;
  }

  save(): void {
    this.saving = true;

    // enforce selection-only behavior: make sure typed names map to existing items
    if (!this.validateAndPrepareModel()) { this.saving = false; return; }

    // make sure cityName is set for backend convenience
    if (this.model.cityID) {
      const city = this.cities.find(c => c.id === this.model.cityID);
      if (city) this.model.cityName = city.cityName ?? '';
    }

    // client-side validation: SNO is required by backend
    if (!this.model.sno || (typeof this.model.sno === 'string' && !this.model.sno.trim())) {
      this.saving = false;
      Swal.fire('خطأ', 'الرجاء إدخال الرقم التأسيسي (SNO) قبل الحفظ', 'error');
      return;
    }

    this.employeeService.updateEmployee(this.model).subscribe({
      next: (res: any) => {
        this.saving = false;
        const message = typeof res === 'string' ? res : (res?.message ?? 'تم التحديث بنجاح');
        Swal.fire('نجاح', message, 'success').then(() => {
          this.dialogRef.close({ success: true, message });
        });
      },
      error: (err) => {
        this.saving = false;
        const msg = err?.error ?? err?.message ?? 'حدث خطأ أثناء الحفظ';
        Swal.fire('خطأ', msg, 'error');
      }
    });
  }

  cancel(): void {
    this.dialogRef.close({ success: false });
  }
}
