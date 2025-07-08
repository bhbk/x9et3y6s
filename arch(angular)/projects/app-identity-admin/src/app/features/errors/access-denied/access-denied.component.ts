import { Component } from '@angular/core';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';

@Component({
  selector: 'app-access-denied',
  standalone: true,
  imports: [KENDO_BUTTONS],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50">
      <div class="text-center">
        <h1 class="text-6xl font-bold text-red-600 mb-4">403</h1>
        <h2 class="text-2xl font-semibold text-gray-800 mb-2">Access Denied</h2>
        <p class="text-gray-600 mb-6">You need administrator privileges to access this portal.</p>
        <a href="/" kendoButton themeColor="primary">Go to User Portal</a>
      </div>
    </div>
  `
})
export class AccessDeniedComponent {}
