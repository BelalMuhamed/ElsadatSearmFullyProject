import { Component, inject, ViewChild } from '@angular/core';
import { TransactionService } from '../../app/Services/transaction.service';
import { Subscription } from 'rxjs';
import { StoreTransactionDto, StoreTransactionFilters, StoreTransactionProductsDto } from '../../app/models/ITransactionVM';
import { log } from 'console';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatFormField, MatLabel, MatOption, MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { HttpClientModule } from '@angular/common/http';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { CommonModule, DatePipe } from '@angular/common';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import Swal from 'sweetalert2';
import { StoreService } from '../../app/Services/store.service';
import { StoreDto, StoreFilteration } from '../../app/models/IstoreVM';
import { MatDatepickerInput, MatDatepicker, MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { TransactionProductsDialogComponent } from '../../app/Popups/transaction-products-dialog/transaction-products-dialog.component';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [MatProgressSpinner, MatColumnDef, MatHeaderCellDef, MatCellDef, MatIcon, MatHeaderRowDef, MatRowDef,
    MatTableModule,
    MatProgressSpinner,
    MatColumnDef,
    MatHeaderCellDef,
    MatCellDef,
    MatIcon,
    MatHeaderRowDef,
    MatRowDef,
    MatSelectModule,
    MatSlideToggleModule, MatFormField, MatLabel, FormsModule, MatTableModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule, HttpClientModule, MatPaginator, DatePipe, ReactiveFormsModule, MatOption, MatDatepickerInput, MatDatepicker
  ,MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    MatSlideToggleModule,
    ReactiveFormsModule,
    CommonModule],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css'
})
export class TransactionsComponent {
private _TransactionService = inject(TransactionService);
private dialog  = inject(MatDialog);

private _TransactionSubscription = new Subscription();
private _StoreService = inject(StoreService);
private _StoreSubscription = new Subscription();
transactionFilters:StoreTransactionFilters={
   sourceName: null,
    destenationName: null,
    createdAt: null,
    page: 1,
    pageSize: 10,
}
 Searchform !: FormGroup;
  private fb = inject(FormBuilder);

   columns: ColumnDef[] = [
    { key: 'sourceName', label: 'من ', type: 'text' },
    { key: 'destenationName', label: 'إلي', type: 'text' },
    { key: 'makeTransactionUser', label: 'منشئ التحويل', type: 'text' },
    { key: 'createdAt', label: 'تاريخ الإنشاء', type: 'date' },
  { key: 'actions', label: 'الكمية المحولة', type: 'actions' },

  ];
  Stores:StoreDto[]=[];
  selectedTransactionProducts: StoreTransactionProductsDto[] = [];
loadingProducts = false;
    displayedColumnKeys = this.columns.map(c => c.key);
         dataSource = new MatTableDataSource<StoreTransactionDto>([]);
         totalCount = 0;
          @ViewChild(MatPaginator) paginator!: MatPaginator;
         isLoading = true;
    ngOnInit():void
    {
    this.GetAllTransactions();
    this.GetStores();
    this.InitSearchForm();

    }
    ngOnDestroy():void
    {
      this._TransactionSubscription?.unsubscribe();
      this._StoreSubscription?.unsubscribe();
    }
viewTransactionProducts(transaction: StoreTransactionDto) {
  if (!transaction.id) return;

  this.loadingProducts = true;
  this._TransactionService.getTransactionProducts(transaction.id).subscribe({
    next: (products) => {
      this.loadingProducts = false;

      // فتح Dialog مع المنتجات
      this.dialog.open(TransactionProductsDialogComponent, {
        width: '600px',
        data: { products }
      });
    },
    error: (err) => {
      // error handled silently
      this.loadingProducts = false;
    }
  });}
     InitSearchForm()
    {
       this.Searchform = this.fb.group({
      sourceName: [''],
      destenationName: [''],
      createdAt: [''],

    });
    }
      GetAllTransactions()
        {
          this._TransactionSubscription.add(this._TransactionService.getAllTransaction(this.transactionFilters).subscribe({
            next:(res)=>{
              this.dataSource.data = res.data;
                        this.totalCount=res.totalCount;
                        this.isLoading = false;

                    },
            error:(err)=>{
                this.isLoading = false;

               Swal.fire({
                      icon: 'error',
                      title: 'خطأ',
                      text: `${err.message}`||'حدثت مشكلة أثناء تغيير الحالة!',
                    });
            }
          }))
        }
         onPageChange(event: PageEvent) {
      this.transactionFilters.page = event.pageIndex + 1;
      this.transactionFilters.pageSize = event.pageSize;
      this.GetAllTransactions();
    }

    onSearch() {
  if (this.Searchform.valid) {
    const formValues = this.Searchform.value;

    this.transactionFilters = {
      ...this.transactionFilters,
      sourceName: formValues.sourceName || null,
      destenationName: formValues.destenationName || null,
      createdAt: formValues.createdAt || null,

    };

    this.GetAllTransactions();
  }
}
ReAsign()
{
   this.transactionFilters = {
      ...this.transactionFilters,
      sourceName:  null,
      destenationName:  null,
      createdAt: null,

    };
this.InitSearchForm();
    this.GetAllTransactions();
}
GetStores()
{
const filter: StoreFilteration = {
  storeName: null,
  page: null,
  pageSize: null,
  isDeleted:null
};
  this._StoreSubscription.add(this._StoreService.getAllStores(filter).subscribe({
    next:(res)=>{
this.Stores=res.data;

    },error:(err)=>{

               Swal.fire({
                      icon: 'error',
                      title: 'خطأ',
                      text: `${err.message}`||'حدثت مشكلة أثناء تغيير الحالة!',
                    });
    }
  }))
}
}
