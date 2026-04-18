import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatOption } from '@angular/material/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { SupplierDto, SupplierFilteration } from '../../app/models/ISupplierModels';
import { Observable, Subscription } from 'rxjs';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import Swal from 'sweetalert2';
import { SupplierService } from '../../app/Services/supplier.service';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-supplier',
  standalone: true,
  imports: [
    MatTableModule,
    MatIconModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    FormsModule,
    HttpClientModule,
    MatPaginator,
    ReactiveFormsModule,
    MatSelectModule,
    CommonModule,
    RouterLink
],
  templateUrl: './supplier.component.html',
  styleUrl: './supplier.component.css'
})
export class SupplierComponent {
private supplierService = inject(SupplierService)
  private supplierSubscription = new Subscription()

  filters: SupplierFilteration = {
    name: null,
    phoneNumbers: null,
    page: 1,
    pageSize: 10,
  }

  @ViewChild(MatPaginator) paginator!: MatPaginator

  isLoaded = false

  columns: ColumnDef[] = [
    { key: "name", label: "اسم المورد" },
    { key: "phoneNumbers", label: "رقم الهاتف" },
    { key: "address", label: "العنوان" },
    { key: "cityName", label: "المدينة" },
    { key: "isDeleted", label: "فعال", type: "boolean" },
    { key: "actions", label: "الإجراءات", type: "actions" },
  ]

  displayedColumnKeys = this.columns.map((c) => c.key)
  totalCount = 0
  dataSource = new MatTableDataSource<SupplierDto>([])

  private dialog = inject(MatDialog)
  private fb = inject(FormBuilder)
  form!: FormGroup

  ngOnInit(): void {
    this.GetAllSuppliers()
    this.initForm()
  }

  ngOnDestroy(): void {
    this.supplierSubscription?.unsubscribe()
  }

  GetAllSuppliers(): void {
    this.supplierSubscription.add(
      this.supplierService.getAllSuppliers(this.filters).subscribe({
        next: (res: any) => {
          this.isLoaded = true
          this.dataSource.data = res.data
          this.totalCount = res.totalCount
        },
        error: (err: any) => {
          Swal.fire({
            icon: "error",
            title: "حدث خطأ",
            text: `${err.message}`,
            confirmButtonText: "موافق",
            confirmButtonColor: "#d33",
          })
          this.isLoaded = true
        },
      }),
    )
  }

  ToggleSupplierStatus(dto: SupplierDto, checked: boolean): void {
    dto.isDeleted = !checked
    this.supplierService.toggleStatus(dto).subscribe({
      next: () => {
        Swal.fire({
          icon: "success",
          title: "تم التحديث",
          text: "تم تحديث حالة المورد بنجاح",
          confirmButtonText: "موافق",
        })
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
    })
  }

  onPageChange(event: PageEvent): void {
    this.filters.page = event.pageIndex + 1
    this.filters.pageSize = event.pageSize
    this.GetAllSuppliers()
  }

  openAddEditPopup(supplier?: SupplierDto): void {
    // This will be implemented when you have the popup component
    // Opening dialog for supplier
  }

  initForm(): void {
    this.form = this.fb.group({
      name: [""],
      phoneNumbers: [""],
    })
  }

  applyFilters(): void {
    const formValues = this.form.value
    this.filters = {
      ...this.filters,
      name: formValues.name || null,
      phoneNumbers: formValues.phoneNumbers || null,
      page: 1,
      pageSize: 10,
    }
    this.GetAllSuppliers()
  }

  resetFilters(): void {
    this.filters = {
      name: null,
      phoneNumbers: null,
      page: 1,
      pageSize: 10,
    }
    this.initForm()
    this.GetAllSuppliers()
  }
}
