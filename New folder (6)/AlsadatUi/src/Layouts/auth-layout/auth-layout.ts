import { SwalService } from './../../app/Services/swal.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../app/Services/auth-service';
import { loginDto } from '../../app/models/IAuthModels';
import { Router } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [ReactiveFormsModule, HttpClientModule],
  templateUrl: './auth-layout.html',
  styleUrl: './auth-layout.css'
})
export class AuthLayout implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;
  roles: string[] = [];
  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router,private swal: SwalService ) {
    this.loginForm = this.fb.group({
  email: ['', [Validators.required]], // username أو phone
  password: ['', [Validators.required, Validators.minLength(6)]]
});
  }

  ngOnInit(): void {
    if (typeof document !== 'undefined') {
      document.body.classList.remove('light-mode');
      document.body.classList.add('dark-mode');
    }
  }


  login() {
     if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    return;
  }

  if (this.loginForm.invalid) return;

  this.isLoading = true;

  const dto: loginDto = {
    email: this.loginForm.value.email,
    password: this.loginForm.value.password
  };

  this.authService.login(dto).subscribe({
    next: (res) => {
      console.log(res);

      this.isLoading = false;

      // 🔥 Handle Result<T>
      if (!res.isSuccess) {
        this.swal.error(res.message || 'Invalid email or password');
        return;
      }
      this.roles=res.data?.roles || [];
      // ✅ Save data
      localStorage.setItem('refreshToken', res.data?.refreshToken ?? "");
      localStorage.setItem('accessToken', res.data?.accessToken ?? "");
      localStorage.setItem('userName', res.data?.userName ?? "");
      localStorage.setItem('userEmail', res.data?.userMail ?? "");
      localStorage.setItem('roles', JSON.stringify(res.data?.roles || []));
      if(this.roles.includes('Admin'))
        this.router.navigate(['/SalesInvoices']);
      else if(this.roles.includes('HR'))
        this.router.navigate(['/hr/employees']);
      else if(this.roles.includes('Accountant'))
        this.router.navigate(['/SalesInvoices']);
      else if(this.roles.includes('StockManager'))
        this.router.navigate(['/stocks']);
      else
        this.router.navigate(['/unauthorized']);

    },

   error: (err) => {
  this.isLoading = false;

  let message = '';

  switch (err.status) {
    case 0:
      message = '🚫 لا يوجد اتصال بالسيرفر';
      break;

    case 401:
      message = '❌ بيانات الدخول غير صحيحة';
      break;

    case 500:
      message = '💥 خطأ في السيرفر';
      break;

    default:
      message = err.error?.message || 'حدث خطأ غير متوقع';
      break;
  }

  this.swal.error(message);
}
  });
}
}
