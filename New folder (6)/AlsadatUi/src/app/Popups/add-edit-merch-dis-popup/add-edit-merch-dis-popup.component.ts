import { Component, Inject, OnDestroy, OnInit, inject } from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Subscription } from 'rxjs';
import { CommonModule } from '@angular/common';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';

import { DisAndMerchantService } from '../../Services/dis-and-merchant.service';
import { CityServiceService } from '../../Services/city-service.service';
import { SwalService } from '../../Services/swal.service';
import { DistributorsAndMerchantsDto } from '../../models/IDisAndMercDto';
import { ICityDto, ICityFilteration } from '../../models/Icity';
import { PhoneInputComponent } from '../../../Components/phone-input/phone-input.component';

/**
 * Add / Edit popup for distributors, merchants, and agents.
 *
 * Validation contract (aligned with backend + Excel import):
 *   REQUIRED → fullName, phoneNumber, type
 *   OPTIONAL → address, gender, cityId, all three special discounts
 *
 * The Save button is intentionally NOT bound to `form.invalid` — clicking it
 * always runs validation so the user gets explicit feedback instead of a
 * silently-disabled button (the symptom of the original bug).
 */
@Component({
  selector: 'app-add-edit-merch-dis-popup',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    PhoneInputComponent
  ],
  templateUrl: './add-edit-merch-dis-popup.component.html',
  styleUrls: ['./add-edit-merch-dis-popup.component.css']
})
export class AddEditMerchDisPopupComponent implements OnInit, OnDestroy {
  // ───────── form state ─────────
  AddEditform!: FormGroup;
  isEditMode = false;
  isSaving = false;

  // ───────── city select state ─────────
  cities: ICityDto[] = [];
  filteredCities: ICityDto[] = [];
  citySearchCtrl = new FormControl('');

  private cityFilters: ICityFilteration = {
    page: null,
    pageSize: null,
    cityName: null,
    governrateName: null
  };

  private readonly subs = new Subscription();

  private readonly fb = inject(FormBuilder);
  private readonly cityService = inject(CityServiceService);
  private readonly disMerchService = inject(DisAndMerchantService);
  private readonly swal = inject(SwalService);

  constructor(
    private readonly dialogRef: MatDialogRef<AddEditMerchDisPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DistributorsAndMerchantsDto | null
  ) {}

  // ───────────────────────────────────────────────────────────────
  // Lifecycle
  // ───────────────────────────────────────────────────────────────

 ngOnInit(): void {
  this.isEditMode = !!this.data?.userId;

  // 1) Build the form SYNCHRONOUSLY first — template can render safely.
  this.initForm();

  // 2) City search — local filtering on the in-memory list.
  this.subs.add(
    this.citySearchCtrl.valueChanges.subscribe(value => {
      this.filterCities(value ?? '');
    })
  );

  // 3) Load cities asynchronously; they'll show up in the dropdown once ready.
  //    The form already has the saved cityId value bound — once the matching
  //    city option exists in the list, mat-select will display it correctly.
  this.loadCities();
}

private loadCities(): void {
  this.subs.add(
    this.cityService.getAllCities(this.cityFilters).subscribe({
      next: res => {
        this.cities = res.data ?? [];
        this.filteredCities = [...this.cities];
        // No need to re-init the form — the cityId is already bound.
        // mat-select will pick up the correct option once `cities` populates.
      },
      error: () => {
        this.cities = [];
        this.filteredCities = [];
        this.swal.error('تعذّر تحميل قائمة المدن');
      }
    })
  );
}

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  // ───────────────────────────────────────────────────────────────
  // Cities
  // ───────────────────────────────────────────────────────────────


  private filterCities(searchTerm: string): void {
    const value = searchTerm.toLowerCase().trim();
    this.filteredCities = !value
      ? [...this.cities]
      : this.cities.filter(c =>
          (c.cityName ?? '').toLowerCase().includes(value)
        );
  }

  // ───────────────────────────────────────────────────────────────
  // Form
  // ───────────────────────────────────────────────────────────────

  private initForm(): void {
    this.AddEditform = this.fb.group({
      // REQUIRED
      fullName: [
        this.data?.fullName ?? '',
        [Validators.required, Validators.minLength(3), Validators.maxLength(200)]
      ],
      phoneNumber: [
        this.data?.phoneNumber ?? '',
        [Validators.required]
      ],
      type: [
        this.data?.type ?? null,
        [Validators.required]
      ],

      // OPTIONAL — to support records imported from Excel
      address: [this.data?.address ?? ''],
      gender:  [this.data?.gender ?? null],
      cityId:  [this.data?.cityId ?? null],

      // OPTIONAL discounts (numeric or null)
      firstSpecialDiscount:  [this.data?.firstSpecialDiscount  ?? null],
      secondSpecialDiscount: [this.data?.secondSpecialDiscount ?? null],
      thirdSpecialDiscount:  [this.data?.thirdSpecialDiscount  ?? null]
    });
  }

  // ───────────────────────────────────────────────────────────────
  // Actions
  // ───────────────────────────────────────────────────────────────

  close(): void {
    this.dialogRef.close(null);
  }

  submit(): void {
    if (this.AddEditform.invalid) {
      this.AddEditform.markAllAsTouched();
      this.swal.warning('برجاء تصحيح الحقول المطلوبة');
      return;
    }

    this.isSaving = true;

    const v = this.AddEditform.value;

    // Build a CLEAN payload — the server owns audit fields.
    const payload: DistributorsAndMerchantsDto = {
      userId: this.data?.userId ?? null,

      fullName:    (v.fullName as string)?.trim() ?? '',
      phoneNumber: v.phoneNumber ?? null,
      type:        v.type,

      address: ((v.address as string)?.trim()) || null,
      gender:  v.gender ?? null,
      cityId:  v.cityId ?? null,

      firstSpecialDiscount:  v.firstSpecialDiscount  ?? null,
      secondSpecialDiscount: v.secondSpecialDiscount ?? null,
      thirdSpecialDiscount:  v.thirdSpecialDiscount  ?? null
    };

    // Parent component handles the API call so it can refresh the list.
    this.dialogRef.close(payload);
  }
}
