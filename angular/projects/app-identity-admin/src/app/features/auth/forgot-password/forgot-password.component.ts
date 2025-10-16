import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PasswordService } from 'lib-identity';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { unlockIcon } from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    KENDO_INPUTS,
    KENDO_BUTTONS,
    KENDO_LABELS,
    KENDO_INDICATORS,
    KENDO_ICONS
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
      <div class="max-w-md w-full">
        <div class="text-center mb-8">
          <div class="flex justify-center mb-4">
            <div class="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center">
              <kendo-svg-icon [icon]="unlockIcon" size="xlarge" class="text-amber-600"></kendo-svg-icon>
            </div>
          </div>
          <p class="text-gray-600">Enter your email to receive a reset link</p>
        </div>

        <div class="bg-white rounded-lg shadow-md p-8">
          @if (submitted) {
            <div class="text-center">
              <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
                </svg>
              </div>
              <h2 class="text-xl font-semibold text-gray-800 mb-2">Check Your Email</h2>
              <p class="text-gray-600 mb-6">If an account exists for that email, we've sent password reset instructions.</p>
              <a routerLink="/login" kendoButton themeColor="primary">Back to Login</a>
            </div>
          } @else {
            @if (error) {
              <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
                {{ error }}
              </div>
            }

            <form [formGroup]="form" (ngSubmit)="onSubmit()">
              <div class="mb-6">
                <kendo-label text="Email Address" for="email"></kendo-label>
                <kendo-textbox
                  id="email"
                  formControlName="email"
                  [style.width.%]="100"
                  placeholder="Enter your email address"
                ></kendo-textbox>
                @if (form.get('email')?.invalid && form.get('email')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Valid email is required</div>
                }
              </div>

              <button
                kendoButton
                type="submit"
                themeColor="primary"
                [style.width.%]="100"
                [disabled]="form.invalid || isLoading"
              >
                @if (isLoading) {
                  <kendo-loader size="small" themeColor="light"></kendo-loader>
                  Sending...
                } @else {
                  Send Reset Link
                }
              </button>

              <div class="mt-4 text-center">
                <a routerLink="/login" class="text-sm text-blue-600 hover:text-blue-500">
                  Back to Login
                </a>
              </div>
            </form>
          }
        </div>
      </div>
    </div>
  `
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly passwordService = inject(PasswordService);

  readonly unlockIcon = unlockIcon;

  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  isLoading = false;
  submitted = false;
  error: string | null = null;

  onSubmit(): void {
    if (this.form.valid) {
      this.isLoading = true;
      this.error = null;

      this.passwordService.requestPasswordReset(this.form.value.email!).subscribe({
        next: () => {
          this.isLoading = false;
          this.submitted = true;
        },
        error: () => {
          // Always show success to prevent email enumeration
          this.isLoading = false;
          this.submitted = true;
        }
      });
    }
  }
}
