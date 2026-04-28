import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { Component, inject, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SalesInvoice } from '../../app/Services/sales-invoice';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';
import { SalesInvoiceItemsResp, SalesInvoicesResponse } from '../../app/models/IsalesInvoice';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { MatExpansionModule, MatExpansionPanel } from '@angular/material/expansion';
import { StockService } from '../../app/Services/stock.service';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { invoiceProductsStock, ProductStockDto } from '../../app/models/IStockVM';
import { SwalService } from '../../app/Services/swal.service';
@Component({
  selector: 'app-invoice-confirmation',
  standalone: true,
  imports: [ MatInputModule,MatFormFieldModule,ReactiveFormsModule,MatIconModule, CommonModule,MatExpansionModule],
  templateUrl: './invoice-confirmation.component.html',
  styleUrl: './invoice-confirmation.component.css'
})
export class InvoiceConfirmationComponent {
  stockForms: { [productId: number]: FormGroup } = {};
    id!: number;
itemsList: SalesInvoiceItemsResp[] = [];
  constructor(private route: ActivatedRoute,private fb: FormBuilder,private router:Router) {}
    private _SalesInvoiceService = inject(SalesInvoice);
    private _SalesInvoiceSubscription = new Subscription();
    private _StockService = inject(StockService);
    private _StockSubscription = new Subscription();
    private  swalService=inject(SwalService);
    isLoaded: boolean = false;
    invoiceData?: SalesInvoicesResponse;

ngOnInit(): void {
  this.route.paramMap.subscribe(params => {
    this.id = Number(params.get('id'));
      this.GetSelectedInvoice();
  });
}
ngOnDestroy():void
    {
      this._SalesInvoiceSubscription?.unsubscribe();
      this._StockSubscription?.unsubscribe();
    }
GetSelectedInvoice()
{
  if(!this.id)
  {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'لم يتم تحديد معرف الفاتورة!',
    });
    return;
  }
   this._SalesInvoiceSubscription.add(
      this._SalesInvoiceService.GetSalesInvoiceById(this.id).subscribe({
        next: (res) => {
          if (!res?.data) {
            Swal.fire({
              icon: 'error',
              title: 'خطأ',
              text: 'لم يتم استرجاع بيانات الفاتورة!',
            });
            return;
          }
          this.isLoaded = true;
                    this.invoiceData = res.data; // حفظ بيانات الفاتورة

          this.itemsList = res.data.items;
          console.log(res.data);


        },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: err.error?.message || 'حدث خطأ أثناء تحميل الفاتورة!',
          });
        }
      })
    );
}


deletedProducts: { [productId: number]: string } = {}; // يخزن رسالة الحذف لكل منتج




GetProductStock(productId: number) {

  if (!productId) return;

  if (this.stockForms[productId]) return;

  this._StockService.getStockByProductId(productId).subscribe({

    next: (res) => {

      const product = res.data;
      if (!product) return;

      if (product.isProductDeleted || product.isCategoryDeleted) {
        this.deletedProducts[productId] =
          'المنتج محذوف أو الفئة الخاصة به محذوفة، الرجاء تعديل الفاتورة أولاً.';
        return;
      }

      const originalQty =
        this.itemsList.find(i => i.productID === product.productId)?.quantity || 0;

      // ✅ تعريف FormArray بنوع صحيح
      const stocksArray = new FormArray<FormGroup>(
        [],
        this.totalWithdrawnValidator(originalQty)
      );

      const productForm = this.fb.group({
        productId: [product.productId],
        stocks: stocksArray
      });

      product.stocks.forEach(stock => {

        const ctrl = this.fb.group({
          storeId: [stock.storeId],
          storeName: [stock.storeName],
          avaliableQuantity: [stock.avaliableQuantity || 0],
          withdrawnQuantity: [
            stock.withdrawnQuantity || 0,
            [Validators.min(0)]
          ]
        });

        ctrl.get('withdrawnQuantity')?.valueChanges.subscribe(value => {

          const withdrawn = Number(value) || 0;
          const available = Number(ctrl.get('avaliableQuantity')?.value) || 0;

          if (withdrawn > available) {
            ctrl.get('withdrawnQuantity')?.setErrors({ exceedAvailable: true });
          } else {

            const errors = ctrl.get('withdrawnQuantity')?.errors;

            if (errors) {
              delete errors['exceedAvailable'];
              ctrl.get('withdrawnQuantity')?.setErrors(
                Object.keys(errors).length ? errors : null
              );
            }
          }

          // إعادة تشغيل validator للمجموع
          stocksArray.updateValueAndValidity();
        });

        stocksArray.push(ctrl);
      });

      this.stockForms[productId] = productForm;
    },

    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: err.error?.message || 'حدث خطأ أثناء جلب بيانات المخزون!',
      });
    }

  });
}

  getStocksForm(productId: number): FormArray {
    return (this.stockForms[productId]?.get('stocks') as FormArray) || this.fb.array([]);
  }

canSubmit(productId: number): boolean {
  const productForm = this.stockForms[productId];
  const product = this.itemsList.find(i => i.productID === productId);
  if (!productForm || !product) return false;


  if (this.deletedProducts[productId]) return false;

  const stocksArray = productForm.get('stocks') as FormArray;


  const totalWithdrawn = stocksArray.controls.reduce((sum, stock) => {
    const val = stock.get('withdrawnQuantity')?.value || 0;
    return sum + val;
  }, 0);

  return totalWithdrawn <= product.quantity;
}
submitProductStock(productId: number) {
  const productForm = this.stockForms[productId];
  const product = productForm?.value as ProductStockDto;

  // تحقق قبل الإرسال
  if (!product) return;

  // تحقق الحذف
  if (this.deletedProducts[productId]) {
    Swal.fire({
      icon: 'warning',
      title: 'لا يمكن الحفظ',
      text: 'المنتج محذوف أو الفئة الخاصة به محذوفة، الرجاء تعديل الفاتورة أولاً.',
    });
    return;
  }

  // تحقق المجموع
  const totalWithdrawn = product.stocks.reduce((sum, s) => sum + (s.withdrawnQuantity || 0), 0);
  const originalQty = this.itemsList.find(i => i.productID === productId)?.quantity || 0;
  if (totalWithdrawn > originalQty) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: `مجموع الكميات المسحوبة يجب أن يكون <= ${originalQty}`,
    });
    return;
  }

  // الآن الـ productForm.value جاهز كـ ProductStockDto
  console.log('تم الحفظ:', product);
}

canSubmitAll(): boolean {
  return this.itemsList.every(item => {

    const productId = item.productID;
    const productForm = this.stockForms[productId];

    // الفورم لازم يكون موجود
    if (!productForm) return false;

    // المنتج مش محذوف
    if (this.deletedProducts[productId]) return false;

    // الفورم لازم يكون valid (يعني المجموع = الكمية بالظبط)
    return productForm.valid;
  });
}
// حفظ كل المنتجات مرة واحدة
submitAllStocks() {

  if (!this.id) {
    Swal.fire({
      icon: 'warning',
      title: 'لا يمكن الحفظ',
      text: `الفاتورة بدون معرف`,
    });
    return;
  }

  const allProducts: ProductStockDto[] = [];

  for (let item of this.itemsList) {

    const productId = item.productID;
    const productForm = this.stockForms[productId];

    // تحقق من الحذف
    if (this.deletedProducts[productId]) {
      Swal.fire({
        icon: 'warning',
        title: 'لا يمكن الحفظ',
        text: `المنتج "${item.productName}" محذوف أو فئته محذوفة، الرجاء تعديل الفاتورة أولاً.`,
      });
      return;
    }

    if (!productForm) continue;

    // 🔥 أهم تعديل: نعتمد على الـ form validity
    if (!productForm.valid) {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: `مجموع الكميات المسحوبة للمنتج "${item.productName}" يجب أن يساوي ${item.quantity}`,
      });
      return;
    }

    allProducts.push(productForm.value as ProductStockDto);
  }

  const req: invoiceProductsStock = {
    invoiceId: this.id,
      updateBy: `${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`, // استبدل هذا بالقيمة الحقيقية للمستخدم الحالي
    withdrwanItemsQuantities : allProducts,
  };
this.onConfirmInvoice(req);


  // هنا تبعت للباك إند
}
onConfirmInvoice(req: invoiceProductsStock) {
  this._SalesInvoiceService
    .confirmInvoice( req)
    .subscribe({
      next: (res) => {
        if (res.isSuccess) {
          Swal.fire({
            icon: 'success',
            title: 'تم تأكيد الفاتورة بنجاح',
          });
          this.router.navigate(['/SalesInvoices']);
        }
      },
      error: (err) => {
        Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: err.error?.message || 'حدث خطأ أثناء جلب بيانات المخزون!',
      });
      }
    });
}

totalWithdrawnValidator(expectedQty: number): ValidatorFn {

  return (control: AbstractControl): ValidationErrors | null => {

    const formArray = control as FormArray;

    if (!formArray.controls) return null;

    const total = formArray.controls.reduce((sum, ctrl) => {
      const val = Number(ctrl.get('withdrawnQuantity')?.value) || 0;
      return sum + val;
    }, 0);
  console.log( 'exp:'+ expectedQty);

    console.log('to'+total);

    return total !== expectedQty
      ? { totalMismatch: true }
      : null;
  };
}
downloadFile(blob: Blob, fileName: string = 'invoice.pdf') {
  const url = window.URL.createObjectURL(blob);

  const a = document.createElement('a');
  a.href = url;
  a.download = fileName;
  a.click();

  window.URL.revokeObjectURL(url);
}
 downloadPdf( fileName: string, errorMsg: string) {
  this._SalesInvoiceService.downloadInvoicePdfToStockPrepare(this.id!)
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


}
