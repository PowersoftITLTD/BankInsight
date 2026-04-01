import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { map, take } from 'rxjs';
import { selectToken } from '../../../store/auth/auth.selectors';

/**
 * Shared auth check
 */
const isAuthenticated = () => {
  const store = inject(Store);

  return store.select(selectToken).pipe(
    take(1),
    map(token => !!token)
  );
};

/**
 * Authenticated routes (must be logged in)
 */
export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);

  return isAuthenticated().pipe(
    map(isLoggedIn => {
      if (!isLoggedIn) {
        router.navigate(['/login'], {
          queryParams: { returnUrl: state.url }
        });
        return false;
      }
      return true;
    })
  );
};

/**
 * Non-authenticated routes (login, register, etc.)
 */
export const noAuthGuard: CanActivateFn = () => {
  const router = inject(Router);

  return isAuthenticated().pipe(
    map(isLoggedIn => {
      if (isLoggedIn) {
        router.navigate(['/dashboard']);
        return false;
      }
      return true;
    })
  );
};
