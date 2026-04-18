import { Component, inject } from '@angular/core';
import { StoreService } from '../../Services/store.service';
import { Subscription } from 'rxjs';
import { StoreDeleteDto, StoreDto, StoreFilteration } from '../../models/IstoreVM';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormField, MatFormFieldModule, MatLabel } from "@angular/material/form-field";
import { MatSelect, MatOption, MatSelectModule } from "@angular/material/select";
import Swal from 'sweetalert2';
import { CommonModule } from '@angular/common';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-delete-store-popup',
  standalone: true,
  imports: [MatFormField, MatLabel, MatSelect, MatOption, CommonModule,        // لازم لـ *ngFor
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule],
  templateUrl: './delete-store-popup.component.html',
  styleUrl: './delete-store-popup.component.css'
})
export class DeleteStorePopupComponent {

  dialogRef = inject(MatDialogRef<DeleteStorePopupComponent>);
  data = inject(MAT_DIALOG_DATA) as StoreDto;
  AllStores!: StoreDto[];
  private _StoreService = inject(StoreService);
  private _StoreSubscription = new Subscription();

  filters: StoreFilteration = {
    page: 1,
    pageSize: 1000, // كل المخازن
    storeName: null,
    isDeleted: false
  };

  selectedStoreId!: number | null;

  constructor() {
    this.GetAllStores();
  }

  GetAllStores() {
    this._StoreSubscription.add(
      this._StoreService.getAllStores(this.filters).subscribe({
        next: (res) => {
          // استبعاد المخزن الحالي
          this.AllStores = res.data.filter(s => s.id !== this.data.id);
        },
        error: (err) => { console.error(err); }
      })
    );
  }
save() {
  // تأكد من وجود المخزن الحالي و id صالح
  if (!this.data || this.data.id == null) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'المخزن الحالي غير صالح أو غير موجود!'
    });
    return;
  }

  // تأكد من اختيار مخزن للنقل إليه
  if (!this.selectedStoreId) {
    Swal.fire({
      icon: 'warning',
      title: 'تنبيه',
      text: 'الرجاء اختيار مخزن لنقل المنتجات إليه قبل الحذف.'
    });
    return;
  }

  // إنشاء DTO مع id غير nullable
  const dto: StoreDeleteDto = {
    id: this.data.id,  // assured non-null
    storeName: this.data.storeName,
    transferedToStoreDto: this.selectedStoreId,
    makeActionUser:localStorage.getItem('userName') + '|' + localStorage.getItem('userEmail'),
  };

  // استدعاء الخدمة
  this._StoreService.deleteStore(dto).subscribe({
    next: (res) => {
      Swal.fire({
        icon: 'success',
        title: 'تم الحذف',
        text: res.message || 'تم حذف المخزن ونقل المنتجات بنجاح'
      });
      this.dialogRef.close(true);
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: err?.error?.message || 'حدثت مشكلة أثناء الحذف'
      });
    }
  });
}



  cancel() {
    this.dialogRef.close(false);
  }

  ngOnDestroy() {
    this._StoreSubscription.unsubscribe();
  }
}
