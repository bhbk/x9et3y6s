import { Routes } from '@angular/router';
import { authGuard, guestGuard, roleGuard } from 'lib-identity';

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

  // All admin routes require authentication and admin role
  {
    path: '',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Identity.Admins'] },
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
        path: 'claims',
        loadComponent: () => import('./features/claims/claims.component').then(m => m.ClaimsComponent)
      },
      {
        path: 'logins',
        loadComponent: () => import('./features/logins/logins.component').then(m => m.LoginsComponent)
      },
      {
        path: 'activity',
        loadComponent: () => import('./features/activity/activity.component').then(m => m.ActivityComponent)
      },
      {
        path: 'motds',
        loadComponent: () => import('./features/motds/motds.component').then(m => m.MotdsComponent)
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
