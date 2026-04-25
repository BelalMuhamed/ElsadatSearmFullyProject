import { Component, inject, ViewChild } from '@angular/core';

import { Subscription } from 'rxjs';
import * as XLSX from 'xlsx';
import { saveAs } from 'file-saver';
import { ProductService } from '../../app/Services/product.service';
import { ProductDto, ProductFilterationDto } from '../../app/models/IProductVM';
import Swal from 'sweetalert2';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormBuilder, FormGroup, FormsModule, Validators, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { CommonModule, CurrencyPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { AddAndEditProductPopupComponent } from '../../app/Popups/add-and-edit-product-popup/add-and-edit-product-popup.component';
import { log } from 'console';

import { MatOption } from "@angular/material/core";
import { MatSelectModule } from '@angular/material/select';
import { SwalService } from '../../app/Services/swal.service';
import { ImportExcelDialogComponent } from '../import-excel-dialog/import-excel-dialog.component';

@Component({
  selector: 'app-product',
  standalone: true,
  imports: [
    MatTableModule,
    MatIconModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    FormsModule,
    HttpClientModule, DatePipe, CurrencyPipe,
    MatPaginator,
    ReactiveFormsModule,
    MatSelectModule,  CommonModule  // ← مهم لـ *ngFor و *ngIf

],
  templateUrl: './product.component.html',
  styleUrl: './product.component.css'
})
export class ProductComponent {

  private ProductService = inject(ProductService);
  private _swal = inject(SwalService);

  private ProductSubscription = new Subscription();
  filters:ProductFilterationDto={

    isDeleted:null,
    name:null,
    page:1,
    pageSize:10
  }
  @ViewChild(MatPaginator) paginator!: MatPaginator;
    isLoaded:boolean=false;
      columns: ColumnDef[] = [
        { key: 'name', label: 'اسم المنتج ' },
        { key: 'productCode', label: 'كود المنتج ' },
        { key: 'sellingPrice', label: 'سعر البيع', type: 'currency' },
        { key: 'pointPerUnit', label: 'عدد النقاط مقابل الوحدة' },
        { key: 'theHighestPossibleQuantity', label: 'أعلي كمية ممكنة ' },
        { key: 'theSmallestPossibleQuantity', label: 'أقل كمية ممكنة ' },
        { key: 'createAt', label: 'وقت الإنشاء ',type:'date' },
        { key: 'createBy', label: 'المنشئ' },
        { key: 'isDeleted', label: 'فعال', type: 'boolean' },
        { key: 'updateAt', label: ' وقت التحديث',type:'date' },
        { key: 'updateBy', label: 'اخر مستخدم قام بالتعديل ' },
        { key: 'deleteAt', label: ' وقت إيقاف التفعيل/التفعيل',type:'date' },
        { key: 'deleteBy', label: 'أخر مستخدم قام بإيقاف التفعيل/التفعيل' },

        { key: 'actions', label: 'الإجراءات', type: 'actions' },


      ];
    displayedColumnKeys = this.columns.map(c => c.key);
    totalCount = 0;
    dataSource = new MatTableDataSource<ProductDto>([]);
    private dialog =inject(MatDialog);

     private fb = inject(FormBuilder);
      form!: FormGroup;

    ngOnInit():void
    {
      this.GetAllProducts();

      this.initForm();

    }
    ngOnDestroy():void
    {
      this.ProductSubscription?.unsubscribe();
    }
    GetAllProducts()
    {
      this.ProductSubscription.add(this.ProductService.getAllProducts(this.filters).subscribe({
          next:(res)=>{

            this.isLoaded=true;
         this.dataSource.data = res.data;
            this.totalCount = res.totalCount;

        },
        error:(err)=>{
            Swal.fire({
                          icon: 'error',
                          title: 'حدث خطأ',
                          text: `${err.error?.message}`,
                          confirmButtonText: 'موافق',
                          confirmButtonColor: '#d33'
                        });
           this.isLoaded=true;

        }
      }))
    }
     ToggleCategoryStatus(dto: ProductDto, checked: boolean) {
    // dto prepared (logging removed)

      dto.isDeleted = !checked;
      dto.deleteAt=new Date().toISOString();
      dto.deleteBy=localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail'),
      this.ProductService.toggleStatus(dto).subscribe({
        next: () => {
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
      });
    }
    onPageChange(event: PageEvent) {
        this.filters.page = event.pageIndex + 1;
        this.filters.pageSize = event.pageSize;
        this.GetAllProducts();
      }
 openAddEditPopup(product?: ProductDto) {
  const dialogRef = this.dialog.open(AddAndEditProductPopupComponent, {
   width: '500px',
    panelClass: 'custom-popup-panel',
    data: product ?? null // لو موجود بيانات يبقى تعديل، لو null يبقى إضافة
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.GetAllProducts(); // إعادة تحميل الجدول بعد الإضافة أو التعديل
    }
  });
}
  initForm() {

      this.form = this.fb.group({
        name: [ '', Validators.required],
         isDeleted: ['', Validators.required],
        categoryName:[ '', Validators.required],

      });

}


isUploading = false; // متغير للتحكم في عرض الـ spinner
selectedFileName: string | null = null;
 private readonly subs = new Subscription();


applyFilters() {
  // Get values from the form
  const formValues = this.form.value;

  // Assign them to the filters object
  this.filters = {
    ...this.filters, // keep existing pagination values
    name: formValues.name,
    isDeleted: formValues.isDeleted,

  };

  // filters updated (logging removed)
  this.GetAllProducts();
}
ReAsign()
{
  this.filters={

    isDeleted:null,
    name:null,
    page:1,
    pageSize:10
  }
this.initForm();
    this.GetAllProducts();
}
openDistributorMerchantImport(): void {

  const ref = this.dialog.open(ImportExcelDialogComponent<ProductDto>, {
    width: '860px',
    maxWidth: '95vw',
    disableClose: true,
    data: {
      title: 'استيراد موزعين / تجار',
      fileHint: 'يجب أن يحتوي الملف على البيانات حسب القالب المحدد',
      templateName: 'تحميل قالب الموزعين والتجار',
      importFn: (file: File) => this.ProductService.importFromExcel(file),
      columns: ['fullName', 'address', 'phoneNumber', 'type']
    }
  });

  ref.afterClosed().subscribe(result => {

        this.GetAllProducts();
  this.initForm();

  });
}
 downloadTemplate(): void {
    this.subs.add(
      this.ProductService.downloadTemplate().subscribe({
        next: blob => {
          saveAs(blob, 'products_Template.xlsx');
        },
        error: () => this._swal.error('تعذر تحميل القالب')
      })
    );
  }
}
