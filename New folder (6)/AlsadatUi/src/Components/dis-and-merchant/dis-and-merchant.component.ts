import { SwalService } from './../../app/Services/swal.service';
import { DisAndMerchantService } from './../../app/Services/dis-and-merchant.service';
import { DistributorsAndMerchantsFilters, DistributorsAndMerchantsDto } from './../../app/models/IDisAndMercDto';
import { HttpClientModule } from '@angular/common/http';
import { Component, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe } from '@angular/common';
import { MatOption } from "@angular/material/core";
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from "@angular/router";
import { AddEditMerchDisPopupComponent } from '../../app/Popups/add-edit-merch-dis-popup/add-edit-merch-dis-popup.component';
import { TreeAccountsService } from '../../app/Services/tree-accounts.service';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ImportExcelDialogComponent } from '../import-excel-dialog/import-excel-dialog.component';
import saveAs from 'file-saver';

@Component({
  selector: 'app-dis-and-merchant',
  standalone: true,
  imports: [MatTooltipModule,MatProgressSpinner, MatColumnDef, MatHeaderCellDef, MatCellDef, MatIcon, MatHeaderRowDef, MatRowDef,
    MatTableModule,
    MatProgressSpinner,
    MatColumnDef,
    MatHeaderCellDef,
    MatCellDef,
    MatIcon,
    MatHeaderRowDef,
    MatRowDef,
    MatSelectModule,
    MatSlideToggleModule, MatFormField, MatLabel, FormsModule, MatTableModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule, HttpClientModule, MatPaginator, DatePipe, ReactiveFormsModule, MatOption],
  templateUrl: './dis-and-merchant.component.html',
  styleUrl: './dis-and-merchant.component.css'
})
export class DisAndMerchantComponent {

   Searchform !: FormGroup;
     private readonly subs = new Subscription();

  private fb = inject(FormBuilder);
  private _DisAndMerchantService = inject(DisAndMerchantService);
  private _DisAndMerchantSubscription = new Subscription();
    private _TreeAccountService = inject(TreeAccountsService);
  private _TreeAccountSubscription = new Subscription();
private _swal=inject(SwalService)
  filters:DistributorsAndMerchantsFilters={
    page:1,
    pageSize:10,
    cityName:null,
    fullName:null,
    phoneNumber:null,
    type:null,
    isDeleted:null
  }
 columns: ColumnDef[] = [
  { key: 'fullName', label: 'الاسم بالكامل', type: 'text' },
  { key: 'address', label: 'العنوان', type: 'text' },
  { key: 'type', label: 'النوع (موزع/تاجر)', type: 'DisOrMecrhant' },


  { key: 'createdAt', label: 'تاريخ الإنشاء', type: 'date' },
  { key: 'createdBy', label: 'أنشئ بواسطة', type: 'text' },
  { key: 'updatedAt', label: 'تاريخ التعديل', type: 'date' },
  { key: 'updatedBy', label: 'عُدّل بواسطة', type: 'text' },

  // Soft Delete toggle
  { key: 'isDelted', label: 'حالة الحذف', type: 'boolean' },

  { key: 'deletedAt', label: 'تاريخ الحذف', type: 'date' },
  { key: 'deletedBy', label: 'حُذف بواسطة', type: 'text' },

  { key: 'cityName', label: 'المدينة', type: 'text' },
  { key: 'phoneNumber', label: 'رقم الهاتف', type: 'text' },
{ key: 'actions', label: 'الإجراءات', type: 'actions' },

];
displayedColumnKeys = this.columns.map(c => c.key);
dataSource = new MatTableDataSource<DistributorsAndMerchantsDto>([]);
totalCount = 0;
 @ViewChild(MatPaginator) paginator!: MatPaginator;
isLoading = true;
ngOnInit(): void {
  this.GetAllDisAdnMerchants();
  this.InitSearchForm();
}
ngOnDestroy():void{
  this._DisAndMerchantSubscription?.unsubscribe();
}
GetAllDisAdnMerchants()
    {
      this._DisAndMerchantSubscription.add(this._DisAndMerchantService.getAllDisAndMerch(this.filters).subscribe({
        next:(res)=>{
  this.dataSource.data = res.data;
            this.totalCount=res.totalCount;
            this.isLoading = false;

        },
        error:(err)=>{
            this.isLoading = false;

           Swal.fire({
                  icon: 'error',
                  title: 'خطأ',
                  text: `${err.error.message}`||'حدثت مشكلة أثناء تغيير الحالة!',
                });
        }
      }))
}
PageChange(event: PageEvent) {
this.filters.page = event.pageIndex + 1;
this.filters.pageSize = event.pageSize;
this.GetAllDisAdnMerchants();
}
ToggleCategoryStatus(dto: DistributorsAndMerchantsDto, checked: boolean) {
      dto.isDelted = !checked;
      dto.deletedAt=new Date().toISOString();
      dto.deletedBy=localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail')
      this._DisAndMerchantService.EditDisOrMerchant(dto).subscribe({
          next: (res) => {


          Swal.fire({
        icon: res.isSuccess ? 'success' : 'error',
        title: res.message ?? res.data,
      });
       this.GetAllDisAdnMerchants();
    },
    error: (err) => {
      Swal.fire('Error', 'Something went wrong', 'error');
       this.GetAllDisAdnMerchants();
    }
      })
}
InitSearchForm()
    {
       this.Searchform = this.fb.group({
      phoneNumber: [''],
      fullName: [''],
      cityName: [''],
      type: [''],
      isDeleted: [''],
    });
}
onSearch() {
  if (this.Searchform.valid) {
    const formValues = this.Searchform.value;

    this.filters = {
      ...this.filters,
      fullName: formValues.fullName || null,
      phoneNumber: formValues.phoneNumber || null,
      cityName: formValues.cityName || null,
      type: formValues.type || null,
      isDeleted: formValues.isDeleted ?? null
    };

    this.GetAllDisAdnMerchants();
  }
}
ReAsign()
{
   this.filters = {
      ...this.filters,
      fullName:  null,
      phoneNumber:  null,
      cityName: null,
      type:  null,
      isDeleted: null
    };
this.InitSearchForm();
    this.GetAllDisAdnMerchants();
}
openAddPopup() {
  const dialogRef = this.dialog.open(AddEditMerchDisPopupComponent, {
    width: '60%',
    height:'90%',
    data: null,
      panelClass: 'custom-popup-panel'
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this._DisAndMerchantService.AddDisOrMerchant(result).subscribe({
        next:(res)=>{
        Swal.fire({
          icon: "success",
          title: "تمت الإضافة بنجاح",
          })
                   this.GetAllDisAdnMerchants();

        },
        error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
            this.GetAllDisAdnMerchants();



}

      });

    }
  });
}
private dialog =inject(MatDialog);
openEditPopup(row: DistributorsAndMerchantsDto) {

  const dialogRef = this.dialog.open(AddEditMerchDisPopupComponent, {
     width: '60%',
    height:'90%',
    data: row,
    panelClass: 'custom-popup-panel'
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this._DisAndMerchantService.EditDisOrMerchant(result).subscribe({
         next:(res)=>{
Swal.fire({
  icon: "success",
  title: "تم التعديل بنجاح",
  html: "<p class='my-swal-text'>تم حفظ البيانات.</p>",
  customClass: {
    popup: 'my-swal-popup',
    title: 'my-swal-title',
    confirmButton: 'my-swal-btn',
  }
});


          this.GetAllDisAdnMerchants();
        },
        error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
          this.GetAllDisAdnMerchants();

}
      });
    }
  });
}



  downloadTemplate(): void {
    this.subs.add(
      this._DisAndMerchantService.downloadImportTemplate().subscribe({
        next: blob => {
          saveAs(blob, 'Suppliers_Template.xlsx');
        },
        error: () => this._swal.error('تعذر تحميل القالب')
      })
    );
  }



GetDisDetailsByUserId(userId: string) {

  this._DisAndMerchantService.getById(userId).subscribe({
    next: (res) => {
console.log(res.data);

      const data = res.data;
const cashBalance = Number(data?.cashBalance ?? 0);
const pointsBalance = Number(data?.pointsBalance ?? 0);

Swal.fire({
  title: 'تفاصيل الحساب',
  html: `
    <div class="swal-details">

      <div class="row">
        <span>الاسم:</span>
        <strong>${data?.fullName ?? '-'}</strong>
      </div>

      <div class="row">
        <span>رصيد النقاط:</span>
        <strong class="points">${pointsBalance}</strong>
      </div>

      <div class="row">
  <span>الرصيد المالي:</span>
  <strong class="${cashBalance >= 0 ? 'positive' : 'negative'}">
    ${cashBalance <= 0 ? cashBalance.toFixed(2) : (-cashBalance).toFixed(2)}
  </strong>
</div>
    </div>
  `,
  confirmButtonText: 'إغلاق',
  background: document.body.classList.contains('dark-mode') ? '#1a1a1a' : '#fff',
  color: document.body.classList.contains('dark-mode') ? '#fff' : '#000'
});

    },
    error: (err) => {
         Swal.fire({
                  icon: 'error',
                  title: 'خطأ',
                  text: `${err.error?.message ?? 'حدثت مشكلة أثناء الاتصال !'}`,
                });
    }
  });

}
openDistributorMerchantImport(): void {

  const ref = this.dialog.open(ImportExcelDialogComponent<DistributorsAndMerchantsDto>, {
    width: '860px',
    maxWidth: '95vw',
    disableClose: true,
    data: {
      title: 'استيراد موزعين / تجار',
      fileHint: 'يجب أن يحتوي الملف على البيانات حسب القالب المحدد',
      templateName: 'تحميل قالب الموزعين والتجار',
      importFn: (file: File) => this._DisAndMerchantService.importFromExcel(file),
      columns: ['fullName', 'address', 'phoneNumber', 'type']
    }
  });

  ref.afterClosed().subscribe(result => {

        this.GetAllDisAdnMerchants();
  this.InitSearchForm();
    
  });
}
}
