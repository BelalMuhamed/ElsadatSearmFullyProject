import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../app/Services/auth-service';

export const roleGuard: CanActivateFn = (route, state) => {

  const router = inject(Router);

  const userRoles: string[] = JSON.parse(localStorage.getItem('roles') || '[]');
  const allowedRoles = route.data?.['roles'] as string[] | undefined;

  if (!allowedRoles || allowedRoles.length === 0)
    return true;

  const hasAccess = userRoles.some((r: string) => allowedRoles.includes(r));

  if (!hasAccess) {
  return router.createUrlTree(['/unauthorized']);
  }

  return true;
};
