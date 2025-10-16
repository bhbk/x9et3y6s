import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PasswordService } from 'lib-identity';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    KENDO_INPUTS,
    KENDO_BUTTONS,
    KENDO_LABELS,
    KENDO_INDICATORS
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
      <div class="max-w-md w-full">
        <div class="text-center mb-8">
          <h1 class="text-3xl font-bold text-gray-900">Reset Password</h1>
          <p class="mt-2 text-gray-600">Enter your new password</p>
        </div>

        <div class="bg-white rounded-lg shadow-md p-8">
          @if (success) {
            <div class="text-center">
              <div class="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <svg class="w-8 h-8 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                </svg>
              </div>
              <h2 class="text-xl font-semibold text-gray-800 mb-2">Password Reset!</h2>
              <p class="text-gray-600 mb-6">Your password has been successfully reset.</p>
              <a routerLink="/login" kendoButton themeColor="primary">Sign In</a>
            </div>
          } @else {
            @if (error) {
              <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
                {{ error }}
              </div>
            }

            <form [formGroup]="form" (ngSubmit)="onSubmit()">
              <div class="mb-4">
                <kendo-label text="New Password" for="password"></kendo-label>
                <kendo-textbox
                  id="password"
                  formControlName="password"
                  type="password"
                  [style.width.%]="100"
                  placeholder="Enter new password"
                ></kendo-textbox>
                @if (form.get('password')?.invalid && form.get('password')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Password must be at least 8 characters</div>
                }
              </div>

              <div class="mb-6">
                <kendo-label text="Confirm Password" for="confirmPassword"></kendo-label>
                <kendo-textbox
                  id="confirmPassword"
                  formControlName="confirmPassword"
                  type="password"
                  [style.width.%]="100"
                  placeholder="Confirm new password"
                ></kendo-textbox>
                @if (form.get('confirmPassword')?.touched && form.errors?.['passwordMismatch']) {
                  <div class="text-red-500 text-sm mt-1">Passwords do not match</div>
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
                  Resetting...
                } @else {
                  Reset Password
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
export class ResetPasswordComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly passwordService = inject(PasswordService);

  form = this.fb.group({
    password: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  token: string | null = null;
  isLoading = false;
  success = false;
  error: string | null = null;

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token');
    if (!this.token) {
      this.error = 'Invalid reset link';
    }
  }

  passwordMatchValidator(form: any) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.form.valid && this.token) {
      this.isLoading = true;
      this.error = null;

      const { password, confirmPassword } = this.form.value;

      this.passwordService.resetPassword(this.token, password!, confirmPassword!).subscribe({
        next: () => {
          this.isLoading = false;
          this.success = true;
        },
        error: (err) => {
          this.isLoading = false;
          this.error = err.error?.message || 'Failed to reset password';
        }
      });
    }
  }
}
