import { Component, inject } from '@angular/core';
import { StoreService } from '../../Services/store.service';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { StoreDto } from '../../models/IstoreVM';
import { MatFormField, MatFormFieldModule, MatLabel } from "@angular/material/form-field";
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-add-edit-store-popup',
  standalone: true,
  imports: [MatFormField, MatLabel,CommonModule, FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './add-edit-store-popup.component.html',
  styleUrl: './add-edit-store-popup.component.css'
})
export class AddEditStorePopupComponent {

  private _storeService = inject(StoreService);
  dialogRef = inject(MatDialogRef<AddEditStorePopupComponent>);
  data = inject(MAT_DIALOG_DATA) as StoreDto | null;

  storeName: string = '';
  isSaving = false;

  constructor() {
    if (this.data) {
      this.storeName = this.data.storeName;
    }
  }

 save() {
  if (!this.storeName || this.storeName.trim().length === 0) {
    Swal.fire({
      icon: 'warning',
      title: 'تنبيه',
      text: 'الرجاء إدخال اسم المخزن!',
    });
    return;
  }

  this.isSaving = true;

  const dto: StoreDto = {
    id: this.data?.id ?? null,
    storeName: this.storeName.trim(),
    isDeleted: null
  };

  // ---------- EDIT MODE ----------
  if (this.data && this.data.id) {
    this._storeService.editStore(dto).subscribe({
      next: (res) => {
        this.isSaving = false;

        Swal.fire({
          icon: 'success',
          title: 'تم التحديث',
          text: res?.message || 'تم تعديل المخزن بنجاح.',
        });

        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isSaving = false;

        Swal.fire({
          icon: 'error',
          title: 'خطأ',
          text: err?.error?.message || 'حدثت مشكلة أثناء تعديل المخزن!',
        });

        this.dialogRef.close(false);
      }
    });
  }

  // ---------- ADD MODE ----------
  else {
    this._storeService.addNewStore(dto).subscribe({
      next: (res) => {
        this.isSaving = false;

        Swal.fire({
          icon: 'success',
          title: 'تم الإضافة',
          text: res?.message || 'تم إضافة المخزن بنجاح.',
        });

        this.dialogRef.close(true);
      },
      error: (err) => {
        this.isSaving = false;

        Swal.fire({
          icon: 'error',
          title: 'خطأ',
          text: err?.error?.message || 'حدثت مشكلة أثناء إضافة المخزن!',
        });

        this.dialogRef.close(false);
      }
    });
  }
}


  cancel() {
    this.dialogRef.close(false);
  }
}
