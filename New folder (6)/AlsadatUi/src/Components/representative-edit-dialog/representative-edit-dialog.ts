import { Component, Inject, OnInit, ViewChild, inject } from '@angular/core';
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
import {
  RepresentativeDTo,
  WeekDays
} from '../../app/models/IRepresentative';
import { RepresentativeService } from '../../app/Services/representative-service';
import { CityServiceService } from '../../app/Services/city-service.service';
import { AuthService } from '../../app/Services/auth-service';
import { ICityDto } from '../../app/models/Icity';
import Swal from 'sweetalert2';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-representative-edit-dialog',
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
  templateUrl: './representative-edit-dialog.html',
  styleUrls: ['./representative-edit-dialog.css']
})
export class RepresentativeEditDialog implements OnInit {
  private representativeService = inject(RepresentativeService);
  private cityService = inject(CityServiceService);
  private authService = inject(AuthService);

  model: RepresentativeDTo = {} as RepresentativeDTo;
  cities: ICityDto[] = [];
  filteredCities: ICityDto[] = [];
  cityFilter: string = '';
  roleFilter: string = '';
  specialCityFilter: string = '';
  specialCitySelectedId: number = 0;
  filteredSpecialCities: ICityDto[] = [];
  rolesRaw: any[] = [];
  roles: string[] = [];
  filteredRoles: string[] = [];
  saving = false;

  @ViewChild('cityAuto', { read: MatAutocompleteTrigger })
  cityTrigger!: MatAutocompleteTrigger;
  @ViewChild('roleAuto', { read: MatAutocompleteTrigger })
  roleTrigger!: MatAutocompleteTrigger;
  @ViewChild('specialCityAuto', { read: MatAutocompleteTrigger })
  specialCityTrigger!: MatAutocompleteTrigger;

  // Week days mapping
  weekDays = [
    { id: 0, name: 'السبت' },
    { id: 1, name: 'الأحد' },
    { id: 2, name: 'الاثنين' },
    { id: 3, name: 'الثلاثاء' },
    { id: 4, name: 'الأربعاء' },
    { id: 5, name: 'الخميس' },
    { id: 6, name: 'الجمعة' }
  ];

  constructor(
    public dialogRef: MatDialogRef<RepresentativeEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: { representative: RepresentativeDTo }
  ) {}

  ngOnInit(): void {
    // Make shallow copy
    this.model = { ...this.data.representative } as RepresentativeDTo;

    // Convert ISO strings to Date for datepickers if needed
    try {
      if (
        this.model.birthDate &&
        typeof this.model.birthDate === 'string'
      ) {
        this.model.birthDate = new Date(this.model.birthDate);
      }
      if (
        this.model.hireDate &&
        typeof this.model.hireDate === 'string'
      ) {
        this.model.hireDate = new Date(this.model.hireDate);
      }
    } catch (e) {
      // Ignore parse errors
    }

    this.loadCities();

    // Load roles (active only)
    this.authService
      .getAllRoles()
      .pipe(
        catchError(() =>
          of({
            isSuccess: true,
            data: [
              {
                roleID: 'default',
                roleName: 'Employee',
                createdAt: null,
                isDeleted: false
              }
            ]
          })
        )
      )
      .subscribe({
        next: (res: any) => {
          const list = (res?.isSuccess ? res.data : res) || [];
          const items = Array.isArray(list)
            ? list.filter((x: any) => !x.isDeleted)
            : [];
          this.rolesRaw = items;
          this.roles = items.map((x: any) => x.roleName);
          this.filteredRoles = [...this.roles];
        },
        error: () => {
          this.roles = ['Employee', 'Admin'];
        }
      });
  }

  openCityPanel() {
    try {
      this.cityTrigger.openPanel();
    } catch {}
  }

  openRolePanel() {
    try {
      this.roleTrigger.openPanel();
    } catch {}
  }

  onCitySelected(cityName: string | null) {
    if (!cityName) {
      this.model.cityName = '';
      this.model.cityID = 0;
      return;
    }
    const name = cityName as string;
    const c = this.cities.find(
      (x) => (x.cityName ?? '').toLowerCase() === name.toLowerCase()
    );
    if (c) {
      this.model.cityName = c.cityName ?? undefined;
      this.model.cityID = (c as any).id ?? 0;
    }
  }

  onRoleSelected(roleName: string | null) {
    if (!roleName) {
      this.model.roleName = '';
      this.model.roleId = undefined;
      return;
    }
    if (typeof roleName === 'string') {
      this.model.roleName = roleName;
      const found = this.rolesRaw.find(
        (r) => r.roleName === roleName || r.role === roleName
      );
      if (found) this.model.roleId = found.roleID ?? found.roleId ?? undefined;
    } else if (typeof roleName === 'object') {
      const obj: any = roleName as any;
      this.model.roleName = obj.roleName ?? obj.role ?? '';
      this.model.roleId = obj.roleID ?? obj.roleId ?? undefined;
    }
  }

  loadCities() {
    this.cityService
      .getAllCities({
        page: 1,
        pageSize: 1000,
        cityName: null,
        governrateName: null
      })
      .subscribe({
        next: (res: any) => {
          if (res && res.data) {
            this.cities = res.data;
            this.filteredCities = [...this.cities];
            this.filteredSpecialCities = [...this.cities];
          }
        },
        error: (err) => {
          console.error('Failed to load cities', err);
        }
      });
  }

  filterCities(value: string) {
    const q = (value || '').trim().toLowerCase();
    if (!q) {
      this.filteredCities = [...this.cities];
      return;
    }
    this.filteredCities = this.cities.filter(
      (c) =>
        (c.cityName ?? '').toLowerCase().includes(q) ||
        (c.governrateName ?? '').toLowerCase().includes(q)
    );
  }

  filterRoles(q: string) {
    const s = (q || '').trim().toLowerCase();
    if (!s) {
      this.filteredRoles = [...this.roles];
      return;
    }
    this.filteredRoles = this.roles.filter((r) =>
      (r || '').toLowerCase().includes(s)
    );
  }

  validateAndPrepareModel(): boolean {
    // City: if name provided but id missing, try to find it
    if (this.model.cityName && !this.model.cityID) {
      const city = this.cities.find(
        (c) => (c.cityName ?? '').toLowerCase() === (this.model.cityName ?? '').toLowerCase()
      );
      if (city) {
        this.model.cityID = (city as any).id ?? 0;
      }
    }

    // Role: map roleName -> roleId
    if (this.model.roleName && !this.model.roleId) {
      const role = this.rolesRaw.find(
        (r) => r.roleName === this.model.roleName || r.role === this.model.roleName
      );
      if (role) {
        this.model.roleId = role.roleID ?? role.roleId ?? undefined;
      }
    }

    return true;
  }

  save(): void {
    this.saving = true;

    // Enforce selection-only behavior
    if (!this.validateAndPrepareModel()) {
      this.saving = false;
      return;
    }

    // Client-side validation: SNO is required by backend
    if (!this.model.sno || (typeof this.model.sno === 'string' && !this.model.sno.trim())) {
      Swal.fire('خطأ', 'يرجى إدخال الرقم القومي', 'error');
      this.saving = false;
      return;
    }

    // Prepare data for backend
    const dataToSend = { ...this.model };

    // Convert dates to YYYY-MM-DD format (DateOnly format for backend)
    if (dataToSend.birthDate) {
      const bd = new Date(dataToSend.birthDate);
      dataToSend.birthDate = bd.toISOString().split('T')[0];
    }
    if (dataToSend.hireDate) {
      const hd = new Date(dataToSend.hireDate);
      dataToSend.hireDate = hd.toISOString().split('T')[0];
    }

    // Make sure cityName is set for backend convenience
    if (this.model.cityID) {
      const city = this.cities.find((c) => (c as any).id === this.model.cityID);
      if (city) {
        dataToSend.cityName = city.cityName ?? undefined;
      }
    }

    this.representativeService.updateRepresentative(dataToSend).subscribe({
      next: (res: any) => {
        this.saving = false;
        Swal.fire('نجح', 'تم تعديل المندوب بنجاح', 'success');
        this.dialogRef.close({ success: true });
      },
      error: (err) => {
        this.saving = false;
        const msg = err?.error?.message ?? 'حدث خطأ في تعديل المندوب';
        Swal.fire('خطأ', msg, 'error');
      }
    });
  }

  openSpecialCityPanel() {
    try {
      this.specialCityTrigger?.openPanel();
    } catch {}
  }

  filterSpecialCities(q: string) {
    this.specialCityFilter = q;
    if (!q) {
      this.filteredSpecialCities = this.cities;
    } else {
      const lq = q.toLowerCase();
      this.filteredSpecialCities = this.cities.filter(
        (x) => (x.cityName ?? '').toLowerCase().includes(lq)
      );
    }
  }

  onSpecialCitySelected(cityName: string | null) {
    if (!cityName) {
      this.specialCitySelectedId = 0;
      return;
    }
    const name = cityName as string;
    const c = this.cities.find(
      (x) => (x.cityName ?? '').toLowerCase() === name.toLowerCase()
    );
    if (c) {
      this.specialCitySelectedId = c.id ?? 0;
      this.specialCityFilter = name;
    }
  }

  addSpecialCity() {
    if (this.specialCitySelectedId <= 0) {
      Swal.fire('تحذير', 'اختر مدينة من القائمة', 'warning');
      return;
    }

    const selectedCity = this.cities.find(
      (x) => x.id === this.specialCitySelectedId
    );
    if (!selectedCity) {
      Swal.fire('تحذير', 'المدينة المختارة غير موجودة', 'warning');
      return;
    }

    // Check if city already added
    const alreadyExists = this.model.specialRepresentiveCities?.some(
      (x) => x.cityId === this.specialCitySelectedId
    );
    if (alreadyExists) {
      Swal.fire('تحذير', 'هذه المدينة مضافة بالفعل', 'warning');
      return;
    }

    // Add the city
    if (!this.model.specialRepresentiveCities) {
      this.model.specialRepresentiveCities = [];
    }
    this.model.specialRepresentiveCities.push({
      id: 0,
      cityId: this.specialCitySelectedId,
      cityName: selectedCity.cityName ?? ''
    });

    // Reset input
    this.specialCityFilter = '';
    this.specialCitySelectedId = 0;
  }

  removeSpecialCity(index: number) {
    if (this.model.specialRepresentiveCities) {
      this.model.specialRepresentiveCities.splice(index, 1);
    }
  }

  cancel(): void {
    this.dialogRef.close({ success: false });
  }
}
