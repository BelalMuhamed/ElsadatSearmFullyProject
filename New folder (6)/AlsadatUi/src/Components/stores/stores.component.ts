import { Component, inject, ViewChild } from '@angular/core';
import { StoreService } from '../../app/Services/store.service';
import { Subscription } from 'rxjs';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatFormField, MatLabel, MatOption, MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { HttpClientModule } from '@angular/common/http';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { DatePipe } from '@angular/common';
import { StoreDto, StoreFilteration } from '../../app/models/IstoreVM';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import Swal from 'sweetalert2';
import { AddEditStorePopupComponent } from '../../app/Popups/add-edit-store-popup/add-edit-store-popup.component';
import { AddEditCityPopupComponent } from '../../app/Popups/add-edit-city-popup/add-edit-city-popup.component';
import { DeleteStorePopupComponent } from '../../app/Popups/delete-store-popup/delete-store-popup.component';

@Component({
  selector: 'app-stores',
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
      MatDialogModule, HttpClientModule, MatPaginator, ReactiveFormsModule],
  templateUrl: './stores.component.html',
  styleUrl: './stores.component.css'
})
export class StoresComponent {
        private dialog =inject(MatDialog);

  Searchform !: FormGroup;
    private fb = inject(FormBuilder);
private _StoreService = inject(StoreService);
private _StoreSubscription = new Subscription();
filters:StoreFilteration={
    page:1,
    pageSize:10,
    storeName:null,
    isDeleted:null
  }
   columns: ColumnDef[] = [
    { key: 'storeName', label: 'اسم المخزن ', type: 'text' },
    { key: 'isDeleted', label: ' الحالة ', type: 'boolean' },

    { key: 'actions', label: 'الإجراءات', type: 'actions' },
  ];
   displayedColumnKeys = this.columns.map(c => c.key);
         dataSource = new MatTableDataSource<StoreDto>([]);
         totalCount = 0;
          @ViewChild(MatPaginator) paginator!: MatPaginator;
         isLoading = true;
    ngOnInit():void
    {
      this.GetAllStores();
      this.InitSearchForm();

    }
    ngOnDestroy():void
    {

    }
 GetAllStores()
    {
      this._StoreSubscription.add(this._StoreService.getAllStores(this.filters).subscribe({
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
                  text: `${err.error.message}`||'حدثت مشكلة أثناء الاتصال بقاعدة البايانات!',
                });
        }
      }))
    }
    InitSearchForm()
    {
       this.Searchform = this.fb.group({
      storeName: [''],

    });
    }
    onSearch() {
  if (this.Searchform.valid) {
    const formValues = this.Searchform.value;

    this.filters = {
      ...this.filters,
      storeName: formValues.storeName || null,

    };

    this.GetAllStores();
  }
}
ReAsign()
{
   this.filters = {
      ...this.filters,
      storeName:  null,

    };
this.InitSearchForm();
    this.GetAllStores();
}
onPageChange(event: PageEvent) {
      this.filters.page = event.pageIndex + 1;
      this.filters.pageSize = event.pageSize;
      this.GetAllStores();
    }

OnToggleChanged(dto: StoreDto, checked: boolean) {
  if (dto.isDeleted) {
    this.UnDeleteStore(dto);

  } else {
   
    this.DeleteStore(dto, checked);
  }
}
UnDeleteStore(dto: StoreDto)
{
   dto.isDeleted=false;
    this._StoreService.editStore(dto).subscribe({
      next: (res) => {


        Swal.fire({
          icon: 'success',
          title: 'تم التحديث',
          text: res?.message || 'تم تعديل المخزن بنجاح.',
        });


      },
      error: (err) => {

        Swal.fire({
          icon: 'error',
          title: 'خطأ',
          text: err?.error?.message || 'حدثت مشكلة أثناء تعديل المخزن!',
        });


      }
    });
}

    DeleteStore(dto: StoreDto, checked: boolean) {
  const ref = this.dialog.open(DeleteStorePopupComponent, { data: dto, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => { if (saved) this.GetAllStores(); });
    }
      openAdd(): void {
    const ref = this.dialog.open(AddEditStorePopupComponent, { data: null, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => { if (saved) this.GetAllStores(); });
  }

  openEdit(store: StoreDto): void {
    const ref = this.dialog.open(AddEditStorePopupComponent, { data: store, width: '420px' });
    ref.afterClosed().subscribe((saved: any) => { if (saved) this.GetAllStores(); });
  }

}
