import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../stores/auth.store';

export const entitlementGuard: CanActivateFn = (route, state) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  if (!authStore.isAuthenticated() || authStore.isTokenExpired()) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url }
    });
  }

  const requiredEntitlements = route.data['entitlements'] as string[] | undefined;
  if (!requiredEntitlements || requiredEntitlements.length === 0) {
    return true;
  }

  /* allow through when entitlements not yet loaded — backend enforces */
  const user = authStore.user();
  if (!user?.entitlements?.length) {
    return true;
  }

  const hasEntitlement = authStore.hasEntitlement();
  const hasRequired = requiredEntitlements.some(ent => hasEntitlement(ent));

  if (hasRequired) {
    return true;
  }

  return router.createUrlTree(['/access-denied']);
};
