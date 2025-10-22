import { Routes } from '@angular/router';
import { authGuard, entitlementGuard, guestGuard } from 'lib-identity';

export const routes: Routes = [
  // Login page - only for unauthenticated users
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'forgot-password',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },

  // All admin routes require authentication, admin role, and entitlement
  {
    path: '',
    canActivate: [authGuard, entitlementGuard],
    data: { entitlements: ['Viewer'] },
    loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'issuers',
        loadComponent: () => import('./features/issuers/issuers.component').then(m => m.IssuersComponent)
      },
      {
        path: 'audiences',
        loadComponent: () => import('./features/audiences/audiences.component').then(m => m.AudiencesComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./features/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./features/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'entitlements',
        loadComponent: () => import('./features/entitlements/entitlements-page.component').then(m => m.EntitlementsPageComponent)
      },
      {
        path: 'claims',
        loadComponent: () => import('./features/claims/claims.component').then(m => m.ClaimsComponent)
      },
      {
        path: 'login-providers',
        loadComponent: () => import('./features/login-providers/login-providers.component').then(m => m.LoginProvidersComponent)
      },
      {
        path: 'activity',
        loadComponent: () => import('./features/activity/activity.component').then(m => m.ActivityComponent)
      },
      {
        path: 'alerts',
        loadComponent: () => import('./features/alerts/alerts.component').then(m => m.AlertsComponent)
      },
      {
        path: 'quotes',
        loadComponent: () => import('./features/quotes/quotes.component').then(m => m.QuotesComponent)
      },
      {
        path: 'llm-providers',
        loadComponent: () => import('./features/llm-providers/llm-providers.component').then(m => m.LlmProvidersComponent)
      },
      {
        path: 'jobs',
        loadComponent: () => import('./features/jobs/jobs.component').then(m => m.JobsComponent)
      },
      {
        path: 'assistant',
        loadComponent: () => import('./features/assistant/assistant.component').then(m => m.AssistantComponent)
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },

  // Error pages
  {
    path: 'access-denied',
    loadComponent: () => import('./features/errors/access-denied/access-denied.component').then(m => m.AccessDeniedComponent)
  },
  {
    path: '**',
    loadComponent: () => import('./features/errors/not-found/not-found.component').then(m => m.NotFoundComponent)
  }
];
