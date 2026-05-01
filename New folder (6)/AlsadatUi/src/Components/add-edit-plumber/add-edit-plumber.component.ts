import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';

import { PlumberService } from '../../app/Services/plumber.service';
import { SwalService } from '../../app/Services/swal.service';

import {
  PlumberDto,
  PLUMBER_SPECIALTY_OPTIONS
} from '../../app/models/IPlumberModels';
import { CityServiceService } from '../../app/Services/city-service.service';
import { ICityDto, ICityFilteration } from '../../app/models/Icity';

/**
 * Add / Edit Plumber form.
 * Same lifecycle pattern as AddEditSupplierComponent — listens on paramMap so
 * navigating between /plumber/add and /plumber/edit/:id resets cleanly.
 */
@Component({
  selector: 'app-add-edit-plumber',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatAutocompleteModule
  ],
  templateUrl: './add-edit-plumber.component.html',
  styleUrls: ['./add-edit-plumber.component.css']
})
export class AddEditPlumberComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly plumberService = inject(PlumberService);
  private readonly cityService = inject(CityServiceService);
  private readonly alert = inject(SwalService);
  private readonly destroyRef = inject(DestroyRef);

  readonly specialtyOptions = PLUMBER_SPECIALTY_OPTIONS;

  form!: FormGroup;
  cities: ICityDto[] = [];
  plumberId: number | null = null;
  isEditMode = false;
  isSaving = false;
  isLoading = false;

  private readonly cityFilters: ICityFilteration = {
    page: null,
    pageSize: null,
    cityName: null,
    governrateName: null
  };

  ngOnInit(): void {
    this.initializeForm();
    this.loadCities();

    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const raw = params.get('id');
        const parsed = raw ? Number(raw) : NaN;
        const validId = Number.isFinite(parsed) && parsed > 0;

        this.plumberId = validId ? parsed : null;
        this.isEditMode = validId;

        this.form.reset({
          id: null,
          name: '',
          phoneNumbers: '',
          address: '',
          cityId: null,
          licenseNumber: '',
          specialty: '',
          isDeleted: false
        });

        if (this.isEditMode && this.plumberId) {
          this.loadPlumber(this.plumberId);
        }
      });
  }

  // ---------------------------------------------------------------
  // Form setup
  // ---------------------------------------------------------------
  private initializeForm(): void {
    this.form = this.fb.group({
      id: [null as number | null],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      phoneNumbers: [
        '',
        [
          Validators.required,
          Validators.maxLength(50),
          // E.164 — same as backend regex
          Validators.pattern(/^\+[1-9]\d{6,14}$/)
        ]
      ],
      address: ['', Validators.maxLength(500)],
      cityId: [null as number | null],
      licenseNumber: ['', Validators.maxLength(50)],
      specialty: ['', Validators.maxLength(100)],
      isDeleted: [false]
    });
  }

  // ---------------------------------------------------------------
  // Data loading
  // ---------------------------------------------------------------
  private loadCities(): void {
    this.cityService.getAllCities(this.cityFilters)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (res: any) => { this.cities = res?.data ?? []; },
        error: () => this.alert.error('تعذر تحميل قائمة المدن')
      });
  }

  private loadPlumber(id: number): void {
    this.isLoading = true;
    this.plumberService.getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isLoading = false;
          if (res.isSuccess && res.data) {
            this.form.patchValue({
              id: res.data.id,
              name: res.data.name,
              phoneNumbers: res.data.phoneNumbers,
              address: res.data.address ?? '',
              cityId: res.data.cityId,
              licenseNumber: res.data.licenseNumber ?? '',
              specialty: res.data.specialty ?? '',
              isDeleted: res.data.isDeleted
            });
          } else {
            this.alert.error(res.message ?? 'السباك غير موجود');
            this.router.navigate(['/plumber/all']);
          }
        },
        error: () => {
          this.isLoading = false;
          this.alert.error('تعذر تحميل بيانات السباك');
        }
      });
  }

  // ---------------------------------------------------------------
  // Actions
  // ---------------------------------------------------------------
  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.alert.warning('من فضلك أكمل جميع الحقول المطلوبة');
      return;
    }
    if (this.isSaving) return;

    const v = this.form.value;
    const rawCityId = v.cityId;

    const dto: PlumberDto = {
      id: this.isEditMode ? this.plumberId : null,
      name: (v.name ?? '').trim(),
      phoneNumbers: (v.phoneNumbers ?? '').trim(),
      address: v.address?.trim() || null,
      cityId: rawCityId === null || rawCityId === undefined || rawCityId === ''
        ? null
        : Number(rawCityId),
      licenseNumber: v.licenseNumber?.trim() || null,
      specialty: v.specialty?.trim() || null,
      isDeleted: !!v.isDeleted,
      cityName: null
    };

    this.isSaving = true;
    const request$ = this.isEditMode
      ? this.plumberService.editPlumber(dto)
      : this.plumberService.addPlumber(dto);

    request$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isSaving = false;
          if (res.isSuccess) {
            this.alert.success(res.message ?? (this.isEditMode ? 'تم التحديث' : 'تم الحفظ'));
            this.router.navigate(['/plumber/all']);
          } else {
            this.alert.error(res.message ?? 'تعذر حفظ السباك');
          }
        },
        error: err => {
          this.isSaving = false;
          this.alert.error(err?.error?.message ?? 'حدث خطأ أثناء الحفظ');
        }
      });
  }

  cancel(): void { this.router.navigate(['/plumber/all']); }
  goBack(): void { this.router.navigate(['/plumber/all']); }
}
