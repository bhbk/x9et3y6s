import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { PasswordService, AuthStore, PasswordChange } from 'lib-identity';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { lockIcon, warningTriangleIcon } from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-security',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    KENDO_INPUTS,
    KENDO_BUTTONS,
    KENDO_LABELS,
    KENDO_INDICATORS,
    KENDO_ICONS
  ],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-semibold text-gray-800 mb-6">Security</h1>

      <!-- Change Password Card -->
      <div class="bg-blue-50 border border-blue-200 rounded-lg mb-6">
        <div class="p-6 border-b border-blue-200">
          <div class="flex items-center gap-3">
            <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
              <kendo-svg-icon [icon]="lockIcon" class="text-blue-600"></kendo-svg-icon>
            </div>
            <div>
              <h2 class="text-lg font-semibold text-gray-900">Change Password</h2>
              <p class="text-sm text-gray-500">Update your password to keep your account secure</p>
            </div>
          </div>
        </div>

        <div class="p-6">
          @if (successMessage()) {
            <div class="mb-4 p-3 bg-green-50 border border-green-200 rounded text-green-700 text-sm">
              {{ successMessage() }}
            </div>
          }

          @if (error()) {
            <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
              {{ error() }}
            </div>
          }

          <form [formGroup]="passwordForm" (ngSubmit)="onSubmit()">
            <div class="space-y-4">
              <div>
                <kendo-label text="Current Password" for="currentPassword"></kendo-label>
                <kendo-textbox
                  id="currentPassword"
                  formControlName="currentPassword"
                  type="password"
                  [style.width.%]="100"
                ></kendo-textbox>
                @if (passwordForm.get('currentPassword')?.invalid && passwordForm.get('currentPassword')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Current password is required</div>
                }
              </div>

              <div>
                <kendo-label text="New Password" for="newPassword"></kendo-label>
                <kendo-textbox
                  id="newPassword"
                  formControlName="newPassword"
                  type="password"
                  [style.width.%]="100"
                ></kendo-textbox>
                @if (passwordForm.get('newPassword')?.hasError('required') && passwordForm.get('newPassword')?.touched) {
                  <div class="text-red-500 text-sm mt-1">New password is required</div>
                } @else if (passwordForm.get('newPassword')?.hasError('minlength') && passwordForm.get('newPassword')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Password must be at least 10 characters</div>
                }
              </div>

              <div>
                <kendo-label text="Confirm New Password" for="confirmPassword"></kendo-label>
                <kendo-textbox
                  id="confirmPassword"
                  formControlName="confirmPassword"
                  type="password"
                  [style.width.%]="100"
                ></kendo-textbox>
                @if (passwordForm.get('confirmPassword')?.hasError('required') && passwordForm.get('confirmPassword')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Please confirm your new password</div>
                } @else if (passwordForm.hasError('passwordMismatch') && passwordForm.get('confirmPassword')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Passwords do not match</div>
                }
              </div>
            </div>

            <!-- Password Requirements & Security Tips -->
            <div class="mt-4 p-4 bg-amber-50 border border-amber-200 rounded-lg">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="flex gap-3">
                  <kendo-svg-icon [icon]="warningIcon" class="text-amber-500 flex-shrink-0 mt-0.5"></kendo-svg-icon>
                  <div>
                    <p class="text-sm font-medium text-amber-800 mb-2">Password Requirements</p>
                    <ul class="text-sm text-amber-700 space-y-1">
                      <li class="flex items-center gap-2">
                        <span [class]="passwordLength ? 'text-green-500' : 'text-amber-400'">&#x2022;</span>
                        At least 10 characters
                      </li>
                      <li class="flex items-center gap-2">
                        <span [class]="passwordHasUpper ? 'text-green-500' : 'text-amber-400'">&#x2022;</span>
                        One uppercase letter
                      </li>
                      <li class="flex items-center gap-2">
                        <span [class]="passwordHasLower ? 'text-green-500' : 'text-amber-400'">&#x2022;</span>
                        One lowercase letter
                      </li>
                      <li class="flex items-center gap-2">
                        <span [class]="passwordHasNumber ? 'text-green-500' : 'text-amber-400'">&#x2022;</span>
                        One number
                      </li>
                      <li class="flex items-center gap-2">
                        <span [class]="passwordHasSymbol ? 'text-green-500' : 'text-amber-400'">&#x2022;</span>
                        One special character
                      </li>
                    </ul>
                  </div>
                </div>
                <div class="flex gap-3">
                  <kendo-svg-icon [icon]="warningIcon" class="text-amber-500 flex-shrink-0 mt-0.5"></kendo-svg-icon>
                  <div>
                    <p class="text-sm font-medium text-amber-800 mb-2">Security Tips</p>
                    <ul class="text-sm text-amber-700 space-y-1.5 list-disc list-inside">
                      <li>Keep your password private and never share it</li>
                      <li>Avoid reusing passwords across accounts</li>
                      <li>Use a password manager for stronger credentials</li>
                      <li>Enable two-factor authentication where possible</li>
                    </ul>
                  </div>
                </div>
              </div>
            </div>

            <div class="mt-6 flex justify-end gap-3">
              <button
                kendoButton
                type="button"
                fillMode="outline"
                (click)="resetForm()"
                [disabled]="isSaving()"
              >
                Cancel
              </button>
              <button
                kendoButton
                type="submit"
                themeColor="primary"
                [disabled]="passwordForm.invalid || isSaving()"
              >
                @if (isSaving()) {
                  <kendo-loader size="small" themeColor="light"></kendo-loader>
                  Updating...
                } @else {
                  Update Password
                }
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `
})
export class SecurityComponent {
  private readonly fb = inject(FormBuilder);
  private readonly passwordService = inject(PasswordService);
  private readonly authStore = inject(AuthStore);

  readonly lockIcon = lockIcon;
  readonly warningIcon = warningTriangleIcon;

  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  passwordForm: FormGroup = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(10)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  get passwordLength(): boolean {
    return (this.passwordForm.get('newPassword')?.value?.length ?? 0) >= 10;
  }

  get passwordHasUpper(): boolean {
    return /[A-Z]/.test(this.passwordForm.get('newPassword')?.value ?? '');
  }

  get passwordHasLower(): boolean {
    return /[a-z]/.test(this.passwordForm.get('newPassword')?.value ?? '');
  }

  get passwordHasNumber(): boolean {
    return /[0-9]/.test(this.passwordForm.get('newPassword')?.value ?? '');
  }

  get passwordHasSymbol(): boolean {
    return /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?`~]/.test(this.passwordForm.get('newPassword')?.value ?? '');
  }

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  resetForm(): void {
    this.passwordForm.reset();
    this.error.set(null);
    this.successMessage.set(null);
  }

  onSubmit(): void {
    if (this.passwordForm.invalid) return;

    this.isSaving.set(true);
    this.error.set(null);
    this.successMessage.set(null);

    const userId = this.authStore.user()?.id;
    if (!userId) {
      this.error.set('User not authenticated');
      this.isSaving.set(false);
      return;
    }

    const { currentPassword, newPassword, confirmPassword } = this.passwordForm.value;

    const change: PasswordChange = {
      entityId: userId,
      currentPassword: currentPassword,
      newPassword: newPassword,
      newPasswordConfirm: confirmPassword
    };

    this.passwordService.setPassword(change).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.successMessage.set('Password updated successfully');
        this.passwordForm.reset();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to update password');
      }
    });
  }
}
