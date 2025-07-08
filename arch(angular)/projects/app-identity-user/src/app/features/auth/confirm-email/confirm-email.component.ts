import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ProfileService } from 'lib-identity';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [RouterLink, KENDO_BUTTONS, KENDO_INDICATORS],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
      <div class="max-w-md w-full text-center">
        @if (isLoading) {
          <kendo-loader size="large" themeColor="primary"></kendo-loader>
          <p class="mt-4 text-gray-600">Confirming your email...</p>
        } @else if (success) {
          <div class="bg-white rounded-lg shadow-md p-8">
            <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
            <h2 class="text-2xl font-semibold text-gray-800 mb-2">Email Confirmed!</h2>
            <p class="text-gray-600 mb-6">Your email address has been successfully verified.</p>
            <a routerLink="/login" kendoButton themeColor="primary">Sign In</a>
          </div>
        } @else {
          <div class="bg-white rounded-lg shadow-md p-8">
            <div class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg class="w-8 h-8 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
              </svg>
            </div>
            <h2 class="text-2xl font-semibold text-gray-800 mb-2">Confirmation Failed</h2>
            <p class="text-gray-600 mb-6">{{ error || 'The confirmation link is invalid or has expired.' }}</p>
            <a routerLink="/login" kendoButton themeColor="primary">Back to Login</a>
          </div>
        }
      </div>
    </div>
  `
})
export class ConfirmEmailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly profileService = inject(ProfileService);

  isLoading = true;
  success = false;
  error: string | null = null;

  ngOnInit(): void {
    const token = this.route.snapshot.paramMap.get('token');
    if (token) {
      this.profileService.confirmEmail(token).subscribe({
        next: () => {
          this.isLoading = false;
          this.success = true;
        },
        error: (err) => {
          this.isLoading = false;
          this.success = false;
          this.error = err.error?.message || 'Failed to confirm email';
        }
      });
    } else {
      this.isLoading = false;
      this.success = false;
      this.error = 'Invalid confirmation link';
    }
  }
}
