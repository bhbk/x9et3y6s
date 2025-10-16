import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../stores/auth.store';

/**
 * Guard that requires specific role(s)
 * Use route data to specify required roles: { data: { roles: ['Admin'] } }
 */
export const roleGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  // First check authentication
  if (!authStore.isAuthenticated() || authStore.isTokenExpired()) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url }
    });
  }

  // Check required roles from route data
  const requiredRoles = route.data['roles'] as string[] | undefined;
  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  // Check if user has any of the required roles
  const hasRole = authStore.hasRole();
  const hasRequiredRole = requiredRoles.some(role => hasRole(role));

  if (hasRequiredRole) {
    return true;
  }

  // User doesn't have required role
  return router.createUrlTree(['/access-denied']);
};
