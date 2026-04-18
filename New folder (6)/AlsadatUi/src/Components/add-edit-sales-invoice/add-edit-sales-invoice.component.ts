import { Component, inject } from '@angular/core';
import { DisAndMerchantService } from '../../app/Services/dis-and-merchant.service';
import { Subscription } from 'rxjs';
import { DistributorsAndMerchantsFilters } from '../../app/models/IDisAndMercDto';
import { ProductDto, ProductFilterationDto } from '../../app/models/IProductVM';
import Swal from 'sweetalert2';
import { ProductService } from '../../app/Services/product.service';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormField, MatInputModule, MatLabel } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatError, MatSelectModule } from '@angular/material/select';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { MatIcon } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { log } from 'console';
import { SalesInvoiceItemsResp, SalesInvoicesResponse } from '../../app/models/IsalesInvoice';
import { SalesInvoice } from '../../app/Services/sales-invoice';

@Component({
  selector: 'app-add-edit-sales-invoice',
  standalone: true,
  imports: [CommonModule,MatInputModule, ReactiveFormsModule, MatFormField, MatLabel, MatAutocompleteModule, MatFormFieldModule,
      MatSelectModule, NgxMatSelectSearchModule, MatIcon,
          MatTooltipModule,
          MatError],
  templateUrl: './add-edit-sales-invoice.component.html',
  styleUrl: './add-edit-sales-invoice.component.css'
})
export class AddEditSalesInvoiceComponent {

  constructor(  private fb: FormBuilder,private route: ActivatedRoute) {}
 private _DisAndMerchantService = inject(DisAndMerchantService);
  private _DisAndMerchantSubscription = new Subscription();
   private _ProductService = inject(ProductService);
  private _ProductSubscription = new Subscription();
     private _SalesInvoiceService = inject(SalesInvoice);
  private _SalesInvoiceSubscription = new Subscription();
  AllCustomers:any[]=[];
  FilteredCustomers:any[]=[];
   invoiceForm!: FormGroup;
   filteredProducts: ProductDto[][] = [];
  AllProducts: ProductDto[] = [];
    CustomerSearchCtrl = new FormControl('');
ProductSearchControl = new FormControl('');
isEditMode = false;
invoiceId: number | null = null;
   Customerfilters:DistributorsAndMerchantsFilters={
      page:null,
      pageSize:null,
      cityName:null,
      fullName:null,
      phoneNumber:null,
      type:null,
      isDeleted:null
    }
     filtersForAllProoducts:ProductFilterationDto={

            isDeleted:false,
            name:null,
            page:null,
            pageSize:null
          }
   ngOnInit(): void
   {
this.initForm();
    this.GetAllCustomers();
    this.customersSearch();
    this.GetProducts();


  this.route.paramMap.subscribe(params => {
    const id = params.get('id');
    if (id) {
      this.isEditMode = true;
      this.invoiceId = +id;
      this.loadInvoice(this.invoiceId);
    }
  });
      this.invoiceForm.get('distributorId')!.valueChanges.subscribe(selectedId => {
    this.onCustomerSelected(selectedId);
  });

  this.invoiceForm.get('firstDiscount')?.valueChanges.subscribe(() => {
    this.recalculateInvoiceTotals();
  });
  this.invoiceForm.get('secondDiscount')?.valueChanges.subscribe(() => {
    this.recalculateInvoiceTotals();
  });
  this.invoiceForm.get('thirdDiscount')?.valueChanges.subscribe(() => {
    this.recalculateInvoiceTotals();
  });


  this.invoiceForm.get('taxPrecentage')?.valueChanges.subscribe(() => {
    this.recalculateInvoiceTotals();
  });
   }
     ngOnDestroy():void
     {
      this._DisAndMerchantSubscription?.unsubscribe();
     }

  GetAllCustomers()
  {
    this._DisAndMerchantSubscription.add(this._DisAndMerchantService.getAllDisAndMerch(this.Customerfilters).subscribe({
      next:(res)=>{
        console.log(res.data);
         this.AllCustomers =  res.data;
         this.FilteredCustomers=[... this.AllCustomers]
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
    }))
  }
  loadInvoice(id: number) {
  this._SalesInvoiceSubscription.add(
    this._SalesInvoiceService.GetSalesInvoiceById(id).subscribe({
      next: (res) => {
        if (!res?.data) {
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: 'لم يتم استرجاع بيانات الفاتورة!',
          });
          return;
        }

        this.invoiceForm.patchValue({
          invoiceNumber: res.data.invoiceNumber,
          distributorId: res.data.distributorId,
          firstDiscount: res.data.firstDiscount,
          secondDiscount: res.data.secondDiscount,
          thirdDiscount: res.data.thirdDiscount,
          totalCopouns: res.data.totalCopouns,
          totalGrowthAmount: res.data.totalGrowthAmount,
          totalNetAmount: res.data.totalNetAmount,
          taxPrecentage: res.data.taxPrecentage,
          taxValue: res.data.taxValue,
          totalPoints: res.data.totalPoints,
          createdAt: res.data.createdAt,
          createdBy: res.data.createdBy,
          updateAt: res.data.updateAt,
          updateBy: res.data.updateBy,
          salesInvoiceStatus: res.data.salesInvoiceStatus,
          deleteStatus: res.data.deleteStatus
        });


        this.items.clear();
        res.data.items.forEach((item: SalesInvoiceItemsResp) => {
          const group = this.createItem();
          group.patchValue(item);
          this.items.push(group);

          const index = this.items.length - 1;
          this.filteredProducts[index] = this.getAvailableProducts(index);
        });
    this.refreshAllProductFilters();
        this.recalculateInvoiceTotals();
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

GetProducts() {
  this._ProductSubscription.add(
    this._ProductService.getAllProducts(this.filtersForAllProoducts)
      .subscribe({
        next: (res) => {
          const activeProducts = res.data || [];

          if (this.isEditMode) {
            const invoiceProductIds =
              this.items.getRawValue().map((x: any) => x.productID);

            const missingProducts =
              this.AllProducts
                .filter(p =>
                  invoiceProductIds.includes(p.id) &&
                  !activeProducts.some(a => a.id === p.id)
                );

            this.AllProducts = [...activeProducts, ...missingProducts];
          } else {
            this.AllProducts = activeProducts;
          }

          this.refreshAllProductFilters();
        }
      })
  );
}
    customersSearch()
    {
        this.CustomerSearchCtrl.valueChanges.subscribe(searchText => {
      const value = searchText?.toLowerCase() || '';
      this.FilteredCustomers = this.AllCustomers.filter(s =>
        s.fullName.toLowerCase().includes(value)

      );


    });
    }

   initForm(): void {
  this.invoiceForm = this.fb.group({
    invoiceNumber: [null],
    distributorId: [null, Validators.required],  // تم
    totalCopouns: [0],// تم
    firstDiscount: [null, [Validators.min(0), Validators.max(100)]],
    secondDiscount: [null, [Validators.min(0), Validators.max(100)]],
    thirdDiscount: [null, [Validators.min(0), Validators.max(100)]],

    totalGrowthAmount: [0, [Validators.min(0)]], // تم
    totalNetAmount: [0, [Validators.min(0)]],// تم

    taxPrecentage: [0],
    taxValue: [0],

    totalPoints: [0],

    salesInvoiceStatus: [null],
    deleteStatus: [0],

    createdAt: [null],
    createdBy: [`${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`],

    updateAt: [null],
    updateBy: [null],

    items: this.fb.array([] as FormGroup[])
  });
}
   get items(): FormArray {
        return this.invoiceForm.get('items') as FormArray;
      }

getAvailableProducts(currentIndex: number): ProductDto[] {

  const selectedProductIds = this.items.controls
    .filter((_, index) => index !== currentIndex)
    .map(item => item.get('productID')?.value)
    .filter(id => id !== null && id !== undefined);

  const currentProductId = this.items.at(currentIndex)?.get('productID')?.value;

  return this.AllProducts.filter(product =>
    !selectedProductIds.includes(product.id) || product.id === currentProductId
  );
}
createItem(): FormGroup {
  const group = this.fb.group({
    productID: [null, Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]],
    sellingPrice: [{ value: 0, disabled: true }],

    precentageRival: [0],
    rivalValue: [0],

    totalRivalValue: [{ value: 0, disabled: true }],
    totalGrowthAmount: [{ value: 0, disabled: true }],
    totalNetAmount: [{ value: 0, disabled: true }]
  });

  this.handleProductDiscountLogic(group);

  group.get('quantity')?.valueChanges.subscribe(() => {
    this.recalculateItem(group);
  });

  return group;
}
 get quantityControl() {
    return this.items.get('quantity');
  }
  handleProductDiscountLogic(item: FormGroup) {

  const percentCtrl = item.get('precentageRival')!;
  const valueCtrl   = item.get('rivalValue')!;

  percentCtrl.valueChanges.subscribe(percent => {
    if (percent && percent > 0) {
      valueCtrl.setValue(0, { emitEvent: false });
      valueCtrl.disable({ emitEvent: false });
    } else {
      valueCtrl.enable({ emitEvent: false });
    }
    this.recalculateItem(item);
  });

  valueCtrl.valueChanges.subscribe(value => {
    if (value && value > 0) {
      percentCtrl.setValue(0, { emitEvent: false });
      percentCtrl.disable({ emitEvent: false });
    } else {
      percentCtrl.enable({ emitEvent: false });
    }
    this.recalculateItem(item);
  });
}
recalculateItem(item: FormGroup) {
  const qty = item.get('quantity')?.value || 0;
  const price = item.get('sellingPrice')?.value || 0;

  const percent = item.get('precentageRival')?.value || 0;
  const cash    = item.get('rivalValue')?.value || 0;

  const gross = qty * price;

  let discount = 0;
  if (percent > 0) {
    discount = gross * (percent / 100);
  } else if (cash > 0) {
    discount = cash;
  }

  const net = gross - discount;

  item.patchValue({
    totalGrowthAmount: gross,
    totalRivalValue: discount,
    totalNetAmount: net
  }, { emitEvent: false });

  this.recalculateInvoiceTotals();
}

recalculateInvoiceTotals() {
  let totalGrossBeforeDiscounts = 0;
  let totalDiscountOnItems = 0;
  let totalNetAfterItemDiscounts = 0;
  let totalPoints = 0;

  this.items.controls.forEach(item => {
    const qty = item.get('quantity')?.value || 0;
    const price = item.get('sellingPrice')?.value || 0;
    const percent = item.get('precentageRival')?.value || 0;
    const cash = item.get('rivalValue')?.value || 0;

    const gross = qty * price;

    let discount = 0;
    if (percent > 0) discount = gross * (percent / 100);
    else if (cash > 0) discount = cash;

    const net = gross - discount;

    totalGrossBeforeDiscounts += gross;
    totalDiscountOnItems += discount;
    totalNetAfterItemDiscounts += net;

    const productId = item.get('productID')?.value;
    const product = this.AllProducts.find(p => p.id === productId);
    if (product) totalPoints += (product.pointPerUnit ?? 0) * qty;


    item.patchValue({
      totalGrowthAmount: gross,
      totalRivalValue: discount,
      totalNetAmount: net
    }, { emitEvent: false });
  });


  let netAfterAllDiscounts = totalNetAfterItemDiscounts;
  const d1 = this.invoiceForm.get('firstDiscount')?.value || 0;
  const d2 = this.invoiceForm.get('secondDiscount')?.value || 0;
  const d3 = this.invoiceForm.get('thirdDiscount')?.value || 0;

  const sequentialDiscounts = [];

  if (d1 > 0) {
    const disc1 = netAfterAllDiscounts * (d1 / 100);
    sequentialDiscounts.push(disc1);
    netAfterAllDiscounts -= disc1;
  }
  if (d2 > 0) {
    const disc2 = netAfterAllDiscounts * (d2 / 100);
    sequentialDiscounts.push(disc2);
    netAfterAllDiscounts -= disc2;
  }
  if (d3 > 0) {
    const disc3 = netAfterAllDiscounts * (d3 / 100);
    sequentialDiscounts.push(disc3);
    netAfterAllDiscounts -= disc3;
  }

  const totalSequentialDiscount = sequentialDiscounts.reduce((a,b)=>a+b,0);


  const taxPercent = this.invoiceForm.get('taxPrecentage')?.value || 0;
  const taxValue = netAfterAllDiscounts * taxPercent / 100;
  const finalNet = netAfterAllDiscounts + taxValue;


  this.invoiceForm.patchValue({
    totalGrowthAmount: totalNetAfterItemDiscounts, // هنا إجمالي المنتجات بعد خصومات السطر
    totalNetAmount: finalNet,
    taxValue: taxValue,
    totalPoints: totalPoints,
    totalCopouns: Math.floor(totalPoints / 60),
    totalDiscountValue: totalDiscountOnItems + totalSequentialDiscount
  }, { emitEvent: false });
}
    onQuantityInput() {
    this.quantityControl?.updateValueAndValidity({ onlySelf: true, emitEvent: true });
  }
  recalculateTotals() {
  let totalPoints = 0;

  this.items.controls.forEach(item => {
    const productId = item.get('productID')?.value;
    const quantity = item.get('quantity')?.value || 0;

    const product = this.AllProducts.find(p => p.id === productId);
    if (!product) return;


    const pointPerUnit = product.pointPerUnit ?? 0;
    const rowPoints = pointPerUnit * quantity;
    totalPoints += rowPoints;
  });


  this.invoiceForm.patchValue({
    totalPoints: totalPoints,
     totalCopouns: Math.floor(totalPoints / 60)
  }, { emitEvent: false });
}
addItem() {
  if (this.items.length > 0) {
    const lastItem = this.items.at(this.items.length - 1);
    if (lastItem.invalid) {
      lastItem.markAllAsTouched();
      Swal.fire({
        icon: 'warning',
        title: 'تنبيه',
        text: 'برجاء إكمال بيانات المنتج الحالي قبل إضافة منتج جديد',
      });
      return;
    }
  }

  this.items.push(this.createItem());

  const index = this.items.length - 1;
  const item = this.items.at(index);


  this.filteredProducts[index] = this.getAvailableProducts(index);
  this.setupProductSearch(index);


  item.get('productID')?.valueChanges.subscribe(productId => {

    const product = this.AllProducts.find(p => p.id === productId);
    if (!product) return;

    item.patchValue({
      sellingPrice: product.sellingPrice,
      productName: product.name
    }, { emitEvent: false });

    this.recalculateTotals();
     this.refreshAllProductFilters();
  });


  item.get('quantity')?.valueChanges.subscribe(() => {
    this.recalculateTotals();
  });

  setTimeout(() => this.scrollToLastProduct(), 150);
}
setupProductSearch(index: number) {
  this.ProductSearchControl.valueChanges.subscribe(search => {
    const value = search?.toLowerCase() || '';

    this.filteredProducts[index] = this.getAvailableProducts(index)
      .filter(p =>
        p.name?.toLowerCase().includes(value) ||
        p.productCode?.toLowerCase().includes(value)
      );
  });
}
      private scrollToLastProduct(): void {
    const productCards = document.querySelectorAll('.product-card');
    if (productCards && productCards.length > 0) {
      const lastCard = productCards[productCards.length - 1] as HTMLElement;
      if (lastCard) {
        lastCard.scrollIntoView({
          behavior: 'smooth',
          block: 'center'
        });


        lastCard.classList.add('highlight-new');
        setTimeout(() => {
          lastCard.classList.remove('highlight-new');
        }, 1500);
      }
    }
  }
    removeItem(index: number) {
    this.items.removeAt(index);
    this.recalculateInvoiceTotals();
     this.refreshAllProductFilters();
  }

onCustomerSelected(customerId: any) {
  if (!customerId) return;
  const selectedCustomer = this.AllCustomers.find(c => c.userId === customerId);
  if (!selectedCustomer) return;

  this.invoiceForm.patchValue({
    firstDiscount: selectedCustomer.firstSpecialDiscount || 0,
    secondDiscount: selectedCustomer.secondSpecialDiscount || 0,
    thirdDiscount: selectedCustomer.thirdSpecialDiscount || 0
  }, { emitEvent: true });
}

private mapFormToDTO(): SalesInvoicesResponse {
  const formValue = this.invoiceForm.value;
  const items: SalesInvoiceItemsResp[] = this.items.getRawValue().map((item: any) => ({
    id: item.id ?? null,
    sellingPrice: item.sellingPrice,
    quantity: item.quantity,
    productID: item.productID,
    productName: item.productName ?? null,
    precentageRival: item.precentageRival ?? null,
    rivalValue: item.rivalValue ?? null,
    totalRivalValue: item.totalRivalValue ?? null,
    totalGrowthAmount: item.totalGrowthAmount ?? null,
    totalNetAmount: item.totalNetAmount ?? null
  }));

  const selectedCustomer = this.AllCustomers.find(c => c.userId === formValue.distributorId);

  return {
  id: this.isEditMode && this.invoiceId != null ? this.invoiceId : 0,
    totalCopouns: formValue.totalCopouns,
    distributorId: formValue.distributorId,
    distributorName: selectedCustomer?.fullName ?? null,
    firstDiscount: formValue.firstDiscount,
    secondDiscount: formValue.secondDiscount,
    thirdDiscount: formValue.thirdDiscount,
    invoiceNumber: formValue.invoiceNumber,
    totalPoints: formValue.totalPoints,
    createdAt: this.isEditMode ? formValue.createdAt : new Date(),
    createdBy: this.isEditMode ? formValue.createdBy : `${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`,
    salesInvoiceStatus: 1,
    deleteStatus: formValue.deleteStatus,
    updateBy: this.isEditMode ? `${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}` : null,
    updateAt: this.isEditMode ? new Date() : null,
    totalGrowthAmount: formValue.totalGrowthAmount,
    totalNetAmount: formValue.totalNetAmount,
    taxPrecentage: formValue.taxPrecentage,
    taxValue: formValue.taxValue,
    reverseJournalEntry:  formValue.reverseJournalEntry ,
    items: items
  };
}

submitInvoice() {
  if (!this.invoiceForm.valid) {
    this.invoiceForm.markAllAsTouched();
    Swal.fire({
      icon: 'warning',
      title: 'تنبيه',
      text: 'يرجى التأكد من اكتمال جميع بيانات الفاتورة قبل الحفظ',
    });
    return;
  }

  const dto = this.mapFormToDTO();
  console.log(dto);

  const obs = this.isEditMode
    ? this._SalesInvoiceService.updateInvoice(dto.id,dto)
    : this._SalesInvoiceService.addInvoice(dto);

  this._SalesInvoiceSubscription.add(
    obs.subscribe({
      next: (res) => {
        Swal.fire({
          icon: 'success',
          title: 'تم الحفظ بنجاح',
          text: `رقم الفاتورة: ${dto.invoiceNumber}`,
        });
        if (!this.isEditMode) {
          this.invoiceForm.reset();
          this.items.clear();
        }
      },
      error: (err) => {
        Swal.fire({
          icon: 'error',
          title: 'خطأ',
          text: err.error?.message || 'حدث خطأ أثناء الحفظ!',
        });
      }
    })
  );
}
refreshAllProductFilters() {
  this.items.controls.forEach((_, index) => {
    this.filteredProducts[index] = this.getAvailableProducts(index);
  });
}
}
