import { PurchaseInvoiceService } from './../../app/Services/purchase-invoice.service';
import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { PurchaseInvoiceDtos } from '../../app/models/IPurchaseInvoiceVMs';
import Swal from 'sweetalert2';
import { MatCard, MatCardTitle, MatCardSubtitle } from "@angular/material/card";
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import { SwalService } from '../../app/Services/swal.service';
import { MatIcon } from "@angular/material/icon";

@Component({
  selector: 'app-purchase-invoice-details',
  standalone: true,
  imports: [MatCard, MatCardTitle, CommonModule, MatCardTitle, MatCard, MatTableModule, MatIcon],
  templateUrl: './purchase-invoice-details.component.html',
  styleUrl: './purchase-invoice-details.component.css'
})
export class PurchaseInvoiceDetailsComponent {
  isPrintMode: boolean = false;
  isUserStockManager:boolean=false;
displayedColumns: string[] = ['product', 'quantity', 'price', 'discount','total'];
get columnsToDisplay(): string[] {




  if (this.isUserStockManager) {
    return ['product', 'quantity']; // بدون أسعار
  }

  // 👈 باقي المستخدمين
  return this.displayedColumns;
} private _PurchaseInvoiceService = inject(PurchaseInvoiceService);
 private swalService = inject(SwalService);

  private _PurchaseInvoiceSubscription = new Subscription();
  constructor(  private route: ActivatedRoute) {}
invoiceId: number | null = null;
invoice!:PurchaseInvoiceDtos;

  ngOnInit(): void
  {

  this.route.paramMap.subscribe(params => {
    const id = params.get('id');
    if (id) {

      this.invoiceId = +id;

    }
  });
  this.isUserStockManager=localStorage.getItem('roles')?.includes('StockManager')!;
  this.GetCurrentInvoiceDetails();
  }

  ngOnDestroy():void
  {
this._PurchaseInvoiceSubscription?.unsubscribe();
  }
GetCurrentInvoiceDetails()
  {
    if(this.invoiceId != null && this.invoiceId != undefined)
    {
this._PurchaseInvoiceSubscription.add(this._PurchaseInvoiceService.getPurchaseInvoiceById(this.invoiceId).subscribe({
next: (res) => {
  if (res.data) {
    console.log(res.data);

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
downloadFile(blob: Blob, fileName: string = 'invoice.pdf') {
  const url = window.URL.createObjectURL(blob);

  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();

  window.URL.revokeObjectURL(url);
}
private downloadPdf(type: 'full' | 'simple', fileName: string, errorMsg: string) {
  this._PurchaseInvoiceService.downloadInvoicePdf(this.invoiceId!, type)
    .subscribe({
      next: (blob) => {
        this.downloadFile(blob, fileName);
        this.swalService.success('تم تحميل الفاتورة بنجاح');
      },
      error: () => {
        this.swalService.error(errorMsg);
      }
    });
}
generateFullPDF() {
  this.downloadPdf('full', `invoice-${this.invoice.invoiceNumber}-${this.invoice.supplierName}-${new Date().toISOString()}.pdf`, 'فشل تحميل الفاتورة الكاملة');
}

generateSimplePDF() {
  this.downloadPdf('simple', `invoice-${this.invoice.invoiceNumber}-${this.invoice.supplierName}-${new Date().toISOString()}.pdf`, 'فشل تحميل الفاتورة المبسطة');
}




}
