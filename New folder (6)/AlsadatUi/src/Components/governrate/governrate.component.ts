import { Component, inject, ViewChild } from '@angular/core';
import { GovernrateService } from '../../app/Services/governrate.service';
import { Subscription } from 'rxjs';
import { GovernrateDto, GovernrateFilteration } from '../../app/models/IGovernrateVM';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { MatCellDef, MatColumnDef, MatHeaderCellDef, MatHeaderRowDef, MatRowDef, MatTableDataSource, MatTableModule } from '@angular/material/table';
import Swal from 'sweetalert2';
import { MatProgressSpinner, MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormField, MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { HttpClientModule } from '@angular/common/http';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { AddEditGovernratePopupComponent } from '../../app/Popups/add-edit-governrate-popup/add-edit-governrate-popup.component';

@Component({
  selector: 'app-governrate',
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
    MatDialogModule, HttpClientModule, MatPaginator],
  templateUrl: './governrate.component.html',
  styleUrl: './governrate.component.css'
})
export class GovernrateComponent {
 private GovernrateService = inject(GovernrateService);
  private GovernrateSubscription = new Subscription();
  filters:GovernrateFilteration={
    name:null,
    page:1,
    pageSize:10
  }
  private dialog =inject(MatDialog);
     columns: ColumnDef[] = [
      { key: 'name', label: 'اسم المحافظة ', type: 'string' },
      { key: 'actions', label: 'الإجراءات', type: 'actions' }
    ];
    displayedColumnKeys = this.columns.map(c => c.key);
      dataSource = new MatTableDataSource<GovernrateDto>([]);
      totalCount = 0;
       @ViewChild(MatPaginator) paginator!: MatPaginator;
      isLoading = false;
   ngOnInit(): void {

this.GetGovernrates();
    }
    ngOnDestroy():void{
      this.GovernrateSubscription?.unsubscribe();
    }
    GetGovernrates()
    {
        this.GovernrateSubscription.add(this.GovernrateService.getAllGovernrates(this.filters).subscribe({
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
               this.isLoading = false;

          }
        }))
    }
     onPageChange(event: PageEvent) {
            this.filters.page = event.pageIndex + 1;
            this.filters.pageSize = event.pageSize;
            this.GetGovernrates();
          }
          openAddPopup() {
  const dialogRef = this.dialog.open(AddEditGovernratePopupComponent, {
    width: '450px',
    data: null
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.GovernrateService.addGovernrate(result).subscribe({
        next:(res)=>{
        Swal.fire({
          icon: "success",
          title: "تمت الإضافة بنجاح",
          })
          this.GetGovernrates();
        },
        error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
          this.GetGovernrates();

}

      });

    }
  });
}
openEditPopup(row: GovernrateDto) {
  const dialogRef = this.dialog.open(AddEditGovernratePopupComponent, {
    width: '450px',
    data: row
  });

  dialogRef.afterClosed().subscribe(result => {
    if (result) {
      this.GovernrateService.updateGovernrate(result).subscribe({
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


          this.GetGovernrates();
        },
        error:(err)=>{
 Swal.fire({
    icon: "error",
    title: "خطأ",
    text: err?.error?.message ?? "هناك مشكلة في الخادم",
  });
          this.GetGovernrates();

}
      });
    }
  });
}
searchGov(value: string) {
  this.filters.name=value;
  this.GetGovernrates();
}

}
