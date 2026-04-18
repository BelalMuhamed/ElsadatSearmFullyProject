import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError, of } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../Services/auth-service';

@Injectable({ providedIn: 'root' })
export class AuthInterceptor implements HttpInterceptor {

  private isRefreshing = false;

  constructor(private authService: AuthService, private router: Router) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // SSR safe
    if (typeof window === 'undefined') return next.handle(req);

    const token = this.authService.getToken();
    let authReq = req;

    if (token) {
      authReq = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }

    return next.handle(authReq).pipe(
      catchError((err: HttpErrorResponse) => {
        if (err.status === 401 && !this.isRefreshing) {
          this.isRefreshing = true;

          return this.authService.refreshToken().pipe(
            switchMap(res => {
              this.isRefreshing = false;
              const retryReq = req.clone({
                setHeaders: { Authorization: `Bearer ${res.accessToken}` }
              });
              return next.handle(retryReq);
            }),
            catchError(() => {
              this.isRefreshing = false;
              this.authService.logout();
              this.router.navigate(['/login']);
              return throwError(() => err);
            })
          );
        }
        return throwError(() => err);
      })
    );
  }
}
