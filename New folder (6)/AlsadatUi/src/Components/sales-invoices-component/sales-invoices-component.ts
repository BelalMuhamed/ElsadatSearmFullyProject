import { InvoiceChangeStatusReq, SalesInvoiceFilters } from './../../app/models/IsalesInvoice';
import { ChangeDetectorRef, Component, ElementRef, inject, ViewChild } from '@angular/core';
import { finalize, Subscription } from 'rxjs';
import { SalesInvoice } from '../../app/Services/sales-invoice';
import { SalesInvoicesResponse } from '../../app/models/IsalesInvoice';
import Swal from 'sweetalert2';
import { ColumnDef, GenericTableComponent } from '../../Layouts/generic-table-component/generic-table-component';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorIntl, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ApiResponse } from '../../app/models/ApiReponse';
import { MatIconModule } from '@angular/material/icon';
import { getArabicPaginatorIntl } from '../../app/transaltes/paginatortranslate';
import { FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { log } from 'console';
import { Router, RouterLink } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { DisAndMerchantService } from '../../app/Services/dis-and-merchant.service';
import { DistributorsAndMerchantsDto, DistributorsAndMerchantsFilters } from '../../app/models/IDisAndMercDto';
import { PurchaseInvoiceService } from '../../app/Services/purchase-invoice.service';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { PurchaseInvoiceDtos } from '../../app/models/IPurchaseInvoiceVMs';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { SwalService } from '../../app/Services/swal.service';

@Component({
  selector: 'app-sales-invoices-component',
   providers: [
    { provide: MatPaginatorIntl, useValue: getArabicPaginatorIntl() }
  ],
  imports: [CommonModule, MatTableModule,MatTooltipModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
    MatIconModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule, MatNativeDateModule, HttpClientModule, HttpClientModule, RouterLink

      ,MatSelectModule],
    standalone:true,
  templateUrl: './sales-invoices-component.html',
  styleUrls: ['./sales-invoices-component.css']
})
export class SalesInvoicesComponent {

   //#region  subscriptions
 private _DisAndMerchantService = inject(DisAndMerchantService);
  private _DisAndMerchantSubscription = new Subscription();
    private _SalesInvoiceService = inject(SalesInvoice);
    private swal = inject(SwalService);

  private _SalesInvoiceSubscription = new Subscription();
    isUserAdmin:boolean=false;
    //#endregion
    //#region  filters
filters:DistributorsAndMerchantsFilters={
    page:null,
    pageSize:null,
    cityName:null,
    fullName:null,
    phoneNumber:null,
    type:null,
    isDeleted:null
  }
    salesInvoiceFilters:SalesInvoiceFilters=
    {
      page:1,
      pageSize:10,
      invoiceNumber:null,
      customerId:null,
      createAt:null,
      deleteStatus:null
    }

    //#endregion

  //#region variables
  AllCustomers:DistributorsAndMerchantsDto[]=[];
  CustomerSearch: string = '';

  //#endregion
  @ViewChild(MatPaginator) paginator!: MatPaginator
   @ViewChild('CustomerSearchInput') customerSearchInput!: ElementRef<HTMLInputElement>;

    isLoaded = false

    columns: ColumnDef[] = [
      { key: "invoiceNumber", label: "رقم الفاتورة" },
      { key: "distributorName", label: "اسم العميل" },
      { key: "totalPoints",  label: "عدد النقاط المستحقة " },
      { key: "totalNetAmount",  label: "الصافي " },
      { key: "createdAt", label: "تاريخ الإنشاء ",type:"date" },
      { key: "createdBy", label: "المنشئ " },
      { key: "updateBy", label: "أخر مستخدم قام بتعديل" },
      { key: "updateAt", label: "تاريخ التعديل ",type:"date" },
      { key: "actions", label: "الإجراءات", type: "actions" },
    ]

    displayedColumnKeys = this.columns.map((c) => c.key)
    totalCount = 0
    dataSource = new MatTableDataSource<SalesInvoicesResponse>([])

    private dialog = inject(MatDialog)
    private fb = inject(FormBuilder)
    form!: FormGroup
  isUserStockManager:boolean=false;

  ngOnInit(): void
  {
    this.isUserStockManager=localStorage.getItem('roles')?.includes('StockManager')!;

      this.isUserAdmin= localStorage.getItem('roles')?.includes('Admin')!;

  this.GetAllCustomers();
  this.GetAllSalesInvoices();
  this.InitSearchForm();
  }
  ngOnDestroy():void
  {
    this._DisAndMerchantSubscription?.unsubscribe();
    this._SalesInvoiceSubscription?.unsubscribe();
  }
  //#region methods
  get filteredCustomers() {

    if (!this.CustomerSearch) return this.AllCustomers;
    return this.AllCustomers.filter(c =>
      c.fullName?.toLowerCase().includes(this.CustomerSearch.toLowerCase())
    );
  }
  GetAllCustomers(): void
  {
    this._DisAndMerchantSubscription.add( this._DisAndMerchantService.getAllDisAndMerch(this.filters).subscribe({
      next: (res) => {

        this.AllCustomers = res.data;

      },
      error: (err) => {
         Swal.fire({
                    icon: "error",
                    title: "حدث خطأ أثناء تجميل العملاء  ",
                    text: `${err?.error?.message}`,
                    confirmButtonText: "موافق",
                    confirmButtonColor: "#d33",
                  })
      }}));
  }
  // GetAllSalesInvoices():void
  // {
  //   this._SalesInvoiceSubscription.add(this._SalesInvoiceService.getAllSalesInvoices(this.salesInvoiceFilters).subscribe({
  //     next:(res)=>
  //     {
  //       console.log(res);

  //        this.isLoaded = true
  //           this.dataSource.data = res.data?.data
  //           console.log(res.data);

  //           this.totalCount = res.totalCount
  //     }
  //       ,
  //       error:(err)=>
  //       {
  //        this.isLoaded = true
  //          Swal.fire({
  //                     icon: "error",
  //                     title: "حدث خطأ أثناء تحميل فواتير المشتريات  ",
  //                     text: `${err?.error?.message}`,
  //                     confirmButtonText: "موافق",
  //                     confirmButtonColor: "#d33",
  //                   })
  //       }
  //   }));
  // }
  GetAllSalesInvoices(): void {
  this.isLoaded = false;

  this._SalesInvoiceSubscription.add(
    this._SalesInvoiceService
      .getAllSalesInvoices(this.salesInvoiceFilters)
      .subscribe({
        next: (res) => {

          this.isLoaded = true;

          // 🔥 handle Result<T>
          if (!res.isSuccess) {
            this.swal.error(res.message || 'حدث خطأ');
            return;
          }

          // ✅ data extraction صح
          this.dataSource.data = res.data?.data ?? [];
          this.totalCount = res.data?.totalCount ?? 0;

        },

        error: (err) => {
          this.isLoaded = true;

          this.swal.error(
            err?.error?.message || 'حدث خطأ أثناء تحميل البيانات'
          );
        }
      })
  );
}
  approveReverse(id: number) {
  this._SalesInvoiceService.reverseInvoice(id).subscribe({
    next: (res) => {
        Swal.fire({
              icon: "success",
              title: "تم الطلب بنجاح",
              text: res.data,
              confirmButtonText: "موافق",
              confirmButtonColor: "#3085d6",
            });
            this.GetAllSalesInvoices();
            this.GetAllCustomers();
    },
    error: (err) => {
         Swal.fire({
                  icon: "error",
                  title: "حدث خطأ",
                  text: `${err.error?.message}`,
                  confirmButtonText: "موافق",
                  confirmButtonColor: "#d33",
                })
    }
  });
}

refuseReverse(id: number) {
  this._SalesInvoiceService.RefusedReverseInvoice(id).subscribe({
    next: (res) => {
        Swal.fire({
              icon: "success",
              title: "تم الرفض بنجاح",
              text: res.data,
              confirmButtonText: "موافق",
              confirmButtonColor: "#3085d6",
            });
            this.GetAllSalesInvoices();
            this.GetAllCustomers();
    },
    error: (err) => {
         Swal.fire({
                  icon: "error",
                  title: "حدث خطأ",
                  text: `${err.error?.message}`,
                  confirmButtonText: "موافق",
                  confirmButtonColor: "#d33",
                })
    }
  });
}
  onPageChange(event: PageEvent): void {
      this.salesInvoiceFilters.page = event.pageIndex + 1
      this.salesInvoiceFilters.pageSize = event.pageSize
      this.GetAllSalesInvoices()
  }
  askToReverse(id: number) {
  this._SalesInvoiceService.askToReverseInvoice(id).subscribe({
    next: (res) => {
       Swal.fire({
              icon: "success",
              title: "تم الطلب بنجاح",
              text: res.data,
              confirmButtonText: "موافق",
              confirmButtonColor: "#3085d6",
            });
            this.GetAllSalesInvoices();
            this.GetAllCustomers();
    },
    error: (err) => {
          Swal.fire({
                  icon: "error",
                  title: "حدث خطأ",
                  text: `${err.error?.message}`,
                  confirmButtonText: "موافق",
                  confirmButtonColor: "#d33",
                })
    }
  });
}
  AskToDeleteSalesInvoice(element:SalesInvoicesResponse):void
  {
    const req:InvoiceChangeStatusReq={
      id:element.id,
      deleteStatus:0,
      salesInvoiceStatus:0,
      updateBy:`${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`
    }
  this._SalesInvoiceSubscription.add(this._SalesInvoiceService.changeInvoiceStatus(req
    ).subscribe({
    next:(res)=>
    {
      Swal.fire({
              icon: "success",
              title: "تم الطلب بنجاح",
              text: "تم إرسال طلب حذف الفاتورة للموافقة",
              confirmButtonText: "موافق",
              confirmButtonColor: "#3085d6",
            });
            this.GetAllSalesInvoices();
            this.GetAllCustomers();
    },
    error:(err)=>{
        Swal.fire({
                  icon: "error",
                  title: "حدث خطأ",
                  text: `${err.error?.message}`,
                  confirmButtonText: "موافق",
                  confirmButtonColor: "#d33",
                })
    }
  }));
  }
  CancelSalesInvoiceDeleteRequest(element:PurchaseInvoiceDtos):void
  {
    if(element.id != null)
    {
    const req:InvoiceChangeStatusReq={
      id:element.id,
      deleteStatus:1,
      salesInvoiceStatus:0,
      updateBy:`${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`
    }
  this._SalesInvoiceSubscription.add(this._SalesInvoiceService.changeInvoiceStatus(req).subscribe({
    next:(res)=>
    {
      Swal.fire({
              icon: "success",
              title: "تم الطلب بنجاح",
              text: "تم إلغاء طلب حذف الفاتورة للموافقة",
              confirmButtonText: "موافق",
              confirmButtonColor: "#3085d6",
            });
            this.GetAllSalesInvoices();
            this.GetAllCustomers();
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
    else
    {
       Swal.fire({
            icon: "error",
            title: "حدث خطأ",
            text: `خطأ أثناء إرسال بينات الفاتورة للحذف لا يوجد معرف `,
            confirmButtonText: "موافق",
            confirmButtonColor: "#d33",
          });
    }
  }
  DeleteSalesInvoice(id: number): void {
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

    this._SalesInvoiceSubscription.add(
      this._SalesInvoiceService.deleteInvoice(id).subscribe({
        next: (res) => {
          Swal.fire({
            icon: "success",
            title: "تم الحذف بنجاح",
            text: "تم حذف الفاتورة بنجاح",
            confirmButtonText: "موافق",
            confirmButtonColor: "#3085d6",
          });
          this.GetAllCustomers();
          this.GetAllSalesInvoices();
        },
        error: (err) => {
          console.log(err);

          Swal.fire({
            icon: "error",
            title: "حدث خطأ",
            text: `${err.error.message || err.message}`,
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
      customerId: [""],
      createAt: [null],
      deleteStatus: [null]
    })
  }
onSearch() {
  if (this.form.valid) {
    const formValues = this.form.value;

    this.salesInvoiceFilters = {
      ...this.salesInvoiceFilters,
      deleteStatus:
        formValues.deleteStatus === null || formValues.deleteStatus === ''
          ? null
          : Number(formValues.deleteStatus),

      invoiceNumber: formValues.invoiceNumber || null,
      customerId: formValues.customerId || null,

      createAt: formValues.createAt
        ? new Date(formValues.createAt).toISOString()
        : null
    };
    console.log(this.salesInvoiceFilters);

    this.GetAllSalesInvoices();
  }
}
  resetFilters(){
    this.salesInvoiceFilters = {
      ...this.salesInvoiceFilters,
     deleteStatus: null,
      invoiceNumber: null,
      customerId: null,
      createAt: null };
    this.InitSearchForm();
    this.GetAllSalesInvoices();
  }
    onCustomerSelectOpened(isOpen: boolean): void {
      if (isOpen) {
        // ركز على input عند فتح select
        setTimeout(() => {
          this.customerSearchInput?.nativeElement.focus();
        }, 100);
      } else {
        // امسح البحث عند إغلاق select
        this.CustomerSearch = '';
      }
    }

    // ...existing code...

  //#endregion

}
