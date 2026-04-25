import { PhoneInputComponent } from './../phone-input/phone-input.component';
import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnDestroy, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MatCard,
  MatCardActions,
  MatCardContent,
  MatCardHeader,
  MatCardSubtitle,
  MatCardTitle
} from '@angular/material/card';
import { MatError, MatFormField, MatLabel } from '@angular/material/form-field';
import { MatIcon } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatOption, MatSelect } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

import { CityServiceService } from '../../app/Services/city-service.service';
import { SupplierService } from '../../app/Services/supplier.service';
import { SwalService } from '../../app/Services/swal.service';
import { ICityDto, ICityFilteration } from '../../app/models/Icity';
import { SupplierDto } from '../../app/models/ISupplierModels';

/**
 * AddEditSupplierComponent
 * ------------------------------------------------------------------
 * Single component for both routes:
 *   /supplier/add         → create mode
 *   /supplier/edit/:id    → edit mode
 *
 * Notes:
 *  - Products completely removed per business decision.
 *  - Phone number uses the reusable <app-phone-input> (E.164 format).
 *  - Subscribes to paramMap (not snapshot) so switching between add/edit
 *    on the same loaded component instance still works.
 *  - Submit button is disabled while a save is in-flight.
 */
@Component({
  selector: 'app-add-edit-supplier',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardSubtitle,
    MatCardContent,
    MatCardActions,
    MatFormField,
    MatLabel,
    MatError,
    MatInputModule,
    MatSelect,
    MatOption,
    MatButtonModule,
    MatIcon,
    MatTooltipModule,
    PhoneInputComponent
  ],
  templateUrl: './add-edit-supplier.component.html',
  styleUrls: ['./add-edit-supplier.component.css']
})
export class AddEditSupplierComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly supplierService = inject(SupplierService);
  private readonly cityService = inject(CityServiceService);
  private readonly alert = inject(SwalService);
  // Captured here (field initializer = injection context) so it can be passed
  // explicitly to takeUntilDestroyed() inside ngOnInit, where the implicit
  // injection context is no longer available (NG0203).
  private readonly destroyRef = inject(DestroyRef);

  private readonly subs = new Subscription();

  form!: FormGroup;
  cities: ICityDto[] = [];
  supplierId: number | null = null;
  isEditMode = false;
  isSaving = false;
  isLoading = false;

  private readonly cityFilters: ICityFilteration = {
    page: null,
    pageSize: null,
    cityName: null,
    governrateName: null
  };

  // ---------------------------------------------------------------
  // Lifecycle
  // ---------------------------------------------------------------
  ngOnInit(): void {
    this.initializeForm();
    this.loadCities();

    // Reacts to every navigation between /supplier/add and /supplier/edit/:id
    // Fixes B-15 (route reuse kept old form data on component reuse).
    //
    // takeUntilDestroyed() needs either an injection context (not available
    // inside ngOnInit) OR an explicit DestroyRef. We pass the DestroyRef we
    // captured in a field initializer above.
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const raw = params.get('id');
        const parsed = raw ? Number(raw) : NaN;
        const validId = Number.isFinite(parsed) && parsed > 0;

        this.supplierId = validId ? parsed : null;
        this.isEditMode = validId;

        this.form.reset({
          id: null,
          name: '',
          phoneNumbers: '',
          address: '',
          cityId: null,
          isDeleted: false
        });

        if (this.isEditMode && this.supplierId) {
          this.loadSupplier(this.supplierId);
        }
      });
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // ---------------------------------------------------------------
  // Form setup
  // ---------------------------------------------------------------
  private initializeForm(): void {
    this.form = this.fb.group({
      id: [null as number | null],
      name: ['', [Validators.required, Validators.maxLength(200)]],
      phoneNumbers: ['', Validators.required], // E.164 validation is done by the phone widget + backend
      address: ['', Validators.maxLength(500)],
      cityId: [null as number | null],         // ← optional now (no Validators.required)
      isDeleted: [false]
    });
  }

  // ---------------------------------------------------------------
  // Data loading
  // ---------------------------------------------------------------
  private loadCities(): void {
    this.subs.add(
      this.cityService.getAllCities(this.cityFilters).subscribe({
        next: (res: any) => {
          this.cities = res?.data ?? [];
        },
        error: () => this.alert.error('تعذر تحميل قائمة المدن')
      })
    );
  }

  private loadSupplier(id: number): void {
    this.isLoading = true;
    this.subs.add(
      this.supplierService.getById(id).subscribe({
        next: res => {
          this.isLoading = false;
          if (res.isSuccess && res.data) {
            this.form.patchValue({
              id: res.data.id,
              name: res.data.name,
              phoneNumbers: res.data.phoneNumbers,
              address: res.data.address ?? '',
              cityId: res.data.cityId,
              isDeleted: res.data.isDeleted
            });
          } else {
            this.alert.error(res.message ?? 'المورد غير موجود');
            this.router.navigate(['/supplier/all']);
          }
        },
        error: () => {
          this.isLoading = false;
          this.alert.error('تعذر تحميل بيانات المورد');
        }
      })
    );
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

    const rawCityId = this.form.value.cityId;
    const dto: SupplierDto = {
      id: this.isEditMode ? this.supplierId : null,
      name: (this.form.value.name ?? '').trim(),
      phoneNumbers: (this.form.value.phoneNumbers ?? '').trim(),
      address: this.form.value.address?.trim() || null,
      // cityId is optional — send null if the user didn't pick one.
      cityId: rawCityId === null || rawCityId === undefined || rawCityId === ''
        ? null
        : Number(rawCityId),
      isDeleted: !!this.form.value.isDeleted,
      cityName: null // ← not sent to backend, only used for display
    };

    this.isSaving = true;
    
    const request$ = this.isEditMode
      ? this.supplierService.ditSupplier(dto)
      : this.supplierService.addSupplier(dto);

    this.subs.add(
      request$.subscribe({
        next: res => {
          this.isSaving = false;
          if (res.isSuccess) {
            this.alert.success(res.message ?? (this.isEditMode ? 'تم التحديث' : 'تم الحفظ'));
            this.router.navigate(['/supplier/all']);
          } else {
            this.alert.error(res.message ?? 'تعذر حفظ المورد');
          }
        },
        error: err => {
          this.isSaving = false;
          const msg = err?.error?.message ?? 'حدث خطأ أثناء الحفظ';
          this.alert.error(msg);
        }
      })
    );
  }

  cancel(): void {
    this.router.navigate(['/supplier/all']);
  }

  goBack(): void {
    this.router.navigate(['/supplier/all']);
  }
}
