import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import Swal from 'sweetalert2';

import { JournalEntryService } from '../../app/Services/journal-entry.service';
import { TreeAccountsService } from '../../app/Services/tree-accounts.service';
import { AccountDto, FilterationAccountsDto } from '../../app/models/TreeAccountDto';


import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { JournalEntriesDto } from '../../app/models/JournalEntryVM';

@Component({
  selector: 'app-add-edit-journal-entry',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './add-edit-journal-entry.component.html',
  styleUrl: './add-edit-journal-entry.component.css'
})
export class AddEditJournalEntryComponent implements OnInit, OnDestroy {

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private journalService = inject(JournalEntryService);
  private treeService = inject(TreeAccountsService);
   isConfirmMode = false;
  private sub = new Subscription();
isViewMode = false;
  isEditMode = false;
  entryId!: number;

  LeafAccounts: AccountDto[] = [];

  AccountsFilters: FilterationAccountsDto = {
    accountCode: null,
    accountName: null,
    type: null,
    parentAccountId: null,
    isLeaf: true,
    isActive: true,
    page: null,
    pageSize: null,
  };

  form!: FormGroup;

  ngOnInit(): void {
    this.initForm();
    this.loadAccounts();
    this.detectMode();
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }

  initForm() {
    this.form = this.fb.group({
      id: [0],
      entryDate: [new Date(), Validators.required],
      referenceType: [null],
      referenceNo: [null],
      desc: ['', Validators.required],
      entryDetails: this.fb.array([])
    });

    this.addDetailRow();
    this.addDetailRow();
  }

  get details(): FormArray {
    return this.form.get('entryDetails') as FormArray;
  }

  createDetailRow(): FormGroup {
    return this.fb.group({
      id: [0],
      accountId: [null, Validators.required],
     debit: [0, Validators.required],
    credit: [0, Validators.required]
    });
  }

  addDetailRow() {
    this.details.push(this.createDetailRow());
  }

  removeDetailRow(index: number) {
    this.details.removeAt(index);
  }

detectMode() {

  const url = this.router.url;
  this.entryId = Number(this.route.snapshot.paramMap.get('id'));

  if (url.includes('confirm')) {
    this.isConfirmMode = true;
    this.loadEntry();
  }

  else if (url.includes('view')) {
    this.isViewMode = true;
    this.loadEntry();
  }

  else if (this.entryId) {
    this.isEditMode = true;
    this.loadEntry();
  }
}

  loadAccounts() {
    this.sub.add(
      this.treeService.getAccounts(this.AccountsFilters).subscribe({
        next: res => this.LeafAccounts = res.data!,
        error: err => this.showError(err?.error?.message)
      })
    );
  }

  loadEntry() {
    this.sub.add(
      this.journalService.getById(this.entryId).subscribe({
        next: res => {
          const data = res.data!;
          this.form.patchValue(data!);

          this.details.clear();
          data.entryDetails.forEach((d: any) => {
            this.details.push(this.fb.group({
              id: d.id,
              accountId: d.accountId,
              debit: d.debit,
              credit: d.credit
            }));
          });
             if (this.isConfirmMode || this.isViewMode) {
  this.form.disable();
}
        },
        error: err => this.showError(err?.error?.message)
      })
    );

  }

confirmEntry() {

  Swal.fire({
    title: 'هل أنت متأكد؟',
    text: 'سيتم ترحيل القيد ولا يمكن تعديله بعد ذلك',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'نعم، تأكيد',
    cancelButtonText: 'إلغاء'
  }).then(result => {

    if (result.isConfirmed) {

      this.sub.add(
        this.journalService.postEntry(this.entryId).subscribe({
          next: () => {
            Swal.fire({
              icon: 'success',
              title: 'تم ترحيل القيد بنجاح'
            }).then(() => {
              this.router.navigate(['/journal-entries']);
            });
          },
          error: err => this.showError(err?.error?.message)
        })
      );

    }

  });

}
  get totalDebit(): number {
    return this.details.value.reduce((sum: number, x: any) => sum + Number(x.debit || 0), 0);
  }

  get totalCredit(): number {
    return this.details.value.reduce((sum: number, x: any) => sum + Number(x.credit || 0), 0);
  }

  save() {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.totalDebit !== this.totalCredit) {
      Swal.fire({
        icon: 'error',
        title: 'القيد غير متوازن',
        text: 'يجب أن يكون مجموع المدين = مجموع الدائن'
      });
      return;
    }
const formValue = this.form.value;

const entryDate = new Date(formValue.entryDate);

const formattedDate =
  entryDate.getFullYear() + '-' +
  String(entryDate.getMonth() + 1).padStart(2, '0') + '-' +
  String(entryDate.getDate()).padStart(2, '0');
    const model: JournalEntriesDto = {
  ...this.form.value,
   entryDate: formattedDate,
  entryDetails: this.form.value.entryDetails.map((x: any) => ({
    ...x,
    debit: Number(x.debit),
    credit: Number(x.credit)
  }))
};

    if (this.isEditMode) {
      this.sub.add(
        this.journalService.editEntry(model).subscribe({
          next: () => this.success(),
          error: err => this.showError(err?.error?.message)
        })
      );
    } else {
      console.log(model);

      this.sub.add(
        this.journalService.addNewEntry(model).subscribe({
          next: () => this.success(),
          error: err => {
            console.log(err);
            this.showError(err?.error?.message);
          }
        })
      );
    }
  }

  success() {
    Swal.fire({
      icon: 'success',
      title: 'تم الحفظ بنجاح'
    }).then(() => {
      this.router.navigate(['/journal-entries']);
    });
  }

  showError(msg: string) {
    Swal.fire({
      icon: 'error',
      title: 'خطأ',
      text: msg || 'حدث خطأ'
    });
  }
}
