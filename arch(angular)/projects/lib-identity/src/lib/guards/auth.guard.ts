import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../stores/auth.store';

/**
 * Guard that requires authentication
 * Restores session from localStorage on reload/HMR, attempts token refresh
 * if expired, and only redirects to login as a last resort.
 */
export const authGuard: CanActivateFn = async (route, state) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  // Restore in-memory state from localStorage (handles HMR / page reload)
  if (!authStore.isAuthenticated()) {
    authStore.initFromStorage();
  }

  if (authStore.isAuthenticated() && !authStore.isTokenExpired()) {
    return true;
  }

  // Token missing or expired — try refreshing via httpOnly cookie
  const refreshed = await authStore.tryRefreshToken();
  if (refreshed) {
    return true;
  }

  // All recovery attempts failed — redirect to login
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });
};
