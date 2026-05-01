import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
  OnInit
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';

import Swal from 'sweetalert2';

import { ProfileService } from '../../app/Services/profile.service';
import { AuthService } from '../../app/Services/auth-service';
import {
  ProfileDto,
  UpdateProfileRequest,
  UpdateProfileResponse
} from '../../app/models/IProfileModels';

/**
 * Profile page — smart container.
 *
 * Two tabs:
 *   1) "البيانات الشخصية" — update phoneNumber / email / userName
 *   2) "كلمة المرور"      — change password (requires current password)
 *
 * Tabs use independent FormGroups because the submit semantics differ:
 *   - Profile tab: identity changes; usernameChanged → force re-login
 *   - Password tab: stays logged in afterwards (user just proved they
 *     know the old password)
 *
 * Why signals here: page-level booleans (loading flags) are signals for
 * change-detection clarity; form state stays in Reactive Forms (the right
 * tool for editable forms with validation).
 */
@Component({
  selector: 'app-profile',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  private readonly profileService = inject(ProfileService);
  private readonly authService = inject(AuthService);

  // ────────────────────────────────────────────────────────────────────
  // State
  // ────────────────────────────────────────────────────────────────────
  readonly isLoading = signal<boolean>(false);
  readonly isSavingProfile = signal<boolean>(false);
  readonly isSavingPassword = signal<boolean>(false);
  readonly profile = signal<ProfileDto | null>(null);

  // Showing/hiding password fields
  readonly showOldPassword = signal<boolean>(false);
  readonly showNewPassword = signal<boolean>(false);
  readonly showConfirmPassword = signal<boolean>(false);

  // ────────────────────────────────────────────────────────────────────
  // Forms
  // ────────────────────────────────────────────────────────────────────
  readonly profileForm: FormGroup = this.fb.group({
    phoneNumber: this.fb.control<string>('', [Validators.maxLength(50)]),
    email: this.fb.control<string>('', [
      Validators.required,
      Validators.email,
      Validators.maxLength(256)
    ]),
    userName: this.fb.control<string>('', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(256)
    ])
  });

  readonly passwordForm: FormGroup = this.fb.group(
    {
      oldPassword: this.fb.control<string>('', [Validators.required]),
      newPassword: this.fb.control<string>('', [
        Validators.required,
        Validators.minLength(6)
      ]),
      confirmPassword: this.fb.control<string>('', [Validators.required])
    },
    { validators: [this.passwordsMatchValidator()] }
  );

  // ────────────────────────────────────────────────────────────────────
  // Lifecycle
  // ────────────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.loadProfile();
  }

  // ────────────────────────────────────────────────────────────────────
  // Load current profile
  // ────────────────────────────────────────────────────────────────────
  private loadProfile(): void {
    this.isLoading.set(true);

    this.profileService
      .getProfile()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isLoading.set(false);
          if (res.isSuccess && res.data) {
            this.profile.set(res.data);
            this.profileForm.patchValue({
              phoneNumber: res.data.phoneNumber ?? '',
              email: res.data.email ?? '',
              userName: res.data.userName ?? ''
            });
            this.profileForm.markAsPristine();
          } else {
            this.showError(res.message ?? 'تعذر تحميل بيانات الملف الشخصي');
          }
        },
        error: err => {
          this.isLoading.set(false);
          this.showError(err?.error?.message ?? 'تعذر تحميل بيانات الملف الشخصي');
        }
      });
  }

  // ────────────────────────────────────────────────────────────────────
  // Submit profile changes
  // ────────────────────────────────────────────────────────────────────
  async submitProfile(): Promise<void> {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      this.showError('يرجى مراجعة الحقول الموضّحة');
      return;
    }

    if (this.profileForm.pristine) {
      this.showInfo('لا توجد تغييرات لحفظها');
      return;
    }

    // Detect username change BEFORE submitting so we can warn the user.
    const currentUserName = this.profile()?.userName ?? '';
    const newUserName = (this.profileForm.value.userName ?? '').trim();
    const willChangeUsername =
      currentUserName.toLowerCase() !== newUserName.toLowerCase();

    if (willChangeUsername) {
      const confirm = await Swal.fire({
        icon: 'warning',
        title: 'تغيير اسم المستخدم',
        html:
          'تغيير اسم المستخدم سيقوم بتسجيل خروجك تلقائيًا.<br>' +
          'سيتعين عليك تسجيل الدخول مجددًا باسم المستخدم الجديد.',
        showCancelButton: true,
        confirmButtonText: 'تأكيد، نفّذ',
        cancelButtonText: 'إلغاء'
      });
      if (!confirm.isConfirmed) return;
    }

    const v = this.profileForm.value;
    const dto: UpdateProfileRequest = {
      phoneNumber: (v.phoneNumber ?? '').trim() || null,
      email: (v.email ?? '').trim(),
      userName: newUserName
    };

    this.isSavingProfile.set(true);

    this.profileService
      .updateProfile(dto)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isSavingProfile.set(false);

          if (!res.isSuccess) {
            this.showError(res.message ?? 'تعذر حفظ التغييرات');
            return;
          }

          const data = res.data as UpdateProfileResponse | null | undefined;
          const usernameChanged = data?.usernameChanged ?? false;

          if (usernameChanged) {
            // Force re-login (Turn 3 decision) — the existing JWT carries the
            // old username and will produce confusing 401s on subsequent calls.
            Swal.fire({
              icon: 'success',
              title: 'تم الحفظ',
              text: 'سيتم تسجيل خروجك الآن لإعادة الدخول باسم المستخدم الجديد.',
              timer: 2200,
              showConfirmButton: false
            }).then(() => this.authService.logout());
            return;
          }

          // Update local cached username/email if they changed (informational).
          if (data?.emailChanged && typeof window !== 'undefined') {
            localStorage.setItem('userEmail', dto.email);
          }

          this.profileForm.markAsPristine();
          this.showSuccess(res.message ?? 'تم تحديث الملف الشخصي بنجاح');

          // Refresh the displayed snapshot from the server.
          this.loadProfile();
        },
        error: err => {
          this.isSavingProfile.set(false);
          this.showError(err?.error?.message ?? 'تعذر حفظ التغييرات');
        }
      });
  }

  // ────────────────────────────────────────────────────────────────────
  // Submit password change
  // ────────────────────────────────────────────────────────────────────
  submitPassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      this.showError('يرجى مراجعة الحقول الموضّحة');
      return;
    }

    const v = this.passwordForm.value;

    this.isSavingPassword.set(true);

    this.profileService
      .changePassword({
        OldPassword: v.oldPassword,
        NewPassword: v.newPassword,
        ConfirmPassword: v.confirmPassword
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isSavingPassword.set(false);

          if (!res.isSuccess) {
            this.showError(res.message ?? 'تعذر تغيير كلمة المرور');
            return;
          }

          // Clear the password fields on success — never leave them populated.
          this.passwordForm.reset({
            oldPassword: '',
            newPassword: '',
            confirmPassword: ''
          });

          this.showSuccess(res.message ?? 'تم تغيير كلمة المرور بنجاح');
        },
        error: err => {
          this.isSavingPassword.set(false);
          this.showError(err?.error?.message ?? 'تعذر تغيير كلمة المرور');
        }
      });
  }

  // ────────────────────────────────────────────────────────────────────
  // Validators
  // ────────────────────────────────────────────────────────────────────
  private passwordsMatchValidator(): ValidatorFn {
    return (group: AbstractControl): ValidationErrors | null => {
      const newPwd = group.get('newPassword')?.value;
      const confirm = group.get('confirmPassword')?.value;
      if (newPwd && confirm && newPwd !== confirm) {
        return { passwordsMismatch: true };
      }
      return null;
    };
  }

  // ────────────────────────────────────────────────────────────────────
  // SweetAlert helpers
  // ────────────────────────────────────────────────────────────────────
  private showSuccess(text: string): void {
    Swal.fire({ icon: 'success', title: 'تم', text, timer: 2000, showConfirmButton: false });
  }

  private showError(text: string): void {
    Swal.fire({ icon: 'error', title: 'خطأ', text });
  }

  private showInfo(text: string): void {
    Swal.fire({ icon: 'info', title: 'تنبيه', text, timer: 1800, showConfirmButton: false });
  }

  // ────────────────────────────────────────────────────────────────────
  // Template helpers — control accessors for the template
  // ────────────────────────────────────────────────────────────────────
  get phoneNumberCtrl(): AbstractControl { return this.profileForm.get('phoneNumber')!; }
  get emailCtrl(): AbstractControl { return this.profileForm.get('email')!; }
  get userNameCtrl(): AbstractControl { return this.profileForm.get('userName')!; }

  get oldPasswordCtrl(): AbstractControl { return this.passwordForm.get('oldPassword')!; }
  get newPasswordCtrl(): AbstractControl { return this.passwordForm.get('newPassword')!; }
  get confirmPasswordCtrl(): AbstractControl { return this.passwordForm.get('confirmPassword')!; }

  togglePasswordVisibility(field: 'old' | 'new' | 'confirm'): void {
    if (field === 'old') this.showOldPassword.update(v => !v);
    else if (field === 'new') this.showNewPassword.update(v => !v);
    else this.showConfirmPassword.update(v => !v);
  }
}
