import { Routes } from '@angular/router';
import { authGuard, guestGuard } from 'lib-identity';

export const routes: Routes = [
  // Public routes
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'confirm/email/:token',
    loadComponent: () => import('./features/auth/confirm-email/confirm-email.component').then(m => m.ConfirmEmailComponent)
  },
  {
    path: 'confirm/phone/:code',
    loadComponent: () => import('./features/auth/confirm-phone/confirm-phone.component').then(m => m.ConfirmPhoneComponent)
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'reset-password/:token',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  // Protected routes
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./features/profile/profile.component').then(m => m.ProfileComponent)
      },
      {
        path: 'security',
        loadComponent: () => import('./features/security/security.component').then(m => m.SecurityComponent)
      },
      {
        path: 'sessions',
        loadComponent: () => import('./features/sessions/sessions.component').then(m => m.SessionsComponent)
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
