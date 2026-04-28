import { SalesInvoiceDetails } from './../../app/models/IsalesInvoice';
import { Component, inject } from '@angular/core';
import { SalesInvoice } from '../../app/Services/sales-invoice';
import { Subscription } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import Swal from 'sweetalert2';
import { MatCardModule, MatCardTitle, MatCardSubtitle, MatCard } from "@angular/material/card";
import { CommonModule } from '@angular/common';
import { SwalService } from '../../app/Services/swal.service';
import { MatIcon } from "@angular/material/icon";
@Component({
  selector: 'app-sales-invoice-details',
  standalone: true,
  imports: [CommonModule, MatCardTitle, MatCardSubtitle, MatCard, MatIcon],
  templateUrl: './sales-invoice-details.component.html',
  styleUrl: './sales-invoice-details.component.css'
})
export class SalesInvoiceDetailsComponent {
  isUserStockManager:boolean=false;

 private _SalesInvoiceService = inject(SalesInvoice);
  private _SalesInvoiceSubscription = new Subscription();
  private _swalService = inject(SwalService);
  constructor(  private route: ActivatedRoute) {}
invoiceId: number | null = null;
invoice!:SalesInvoiceDetails;

  ngOnInit(): void
  {
  this.isUserStockManager=localStorage.getItem('roles')?.includes('StockManager')!;

this.route.paramMap.subscribe(params => {
    const id = params.get('id');
    if (id) {
      this.invoiceId = +id;
      this.GetCurrentInvoiceDetails(); // move here
    }
  });
  }

  ngOnDestroy():void
  {
this._SalesInvoiceSubscription?.unsubscribe();
  }
  private downloadFile(blob: Blob, fileName: string) {
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();
  window.URL.revokeObjectURL(url);
}
downloadConfirmedStorePdf() {
  if (!this.invoiceId) {
    this._swalService.error('لا يمكن تحميل الفاتورة بدون معرف');
    return;
  }

  this._swalService.info('جاري تحميل ملف PDF...');

  this._SalesInvoiceSubscription.add(
    this._SalesInvoiceService.getConfirmedPdfStore(this.invoiceId).subscribe({
      next: (blob: Blob) => {
        this.downloadFile(blob, `invoice-${this.invoice.invoiceNumber}-${this.invoice.distributorName}.pdf`);
        this._swalService.success('تم تحميل الملف بنجاح');
      },
      error: () => {
        this._swalService.error('حدث خطأ أثناء تحميل ملف PDF');
      }
    })
  );
}
downloadConfirmedPdf() {
  if (!this.invoiceId) {
    this._swalService.error('لا يمكن تحميل الفاتورة بدون معرف');
    return;
  }

  this._swalService.info('جاري تحميل ملف PDF...');

  this._SalesInvoiceSubscription.add(
    this._SalesInvoiceService.getConfirmedPdf(this.invoiceId).subscribe({
      next: (blob: Blob) => {
         this.downloadFile(blob, `invoice-${this.invoice.invoiceNumber}-${this.invoice.distributorName}.pdf`);
        this._swalService.success('تم تحميل الملف بنجاح');
      },
      error: () => {
        this._swalService.error('حدث خطأ أثناء تحميل ملف PDF');
      }
    })
  );
}
  GetCurrentInvoiceDetails()
  {
    if(this.invoiceId != null && this.invoiceId != undefined)
    {
this._SalesInvoiceSubscription.add(this._SalesInvoiceService.GetInvoiceDetails(this.invoiceId).subscribe({
next: (res) => {
  console.log(res.data);

  if (res.data) {
    this.invoice = res.data;
    console.log(res);
  } else {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'الفاتورة غير موجودة',
      confirmButtonText: 'موافق',
      confirmButtonColor: '#d33'
    });
  }
},
error:(err)=>{
console.log(err);


}

}))
    }
    else
    {
        Swal.fire({
                      icon: "error",
                      title: "حدث خطأ",
                      text: `لا يمكن تحميل بيانات فاتورة بدون معرف `,
                      confirmButtonText: "موافق",
                      confirmButtonColor: "#d33",
                    })
    }

  }

}
