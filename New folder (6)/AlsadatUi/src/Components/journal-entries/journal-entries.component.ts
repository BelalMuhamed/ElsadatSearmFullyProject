import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Subscription } from 'rxjs';
import { JournalEntryService } from '../../app/Services/journal-entry.service';
import { ColumnDef } from '../../Layouts/generic-table-component/generic-table-component';
import { JournalEntriesDto, JournalEntryFilterationReq } from '../../app/models/JournalEntryVM';
import Swal from 'sweetalert2';
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-journal-entries',
  standalone: true,
  imports: [
    CommonModule, MatTableModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatInputModule,
    MatButtonModule,
    FormsModule,
    MatIconModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule, MatNativeDateModule, HttpClientModule,
    MatSelectModule,
    RouterLink
],
  templateUrl: './journal-entries.component.html',
  styleUrl: './journal-entries.component.css'
})
export class JournalEntriesComponent {
  private _journalEntryService = inject(JournalEntryService);
  private _journalEntrySubscription = new Subscription();
    @ViewChild(MatPaginator) paginator!: MatPaginator;
     isLoaded = false;
       columns: ColumnDef[] = [
      { key: "id", label: "رقم القيد" },
      { key: "entryDate", label: "تاريخ الإنشاء ",type:"date" },
      { key: "referenceType", label: "نوع المرجع",type:"referenceType" },
      { key: "referenceNo",  label: "رقم المرجع " },
      { key: "desc",  label: "الوصف " },
      { key: "isPosted", label: "حالة الترحيل  ",type:"boolean" },
      { key: "postedDate", label: "تاريخ الترحيل ",type:"date" },
      { key: "actions", label: "الإجراءات", type: "actions" },
    ]

    displayedColumnKeys = this.columns.map((c) => c.key);
    totalCount = 0;
    dataSource = new MatTableDataSource<JournalEntriesDto>([]);
    private fb = inject(FormBuilder);
    form!: FormGroup;
    filters:JournalEntryFilterationReq=
    {
      entryDate:null,
      referenceType:null,
      referenceNo:null,
      isPosted:null,
      postedDate:null,
      page:1,
      pageSize:10

    }
    onDelete(id: number) {

  Swal.fire({
    title: 'هل أنت متأكد؟',
    text: 'سيتم حذف القيد نهائياً!',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'نعم، حذف',
    cancelButtonText: 'إلغاء',
    confirmButtonColor: '#d33'
  }).then(result => {

    if (result.isConfirmed) {

      this._journalEntrySubscription.add(
        this._journalEntryService.deleteEntry(id).subscribe({
          next: () => {

            Swal.fire({
              icon: 'success',
              title: 'تم الحذف بنجاح'
            });

            this.GetJournalEntries(); // إعادة تحميل الجدول
          },
          error: (err) => {
            console.log(err);

            Swal.fire({
              icon: 'error',
              title: 'خطأ',
              text: err?.error?.message || 'حدث خطأ أثناء الحذف'
            });
          }
        })
      );

    }

  });

}
    GetJournalEntries()
    {
      this._journalEntrySubscription.add(this._journalEntryService.getAllJournalEntries(this.filters).subscribe({
        next:(res)=>{
            this.isLoaded = true
            this.dataSource.data = res.data?.data!;
            console.log(res.data);

            this.totalCount = res.data?.totalCount!;
        },
        error:(err)=>{
             this.isLoaded = true
        Swal.fire({
        icon: "error",
        title: "حدث خطأ أثناء تحميل البيانات !    ",
        text: `${err?.error?.message}`,
        confirmButtonText: "موافق",
        confirmButtonColor: "#d33",
        })
        }
      }))
    }
    ngOnInit(): void
    {
    this.GetJournalEntries();
    this.InitSearchForm();
    }
    ngOnDestroy():void
    {
      this._journalEntrySubscription?.unsubscribe();
    }
    InitSearchForm()
  {
    this.form = this.fb.group({
       entryDate: [""],
      referenceType: [null],
      referenceNo: [""],
      isPosted: [null],

    })
  }
   resetFilters(){
    this.filters = {
      ...this.filters,
      entryDate:null,
      referenceType:null,
      referenceNo:null,
      isPosted:null,
      postedDate:null,
      page:1,
      pageSize:10
 };
    this.InitSearchForm();
    this.GetJournalEntries();
  }
 onSearch() {
  console.log(this.form.value);

  if (this.form.valid) {
    const formValues = this.form.value;

    let isoDate: string | null = null;

    if (formValues.entryDate) {
      const date = new Date(formValues.entryDate);
      date.setHours(12); // نتجنب مشكلة الـ timezone
      isoDate = date.toISOString();
    }

    this.filters = {
      ...this.filters,
      entryDate: isoDate,
      referenceType: formValues.referenceType || null,
      referenceNo: formValues.referenceNo || null,
      isPosted: formValues.isPosted
    };

    console.log(this.filters);
    this.GetJournalEntries();
  }
}
 onPageChange(event: PageEvent): void {
      this.filters.page = event.pageIndex + 1
      this.filters.pageSize = event.pageSize
      this.GetJournalEntries()
  }
}
