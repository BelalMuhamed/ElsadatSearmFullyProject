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
import {
  RepresentativeDTo,
  Gender,
  RepresentiveType,
  WeekDays
} from '../../app/models/IRepresentative';
import { RepresentativeService } from '../../app/Services/representative-service';
import { CityServiceService } from '../../app/Services/city-service.service';
import { AuthService } from '../../app/Services/auth-service';
import { ICityDto } from '../../app/models/Icity';
import Swal from 'sweetalert2';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-representative-add',
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
  templateUrl: './representative-add-component.html',
  styleUrls: ['./representative-add-component.css']
})
export class RepresentativeAddComponent {
  private representativeService = inject(RepresentativeService);
  private cityService = inject(CityServiceService);
  private authService = inject(AuthService);
  private router = inject(Router);

  model: RepresentativeDTo = {} as RepresentativeDTo;

  cities: ICityDto[] = [];
  filteredCities: ICityDto[] = [];
  cityFilter: string = '';
  roleFilter: string = '';
  specialCityFilter: string = '';
  specialCitySelectedId: number = 0;
  filteredSpecialCities: ICityDto[] = [];
  isSaving = false;

  @ViewChild('cityAuto', { read: MatAutocompleteTrigger })
  cityTrigger!: MatAutocompleteTrigger;
  @ViewChild('roleAuto', { read: MatAutocompleteTrigger })
  roleTrigger!: MatAutocompleteTrigger;
  @ViewChild('specialCityAuto', { read: MatAutocompleteTrigger })
  specialCityTrigger!: MatAutocompleteTrigger;

  showPassword = false;
  roles: string[] = [];
  filteredRoles: string[] = [];
  rolesRaw: any[] = [];

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

  constructor() {
    this.loadCities();

    // Initialize model defaults
    this.model.birthDate = this.model.birthDate ?? new Date();
    this.model.hireDate = this.model.hireDate ?? new Date();
    this.model.timeIn = this.model.timeIn ?? '09:00:00';
    this.model.timeOut = this.model.timeOut ?? '17:00:00';
    this.model.overtimeRatePerHour = this.model.overtimeRatePerHour ?? 0;
    this.model.pointsWallet = this.model.pointsWallet ?? 0;
    this.model.moneyDeposit = this.model.moneyDeposit ?? 0;
    this.model.salary = this.model.salary ?? 0;
    this.model.weekHoliday1 = this.model.weekHoliday1 ?? WeekDays.Friday;
    this.model.password = this.model.password ?? '';
    this.model.representativeCode = this.model.representativeCode ?? '';
    this.model.isDeleted = false;
    this.model.rolesName = [];
    this.model.rolesId = [];
    this.model.specialRepresentiveCities = [];

    // Load active roles from backend
    this.authService.getAllRoles()
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
          let items = Array.isArray(list)
            ? list.filter((x: any) => !x.isDeleted)
            : [];
          const userRoles = this.authService.getRoles() || [];
          if (!userRoles.includes('Admin')) {
            items = items.filter((r: any) => ((r.roleName ?? r.role) + '') !== 'Admin');
          }
          this.rolesRaw = items;
          this.roles = items.map((x: any) => x.roleName);
          this.filteredRoles = [...this.roles];
        },
        error: () => {
          const userRoles = this.authService.getRoles() || [];
          this.roles = ['Employee'];
          if (userRoles.includes('Admin')) this.roles.push('Admin');
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
        error: (err) => console.error('Failed to load cities', err)
      });
  }

  filterCities(q: string) {
    const s = (q || '').trim().toLowerCase();
    if (!s) {
      this.filteredCities = [...this.cities];
      return;
    }
    this.filteredCities = this.cities.filter(
      (c) =>
        (c.cityName ?? '').toLowerCase().includes(s) ||
        (c.governrateName ?? '').toLowerCase().includes(s)
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

  cancel() {
    this.router.navigate(['/sales/representatives']);
  }

  goBack() {
    this.router.navigate(['/sales/representatives']);
  }

  save() {
    this.isSaving = true;

    // Basic required checks
    if (!this.model.email || !this.model.fullName || !this.model.sno) {
      Swal.fire(
        'خطأ',
        'يرجى إدخال البريد الإلكتروني والاسم والرقم القومي',
        'error'
      );
      this.isSaving = false;
      return;
    }

    if (!this.model.password || this.model.password.trim() === '') {
      Swal.fire('خطأ', 'يرجى إدخال كلمة المرور', 'error');
      this.isSaving = false;
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

    // Ensure cityName copied for backend
    if (this.model.cityID) {
      const city = this.cities.find((c) => (c as any).id === this.model.cityID);
      if (city) {
        dataToSend.cityName = city.cityName ?? undefined;
      }
    }

    this.representativeService.addRepresentative(dataToSend).subscribe({
      next: (res: any) => {
        this.isSaving = false;
        Swal.fire('نجح', 'تم إضافة المندوب بنجاح', 'success');
        this.router.navigate(['/sales/representatives']);
      },
      error: (err) => {
        this.isSaving = false;
        const msg = err?.error?.message ?? 'حدث خطأ في إضافة المندوب';
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

  toggleShowPassword() {
    this.showPassword = !this.showPassword;
  }
}
