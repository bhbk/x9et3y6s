import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ConfigService } from '../services/config.service';

/**
 * Adds withCredentials to STS API requests for httpOnly cookie support
 */
export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const config = inject(ConfigService);

  // Skip if config not loaded (e.g., during SSR)
  if (!config.isLoaded) {
    return next(req);
  }

  // Only add credentials for STS API (oauth2 endpoints)
  if (config.stsApiUrl && req.url.startsWith(config.stsApiUrl)) {
    req = req.clone({
      withCredentials: true
    });
  }

  return next(req);
};
