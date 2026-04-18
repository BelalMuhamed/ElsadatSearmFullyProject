import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../app/Services/auth-service';
import { catchError, map, of } from 'rxjs';
import { HttpClientModule } from '@angular/common/http';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  if (typeof window === 'undefined') return false; // SSR safe

  const token = localStorage.getItem('accessToken');

  if (token) {
    return true;
  }

  // حاول تجديد الـ token باستخدام refreshToken
  return authService.refreshToken().pipe(
    map(res => {
      // التجديد نجح → السماح بالوصول
      return true;
    }),
    catchError(err => {
      // التجديد فشل → تحويل للـ login
      return of(router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } }));
    })
  );
};
