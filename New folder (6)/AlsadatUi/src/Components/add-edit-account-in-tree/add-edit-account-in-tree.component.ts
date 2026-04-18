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
import { TreeAccountsService } from '../../app/Services/tree-accounts.service';
import { AccountDto, FilterationAccountsDto } from '../../app/models/TreeAccountDto';
import Swal from 'sweetalert2';
import { forkJoin } from 'rxjs';

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
    MatIconModule
  ],
  templateUrl: './add-edit-account-in-tree.component.html',
  styleUrl: './add-edit-account-in-tree.component.css'
})
export class AddEditAccountInTreeComponent implements OnInit {

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(TreeAccountsService);

  form!: FormGroup;
  isEditMode = false;
  accountId!: number;
  ParentAccounts:AccountDto[] = [];
    filters:FilterationAccountsDto={
      accountCode: null,
      accountName: null,
      type: null,
      parentAccountId: null,
      isLeaf: false,
      isActive: true,
      page: null,
      pageSize: null
      }
ngOnInit(): void {
  this.initForm();

  const id = this.route.snapshot.paramMap.get('id');
  this.isEditMode = !!id;

  // 1️⃣ أولًا نحمل كل الـ ParentAccounts
  this.service.getAccounts(this.filters).subscribe(res => {
    console.log(res.data);

    this.ParentAccounts = res.data ?? [];

    if (this.isEditMode && id) {
      this.accountId = +id;

      // 2️⃣ بعد ما الـ options جاهزة، نحمل بيانات الحساب للتعديل
      this.service.getByAccountId(this.accountId).subscribe(account => {
        if (account.isSuccess && account.data) {
          this.form.patchValue({
            accountCode: account.data.accountCode,
            accountName: account.data.accountName,
            userId: account.data.userId,
            type: account.data.type,
            parentAccountId: account.data.parentAccountId ?? 0,
            isLeaf: account.data.isLeaf,
            isActive: account.data.isActive
          });
        }
      });
    } else {
      // وضع الإضافة: القيمة الافتراضية للحساب الأب بعد تحميل الخيارات
      this.form.patchValue({ parentAccountId: 0 });
    }
  });
}
onselectparent(id:any)
{
console.log(id);

}
 initForm() {
  this.form = this.fb.group({
    accountCode: ['', Validators.required],
    accountName: ['', Validators.required],
    userId: [''],
    type: [ "", Validators.required],
  parentAccountId: [0, Validators.required],
    isLeaf: [true],
    isActive: [true]
  });
}
compareFn(a: number | null, b: number | null): boolean {

  const numA = a ?? 0;
  const numB = b ?? 0;
  return numA === numB;
}
  loadAccount() {
    this.service.getByAccountId(this.accountId).subscribe({
      next: (res) => {
        console.log(res.data);
        if (res.isSuccess && res.data) {

          this.form.patchValue(res.data);
          console.log(res.data);

console.log(this.form.value.parentAccountId);
console.log(typeof this.form.value.parentAccountId);
console.log(this.form.value.parentAccountId);
        }
      }
    });
  }

  submit() {
    if (this.form.invalid) return;

    const payload = {
      ...this.form.value,
      id: this.isEditMode ? this.accountId : null
    };

    if (this.isEditMode) {
      this.service.updateAccount(payload).subscribe({
        next: (res) => {
                    console.log(res);

          if (res.isSuccess) {

            this.showSuccessAndNavigate();
          }
          else{
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
    } else {
      this.service.addAccount(payload).subscribe({
        next: (res) => {
          console.log(res.isSuccess);

          if (res.isSuccess) {
            this.showSuccessAndNavigate();
          }
           else{
             Swal.fire({
        icon: 'error',
        title: 'حدث خطأ',
        text: res.message || 'حدث خطأ غير متوقع',
        confirmButtonText: 'موافق'
      });
          }
        }
        ,
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
  }
  private showSuccessAndNavigate() {
  Swal.fire({
    icon: 'success',
    title: this.isEditMode ? 'تم التعديل بنجاح' : 'تمت الإضافة بنجاح',
    confirmButtonText: 'موافق'
  }).then(() => {
    this.router.navigate(['/tree']);
  });
}
}
