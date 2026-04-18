import { DisAndMerchantService } from './../../Services/dis-and-merchant.service';
import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogActions, MatDialogModule } from '@angular/material/dialog';
import { DistributorsAndMerchantsDto } from '../../models/IDisAndMercDto';
import { FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CityServiceService } from '../../Services/city-service.service';
import { ICityDto, ICityFilteration } from '../../models/Icity';
import Swal from 'sweetalert2';
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatSelectModule } from "@angular/material/select";
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';

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
    MatButtonModule
  ],
  templateUrl: './add-edit-merch-dis-popup.component.html',
  styleUrls: ['./add-edit-merch-dis-popup.component.css']
})
export class AddEditMerchDisPopupComponent {
  citySearchCtrl = new FormControl('');

  filteredCities: any[] = [];
  searchValue: string = '';
 isEditMode = false;
cities:ICityDto[]=[];
  constructor(
    private dialogRef: MatDialogRef<AddEditMerchDisPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DistributorsAndMerchantsDto | null
  ) {}
private fb = inject(FormBuilder);
private _DisAndMerchantService = inject(DisAndMerchantService);
  private _DisAndMerchantServiceSubscription = new Subscription();
    private CityService = inject(CityServiceService);
    private CitySubscription = new Subscription();
     cityFilters:ICityFilteration={
  page:null,
  pageSize:null,
  cityName:null,
  governrateName:null
    }




GetAllCities() {
  this.CitySubscription.add(
    this.CityService.getAllCities(this.cityFilters).subscribe({
      next: (res) => {
        this.cities = res.data;
        this.filteredCities = this.cities; // ⭐ مهم جداً
      },
      error: (err) => {
        Swal.fire({
          icon: 'error',
          title: 'حدث خطأ',
          text: `${err.message}`,
          confirmButtonText: 'موافق',
          confirmButtonColor: '#d33'
        });
      }
    })
  );
}
filterCities() {
  const value = this.searchValue.toLowerCase();

  this.filteredCities = this.cities.filter(city =>
    (city.cityName ?? '').toLowerCase().includes(value)
  );
}


   ngOnInit(): void
   {
    this.citySearchCtrl.valueChanges.subscribe(value => {
    this.searchValue = value ?? '';
    this.filterCities();
  });
    this.GetAllCities();
    this.initForm();
   }
   ngOnDestroy():void{
this._DisAndMerchantServiceSubscription?.unsubscribe();
this.CitySubscription?.unsubscribe();
 }

  AddEditform!: FormGroup;



   initForm() {
  this.AddEditform = this.fb.group({
    fullName: [
      this.data?.fullName || '',
      [Validators.required, Validators.minLength(3)]
    ],
    address: [
      this.data?.address || '',
      [Validators.required, Validators.minLength(3)]
    ],
    gender: [
      this.data?.gender ?? null,
      Validators.required
    ],
    type: [
      this.data?.type ?? null,
      Validators.required
    ],
    cityId: [
      this.data?.cityId ?? null,
      Validators.required
    ],
    phoneNumber: [
      this.data?.phoneNumber || '',
      [
        Validators.required,
        Validators.pattern(/^01[0125][0-9]{8}$/)
      ]
    ],
     firstSpecialDiscount: [
      this.data?.firstSpecialDiscount || ''
    ],
     secondSpecialDiscount: [
      this.data?.secondSpecialDiscount || ''

    ],
     thirdSpecialDiscount: [
      this.data?.thirdSpecialDiscount || ''
    ],
  });

  this.isEditMode = !!this.data;
}

       close() {
    this.dialogRef.close(null);
  }
   submit() {
  if (this.AddEditform.invalid) {
    this.AddEditform.markAllAsTouched();
    return;
  }

  const formValue = this.AddEditform.value;

  const result: DistributorsAndMerchantsDto = {
    userId: this.isEditMode ? (this.data?.userId ?? null) : null,
    fullName: formValue.fullName ?? null,
    address: formValue.address ?? null,
    gender: formValue.gender ?? null,
    type: formValue.type ?? null,
    createdAt: !this.isEditMode ? new Date().toISOString():null,
    createdBy: !this.isEditMode ?localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail'): null,
    updatedAt: this.isEditMode ? new Date().toISOString():null,
    updatedBy:this.isEditMode ?localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail'): null,
    cityId: formValue.cityId ?? null,
    phoneNumber:formValue.phoneNumber ?? null,
    firstSpecialDiscount:formValue.firstSpecialDiscount?? null,
    secondSpecialDiscount:formValue.secondSpecialDiscount?? null,
    thirdSpecialDiscount:formValue.thirdSpecialDiscount??null



  };

  this.dialogRef.close(result);
}
}
