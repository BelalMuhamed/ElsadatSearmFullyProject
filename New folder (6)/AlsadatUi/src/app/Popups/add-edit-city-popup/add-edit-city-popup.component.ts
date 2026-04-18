import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ICityDto } from '../../models/Icity';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { GovernrateService } from '../../Services/governrate.service';
import { Subscription } from 'rxjs';
import { GovernrateDto, GovernrateFilteration } from '../../models/IGovernrateVM';
import Swal from 'sweetalert2';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-add-edit-city-popup',
  standalone: true,
  imports: [MatFormField, MatLabel,MatFormField, MatLabel,
         CommonModule,
      ReactiveFormsModule,
      FormsModule,
      MatDialogModule,
      MatFormFieldModule,
      MatInputModule,
      MatSelectModule,
      MatButtonModule],
  templateUrl: './add-edit-city-popup.component.html',
  styleUrl: './add-edit-city-popup.component.css'
})
export class AddEditCityPopupComponent {
  governrates:GovernrateDto[]=[];
 isEditMode = false;

  constructor(
    private dialogRef: MatDialogRef<AddEditCityPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ICityDto | null
  ) {}
private fb = inject(FormBuilder);
private GovernrateService = inject(GovernrateService);
  private GovernrateSubscription = new Subscription();
  filters:GovernrateFilteration={
    name:null,
    page:null,
    pageSize:null
  }
  form!: FormGroup;
   ngOnInit(): void {
    this.initForm();
    this.GetAllGovernrates();
   }
  ngOnDestroy():void{}
   initForm() {
      this.form = this.fb.group({
        cityName: [this.data?.cityName || '', Validators.required],
       governrateId: [this.data?.governrateId || '', Validators.required],
      });

      this.isEditMode = !!this.data;
    }
    close() {
    this.dialogRef.close(null);
  }
  GetAllGovernrates()
  {
    this.GovernrateSubscription.add(this.GovernrateService.getAllGovernrates(this.filters).subscribe({
      next:(res)=>{
        this.governrates=res.data;
        console.log(this.governrates);

      },error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
      }
    }))
  }
  submit() {
  if (this.form.invalid) {
    this.form.markAllAsTouched();
    return;
  }

  const formValue = this.form.value;

  const result: ICityDto = {
    id: this.isEditMode ? (this.data?.id ?? null) : null,
    governrateId: formValue.governrateId ?? null,
    cityName: formValue.cityName ?? null,
    governrateName: null // حسب ما طلبت، نخليها null دائماً هنا
  };

  this.dialogRef.close(result);
}
}
