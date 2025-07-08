import { Component, inject, OnInit, OnDestroy, effect, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthStore, ConfigService } from 'lib-identity';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { lockIcon } from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-login',
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
    @if (isTransferring()) {
      <div class="min-h-screen flex items-center justify-center bg-gray-50">
        <div class="text-center">
          <div class="flex justify-center mb-6">
            <div class="w-20 h-20 bg-blue-100 rounded-full flex items-center justify-center animate-pulse">
              <kendo-svg-icon [icon]="lockIcon" size="xlarge" class="text-blue-600"></kendo-svg-icon>
            </div>
          </div>
          <h2 class="text-xl font-semibold text-gray-800 mb-2">Signing you in...</h2>
          <p class="text-gray-500 mb-6">Transferring your session securely</p>
          <div class="w-48 mx-auto">
            <div class="h-1.5 bg-gray-200 rounded-full overflow-hidden">
              <div class="h-full bg-blue-500 rounded-full animate-progress"></div>
            </div>
          </div>
        </div>
      </div>
    } @else {
      <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4">
        <div class="max-w-md w-full">
          <div class="text-center mb-8">
            <div class="flex justify-center mb-4">
              <div class="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center">
                <kendo-svg-icon [icon]="lockIcon" size="xlarge" class="text-blue-600"></kendo-svg-icon>
              </div>
            </div>
            <p class="text-gray-600">Sign in to your account</p>
          </div>

          <div class="bg-white rounded-lg shadow-md p-8">
            @if (authStore.error()) {
              <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
                {{ authStore.error() }}
              </div>
            }

            <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
              <div class="mb-4">
                <kendo-label text="Email Address" for="user"></kendo-label>
                <kendo-textbox
                  id="user"
                  formControlName="user"
                  [style.width.%]="100"
                  placeholder="Enter your email address"
                ></kendo-textbox>
                @if (loginForm.get('user')?.invalid && loginForm.get('user')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Email is required</div>
                }
              </div>

              <div class="mb-4">
                <kendo-label text="Password" for="password"></kendo-label>
                <kendo-textbox
                  id="password"
                  formControlName="password"
                  type="password"
                  [style.width.%]="100"
                  placeholder="Enter your password"
                ></kendo-textbox>
                @if (loginForm.get('password')?.invalid && loginForm.get('password')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Password is required</div>
                }
              </div>

              <div class="flex justify-between items-center mb-6">
                <label class="flex items-center">
                  <input type="checkbox" formControlName="rememberMe" class="rounded border-gray-300">
                  <span class="ml-2 text-sm text-gray-600">Remember me</span>
                </label>
                <a routerLink="/forgot-password" class="text-sm text-blue-600 hover:text-blue-500">
                  Forgot password?
                </a>
              </div>

              <button
                kendoButton
                type="submit"
                themeColor="primary"
                [style.width.%]="100"
                [disabled]="loginForm.invalid || authStore.isLoading()"
              >
                @if (authStore.isLoading()) {
                  <kendo-loader size="small" themeColor="light"></kendo-loader>
                  Signing in...
                } @else {
                  Sign In
                }
              </button>
            </form>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes progress {
      0% { width: 0%; margin-left: 0; }
      50% { width: 60%; margin-left: 20%; }
      100% { width: 0%; margin-left: 100%; }
    }
    .animate-progress {
      animation: progress 1.5s ease-in-out infinite;
    }
  `]
})
export class LoginComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly config = inject(ConfigService);
  readonly authStore = inject(AuthStore);

  readonly lockIcon = lockIcon;
  readonly isTransferring = signal(false);

  loginForm!: FormGroup;
  private returnUrl = '/dashboard';
  private pendingNavigation = false;

  constructor() {
    // Use effect to watch for authentication state changes
    effect(() => {
      const isAuthenticated = this.authStore.isAuthenticated();
      const isLoading = this.authStore.isLoading();

      if (this.pendingNavigation && !isLoading && isAuthenticated) {
        this.pendingNavigation = false;
        this.router.navigateByUrl(this.returnUrl);
      }
    });
  }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      user: ['', Validators.required],
      password: ['', Validators.required],
      rememberMe: [false]
    });

    // Get return URL from query params
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';

    // Clear any previous errors
    this.authStore.clearError();

    // Check for cross-SPA token transfer
    const incomingToken = this.route.snapshot.queryParams['token'];
    if (incomingToken) {
      this.hydrateFromToken(incomingToken);
      return;
    }

    // Initialize from storage in case user has valid session
    this.authStore.initFromStorage();
    if (this.authStore.isAuthenticated() && !this.authStore.isTokenExpired()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  ngOnDestroy(): void {
    this.pendingNavigation = false;
  }

  private hydrateFromToken(token: string): void {
    this.isTransferring.set(true);
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const payload = JSON.parse(decodeURIComponent(
        atob(base64).split('').map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')
      ));
      const expiresIn = payload.exp - Math.floor(Date.now() / 1000);
      if (expiresIn > 0) {
        sessionStorage.setItem('identity_access_token', JSON.stringify({
          accessToken: token,
          expiresIn,
          savedAt: new Date().toISOString()
        }));
        this.authStore.initFromStorage();
        if (this.authStore.isAuthenticated() && !this.authStore.isTokenExpired()) {
          setTimeout(() => this.router.navigateByUrl(this.returnUrl), 1000);
          return;
        }
      }
    } catch {
      // Invalid token, fall through to login form
    }
    this.isTransferring.set(false);
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      const { user, password, rememberMe } = this.loginForm.value;

      // Get issuer and client from config
      const issuer = this.config.defaultIssuer;
      const client = this.config.defaultClient;

      if (!issuer) {
        console.error('No default issuer configured');
        return;
      }

      this.pendingNavigation = true;
      this.authStore.login({ issuer, user, password, client, rememberMe });
    }
  }
}
