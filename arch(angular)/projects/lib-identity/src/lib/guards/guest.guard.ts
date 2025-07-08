import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../stores/auth.store';

/**
 * Guard for guest-only routes (login, register)
 * Redirects to dashboard if already authenticated
 */
export const guestGuard: CanActivateFn = () => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  if (authStore.isAuthenticated() && !authStore.isTokenExpired()) {
    // Already logged in, redirect to dashboard
    return router.createUrlTree(['/dashboard']);
  }

  return true;
};
