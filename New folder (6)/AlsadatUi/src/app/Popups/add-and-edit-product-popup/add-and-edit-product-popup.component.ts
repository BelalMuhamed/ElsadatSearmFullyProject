import { Component, inject, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ProductDto } from '../../models/IProductVM';

import { Subscription } from 'rxjs';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProductService } from '../../Services/product.service';

import { MatError, MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatOption, MatSelect, MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import Swal from 'sweetalert2';
import { MatCard } from "@angular/material/card";
import { MatAutocompleteModule } from "@angular/material/autocomplete";

@Component({
  selector: 'app-add-and-edit-product-popup',
  standalone: true,
  imports: [MatFormField, MatLabel, MatError,
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule, MatCard, MatAutocompleteModule],
  templateUrl: './add-and-edit-product-popup.component.html',
  styleUrl: './add-and-edit-product-popup.component.css'
})
export class AddAndEditProductPopupComponent {


      private ProductService = inject(ProductService);
      private ProductSubscription = new Subscription();
    private fb = inject(FormBuilder);

    form!: FormGroup;
    isEditMode = false;
constructor(
    private dialogRef: MatDialogRef<AddAndEditProductPopupComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProductDto | null
  ) {}
    ngOnInit(): void {
      this.initForm();


  }

  ngOnDestroy(): void {

    this.ProductSubscription?.unsubscribe();
  }
  initForm() {
      this.form = this.fb.group({
        name: [this.data?.name || '', Validators.required],
         sellingPrice: [this.data?.sellingPrice || '', Validators.required],
         productCode:[this.data?.productCode || '', Validators.required],
        pointPerUnit:[this.data?.pointPerUnit || '', Validators.required],
        theSmallestPossibleQuantity:[this.data?.theSmallestPossibleQuantity||'',Validators.required]
      });

      this.isEditMode = !!this.data;
    }

      close() {
    this.dialogRef.close(null);
  }

save() {
      console.log(this.data);

  if (!this.form.valid) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'النموذج غير صالح',
    });
    return;
  }

  let product: ProductDto;

  if (this.isEditMode) {
    // حالة التعديل: لا نغير createBy/createAt
    product = {
      id: this.data!.id,
      productCode:this.form.get('productCode')?.value ?? '',
      name: this.form.get('name')?.value ?? null,
      sellingPrice: Number(this.form.get('sellingPrice')?.value ?? 0),
      pointPerUnit: Number(this.form.get('pointPerUnit')?.value ?? 0),
      categoryId: Number(this.form.get('categoryId')?.value ?? 0),
      createBy: this.data!.createBy,
      createAt: this.data!.createAt,
      updateBy: localStorage.getItem('userName') + '|' + localStorage.getItem('userEmail'),
      updateAt: new Date().toISOString(),
      isDeleted: this.data!.isDeleted,
      deleteBy: this.data!.deleteBy,
      deleteAt: this.data!.deleteAt,

      theSmallestPossibleQuantity:Number(this.form.get('theSmallestPossibleQuantity')?.value ?? 0),
    };

    this.ProductSubscription.add(
      this.ProductService.editProduct(product).subscribe({
        next: () => {
          Swal.fire({ icon: 'success', title: 'تم التعديل بنجاح', timer: 1200, showConfirmButton: false });
          this.dialogRef.close(true);
        },
        error: (err) => {
          Swal.fire({ icon: 'error', title: 'خطأ', text: err.error?.message || 'حدثت مشكلة أثناء الحفظ!' });
        }
      })
    );
  } else {
    // حالة الإضافة: لا نرسل updateBy/updateAt
    product = {
      id: null,
      productCode:this.form.get('productCode')?.value ?? '',

      name: this.form.get('name')?.value ?? null,
      sellingPrice: Number(this.form.get('sellingPrice')?.value ?? 0),
      pointPerUnit: Number(this.form.get('pointPerUnit')?.value ?? 0),
      categoryId: Number(this.form.get('categoryId')?.value ?? 0),

      createBy: localStorage.getItem('userName') + '|' + localStorage.getItem('userEmail'),
      createAt: new Date().toISOString(),
      updateBy: null,
      updateAt: null,
      isDeleted: false,
      deleteBy: null,
      deleteAt: null,

      theSmallestPossibleQuantity:Number(this.form.get('theSmallestPossibleQuantity')?.value ?? 0),


    };

    this.ProductSubscription.add(
      this.ProductService.addProduct(product).subscribe({
        next: () => {
          Swal.fire({ icon: 'success', title: 'تمت الإضافة بنجاح', timer: 1200, showConfirmButton: false });
          this.dialogRef.close(true);
        },
        error: (err) => {
          Swal.fire({ icon: 'error', title: 'خطأ', text: err.error?.message || 'حدثت مشكلة أثناء الحفظ!' });
        }
      })
    );
  }
}


}
