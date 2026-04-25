import { Component, ElementRef, inject, ViewChild } from '@angular/core';
import { SupplierService } from '../../app/Services/supplier.service';
import { Subscription } from 'rxjs';
import { StoreService } from '../../app/Services/store.service';
import { SupplierDto, SupplierFilteration, SupplierLookupDto, SupplierLookupFilter } from '../../app/models/ISupplierModels';
import Swal from 'sweetalert2';
import { PurchaseInvoiceService } from '../../app/Services/purchase-invoice.service';
import { PurchaseInvoiceDtos, PurchaseInvoiceFilteration } from '../../app/models/IPurchaseInvoiceVMs';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatIcon, MatIconModule } from "@angular/material/icon";
import { RouterLink } from "@angular/router";
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClientModule } from '@angular/common/http';
import { MatSelectModule } from '@angular/material/select';
import { CommonModule } from '@angular/common';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatLine } from "@angular/material/core";
import { log } from 'console';
import { SwalService } from '../../app/Services/swal.service';

@Component({
  selector: 'app-purchase-invoices',
  standalone: true,
  imports: [CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatTableModule,
    MatPaginatorModule,  // ✅ صح
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,     // ✅ هذا ضروري لـ ngValue
    MatIconModule,
    MatDialogModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    RouterLink,MatSelectModule ],
  templateUrl: './purchase-invoices.component.html',
   styleUrls: ['./purchase-invoices.component.css']
})
export class PurchaseInvoicesComponent {
  //#region  subscriptions
 private _SupplierService = inject(SupplierService);
  private _SupplierSubscription = new Subscription();
  private _PurchaseInvoiceService = inject(PurchaseInvoiceService);
  private _PurchaseInvoiceSubscription = new Subscription();
  private _swalService = inject(SwalService)
  isUserAdmin:boolean=false;
  isUserStockManager:boolean=false;
  //#endregion
  //#region  filters
supplierfilters: SupplierLookupFilter = {
    name: null,

  }
  purchaseInvoiceFilters:PurchaseInvoiceFilteration=
  {
    page:1,
    pageSize:10,
    invoiceNumber:null,
    supplierId:null,
    settledStatus:null,
    deleteStatus:null
  }

  //#endregion

//#region variables
AllSuppliers:SupplierLookupDto[]=[];
supplierSearch: string = '';

//#endregion
@ViewChild(MatPaginator) paginator!: MatPaginator
 @ViewChild('supplierSearchInput') supplierSearchInput!: ElementRef<HTMLInputElement>;

  isLoaded = false

  columns: ColumnDef[] = [
    { key: "invoiceNumber", label: "رقم الفاتورة" },
    { key: "supplierName", label: "اسم المورد" },
    { key: "settledStatus", label: "حالة التسكين في المخزن", type: "settledStatus" },
    { key: "createdAt", label: "تاريخ الإنشاء ",type:"date" },
    { key: "createdBy", label: "المنشئ " },
    { key: "updatedBy", label: "أخر مستخدم قام بتعديل" },
    { key: "updatedAt", label: "تاريخ التعديل ",type:"date" },
    { key: "totalGrowthAmount", label: "الإجمالي " },
    { key: "totalNetAmount",  label: "الصافي " },
    { key: "actions", label: "الإجراءات", type: "actions" },
  ]

  displayedColumnKeys = this.columns.map((c) => c.key)
  totalCount = 0
  dataSource = new MatTableDataSource<PurchaseInvoiceDtos>([])

  private dialog = inject(MatDialog)
  private fb = inject(FormBuilder)
  form!: FormGroup

ngOnInit(): void
{
  this.isUserAdmin= localStorage.getItem('roles')?.includes('Admin')!;
this.isUserStockManager=localStorage.getItem('roles')?.includes('StockManager')!;
  console.log(localStorage.getItem('roles'));
  console.log(this.isUserAdmin);


this.GetAllSuppliers();
this.GetAllPurchaseInvoices();
this.InitSearchForm();
}
ngOnDestroy():void
{
  this._SupplierSubscription?.unsubscribe();
  this._PurchaseInvoiceSubscription?.unsubscribe();
}
//#region methods
get filteredSuppliers() {
  if (!this.supplierSearch) return this.AllSuppliers;
  return this.AllSuppliers.filter(s =>
    s.name?.toLowerCase().includes(this.supplierSearch.toLowerCase())
  );
}
GetAllSuppliers(): void
{
  this._SupplierSubscription.add( this._SupplierService.getLookups(this.supplierfilters).subscribe({
    next: (res) => {
      this.AllSuppliers = res.data!;

    },
    error: (err) => {
      this._swalService.error(err.message)
    }}));
}
GetAllPurchaseInvoices():void
{
  this._PurchaseInvoiceSubscription.add(this._PurchaseInvoiceService.GetAllPurchaseInvoices(this.purchaseInvoiceFilters).subscribe({
    next:(res)=>
    {
       this.isLoaded = true
          this.dataSource.data = res.data
          this.totalCount = res.totalCount
    }
      ,
      error:(err)=>
      {
       this.isLoaded = true
        this._swalService.error(err.message)
      }
  }));
}
onPageChange(event: PageEvent): void {
    this.purchaseInvoiceFilters.page = event.pageIndex + 1
    this.purchaseInvoiceFilters.pageSize = event.pageSize
    this.GetAllPurchaseInvoices()
}
AskToDeletePurchaseInvoice(element:PurchaseInvoiceDtos):void
{
this._PurchaseInvoiceSubscription.add(this._PurchaseInvoiceService.editPurchaseInvoice(element.id!,{...element,deleteStatus:0,
  updatedBy:`${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`
}).subscribe({
  next:(res)=>
  {
   this._swalService.handleResponse(res)
          this.GetAllPurchaseInvoices();
          this.GetAllSuppliers();
  },
  error:(err)=>{


    this._swalService.error("فشل تحميل الفواتير")
  }
}));
}
CancelDeleteRequestPurchaseInvoice(element:PurchaseInvoiceDtos):void
{
this._PurchaseInvoiceSubscription.add(this._PurchaseInvoiceService.editPurchaseInvoice(element.id!,{...element,deleteStatus:1,
   updatedBy:`${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`
}).subscribe({
  next:(res)=>
  {
    Swal.fire({
            icon: "success",
            title: "تم الطلب بنجاح",
            text: "تم إلغاء طلب حذف الفاتورة للموافقة",
            confirmButtonText: "موافق",
            confirmButtonColor: "#3085d6",
          });
          this.GetAllPurchaseInvoices();
          this.GetAllSuppliers();
  },
  error:(err)=>{
      Swal.fire({
                icon: "error",
                title: "حدث خطأ",
                text: `${err.message}`,
                confirmButtonText: "موافق",
                confirmButtonColor: "#d33",
              })
  }
}));
}
DeletePurchaseInvoice(id: number): void {
  if (!id) {
    Swal.fire({
      icon: "error",
      title: "خطأ",
      text: "رقم الفاتورة غير موجود",
      confirmButtonText: "موافق",
      confirmButtonColor: "#d33",
    });
    return;
  }

  this._PurchaseInvoiceSubscription.add(
    this._PurchaseInvoiceService.deletePurchaseInvoice(id).subscribe({
      next: (res) => {

        Swal.fire({
          icon: "success",
          title: "تم الحذف بنجاح",
          text: "تم حذف الفاتورة بنجاح",
          confirmButtonText: "موافق",
          confirmButtonColor: "#3085d6",
        });
        this.GetAllPurchaseInvoices();
        this.GetAllSuppliers();
      },
      error: (err) => {
        console.log(err);

        Swal.fire({
          icon: "error",
          title: "حدث خطأ",
          text: `${err.error.message}`,
          confirmButtonText: "موافق",
          confirmButtonColor: "#d33",
        });
      },
    })
  );
}
InitSearchForm()
{
  this.form = this.fb.group({
     invoiceNumber: [""],
    supplierId: [""],
    settledStatus: [null],
    deleteStatus: [null]
  })
}
onSearch() {
  if (this.form.valid) {
    const formValues = this.form.value;

    this.purchaseInvoiceFilters = {
      ...this.purchaseInvoiceFilters,
      deleteStatus:
        formValues.deleteStatus === null || formValues.deleteStatus === '' ? null : Number(formValues.deleteStatus),
      invoiceNumber: formValues.invoiceNumber || null,
      supplierId: formValues.supplierId || null,
      settledStatus:
        formValues.settledStatus === null || formValues.settledStatus === '' ? null : Number(formValues.settledStatus)
    };

    this.GetAllPurchaseInvoices();
  }
}
resetFilters(){
  this.purchaseInvoiceFilters = {
    ...this.purchaseInvoiceFilters,
   deleteStatus: null,
    invoiceNumber: null,
    supplierId: null,
    settledStatus: null };
  this.InitSearchForm();
  this.GetAllPurchaseInvoices();
}
  onSupplierSelectOpened(isOpen: boolean): void {
    if (isOpen) {
      // ركز على input عند فتح select
      setTimeout(() => {
        this.supplierSearchInput?.nativeElement.focus();
      }, 100);
    } else {
      // امسح البحث عند إغلاق select
      this.supplierSearch = '';
    }
  }

  // ...existing code...

//#endregion

}
