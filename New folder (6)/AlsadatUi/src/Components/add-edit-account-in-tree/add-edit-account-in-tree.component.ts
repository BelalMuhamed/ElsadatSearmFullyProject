import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TreeAccountsService } from '../../app/Services/tree-accounts.service';
import {
  AccountDto,
  CreateAccountDto,
  FilterationAccountsDto
} from '../../app/models/TreeAccountDto';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-add-edit-account-in-tree',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatButtonModule,
    MatSelectModule,
    MatIconModule,
    MatTooltipModule
  ],
  templateUrl: './add-edit-account-in-tree.component.html',
  styleUrl: './add-edit-account-in-tree.component.css'
})
export class AddEditAccountInTreeComponent implements OnInit {

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
public readonly router = inject(Router);
  private service = inject(TreeAccountsService);

  form!: FormGroup;
  isEditMode = false;
  accountId!: number;
  ParentAccounts: AccountDto[] = [];

  // Loaded record (Edit mode only) — used for non-editable display fields like accountCode + type.
  private loadedAccount: AccountDto | null = null;

  filters: FilterationAccountsDto = {
    accountCode: null,
    accountName: null,
    type: null,
    parentAccountId: null,
    isLeaf: false,
    isActive: true,
    page: null,
    pageSize: null
  };

  ngOnInit(): void {
    this.initForm();

    const id = this.route.snapshot.paramMap.get('id');
    this.isEditMode = !!id;

    this.service.getAccounts(this.filters).subscribe(res => {
      this.ParentAccounts = res.data ?? [];

      if (this.isEditMode && id) {
        this.accountId = +id;
        this.service.getByAccountId(this.accountId).subscribe(account => {
          if (account.isSuccess && account.data) {
            this.loadedAccount = account.data;
            this.form.patchValue({
              accountCode:     account.data.accountCode,         // shown disabled
              accountName:     account.data.accountName,
              userId:          account.data.userId,
              type:            account.data.type,
              parentAccountId: account.data.parentAccountId ?? 0,
              isLeaf:          account.data.isLeaf,
              isActive:        account.data.isActive
            });
          }
        });
      } else {
        this.form.patchValue({ parentAccountId: 0 });
      }
    });
  }

  /**
   * Form schema:
   *  - `accountCode` is purely for display in Edit mode. It is created as a
   *     DISABLED control so it never gets submitted by `form.value`. On Create
   *     mode the field is also disabled (and hidden in the template).
   *  - `type` is included for display in Edit mode but is also disabled — it's
   *     inherited from the parent server-side and must not be editable.
   */
  private initForm(): void {
    this.form = this.fb.group({
      accountCode:     [{ value: '', disabled: true }],     // always disabled — never submitted
      accountName:     ['', [Validators.required, Validators.maxLength(200)]],
      userId:          [''],
      type:            [{ value: '', disabled: true }],     // inherited from parent server-side
      parentAccountId: [0, Validators.required],
      isLeaf:          [true],
      isActive:        [true]
    });
  }

  compareFn(a: number | null, b: number | null): boolean {
    return (a ?? 0) === (b ?? 0);
  }

  /**
   * Build the typed payload for create/update, excluding any field that must
   * never leave the client (accountCode in particular).
   * `getRawValue()` is needed because disabled controls are skipped by `value`.
   */
  private buildPayload(): CreateAccountDto | AccountDto {
    const raw = this.form.getRawValue();

    if (this.isEditMode) {
      // PUT — keep id + (read-only) accountCode for traceability, but server will ignore code.
      const editPayload: AccountDto = {
        id:              this.accountId,
        accountCode:     raw.accountCode,
        userId:          raw.userId || null,
        accountName:     raw.accountName,
        type:            raw.type,
        parentAccountId: raw.parentAccountId,
        isLeaf:          raw.isLeaf,
        isActive:        raw.isActive
      };
      return editPayload;
    }

    // POST — strictly the create contract: NO id, NO accountCode, NO type.
    const createPayload: CreateAccountDto = {
      userId:          raw.userId || null,
      accountName:     raw.accountName,
      parentAccountId: raw.parentAccountId,
      isLeaf:          raw.isLeaf,
      isActive:        raw.isActive
    };
    return createPayload;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();

    const request$ = this.isEditMode
      ? this.service.updateAccount(payload as AccountDto)
      : this.service.addAccount(payload as CreateAccountDto);

    request$.subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.showSuccessAndNavigate();
        } else {
          Swal.fire({
            icon: 'error',
            title: 'حدث خطأ',
            text: res.message || 'حدث خطأ غير متوقع',
            confirmButtonText: 'موافق'
          });
        }
      },
      error: (err) => {
        Swal.fire({
          icon: 'error',
          title: 'حدث خطأ',
          text: err?.error?.message || 'حدث خطأ غير متوقع',
          confirmButtonText: 'موافق'
        });
      }
    });
  }

  private showSuccessAndNavigate(): void {
    Swal.fire({
      icon: 'success',
      title: this.isEditMode ? 'تم التعديل بنجاح' : 'تمت الإضافة بنجاح',
      confirmButtonText: 'موافق'
    }).then(() => {
      this.router.navigate(['/tree']);
    });
  }
}
