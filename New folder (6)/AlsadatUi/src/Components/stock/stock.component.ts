import { HttpClientModule } from '@angular/common/http';
import { Component, inject, ViewChild } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormField, MatLabel, MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { StockService } from '../../app/Services/stock.service';
import { Subscription } from 'rxjs';
import { StockDto, StockFilterations } from '../../app/models/IStockVM';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { CommonModule } from '@angular/common';
import { StoreService } from '../../app/Services/store.service';
import { StoreDto, StoreFilteration } from '../../app/models/IstoreVM';
import Swal from 'sweetalert2';
import { log } from 'console';
import { StoreTransactionDto, StoreTransactionProductsDto } from '../../app/models/ITransactionVM';
import { MatCard, MatCardTitle, MatCardContent } from "@angular/material/card";
import { TransactionService } from '../../app/Services/transaction.service';

@Component({
  selector: 'app-stock',
  standalone: true,
  imports: [MatColumnDef, MatHeaderCellDef, MatCellDef, MatHeaderRowDef, MatRowDef,
    MatTableModule,
    MatSelectModule,
    MatSlideToggleModule, FormsModule, MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule, HttpClientModule  ,MatPaginator, ReactiveFormsModule, CommonModule, MatCard, MatCardTitle, MatCardContent],
  templateUrl: './stock.component.html',
  styleUrl: './stock.component.css'
})
export class StockComponent {
 private dialog = inject(MatDialog);
  private fb = inject(FormBuilder);
  private _StockService = inject(StockService);

  Searchform!: FormGroup;
  private _StockSubscription = new Subscription();
private _StoreService = inject(StoreService);
private _TransactionService = inject(TransactionService);
private _StoreSubscription = new Subscription();
 Stores:StoreDto[]=[];
  filters: StockFilterations = {
    page: 1,
    pageSize: 10,
    storeName: null
  };
getQuantity(element: StockDto, productName: string): number {
  if (!element.storeStocks) return 0;
  const product = element.storeStocks.find(p => p.productName === productName);
  return product ? product.quantity : 0;
}

  dataSource = new MatTableDataSource<StockDto>([]);
  displayedColumns: string[] = []; // dynamic columns
  totalCount = 0;
  isLoading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  ngOnInit(): void {
    this.initSearchForm();
    this.getAllStocks();
    this.GetStores();
  }

  ngOnDestroy(): void {
    this._StockSubscription?.unsubscribe();
  }

  initSearchForm() {
    this.Searchform = this.fb.group({
      storeName: ['']
    });
  }

  getAllStocks() {
    this.isLoading = true;
    this._StockSubscription.add(
      this._StockService.getAllStocks(this.filters).subscribe({
        next: (res) => {
          if (res?.data?.length > 0) {
            this.dataSource.data = res.data;

            const productColumns: string[] = res.data[0].storeStocks?.map(p => p.productName) || [];
            this.displayedColumns = ['storeName', ...productColumns];

            this.totalCount = res.totalCount;


          } else {
            this.dataSource.data = [];
            this.displayedColumns = ['storeName'];
            this.totalCount = 0;
          }

          this.isLoading = false;
        },
        error: (err) => {
          this.isLoading = false;
        }
      })
    );
  }

  onSearch() {
    if (this.Searchform.valid) {
      const formValues = this.Searchform.value;
      this.filters = {
        ...this.filters,
        storeName: formValues.storeName || null,
        page: 1
      };
      this.getAllStocks();
    }
  }

  onPageChange(event: PageEvent) {
    this.filters.page = event.pageIndex + 1;
    this.filters.pageSize = event.pageSize;
    this.getAllStocks();
  }
  getProduct(element: StockDto, productName: string) {
  if (!element.storeStocks) return null;
  return element.storeStocks.find(p => p.productName === productName);
}

GetStores()
{
const filter: StoreFilteration = {
  storeName: null,
  page: null,
  pageSize: null,
  isDeleted:false
};
  this._StoreSubscription.add(this._StoreService.getAllStores(filter).subscribe({
    next:(res)=>{
  this.Stores=res.data;

    },error:(err)=>{

               Swal.fire({
                      icon: 'error',
                      title: 'خطأ',
                      text: `${err.message}`||'حدثت مشكلة أثناء الاتصال بقاعدة البيانات !',
                    });
    }
  }))
}

SourceStoreName:string|null="";
SourceStock!:StockDto;
showTransferCard = false;
transferProducts: StoreTransactionProductsDto[] = [];
sourceStockMap = new Map<number, number>();
 transferForm!: FormGroup;
// onSourceStoreChange(sourceStore: StoreDto) {
//   this._StockService.getStoreStockById(sourceStore.id).subscribe({
//     next: (res) => {
//       this.SourceStock = res.data;
//       console.log(res);


//       // build map (productId => quantity)
//       this.sourceStockMap.clear();
//       this.SourceStock.storeStocks?.forEach(p => {
//         this.sourceStockMap.set(p.productId, p.quantity);
//       });

//       this.transferProducts = this.SourceStock.storeStocks
//         ?.filter(p => p)
//         .map(p => ({
//           transactionId:null,
//           productId: p.productId,
//           productName: p.productName,
//           quantity: 0
//         })) || [];

//       this.showTransferCard = true;
//     }
//   });
// }
  onSourceStoreChange(sourceStore: StoreDto) {
    this._StockService.getStoreStockById(sourceStore.id).subscribe({
      next: (res) => {
        this.SourceStock = res.data;

        // بناء FormArray للمنتجات
  const productsFGs = this.SourceStock.storeStocks?.map(p =>
  this.fb.group({
    productId: [p.productId],
    productName: [p.productName],
    availableQuantity: [p.quantity],
    quantity: [0, [Validators.min(0)]]
  }, { validators: this.quantityNotExceedAvailable }) // ⭐
) || [];
        this.transferForm = this.fb.group({
  sourceId: [sourceStore.id],
  sourceName: [sourceStore.storeName],
  destenationId: [null, Validators.required],
  destenationName: [null, Validators.required],
  makeTransactionUser: [''],
  transactionProducts: this.fb.array(productsFGs)
}, { validators: this.sourceNotEqualDestination }); // ⭐


        this.showTransferCard = true;
      }
    });
  }
get transactionProducts(): FormArray {
  return this.transferForm?.get('transactionProducts') as FormArray;
}


  onDesStoreChange(destStore: StoreDto) {
    if(destStore.storeName === this.SourceStoreName) {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: 'لا يمكن النقل للمخزن المصدر',
      });
      return;
    }

    if (this.transferForm) {
      this.transferForm.patchValue({
        destenationId: destStore.id,
        destenationName: destStore.storeName
      });
    }
  }
submitTransfer() {

  // 1️⃣ نفس المخزن
  if (this.transferForm.errors?.['sameStore']) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'لا يمكن النقل إلى نفس المخزن'
    });
    return;
  }

  // 2️⃣ كميات أكبر من المتاح
  const invalidProduct = this.transactionProducts.controls.find(c =>
    c.errors?.['exceedAvailable']
  );

  if (invalidProduct) {
    const productName = invalidProduct.get('productName')?.value;
    Swal.fire({
      icon: 'error',
      title: 'كمية غير صحيحة',
      text: `الكمية المطلوبة للمنتج (${productName}) أكبر من المتاح`
    });
    return;
  }

  // 3️⃣ فورم غير صالح
  if (this.transferForm.invalid) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: 'يرجى التأكد من إدخال جميع البيانات بشكل صحيح'
    });
    return;
  }

  // ⭐ 4️⃣ MAPPING
  const formValue = this.transferForm.value;


  const transaction: StoreTransactionDto = {
    id:null,
    sourceId: formValue.sourceId,
    destenationId: formValue.destenationId,
    sourceName: formValue.sourceName,
    destenationName: formValue.destenationName,
    makeTransactionUser:
      `${localStorage.getItem('userName')}|${localStorage.getItem('userEmail')}`,
    createdAt: null,

    transactionProducts: formValue.transactionProducts
      .filter((p: any) => p.quantity > 0)
      .map((p: any) => ({
        transactionId: null,
        productName:null,
        productId: p.productId,
        quantity: p.quantity
      }))
  };

  // 🟡 Loading
  Swal.fire({
    title: 'جاري التنفيذ...',
    text: 'برجاء الانتظار',
    allowOutsideClick: false,
    didOpen: () => {
      Swal.showLoading();
    }
  });
console.log(transaction);

  // ⬇️ 5️⃣ Call API
  this._TransactionService.addNewTransaction(transaction).subscribe({
    next: (res) => {
      Swal.fire({
        icon: 'success',
        title: 'تم بنجاح',
        text: res?.message || 'تمت عملية النقل بنجاح'
      });

      // Reset UI
      this.transferForm.reset();
      this.showTransferCard = false;
      this.getAllStocks();
      this.GetStores();
    },
    error: (err) => {
      Swal.fire({
        icon: 'error',
        title: 'فشل التنفيذ',
        text: err?.error?.message || 'حدث خطأ أثناء تنفيذ عملية النقل'
      });
       this.getAllStocks();
      this.GetStores();
    }
  });
}


   quantityNotExceedAvailable(control: AbstractControl): ValidationErrors | null {
  const qty = control.get('quantity')?.value;
  const available = control.get('availableQuantity')?.value;

  if (qty > available) {
    return { exceedAvailable: true };
  }
  return null;
}
sourceNotEqualDestination(form: AbstractControl): ValidationErrors | null {
  const sourceId = form.get('sourceId')?.value;
  const destId = form.get('destenationId')?.value;

  if (sourceId && destId && sourceId === destId) {
    return { sameStore: true };
  }
  return null;
}
}
