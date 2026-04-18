import { HttpClientModule } from '@angular/common/http';
import { Component, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableDataSource, MatTableModule } from '@angular/material/table';
import { GovernrateService } from '../../app/Services/governrate.service';
import { Subscription } from 'rxjs';
import { GovernrateDto, GovernrateFilteration } from '../../app/models/IGovernrateVM';
import { CityServiceService } from '../../app/Services/city-service.service';
import Swal from 'sweetalert2';
import { ICityDto, ICityFilteration } from '../../app/models/Icity';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { AddEditGovernratePopupComponent } from '../../app/Popups/add-edit-governrate-popup/add-edit-governrate-popup.component';
import { AddEditCityPopupComponent } from '../../app/Popups/add-edit-city-popup/add-edit-city-popup.component';

@Component({
  selector: 'app-city',
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
    MatSlideToggleModule, MatFormField, MatLabel, FormsModule, MatTableModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule, HttpClientModule, MatPaginator, ReactiveFormsModule],
  templateUrl: './city.component.html',
  styleUrl: './city.component.css'
})
export class CityComponent {
    cityForm!: FormGroup;


  private CityService = inject(CityServiceService);
  private CitySubscription = new Subscription();
    private dialog =inject(MatDialog);
private fb= inject(FormBuilder);
  columns: ColumnDef[] = [
       { key: 'cityName', label: 'اسم المدينة ', type: 'string' },
       { key: 'governrateName', label: 'اسم المدينة ', type: 'string' },
       { key: 'actions', label: 'الإجراءات', type: 'actions' }
     ];
     displayedColumnKeys = this.columns.map(c => c.key);
       dataSource = new MatTableDataSource<ICityDto>([]);
       totalCount = 0;
        @ViewChild(MatPaginator) paginator!: MatPaginator;
       isLoading = true;
  cityFilters:ICityFilteration={
  page:1,
  pageSize:10,
  cityName:null,
  governrateName:null
    }

   ngOnInit(): void
   {
  this.GetAllCities();
  this.InitCitySearchForm();

   }
   ngOnDestroy():void
   {
    this.CitySubscription?.unsubscribe();
   }
   GetAllCities()
   {
    this.CitySubscription.add(this.CityService.getAllCities(this.cityFilters).subscribe({
      next:(res)=>{
          this.dataSource.data = res.data;
            this.totalCount=res.totalCount;
            this.isLoading = false;
      },
      error:(err)=>{
           Swal.fire({
                                   icon: 'error',
                                   title: 'حدث خطأ',
                                   text: `${err.message}`,
                                   confirmButtonText: 'موافق',
                                   confirmButtonColor: '#d33'
                                 });
      }
    }))
   }

onPageChange(event: PageEvent) {
  this.cityFilters.page = event.pageIndex + 1;
  this.cityFilters.pageSize = event.pageSize;
  this.GetAllCities();
}
          openAddPopup() {
  const dialogRef = this.dialog.open(AddEditCityPopupComponent, {
    width: '450px',
    data: null
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.CityService.addCity(result).subscribe({
        next:(res)=>{
        Swal.fire({
          icon: "success",
          title: "تمت الإضافة بنجاح",
          })
          this.GetAllCities();
        },
        error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
   this.GetAllCities();


}

      });

    }
  });
}
openEditPopup(row: ICityDto) {
  const dialogRef = this.dialog.open(AddEditCityPopupComponent, {
    width: '450px',
    data: row
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.CityService.updateCity(result).subscribe({
         next:(res)=>{
Swal.fire({
  icon: "success",
  title: "تم التعديل بنجاح",
  html: "<p class='my-swal-text'>تم حفظ البيانات.</p>",
  customClass: {
    popup: 'my-swal-popup',
    title: 'my-swal-title',
    confirmButton: 'my-swal-btn',
  }
});


          this.GetAllCities();
        },
        error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
          this.GetAllCities();

}
      });
    }
  });
}
InitCitySearchForm()
{
  this.cityForm = this.fb.group({
      cityName: [null],
      governrateName: [null]
    });
}
 submitSearchForm() {
    if (this.cityForm.valid) {
     this.cityFilters.cityName=this.cityForm.value.cityName;
     this.cityFilters.governrateName=this.cityForm.value.governrateName;
     this.GetAllCities();
    } else {
      this.cityForm.markAllAsTouched();
    }
  }
}
