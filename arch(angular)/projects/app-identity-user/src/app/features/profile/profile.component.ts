import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ProfileService, AuthStore, UserV1 } from 'lib-identity';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { userIcon, checkIcon, xIcon } from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-profile',
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
      <h1 class="text-2xl font-semibold text-gray-800 mb-6">Profile</h1>

      @if (isLoading()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else if (error()) {
        <div class="bg-red-50 border border-red-200 rounded p-4 text-red-700">
          {{ error() }}
        </div>
      } @else {
        <div class="bg-white rounded-lg shadow">
          <!-- Profile Header -->
          <div class="p-6 border-b border-gray-200">
            <div class="flex items-center gap-4">
              <div class="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="userIcon" size="xlarge" class="text-blue-600"></kendo-svg-icon>
              </div>
              <div>
                <h2 class="text-xl font-semibold text-gray-900">{{ profile()?.firstName }} {{ profile()?.lastName }}</h2>
                <p class="text-gray-500">{{ profile()?.email }}</p>
                <p class="text-sm text-gray-400">{{ '@' + profile()?.userName }}</p>
              </div>
            </div>
          </div>

          <!-- Account Status -->
          <div class="p-6 border-b border-gray-200">
            <h3 class="text-lg font-medium text-gray-900 mb-4">Account Status</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div class="flex items-center gap-2">
                @if (profile()?.emailConfirmed) {
                  <kendo-svg-icon [icon]="checkIcon" class="text-green-500"></kendo-svg-icon>
                  <span class="text-gray-600">Email verified</span>
                } @else {
                  <kendo-svg-icon [icon]="xIcon" class="text-red-500"></kendo-svg-icon>
                  <span class="text-gray-600">Email not verified</span>
                }
              </div>
              <div class="flex items-center gap-2">
                @if (profile()?.passwordConfirmed) {
                  <kendo-svg-icon [icon]="checkIcon" class="text-green-500"></kendo-svg-icon>
                  <span class="text-gray-600">Password confirmed</span>
                } @else {
                  <kendo-svg-icon [icon]="xIcon" class="text-red-500"></kendo-svg-icon>
                  <span class="text-gray-600">Password not confirmed</span>
                }
              </div>
              <div class="flex items-center gap-2">
                @if (profile()?.phoneNumberConfirmed) {
                  <kendo-svg-icon [icon]="checkIcon" class="text-green-500"></kendo-svg-icon>
                  <span class="text-gray-600">Phone verified</span>
                } @else {
                  <kendo-svg-icon [icon]="xIcon" class="text-amber-500"></kendo-svg-icon>
                  <span class="text-gray-600">Phone not verified</span>
                }
              </div>
            </div>
          </div>

          <!-- Edit Profile Form -->
          <div class="p-6">
            <h3 class="text-lg font-medium text-gray-900 mb-4">Edit Profile</h3>

            @if (successMessage()) {
              <div class="mb-4 p-3 bg-green-50 border border-green-200 rounded text-green-700 text-sm">
                {{ successMessage() }}
              </div>
            }

            <form [formGroup]="profileForm" (ngSubmit)="onSubmit()">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <kendo-label text="First Name" for="firstName"></kendo-label>
                  <kendo-textbox
                    id="firstName"
                    formControlName="firstName"
                    [style.width.%]="100"
                  ></kendo-textbox>
                  @if (profileForm.get('firstName')?.invalid && profileForm.get('firstName')?.touched) {
                    <div class="text-red-500 text-sm mt-1">First name is required</div>
                  }
                </div>

                <div>
                  <kendo-label text="Last Name" for="lastName"></kendo-label>
                  <kendo-textbox
                    id="lastName"
                    formControlName="lastName"
                    [style.width.%]="100"
                  ></kendo-textbox>
                  @if (profileForm.get('lastName')?.invalid && profileForm.get('lastName')?.touched) {
                    <div class="text-red-500 text-sm mt-1">Last name is required</div>
                  }
                </div>

                <div class="md:col-span-2">
                  <kendo-label text="Phone Number" for="phoneNumber"></kendo-label>
                  <kendo-textbox
                    id="phoneNumber"
                    formControlName="phoneNumber"
                    [style.width.%]="100"
                    placeholder="+1 (555) 123-4567"
                  ></kendo-textbox>
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
                  [disabled]="profileForm.invalid || profileForm.pristine || isSaving()"
                >
                  @if (isSaving()) {
                    <kendo-loader size="small" themeColor="light"></kendo-loader>
                    Saving...
                  } @else {
                    Save Changes
                  }
                </button>
              </div>
            </form>
          </div>
        </div>
      }
    </div>
  `
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly profileService = inject(ProfileService);
  private readonly authStore = inject(AuthStore);

  readonly userIcon = userIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;

  readonly profile = signal<UserV1 | null>(null);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly error = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  profileForm!: FormGroup;

  ngOnInit(): void {
    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['']
    });

    this.loadProfile();
  }

  private loadProfile(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.profileService.getProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.profileForm.patchValue({
          firstName: profile.firstName,
          lastName: profile.lastName,
          phoneNumber: profile.phoneNumber || ''
        });
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load profile');
        this.isLoading.set(false);
      }
    });
  }

  resetForm(): void {
    const p = this.profile();
    if (p) {
      this.profileForm.patchValue({
        firstName: p.firstName,
        lastName: p.lastName,
        phoneNumber: p.phoneNumber || ''
      });
      this.profileForm.markAsPristine();
    }
    this.successMessage.set(null);
  }

  onSubmit(): void {
    if (this.profileForm.invalid) return;

    this.isSaving.set(true);
    this.successMessage.set(null);

    const { firstName, lastName, phoneNumber } = this.profileForm.value;
    const currentProfile = this.profile();

    if (!currentProfile) return;

    const update: Partial<UserV1> = {
      id: currentProfile.id,
      firstName: firstName,
      lastName: lastName,
      phoneNumber: phoneNumber || undefined
    };

    this.profileService.updateProfile(update).subscribe({
      next: (updatedProfile) => {
        this.profile.set(updatedProfile);
        this.profileForm.markAsPristine();
        this.isSaving.set(false);
        this.successMessage.set('Profile updated successfully');
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to update profile');
        this.isSaving.set(false);
      }
    });
  }
}
