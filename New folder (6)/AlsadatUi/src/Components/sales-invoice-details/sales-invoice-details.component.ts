import { SalesInvoiceDetails } from './../../app/models/IsalesInvoice';
import { Component, inject } from '@angular/core';
import { SalesInvoice } from '../../app/Services/sales-invoice';
import { Subscription } from 'rxjs';
import { ActivatedRoute } from '@angular/router';
import Swal from 'sweetalert2';
import { MatCardModule, MatCardTitle, MatCardSubtitle, MatCard } from "@angular/material/card";
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-sales-invoice-details',
  standalone: true,
  imports: [CommonModule, MatCardTitle, MatCardSubtitle, MatCard],
  templateUrl: './sales-invoice-details.component.html',
  styleUrl: './sales-invoice-details.component.css'
})
export class SalesInvoiceDetailsComponent {

 private _SalesInvoiceService = inject(SalesInvoice);
  private _SalesInvoiceSubscription = new Subscription();
  constructor(  private route: ActivatedRoute) {}
invoiceId: number | null = null;
invoice!:SalesInvoiceDetails;

  ngOnInit(): void
  {

  this.route.paramMap.subscribe(params => {
    const id = params.get('id');
    if (id) {

      this.invoiceId = +id;

    }
  });
  this.GetCurrentInvoiceDetails();
  }

  ngOnDestroy():void
  {
this._SalesInvoiceSubscription?.unsubscribe();
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
