import { Component, ElementRef, inject, QueryList, ViewChildren } from '@angular/core';
import { Subscription } from 'rxjs';
import { SupplierService } from '../../app/Services/supplier.service';
import { SupplierDto, SupplierFilteration, SupplierLookupFilter } from '../../app/models/ISupplierModels';
import Swal from 'sweetalert2';
import { StoreService } from '../../app/Services/store.service';
import { StoreDto, StoreFilteration } from '../../app/models/IstoreVM';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PurchaseInvoiceDtos } from '../../app/models/IPurchaseInvoiceVMs';
import { MatError, MatFormField, MatFormFieldModule, MatLabel } from "@angular/material/form-field";
import { MatAutocompleteModule } from "@angular/material/autocomplete";
import { MatSelectModule } from '@angular/material/select';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { MatInputModule } from '@angular/material/input';
import { MatIcon } from "@angular/material/icon";
import { MatTooltipModule } from '@angular/material/tooltip';
import { CommonModule } from '@angular/common';
import { ProductDto, ProductFilterationDto } from '../../app/models/IProductVM';
import { ProductService } from '../../app/Services/product.service';
import { PurchaseInvoiceService } from '../../app/Services/purchase-invoice.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-add-edit-purchase-invoice',
  standalone: true,
  imports: [CommonModule,MatInputModule, ReactiveFormsModule, MatFormField, MatLabel, MatAutocompleteModule, MatFormFieldModule,
    MatSelectModule, NgxMatSelectSearchModule, MatIcon,
        MatTooltipModule,
        MatError],
  templateUrl: './add-edit-purchase-invoice.component.html',
  styleUrl: './add-edit-purchase-invoice.component.css'
})
export class AddEditPurchaseInvoiceComponent {
    @ViewChildren('productCard') productCards!: QueryList<ElementRef>;
 isEdit = false;
  itemId!: number;
  constructor(private fb: FormBuilder,private route: ActivatedRoute) {
        this.route.paramMap.subscribe(params => {
      const id = params.get('id');

      if (id) {
        this.isEdit = true;
        this.itemId = +id;
        this.loadItem(this.itemId);
      }
    });
  }
loadItem(id: number) {
  this._PurchaseService.getPurchaseInvoiceById(id).subscribe(res => {

    if (!res?.data) return;

    // 1️⃣ البيانات الأساسية (غير items)
    this.invoiceForm.patchValue({
      invoiceNumber: res.data.invoiceNumber,
      supplierId: res.data.supplierId,
      supplierName: res.data.supplierName,
      settledStoreId: res.data.settledStoreId,
      totalGrowthAmount: res.data.totalGrowthAmount,
      totalNetAmount: res.data.totalNetAmount,
      precentageRival: res.data.precentageRival,
      rivalValue: res.data.rivalValue,
      totalRivalValue: res.data.totalRivalValue,
      taxPrecentage: res.data.taxPrecentage,
      taxValue: res.data.taxValue,
      createdBy: res.data.createdBy
    });

    // 2️⃣ تفريغ المنتجات القديمة
    this.items.clear();

    // 3️⃣ بناء المنتجات من الداتا
    res.data.items?.forEach(item => {
      const fg = this.createItem();
      fg.patchValue(item);
      this.items.push(fg);
    });

    // لو الفاتورة فاضية (احتياطي)
    if (this.items.length === 0) {
      this.addItem();
    }

    // 4️⃣ إعادة الحساب
    this.recalculateInvoice();
  });
}



  invoiceForm!: FormGroup;
  filteredSuppliers: any[] = [];
  AllProducts: ProductDto[] = [];
  storeSearchCtrl = new FormControl('');
filteredStores: StoreDto[] = [];
  private supplierService = inject(SupplierService);
  private supplierSubscription = new Subscription();
  private _StoreService = inject(StoreService);
  private _router = inject(Router);

  private _StoreSubscription = new Subscription();
  private _PurchaseService = inject(PurchaseInvoiceService);


   private ProductService = inject(ProductService);

  filtersForAllProoducts:ProductFilterationDto={

        isDeleted:false,
        name:null,
        page:null,
        pageSize:null
      }
   ngOnInit():void
    {
      this.GetAllStores();
      this.GetAllSuppliers();
      this.initForm();
      this.GetProducts();
      this.storeSearchCtrl.valueChanges.subscribe(searchText => {
    const value = searchText?.toLowerCase() || '';
    this.filteredStores = this.stores.filter(s =>
      s.storeName.toLowerCase().includes(value)
    );
  });
         this.supplierSearchCtrl.valueChanges.subscribe(searchText => {
      const value = searchText?.toLowerCase() || '';
      this.filteredSuppliers = this.suppliers.filter(s =>
        s.name.toLowerCase().includes(value)
      );
    });

    }
  Storefilters:StoreFilteration={
      page:null,
      pageSize:null,
      storeName:null,
      isDeleted:null

    }
    supplierSearchCtrl = new FormControl('');
  suppliers !: SupplierDto[] ;
  stores !:StoreDto[];
  filters: SupplierLookupFilter = {
      name: null,
     
    }
    GetProducts()
    {
      this.ProductService.getAllProducts(this.filtersForAllProoducts).subscribe({
        next:(res)=>{
            this.AllProducts=res.data||[];
        },
        error:(err)=>{
           Swal.fire({
                  icon: 'error',
                  title: 'خطأ',
                  text: `${err.error.message}`||'حدثت مشكلة أثناء الاتصال بقاعدة البايانات!',
                });
        }

      })
    }
GetAllSuppliers(): void {
        this.supplierSubscription.add(
          this.supplierService.getLookups(this.filters).subscribe({
            next: (res: any) => {

            this.suppliers = res.data;
             this.filteredSuppliers = [...this.suppliers];

              console.log(this.suppliers);

            },
            error: (err: any) => {
              Swal.fire({
                icon: "error",
                title: "حدث خطأ",
                text: `${err.message}`,
                confirmButtonText: "موافق",
                confirmButtonColor: "#d33",
              })

            },
          }),
        )
}

GetAllStores() {
  this._StoreSubscription.add(this._StoreService.getAllStores(this.Storefilters).subscribe({
    next: (res) => {
      this.stores = res.data || [];
      this.filteredStores = [...this.stores]; // نسخة للفلترة
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: `${err.error.message}` || 'حدثت مشكلة أثناء الاتصال بقاعدة البيانات!',
      });
    }
  }));
}

    initForm()
    {
       this.invoiceForm = this.fb.group({

      invoiceNumber: [null],  //done
      supplierId: [null, Validators.required], //done
      supplierName: [null],
      totalGrowthAmount: [0, [Validators.min(0)]],
      totalNetAmount: [0, [Validators.min(0)]],
      precentageRival: [0, [Validators.min(0), Validators.max(100)]],
      rivalValue: [0, [Validators.min(0)]],
      totalRivalValue: [0],
      taxPrecentage: [0, [Validators.min(0), Validators.max(100)]],
      taxValue: [0],
      settledStoreId: [null, Validators.required], //done
      createdBy:[`${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`],
      items: this.fb.array([]),
    });

    this.addItem();
    }
     // Getter for items FormArray
  get items(): FormArray {
    return this.invoiceForm.get('items') as FormArray;
  }

  // Create a new item FormGroup
  private createItem(): FormGroup {
    return this.fb.group({
      id: [null],
      productId: [null, Validators.required],
      productName: [null],
      itemCode: [null],
      quantity: [0, [Validators.required, Validators.min(1)]],
      buyingPricePerUnit: [0, [Validators.required, Validators.min(0)]],
      precentageRival: [0, [Validators.required,Validators.min(0), Validators.max(100)]],
      rivalValue: [0, [Validators.min(0)]],
      totalRivalValue: [0],
      totalGrowthAmount: [{ value: 0, disabled: true }],
      totalNetAmount: [0],
    });
  }
 getAvailableProducts(currentIndex: number): ProductDto[] {
      // جمع كل الـ productIds المختارة في الأسطر الأخرى
      const selectedProductIds = this.items.controls
        .filter((_, index) => index !== currentIndex) // استثناء السطر الحالي
        .map(item => item.get('productId')?.value)
        .filter(id => id !== null && id !== undefined);

      // إرجاع المنتجات الغير مختارة + المنتج المختار في السطر الحالي (لو موجود)
      const currentProductId = this.items.at(currentIndex)?.get('productId')?.value;

      return this.AllProducts.filter(product =>
        !selectedProductIds.includes(product.id) || product.id === currentProductId
      );
    }

calculateTotal(index: number): void {
  const item = this.items.at(index) as FormGroup;

  const qty = item.get('quantity')?.value || 0;
  const price = item.get('buyingPricePerUnit')?.value || 0;

  if (qty <= 0 || price < 0) {
    item.patchValue({
      totalGrowthAmount: 0,
      totalNetAmount: 0
    }, { emitEvent: false });
    return;
  }

  const total = qty * price;
  const percent = item.get('precentageRival')?.value || 0;
  const cash = item.get('rivalValue')?.value || 0;

  let discount = 0;
  if (percent > 0 && cash === 0)
    discount = total * (percent / 100);
  else if (cash > 0 && percent === 0)
    discount = cash;

  item.patchValue({
    totalGrowthAmount: total,
    totalRivalValue: discount,
    totalNetAmount: total - discount
  }, { emitEvent: false });

  this.recalculateInvoice(); // 👈 نقطة واحدة بس
}


onInvoiceDiscountChange() {
  this.recalculateInvoice();
}

calculateDiscount(index: number): void {
  const item = this.items.at(index) as FormGroup;

  const total = item.get('totalGrowthAmount')?.value || 0;
  const percent = item.get('precentageRival')?.value || 0;
  const cash = item.get('rivalValue')?.value || 0;

  if (percent > 0 && cash > 0) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'لا يمكن إدخال نسبة الخصم وخصم نقدي في نفس الوقت!',
    });
    item.get('totalRivalValue')?.setValue(0);
    item.get('totalNetAmount')?.setValue(total);
  } else {
    let discount = 0;
    if (percent > 0) discount = total * (percent / 100);
    else if (cash > 0) discount = cash;

    item.get('totalRivalValue')?.setValue(discount);
    item.get('totalNetAmount')?.setValue(total - discount);
  }this.recalculateInvoice(); // 👈 نقطة واحدة بس
}
calculateInvoiceDiscount(): void {
  const totalBeforeInvoiceDiscount = this.items.controls
    .reduce((sum, item) => sum + (item.get('totalNetAmount')?.value || 0), 0);

  const percent = this.invoiceForm.get('precentageRival')?.value || 0;
  const cash = this.invoiceForm.get('rivalValue')?.value || 0;

  if (percent > 0 && cash > 0) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'لا يمكن إدخال نسبة الخصم وخصم نقدي في نفس الوقت على الفاتورة!',
    });
    this.invoiceForm.get('totalRivalValue')?.setValue(0);
    this.invoiceForm.get('totalNetAmount')?.setValue(totalBeforeInvoiceDiscount);
    return;
  }

  let discount = 0;
  if (percent > 0) discount = totalBeforeInvoiceDiscount * (percent / 100);
  else if (cash > 0) discount = cash;

  this.invoiceForm.get('totalRivalValue')?.setValue(discount);
  this.invoiceForm.get('totalNetAmount')?.setValue(totalBeforeInvoiceDiscount - discount);
    this.calculateTax();
}



calculateInvoiceTotal(): void {
  const itemsArray = this.items.controls;
  let total = 0;

  itemsArray.forEach(item => {
    const net = item.get('totalNetAmount')?.value || 0;
    total += net;
  });

  this.invoiceForm.get('totalGrowthAmount')?.setValue(total);
  this.calculateInvoiceDiscount(); // استخدم نفس الحقل لإظهار الإجمالي
}

calculateTax(): void {
  const netBeforeTax = this.invoiceForm.get('totalNetAmount')?.value || 0;
  const taxPercent = this.invoiceForm.get('taxPrecentage')?.value || 0;

  const taxValue = netBeforeTax * (taxPercent / 100);

  this.invoiceForm.get('taxValue')?.setValue(taxValue);


  this.invoiceForm.get('totalNetAmount')?.setValue(netBeforeTax + taxValue);
  this.calculateInvoiceDiscount();
}

 get quantityControl() {
    return this.items.get('quantity');
  }

  // optional: force validation on every input
  onQuantityInput() {
    this.quantityControl?.updateValueAndValidity({ onlySelf: true, emitEvent: true });
  }
  // Add a new item to the FormArray


addItem() {
    // التحقق من صحة آخر منتج قبل إضافة منتج جديد
    if (this.items.length > 0) {
      const lastItem = this.items.at(this.items.length - 1);

      // التحقق من الحقول المطلوبة
      if (lastItem.invalid) {
        lastItem.markAllAsTouched();

        Swal.fire({
          icon: 'warning',
          title: 'تنبيه',
          text: 'برجاء إكمال بيانات المنتج الحالي قبل إضافة منتج جديد',
          confirmButtonText: 'حسناً',
          confirmButtonColor: '#d4af37'
        });

        return;
      }
    }

    // إضافة المنتج الجديد
    this.items.push(this.createItem());

    // الانتقال للمنتج الجديد بعد رندر الـ DOM
    setTimeout(() => {
      this.scrollToLastProduct();
    }, 150);
  }

  // دالة للتمرير إلى آخر منتج
  private scrollToLastProduct(): void {
    const productCards = document.querySelectorAll('.product-card');
    if (productCards && productCards.length > 0) {
      const lastCard = productCards[productCards.length - 1] as HTMLElement;
      if (lastCard) {
        lastCard.scrollIntoView({
          behavior: 'smooth',
          block: 'center'
        });

        // إضافة تأثير highlight مؤقت للمنتج الجديد
        lastCard.classList.add('highlight-new');
        setTimeout(() => {
          lastCard.classList.remove('highlight-new');
        }, 1500);
      }
    }
  }
  // Remove an item
  removeItem(index: number) {
    this.items.removeAt(index);
  }
  recalculateInvoice(): void {
  // 1️⃣ إجمالي المنتجات بعد خصم كل منتج
  const itemsTotal = this.items.controls.reduce(
    (sum, item) => sum + (item.get('totalNetAmount')?.value || 0),
    0
  );

  // 2️⃣ خصم الفاتورة
  const percent = this.invoiceForm.get('precentageRival')?.value || 0;
  const cash = this.invoiceForm.get('rivalValue')?.value || 0;

  let invoiceDiscount = 0;
  if (percent > 0 && cash === 0)
    invoiceDiscount = itemsTotal * (percent / 100);
  else if (cash > 0 && percent === 0)
    invoiceDiscount = cash;

  const netBeforeTax = itemsTotal - invoiceDiscount;

  // 3️⃣ الضريبة
  const taxPercent = this.invoiceForm.get('taxPrecentage')?.value || 0;
  const taxValue = netBeforeTax * (taxPercent / 100);

  // 4️⃣ تحديث الفورم مرة واحدة
  this.invoiceForm.patchValue({
    totalGrowthAmount: itemsTotal,
    totalRivalValue: invoiceDiscount,
    taxValue: taxValue,
    totalNetAmount: netBeforeTax + taxValue
  }, { emitEvent: false });
}


// submit() {
//   if (this.invoiceForm.invalid) {
//     this.invoiceForm.markAllAsTouched();
//     Swal.fire({
//       icon: 'error',
//       title: 'خطأ',
//       text: 'يرجى تعبئة جميع الحقول المطلوبة بشكل صحيح',
//     });
//     return;
//   }

//   const dto: PurchaseInvoiceDtos = this.invoiceForm.value;

//   this._PurchaseService.addPurchaseInvoice(dto).subscribe({
//     next: (res) => {
//       if (res.isSuccess) {
//         Swal.fire({
//           icon: 'success',
//           title: 'تم الحفظ',
//           text: res.data || 'تمت إضافة الفاتورة بنجاح',
//           confirmButtonText: 'حسناً',
//         });

//         this.invoiceForm.reset();
//         this.items.clear();
//         this.addItem();
//       } else {
//         Swal.fire({
//           icon: 'error',
//           title: 'خطأ',
//           text: res.message || 'حدث خطأ أثناء الإضافة',
//         });
//       }
//     },
//     error: (err) => {
//       Swal.fire({
//         icon: 'error',
//         title: 'خطأ',
//         text: err?.error?.message || 'حدث خطأ أثناء الاتصال بالخادم',
//       });
//     },
//   });
// }
submit() {

  if (this.invoiceForm.invalid) {
    this.invoiceForm.markAllAsTouched();
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'يرجى تعبئة جميع الحقول المطلوبة بشكل صحيح',
    });
    return;
  }

  const dto: PurchaseInvoiceDtos = this.invoiceForm.getRawValue();

  if (this.isEdit) {

    this._PurchaseService.editPurchaseInvoice(this.itemId, dto)
      .subscribe({
        next: (res) => {

          if (res.isSuccess) {
            Swal.fire({
              icon: 'success',
              title: 'تم التعديل',
              text: res.data || 'تم تعديل الفاتورة بنجاح',
            });
          } else {
            Swal.fire({
              icon: 'error',
              title: 'خطأ',
              text: res.message || 'حدث خطأ أثناء التعديل',
            });
          }

        },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: err?.error?.message || 'حدث خطأ أثناء الاتصال بالخادم',
          });
        }
      });

  } else {

    this._PurchaseService.addPurchaseInvoice(dto).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          Swal.fire({
            icon: 'success',
            title: 'تم الحفظ',
            text: res.data || 'تمت إضافة الفاتورة بنجاح',
          });

                  this._router.navigate(['/purchase-invoice/all']);

        }
      },
        error: (err) => {
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: err?.error?.message || 'حدث خطأ أثناء الاتصال بالخادم',
          });
        }
    });

  }
}



}
