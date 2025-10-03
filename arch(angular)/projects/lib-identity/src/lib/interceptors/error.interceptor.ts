import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '../stores/auth.store';

/**
 * Handles HTTP errors globally
 * - 401 Unauthorized: Redirects to login
 * - 403 Forbidden: Redirects to access denied
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const authStore = inject(AuthStore);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Clear auth state and redirect to login
        authStore.clearSession();
        router.navigate(['/login'], {
          queryParams: { returnUrl: router.url }
        });
      } else if (error.status === 403) {
        // User is authenticated but not authorized
        router.navigate(['/access-denied']);
      }

      return throwError(() => error);
    })
  );
};
