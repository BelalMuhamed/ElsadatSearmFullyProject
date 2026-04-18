import { Component, inject, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TreeAccountsService } from '../../app/Services/tree-accounts.service';
import { AccountDetailsDto, AccountMovementDto, AccountDetailsDtoReq } from '../../app/models/TreeAccountDto';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';
import { MatProgressSpinner } from "@angular/material/progress-spinner";

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatPaginatorModule,
    MatTableModule,
    MatProgressSpinner
],
  templateUrl: './account-details.component.html',
  styleUrls: ['./account-details.component.css']
})
export class AccountDetailsComponent {
  private route = inject(ActivatedRoute);
  private treeAccountsService = inject(TreeAccountsService);
  private fb = inject(FormBuilder);

  private _accountSubscription = new Subscription();

  account: AccountDetailsDto | null = null;
  movements: AccountMovementDto[] = [];

  isLoaded = false;
  msgerror: string | null = null;

  accountId!: number;

  page = 1;
  pageSize = 10;
  totalCount = 0;

  dataSource = new MatTableDataSource<AccountMovementDto>([]);

  form: FormGroup = this.fb.group({
    entryId: [null],
    entryDate: [null]
  });

  @ViewChild(MatPaginator) paginator!: MatPaginator;

  columns = [
    { key: 'entryId', label: 'رقم القيد' },
    { key: 'entryDate', label: 'تاريخ القيد', type: 'date' },
    { key: 'description', label: 'الوصف' },
    { key: 'debit', label: 'مدين' },
    { key: 'credit', label: 'دائن' },
    { key: 'runningBalance', label: 'الرصيد' }
  ];
  displayedColumnKeys = this.columns.map(c => c.key);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.msgerror = 'لم يتم تحديد الحساب.';
      this.isLoaded = true;
      return;
    }
    this.accountId = +id;
    this.loadAccountDetails();
  }

  ngOnDestroy(): void {
    this._accountSubscription.unsubscribe();
  }

  loadAccountDetails(): void {
    this.isLoaded = false;
    const formValue = this.form.value;

    const req: AccountDetailsDtoReq = {
      accountId: this.accountId,
      page: this.page,
      pageSize: this.pageSize,
      entryId: formValue.entryId && formValue.entryId > 0 ? formValue.entryId : null,
      entryDate: formValue.entryDate ? formValue.entryDate : null
    };

    this._accountSubscription.add(
      this.treeAccountsService.getAccountJournalEntryDetails(req).subscribe({
        next: res => {
          this.isLoaded = true;
          if (res.isSuccess && res.data) {
            this.account = res.data;
            this.movements = res.data.movements.data;
            this.totalCount = res.data.movements.totalCount;
            this.dataSource.data = this.movements;
          } else {
            this.msgerror = res.message || 'فشل في تحميل تفاصيل الحساب.';
          }
        },
        error: err => {
          this.isLoaded = true;
          this.msgerror = err?.error?.message??  'حدث خطأ أثناء تحميل تفاصيل الحساب.';
        Swal.fire({
  icon: 'error',
  title: 'خطأ',
  text: err?.error?.message ?? 'حدث خطأ غير متوقع أثناء تحميل تفاصيل الحساب.',
  confirmButtonText: 'موافق',
  confirmButtonColor: '#d33'
});
        }
      })
    );
  }

  applyFilters() {
    this.page = 1;
    this.loadAccountDetails();
  }

  resetFilters() {
    this.form.reset();
    this.page = 1;
    this.loadAccountDetails();
  }

  onPageChange(event: PageEvent) {
    this.page = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadAccountDetails();
  }
}
