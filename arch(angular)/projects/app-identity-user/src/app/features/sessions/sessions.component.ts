import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { SessionService, RefreshV1, AuthStore } from 'lib-identity';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_DIALOGS } from '@progress/kendo-angular-dialog';
import {
  laptopOutlineIcon,
  mobileOutlineIcon,
  globeIcon,
  trashIcon,
  checkCircleIcon
} from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-sessions',
  standalone: true,
  imports: [
    KENDO_BUTTONS,
    KENDO_INDICATORS,
    KENDO_ICONS,
    KENDO_DIALOGS
  ],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <div>
          <h1 class="text-2xl font-semibold text-gray-800">Sessions</h1>
          <p class="text-gray-500 mt-1">Manage your sessions and sign out from other devices</p>
        </div>
        @if (sessions().length > 1) {
          <button
            kendoButton
            fillMode="outline"
            themeColor="error"
            (click)="confirmRevokeAll()"
            [disabled]="isLoading()"
          >
            <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
            Sign Out All
          </button>
        }
      </div>

      @if (isLoading()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else if (error()) {
        <div class="bg-red-50 border border-red-200 rounded p-4 text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="loadSessions()">
            Retry
          </button>
        </div>
      } @else if (sessions().length === 0) {
        <div class="bg-white rounded-lg shadow p-8 text-center">
          <kendo-svg-icon [icon]="computerIcon" size="xlarge" class="text-gray-300 mb-4"></kendo-svg-icon>
          <p class="text-gray-500">No active sessions found</p>
        </div>
      } @else {
        <div class="space-y-4">
          @for (session of sessions(); track session.id) {
            <div class="bg-white rounded-lg shadow p-4">
              <div class="flex items-start justify-between">
                <div class="flex items-start gap-4">
                  <div class="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center">
                    <kendo-svg-icon [icon]="getDeviceIcon(session)" size="large" class="text-gray-600"></kendo-svg-icon>
                  </div>
                  <div>
                    <div class="flex items-center gap-2">
                      <span class="font-medium text-gray-900">{{ getDeviceName(session) }}</span>
                      @if (isCurrentSession(session)) {
                        <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                          <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                          Current
                        </span>
                      }
                    </div>
                    @if (session.ipAddress) {
                      <p class="text-sm text-gray-500 mt-1">
                        {{ session.ipAddress }}
                      </p>
                    }
                    <div class="mt-2 text-xs text-gray-400 space-y-0.5">
                      <p>Created: {{ formatDate(session.issuedUtc) }}</p>
                      <p>Expires: {{ formatDate(session.validToUtc) }}</p>
                    </div>
                  </div>
                </div>
                <div>
                  @if (!isCurrentSession(session)) {
                    <button
                      kendoButton
                      fillMode="flat"
                      themeColor="error"
                      (click)="confirmRevoke(session)"
                      [disabled]="revokingId() === session.id"
                    >
                      @if (revokingId() === session.id) {
                        <kendo-loader size="small"></kendo-loader>
                      } @else {
                        <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
                        Revoke
                      }
                    </button>
                  }
                </div>
              </div>
            </div>
          }
        </div>
      }

      <!-- Revoke Single Session Dialog -->
      @if (showRevokeDialog()) {
        <kendo-dialog
          title="Revoke Session"
          (close)="closeRevokeDialog()"
          [minWidth]="350"
          [width]="420"
        >
          <p>Are you sure you want to revoke this session? The device will need to sign in again.</p>
          <kendo-dialog-actions>
            <button kendoButton (click)="closeRevokeDialog()">Cancel</button>
            <button kendoButton themeColor="error" (click)="executeRevoke()">Revoke</button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }

      <!-- Revoke All Sessions Dialog -->
      @if (showRevokeAllDialog()) {
        <kendo-dialog
          title="Sign Out All Sessions"
          (close)="closeRevokeAllDialog()"
          [minWidth]="350"
          [width]="420"
        >
          <p>Are you sure you want to sign out of all sessions? All devices will need to sign in again.</p>
          <kendo-dialog-actions>
            <button kendoButton (click)="closeRevokeAllDialog()">Cancel</button>
            <button kendoButton themeColor="error" (click)="executeRevokeAll()">Sign Out All</button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }
    </div>
  `
})
export class SessionsComponent implements OnInit {
  private readonly sessionService = inject(SessionService);
  private readonly authStore = inject(AuthStore);
  private readonly router = inject(Router);

  readonly computerIcon = laptopOutlineIcon;
  readonly mobileIcon = mobileOutlineIcon;
  readonly globeIcon = globeIcon;
  readonly trashIcon = trashIcon;
  readonly checkIcon = checkCircleIcon;

  readonly sessions = signal<RefreshV1[]>([]);
  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly revokingId = signal<string | null>(null);

  // Dialog state
  readonly showRevokeDialog = signal(false);
  readonly showRevokeAllDialog = signal(false);
  private pendingRevokeSession: RefreshV1 | null = null;

  ngOnInit(): void {
    this.loadSessions();
  }

  loadSessions(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.sessionService.getSessions().subscribe({
      next: (sessions) => {
        this.sessions.set(sessions);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load sessions');
        this.isLoading.set(false);
      }
    });
  }

  getDeviceIcon(session: RefreshV1) {
    const type = session.refreshType?.toLowerCase() || '';
    if (type.includes('mobile')) return this.mobileIcon;
    return this.computerIcon;
  }

  getDeviceName(session: RefreshV1): string {
    if (session.deviceName) return session.deviceName;
    if (session.userAgent) return this.parseUserAgent(session.userAgent);
    return 'Browser Session';
  }

  private parseUserAgent(ua: string): string {
    if (ua.includes('Chrome') && !ua.includes('Edg')) return 'Chrome';
    if (ua.includes('Edg')) return 'Microsoft Edge';
    if (ua.includes('Firefox')) return 'Firefox';
    if (ua.includes('Safari') && !ua.includes('Chrome')) return 'Safari';
    return 'Browser Session';
  }

  isCurrentSession(session: RefreshV1): boolean {
    return false;
  }

  formatDate(dateStr: string): string {
    try {
      return DateTime.fromISO(dateStr).toLocaleString(DateTime.DATETIME_MED);
    } catch {
      return dateStr;
    }
  }

  confirmRevoke(session: RefreshV1): void {
    this.pendingRevokeSession = session;
    this.showRevokeDialog.set(true);
  }

  closeRevokeDialog(): void {
    this.showRevokeDialog.set(false);
    this.pendingRevokeSession = null;
  }

  executeRevoke(): void {
    if (!this.pendingRevokeSession) return;
    const id = this.pendingRevokeSession.id;
    this.closeRevokeDialog();
    this.revokeSession(id);
  }

  private revokeSession(id: string): void {
    this.revokingId.set(id);

    this.sessionService.revokeSession(id).subscribe({
      next: () => {
        this.sessions.update(sessions => sessions.filter(s => s.id !== id));
        this.revokingId.set(null);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to revoke session');
        this.revokingId.set(null);
      }
    });
  }

  confirmRevokeAll(): void {
    this.showRevokeAllDialog.set(true);
  }

  closeRevokeAllDialog(): void {
    this.showRevokeAllDialog.set(false);
  }

  executeRevokeAll(): void {
    this.closeRevokeAllDialog();
    this.revokeAllSessions();
  }

  private revokeAllSessions(): void {
    this.isLoading.set(true);

    this.sessionService.revokeAllSessions().subscribe({
      next: () => {
        // All refresh tokens are revoked server-side; clear local auth state and redirect to login
        this.authStore.clearSession();
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to revoke sessions');
        this.isLoading.set(false);
      }
    });
  }
}
