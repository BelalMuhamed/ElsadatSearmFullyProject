import Swal from 'sweetalert2';
import { AddCopounCoponentPopup } from './../../app/Popups/add-copoun-coponent-popup/add-copoun-coponent-popup';
import { ChangeDetectorRef, Component, inject, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { CopounService } from '../../app/Services/copoun-service';
import { CopounRespDto } from '../../app/models/Icopoun';
import { log } from 'node:console';
import { MatPaginator } from '@angular/material/paginator';
import { MatTableDataSource, MatColumnDef, MatHeaderCellDef, MatCellDef, MatHeaderRowDef, MatRowDef } from '@angular/material/table';
import { MatProgressSpinner, MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { MatIcon, MatIconModule } from "@angular/material/icon";
import { MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormField, MatInputModule, MatLabel } from "@angular/material/input";
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-copoun-component',
  imports: [MatProgressSpinner, MatColumnDef, MatHeaderCellDef, MatCellDef, MatIcon, MatHeaderRowDef, MatRowDef,
    MatTableModule,
    MatProgressSpinner,
    MatColumnDef,
    MatHeaderCellDef,
    MatCellDef,
    MatIcon,
    MatHeaderRowDef,
    MatRowDef,
    MatSlideToggleModule, MatFormField, MatLabel, FormsModule, MatTableModule,
  MatSlideToggleModule,
  MatFormFieldModule,
  MatInputModule,
  MatIconModule,
  MatProgressSpinnerModule,
  MatDialogModule,HttpClientModule],
  standalone:true,
  templateUrl: './copoun-component.html',
  styleUrl: './copoun-component.css'

})
export class CopounComponent {
  updatedPoints: number = 0;
  private CopounService = inject(CopounService);
  private CopounSubscription = new Subscription();
private dialog =inject(MatDialog);
   columns: ColumnDef[] = [
    { key: 'copounDesc', label: 'فئه الكوبون', type: 'string' },
    { key: 'copounPaiedType', label: 'نوع عملية الدفع للسباك ' ,type:'copounPaiedType'},
    { key: 'pointsToCollectCopoun', label: 'النقاط المستحقة لتجميع الكوبون  ' },
    { key: 'stars', label: 'النجوم المستحقة  ' },
    { key: 'isActive', label: 'فعال', type: 'boolean' },
    { key: 'paiedCash', label: ' المبلغ المستحق الدفع بالجنيه' },
    { key: 'actions', label: 'الإجراءات', type: 'actions' }
  ];
private cdr=inject(ChangeDetectorRef)
  displayedColumnKeys = this.columns.map(c => c.key);
  dataSource = new MatTableDataSource<CopounRespDto>([]);
  totalCount = 0;
  isLoading = false;


    ngOnInit(): void {

      setTimeout(() =>this.GetAllCopouns(), 0);
    }
    ngOnDestroy():void{
      this.CopounSubscription?.unsubscribe();
    }
    GetAllCopouns()
    {
      this.CopounSubscription.add(this.CopounService.GetAllCopouns().subscribe({
        next:(res)=>{
      this.dataSource.data = res;
      this.isLoading = false;
      this.cdr.markForCheck();
        },
        error:(err)=>{
          Swal.fire({
                  icon: 'error',
                  title: 'حدث خطأ',
                  text: `${err.message}`,
                  confirmButtonText: 'موافق',
                  confirmButtonColor: '#d33'
                });
      this.isLoading = false;
      this.cdr.markForCheck();
        }
      }))
    }
   toggleActive(element: CopounRespDto) {
  // نغيّر قيمة isActive فقط
  const updatedDto: CopounRespDto = {
    ...element,
    isActive: !element.isActive
  };

  this.CopounService.updateCopoun(updatedDto).subscribe({
    next: () => {
      element.isActive = updatedDto.isActive;

      Swal.fire({
        icon: 'success',
        title: 'تم التحديث',
        text: 'تم تغيير حالة الكوبون.',
        timer: 1200,
        showConfirmButton: false
      });
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: `${err.error.message}`||'حدثت مشكلة أثناء تغيير الحالة!',
      });
    }
  });
}

updatePoints() {
  if (!this.updatedPoints || this.updatedPoints <= 0) {
    Swal.fire({
      icon: 'warning',
      title: 'تنبيه',
      text: 'من فضلك أدخل قيمة صحيحة للنقاط',
      confirmButtonText: 'موافق'
    });
    return;
  }

  this.CopounService.updateAllPoints(this.updatedPoints).subscribe({
    next: () => {
      Swal.fire({
        icon: 'success',
        title: 'تم التحديث بنجاح',
        text: 'تم تحديث نقاط تجميع جميع الكوبونات.',
        confirmButtonText: 'موافق'
      }).then(() => window.location.reload());
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'حدث خطأ',
        text: err.error?.message || 'هناك مشكلة بالخادم!',
        confirmButtonText: 'موافق'
      });
    }
  });
}
     openAddCouponPopup() {
    this.dialog.open(AddCopounCoponentPopup, {
      width: '600px',
      panelClass: 'custom-popup-panel'
    });
  }
  onEdit(item: any) {
this.dialog.open(AddCopounCoponentPopup, {
    width: "550px",
    data: { mode: "edit",item  }
  });
}

}
