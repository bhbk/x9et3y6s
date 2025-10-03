import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../stores/auth.store';

/**
 * Guard for guest-only routes (login, register)
 * Restores session from localStorage on reload/HMR, attempts token refresh
 * if expired, and redirects to dashboard if already authenticated.
 */
export const guestGuard: CanActivateFn = async (route) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  // Cross-SPA token transfer — let the login component handle it
  if (route.queryParams['token']) {
    return true;
  }

  // Restore in-memory state from localStorage (handles HMR / page reload)
  if (!authStore.isAuthenticated()) {
    authStore.initFromStorage();
  }

  if (authStore.isAuthenticated() && !authStore.isTokenExpired()) {
    return router.createUrlTree(['/dashboard']);
  }

  // Token missing or expired — try refreshing via httpOnly cookie
  const refreshed = await authStore.tryRefreshToken();
  if (refreshed) {
    return router.createUrlTree(['/dashboard']);
  }

  return true;
};
