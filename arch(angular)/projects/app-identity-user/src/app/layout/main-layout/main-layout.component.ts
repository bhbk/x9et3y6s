import { Component, inject, signal, computed, HostListener } from '@angular/core';
import { Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStore, ConfigService } from 'lib-identity';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { menuIcon, userIcon, lockIcon, gridLayoutIcon, logoutIcon, gearIcon } from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, KENDO_BUTTONS, KENDO_ICONS],
  template: `
    <div class="min-h-screen bg-gray-50 flex flex-col">
      <!-- Top Bar -->
      <header class="h-16 bg-white flex items-center px-4 shrink-0 relative z-50">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
            <kendo-svg-icon [icon]="lockIcon" size="medium" class="text-blue-600"></kendo-svg-icon>
          </div>
          <span class="text-2xl font-semibold text-gray-800 whitespace-nowrap">Identity User</span>
        </div>
        <div class="ml-auto relative" data-menu-container>
          <button
            kendoButton
            fillMode="flat"
            (click)="toggleMenu($event)"
            class="!p-1"
          >
            <kendo-svg-icon [icon]="hamburgerIcon" size="medium"></kendo-svg-icon>
          </button>
          @if (menuOpen()) {
            <div class="absolute right-0 top-full mt-1 w-56 bg-white rounded-lg shadow-lg border border-gray-200 py-1">
              <div class="px-4 py-2 border-b border-gray-100">
                <p class="text-sm font-medium text-gray-900 truncate">{{ authStore.userDisplayName() }}</p>
              </div>
              @if (isAdmin() && adminPortalUrl) {
                <a
                  [href]="getPortalUrl()"
                  class="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 hover:bg-gray-100"
                >
                  <kendo-svg-icon [icon]="gearIcon" size="small"></kendo-svg-icon>
                  Admin Portal
                </a>
              }
              <button
                (click)="logout()"
                class="flex items-center gap-2 w-full px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 text-left"
              >
                <kendo-svg-icon [icon]="logoutIcon" size="small"></kendo-svg-icon>
                Sign Out
              </button>
            </div>
          }
        </div>
      </header>

      <div class="flex flex-1 overflow-hidden">
        <!-- Sidebar -->
        <aside class="w-64 bg-white shadow-sm flex flex-col overflow-y-auto shrink-0">
          <nav class="flex-1 p-4 space-y-1">
            <a routerLink="/dashboard" routerLinkActive="bg-blue-50 text-blue-600"
               class="flex items-center gap-3 px-3 py-2 rounded-md text-gray-700 hover:bg-gray-100 transition-colors">
              <kendo-svg-icon [icon]="gridIcon" size="medium"></kendo-svg-icon>
              Dashboard
            </a>
            <a routerLink="/profile" routerLinkActive="bg-blue-50 text-blue-600"
               class="flex items-center gap-3 px-3 py-2 rounded-md text-gray-700 hover:bg-gray-100 transition-colors">
              <kendo-svg-icon [icon]="userIcon" size="medium"></kendo-svg-icon>
              Profile
            </a>
            <a routerLink="/security" routerLinkActive="bg-blue-50 text-blue-600"
               class="flex items-center gap-3 px-3 py-2 rounded-md text-gray-700 hover:bg-gray-100 transition-colors">
              <kendo-svg-icon [icon]="lockIcon" size="medium"></kendo-svg-icon>
              Security
            </a>
            <a routerLink="/sessions" routerLinkActive="bg-blue-50 text-blue-600"
               class="flex items-center gap-3 px-3 py-2 rounded-md text-gray-700 hover:bg-gray-100 transition-colors">
              <kendo-svg-icon [icon]="sessionsIcon" size="medium"></kendo-svg-icon>
              Sessions
            </a>
          </nav>
        </aside>

        <!-- Main content -->
        <main class="flex-1 overflow-auto">
          <router-outlet></router-outlet>
        </main>
      </div>
    </div>
  `
})
export class MainLayoutComponent {
  private readonly router = inject(Router);
  private readonly config = inject(ConfigService);
  readonly authStore = inject(AuthStore);

  readonly hamburgerIcon = menuIcon;
  readonly userIcon = userIcon;
  readonly lockIcon = lockIcon;
  readonly gridIcon = gridLayoutIcon;
  readonly logoutIcon = logoutIcon;
  readonly gearIcon = gearIcon;
  readonly sessionsIcon = menuIcon;

  readonly menuOpen = signal(false);
  readonly isAdmin = computed(() => this.authStore.hasRole()('Identity.Admins'));

  get adminPortalUrl(): string | undefined {
    return this.config.adminPortalUrl;
  }

  getPortalUrl(): string {
    const base = this.config.adminPortalUrl ?? '';
    const token = this.authStore.accessToken();
    return token ? `${base}/login?token=${encodeURIComponent(token)}` : base;
  }

  toggleMenu(event: MouseEvent): void {
    event.stopPropagation();
    this.menuOpen.update(v => !v);
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.menuOpen.set(false);
  }

  logout(): void {
    this.menuOpen.set(false);
    this.authStore.clearSession();
    this.router.navigate(['/login']);
  }
}
