import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { AuthStore } from '../stores/auth.store';

/**
 * Adds Authorization header with Bearer token to API requests
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const config = inject(ConfigService);
  const authStore = inject(AuthStore);

  // Skip if config not loaded (e.g., during SSR)
  if (!config.isLoaded) {
    return next(req);
  }

  // Skip auth header for STS API (uses cookies) and public endpoints
  if (config.stsApiUrl && req.url.startsWith(config.stsApiUrl)) {
    return next(req);
  }

  // Add auth header for Admin, User, and Alert APIs
  const token = authStore.accessToken();
  if (token && isProtectedUrl(req.url, config)) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req);
};

function isProtectedUrl(url: string, config: ConfigService): boolean {
  return Boolean(
    (config.adminApiUrl && url.startsWith(config.adminApiUrl)) ||
    (config.userApiUrl && url.startsWith(config.userApiUrl)) ||
    (config.alertApiUrl && url.startsWith(config.alertApiUrl))
  );
}
