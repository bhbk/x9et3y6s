import { computed, inject } from '@angular/core';
import { signalStore, withState, withComputed, withMethods, patchState } from '@ngrx/signals';
import { rxMethod } from '@ngrx/signals/rxjs-interop';
import { pipe, switchMap, tap, catchError, of, filter, interval, takeUntil, Subject } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AuthState, AuthUser, JwtPayload, UserJwtV2 } from '../models';

const initialState: AuthState = {
  isAuthenticated: false,
  accessToken: null,
  tokenExpiry: null,
  user: null,
  isLoading: false,
  error: null
};

const TOKEN_STORAGE_KEY = 'identity_access_token';
const REMEMBER_ME_KEY = 'identity_remember_me';
const TOKEN_REFRESH_THRESHOLD_MS = 60000; // Refresh 1 minute before expiry

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withComputed((store) => ({
    isTokenExpired: computed(() => {
      const expiry = store.tokenExpiry();
      if (!expiry) return true;
      return new Date() >= expiry;
    }),
    shouldRefreshToken: computed(() => {
      const expiry = store.tokenExpiry();
      if (!expiry) return false;
      const now = new Date().getTime();
      const expiryTime = expiry.getTime();
      return (expiryTime - now) < TOKEN_REFRESH_THRESHOLD_MS && (expiryTime - now) > 0;
    }),
    userDisplayName: computed(() => {
      const user = store.user();
      if (!user) return null;
      if (user.name) return user.name;
      if (user.firstName && user.lastName) return `${user.firstName} ${user.lastName}`;
      return user.email || user.id;
    }),
    hasRole: computed(() => (role: string) => {
      const user = store.user();
      return user?.roles?.includes(role) ?? false;
    })
  })),
  withMethods((store, authService = inject(AuthService)) => {
    const refreshSubject = new Subject<void>();

    // Parse JWT payload
    const parseJwt = (token: string): JwtPayload | null => {
      try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(
          atob(base64)
            .split('')
            .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
            .join('')
        );
        return JSON.parse(jsonPayload);
      } catch {
        return null;
      }
    };

    // Extract roles from JWT payload (.NET uses various claim names)
    const extractRoles = (payload: JwtPayload): string[] => {
      const raw = payload.roles
        ?? payload['role']
        ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
      if (!raw) return [];
      return Array.isArray(raw) ? raw as string[] : [raw as string];
    };

    // Extract user info from JWT
    const extractUser = (jwt: UserJwtV2): AuthUser | null => {
      const payload = parseJwt(jwt.access_token);
      if (!payload) return null;

      return {
        id: payload.sub,
        issuer: jwt.issuer,
        clients: jwt.client || [],
        email: payload.email,
        name: payload.name,
        firstName: payload.given_name,
        lastName: payload.family_name,
        roles: extractRoles(payload)
      };
    };

    // Check if rememberMe was previously selected
    const isRememberMe = (): boolean => {
      if (typeof window === 'undefined') return false;
      return localStorage.getItem(REMEMBER_ME_KEY) === 'true';
    };

    // Save token and rememberMe preference to localStorage
    const saveToken = (jwt: UserJwtV2, rememberMe?: boolean) => {
      if (typeof window === 'undefined') return;

      if (rememberMe !== undefined) {
        if (rememberMe) {
          localStorage.setItem(REMEMBER_ME_KEY, 'true');
        } else {
          localStorage.removeItem(REMEMBER_ME_KEY);
        }
      }

      localStorage.setItem(TOKEN_STORAGE_KEY, JSON.stringify({
        accessToken: jwt.access_token,
        expiresIn: jwt.expires_in,
        savedAt: new Date().toISOString()
      }));
    };

    // Load token from localStorage
    const loadStoredToken = (): { accessToken: string; expiry: Date } | null => {
      if (typeof window === 'undefined') return null;

      const stored = localStorage.getItem(TOKEN_STORAGE_KEY);
      if (!stored) return null;

      try {
        const data = JSON.parse(stored);
        const savedAt = new Date(data.savedAt);
        const expiry = new Date(savedAt.getTime() + data.expiresIn * 1000);

        if (expiry <= new Date()) {
          localStorage.removeItem(TOKEN_STORAGE_KEY);
          return null;
        }

        return { accessToken: data.accessToken, expiry };
      } catch {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
        return null;
      }
    };

    // Clear stored token and rememberMe preference
    const clearToken = () => {
      if (typeof window !== 'undefined') {
        localStorage.removeItem(TOKEN_STORAGE_KEY);
        localStorage.removeItem(REMEMBER_ME_KEY);
      }
    };

    // Handle successful authentication
    const handleAuthSuccess = (jwt: UserJwtV2, rememberMe?: boolean) => {
      const user = extractUser(jwt);
      const expiry = new Date(Date.now() + jwt.expires_in * 1000);

      saveToken(jwt, rememberMe);

      patchState(store, {
        isAuthenticated: true,
        accessToken: jwt.access_token,
        tokenExpiry: expiry,
        user,
        isLoading: false,
        error: null
      });
    };

    // Handle authentication error
    const handleAuthError = (error: unknown) => {
      clearToken();
      const message = error instanceof Error ? error.message : 'Authentication failed';

      patchState(store, {
        isAuthenticated: false,
        accessToken: null,
        tokenExpiry: null,
        user: null,
        isLoading: false,
        error: message
      });
    };

    return {
      // Login with credentials
      login: rxMethod<{ issuer: string; user: string; password: string; client?: string; rememberMe?: boolean }>(
        pipe(
          tap(() => patchState(store, { isLoading: true, error: null })),
          switchMap(({ issuer, user, password, client, rememberMe }) =>
            authService.login(issuer, user, password, client).pipe(
              tap(jwt => handleAuthSuccess(jwt, rememberMe)),
              catchError(error => {
                handleAuthError(error);
                return of(null);
              })
            )
          )
        )
      ),

      // Refresh token
      refresh: rxMethod<{ issuer: string; client?: string }>(
        pipe(
          filter(() => !store.isLoading()),
          tap(() => patchState(store, { isLoading: true })),
          switchMap(({ issuer, client }) =>
            authService.refreshToken(issuer, client).pipe(
              tap(jwt => handleAuthSuccess(jwt)),
              catchError(error => {
                handleAuthError(error);
                return of(null);
              })
            )
          )
        )
      ),

      // Logout
      logout: rxMethod<void>(
        pipe(
          tap(() => patchState(store, { isLoading: true })),
          switchMap(() =>
            authService.logout().pipe(
              tap(() => {
                clearToken();
                refreshSubject.next();
                patchState(store, initialState);
              }),
              catchError(() => {
                // Clear local state even if server logout fails
                clearToken();
                refreshSubject.next();
                patchState(store, initialState);
                return of(null);
              })
            )
          )
        )
      ),

      // Initialize from stored token
      initFromStorage: () => {
        const stored = loadStoredToken();
        if (stored) {
          const payload = parseJwt(stored.accessToken);
          if (payload) {
            const user: AuthUser = {
              id: payload.sub,
              issuer: payload.iss,
              clients: Array.isArray(payload.aud) ? payload.aud : [payload.aud],
              email: payload.email,
              name: payload.name,
              firstName: payload.given_name,
              lastName: payload.family_name,
              roles: extractRoles(payload)
            };

            patchState(store, {
              isAuthenticated: true,
              accessToken: stored.accessToken,
              tokenExpiry: stored.expiry,
              user,
              isLoading: false,
              error: null
            });
          }
        }
      },

      // Start auto-refresh timer (only refreshes when rememberMe is enabled)
      startAutoRefresh: (issuer: string, client?: string) => {
        return interval(30000).pipe(
          takeUntil(refreshSubject),
          filter(() => isRememberMe() && store.shouldRefreshToken() && !store.isLoading())
        ).subscribe(() => {
          authService.refreshToken(issuer, client).subscribe({
            next: jwt => handleAuthSuccess(jwt),
            error: () => handleAuthError(new Error('Token refresh failed'))
          });
        });
      },

      // Clear local session without server call
      clearSession: (error?: string) => {
        clearToken();
        refreshSubject.next();
        patchState(store, { ...initialState, error: error ?? null });
      },

      // Clear error
      clearError: () => {
        patchState(store, { error: null });
      },

      // Set error message
      setError: (error: string) => {
        patchState(store, { error });
      }
    };
  })
);
