import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, KENDO_BUTTONS],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        <h1 class="text-6xl font-bold text-gray-400 mb-4">404</h1>
        <h2 class="text-2xl font-semibold text-gray-800 mb-2">Page Not Found</h2>
        <p class="text-gray-600 mb-6">The page you're looking for doesn't exist.</p>
        <a routerLink="/dashboard" kendoButton themeColor="primary">Go to Dashboard</a>
      </div>
    </div>
  `
})
export class NotFoundComponent {}
