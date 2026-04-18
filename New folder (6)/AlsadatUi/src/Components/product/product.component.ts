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
downloadExcelTemplate() {

  const headers = [
    ['productName', 'productCode', 'sellingPrice', 'pointsPerUnit', 'minQuantity']
  ];

  const worksheet: XLSX.WorkSheet = XLSX.utils.aoa_to_sheet(headers);

  const workbook: XLSX.WorkBook = {
    Sheets: { 'Products': worksheet },
    SheetNames: ['Products']
  };

  const excelBuffer: any = XLSX.write(workbook, {
    bookType: 'xlsx',
    type: 'array'
  });

  const blob = new Blob([excelBuffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  });

  saveAs(blob, 'Products_Template.xlsx');
}
// onFileSelected(event: any) {
//   const file: File = event.target.files[0];
//   if (!file) return;

//   const createdUser =localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail'); // أو اسم المستخدم الحالي من session

//   this.ProductService.uploadExcel(file, createdUser).subscribe({
//     next: (res) => {
//       if (res.isSuccess) {
//         console.log('تمت الإضافة:', res.data);
//         if (res.message) alert(res.message);
//       } else {
//         console.error('فشل:', res.message);
//       }
//     },
//     error: (err) => {
//       console.error('خطأ في السيرفر:', err);
//     }
//   });
// }
// onFileSelected(event: any) {
//   const file: File = event.target.files[0];
//   if (!file) return;

//   const createdUser = localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail');

//   this.ProductService.uploadExcel(file, createdUser).subscribe({
//     next: (res) => {
//       // تحديد الثيم الحالي
//       const isDarkMode = document.body.classList.contains('dark-mode');

//       // إذا كان في أخطاء
//       if (res.isSuccess && res.data && res.data.length && res.message?.includes('بعض الأخطاء')) {
//         // تجهيز النص للأخطاء
//         const failedProducts = res.errors
//           .filter((p: any) => p.errors && p.errors.length)
//           .map((p: any) => p.errors.map((e: any) => `${e.Column}: ${e.Message}`).join('\n'))
//           .join('\n\n');

//         Swal.fire({
//           title: 'تمت الإضافة مع بعض الأخطاء',
//           html: `<pre style="text-align:left;">${failedProducts}</pre>`,
//           icon: 'warning',
//           confirmButtonText: 'حسناً',
//           background: isDarkMode ? '#222' : '#fff',
//           color: isDarkMode ? '#fff' : '#000',
//         });
//       }
//       // إذا نجاح كامل
//       else if (res.isSuccess) {
//         Swal.fire({
//           title: 'تمت الإضافة بنجاح',
//           text: res.message || 'تمت إضافة جميع المنتجات بنجاح',
//           icon: 'success',
//           confirmButtonText: 'حسناً',
//           background: isDarkMode ? '#222' : '#fff',
//           color: isDarkMode ? '#fff' : '#000',
//         });
//       }
//       // فشل كامل
//       else {
//         Swal.fire({
//           title: 'فشل إضافة المنتجات',
//           text: res.message || 'حدث خطأ أثناء إضافة المنتجات',
//           icon: 'error',
//           confirmButtonText: 'حسناً',
//           background: isDarkMode ? '#222' : '#fff',
//           color: isDarkMode ? '#fff' : '#000',
//         });
//       }
//     },
//     error: (err) => {
//       const isDarkMode = document.body.classList.contains('dark-mode');
//       Swal.fire({
//         title: 'حدث خطأ في السيرفر',
//         text: err?.message || 'تعذر الاتصال بالسيرفر',
//         icon: 'error',
//         confirmButtonText: 'حسناً',
//         background: isDarkMode ? '#222' : '#fff',
//         color: isDarkMode ? '#fff' : '#000',
//       });
//     }
//   });
// }
isUploading = false; // متغير للتحكم في عرض الـ spinner
selectedFileName: string | null = null;
// onFileSelected(event: any) {
//   const file: File = event.target.files[0];
//   if (!file) return;

//   const createdUser = localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail');

//   this.isUploading = true; // إظهار الـ spinner

//   this.ProductService.uploadExcel(file, createdUser).subscribe({
//     next: (res) => {
//       this.isUploading = false; // إخفاء الـ spinner
//       const isDarkMode = document.body.classList.contains('dark-mode');

//       // إذا كان هناك بعض الأخطاء
//     if (res.isSuccess && res.data?.errors?.length) {

//   const failedProducts = res.data.errors
//     .map((e: any) => `${e.column}: ${e.message}`)
//     .join('<br>');

//   Swal.fire({
//     title: 'تمت الإضافة مع بعض الأخطاء',
//     html: `<pre style="text-align:left;">${failedProducts}</pre>`,
//     icon: 'warning'
//   });

// }
//       // نجاح كامل
//       else if (res.isSuccess) {
//         Swal.fire({
//           title: 'تمت الإضافة بنجاح',
//           text: res.message || 'تمت إضافة جميع المنتجات بنجاح',
//           icon: 'success',
//           confirmButtonText: 'حسناً',
//           background: isDarkMode ? '#222' : '#fff',
//           color: isDarkMode ? '#fff' : '#000',
//         });
//       }
//       // فشل كامل
//       else {
//         Swal.fire({
//           title: 'فشل إضافة المنتجات',
//           text: res.message || 'حدث خطأ أثناء إضافة المنتجات',
//           icon: 'error',
//           confirmButtonText: 'حسناً',
//           background: isDarkMode ? '#222' : '#fff',
//           color: isDarkMode ? '#fff' : '#000',
//         });
//       }
//     },
//     error: (err) => {
//       this.isUploading = false; // إخفاء الـ spinner
//       const isDarkMode = document.body.classList.contains('dark-mode');
//       Swal.fire({
//         title: 'حدث خطأ في السيرفر',
//         text: err?.message || 'تعذر الاتصال بالسيرفر',
//         icon: 'error',
//         confirmButtonText: 'حسناً',
//         background: isDarkMode ? '#222' : '#fff',
//         color: isDarkMode ? '#fff' : '#000',
//       });
//     }
//   });
// }
onFileSelected(event: any) {

  const file: File = event.target.files[0];
  if (!file) return;

    this.selectedFileName = file.name;

  const createdUser =
    localStorage.getItem('userName') + "|" + localStorage.getItem('userEmail');

  this.isUploading = true;

  this.ProductService.uploadExcel(file, createdUser).subscribe({

    next: (res) => {

      this.isUploading = false;

      const isDarkMode = document.body.classList.contains('dark-mode');
      const hasSuccessRows = res.data?.data?.length > 0;

      // بعض الصفوف فشلت
      if (res.isSuccess && res.data?.errors?.length) {

        const failedProducts = res.data.errors
          .map((e: any) => `${e.column}: ${e.message}`)
          .join('<br>');

        Swal.fire({
          title: 'تمت الإضافة مع بعض الأخطاء',
          html: `<div style="text-align:left">${failedProducts}</div>`,
          icon: 'warning',
          confirmButtonText: 'حسناً',
          background: isDarkMode ? '#1e1e1e' : '#ffffff',
          color: isDarkMode ? '#ffffff' : '#000000'
        }).then(() => {

          if (hasSuccessRows) {
            this.GetAllProducts();
          }

        });

      }

      // نجاح كامل
      else if (res.isSuccess) {

        Swal.fire({
          title: 'تمت الإضافة بنجاح',
          text: res.message || 'تمت إضافة جميع المنتجات بنجاح',
          icon: 'success',
          confirmButtonText: 'حسناً',
          background: isDarkMode ? '#1e1e1e' : '#ffffff',
          color: isDarkMode ? '#ffffff' : '#000000'
        }).then(() => {

          this.GetAllProducts();

        });

      }

      // فشل كامل
      else {

        Swal.fire({
          title: 'فشل إضافة المنتجات',
          text: res.message || 'حدث خطأ أثناء إضافة المنتجات',
          icon: 'error',
          confirmButtonText: 'حسناً',
          background: isDarkMode ? '#1e1e1e' : '#ffffff',
          color: isDarkMode ? '#ffffff' : '#000000'
        });

      }

    },

    error: (err) => {

      this.isUploading = false;

      const isDarkMode = document.body.classList.contains('dark-mode');

      Swal.fire({
        title: 'حدث خطأ في السيرفر',
        text: err?.message || 'تعذر الاتصال بالسيرفر',
        icon: 'error',
        confirmButtonText: 'حسناً',
        background: isDarkMode ? '#1e1e1e' : '#ffffff',
        color: isDarkMode ? '#ffffff' : '#000000'
      });

    }

  });

}
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
}
