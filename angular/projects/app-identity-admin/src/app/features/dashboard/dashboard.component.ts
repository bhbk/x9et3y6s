import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { forkJoin } from 'rxjs';
import {
  UserService,
  AuthActivityService,
  AuthActivityV1,
  UserV1
} from 'lib-identity';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DatePipe, KENDO_BUTTONS, KENDO_INDICATORS],
  template: `
    <div class="p-6">
      @if (isLoading()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else if (error()) {
        <div class="p-3 bg-red-50 border border-red-200 rounded text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
        </div>
      } @else {
        <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-6 mb-8">
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-medium text-gray-500 mb-1">Total Users</h3>
            <p class="text-2xl font-medium text-gray-900">{{ userCount() }}</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-medium text-gray-500 mb-1">Active Sessions</h3>
            <p class="text-2xl font-medium text-gray-900">{{ activityCount() }}</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-medium text-gray-500 mb-1">Failed Logins</h3>
            <p class="text-3xl font-bold text-red-600">{{ failedLoginCount() }}</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-medium text-gray-500 mb-1">Locked Accounts</h3>
            <p class="text-3xl font-bold text-amber-600">{{ lockedAccountCount() }}</p>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h3 class="text-sm font-medium text-gray-500 mb-1">Pending Confirmations</h3>
            <p class="text-3xl font-bold text-blue-600">{{ pendingConfirmationCount() }}</p>
          </div>
        </div>

        <div>
          <div class="bg-white rounded-lg border border-gray-200 p-6">
            <h2 class="text-lg font-medium text-gray-800 mb-4">Recent Activity</h2>
            @if (recentActivity().length === 0) {
              <p class="text-gray-500">No recent activity found.</p>
            } @else {
              <div class="overflow-x-auto">
                <table class="min-w-full text-sm">
                  <thead>
                    <tr class="border-b border-gray-200">
                      <th class="text-left py-2 pr-4 font-medium text-gray-500">Date</th>
                      <th class="text-left py-2 pr-4 font-medium text-gray-500">User</th>
                      <th class="text-left py-2 pr-4 font-medium text-gray-500">Type</th>
                      <th class="text-left py-2 pr-4 font-medium text-gray-500">Outcome</th>
                      <th class="text-left py-2 pr-4 font-medium text-gray-500">Remote Address</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (item of recentActivity(); track item.id) {
                      <tr class="border-b border-gray-100">
                        <td class="py-2 pr-4 text-gray-700">{{ item.createdUtc | date:'short' }}</td>
                        <td class="py-2 pr-4 text-gray-700">{{ getUserName(item.userId) }}</td>
                        <td class="py-2 pr-4 text-gray-700">{{ item.loginType }}</td>
                        <td class="py-2 pr-4">
                          <span [class]="item.loginOutcome === 'Success'
                            ? 'inline-flex px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 text-green-800 border border-green-200'
                            : 'inline-flex px-2 py-0.5 text-xs font-medium rounded-full bg-red-100 text-red-800 border border-red-200'">
                            {{ item.loginOutcome }}
                          </span>
                        </td>
                        <td class="py-2 pr-4 text-gray-500">{{ item.remoteEndpoint }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `
})
export class DashboardComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly activityService = inject(AuthActivityService);

  readonly isLoading = signal(true);
  readonly error = signal<string | null>(null);
  readonly userCount = signal('0');
  readonly activityCount = signal('0');
  readonly failedLoginCount = signal('0');
  readonly lockedAccountCount = signal('0');
  readonly pendingConfirmationCount = signal('0');
  readonly recentActivity = signal<AuthActivityV1[]>([]);
  private userMap = new Map<string, string>();

  getUserName(userId?: string): string {
    if (!userId) return '—';
    return this.userMap.get(userId) ?? userId;
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);
    forkJoin({
      users: this.userService.getAll({ skip: 0, take: 100, sort: [{ field: 'userName', dir: 'asc' }] }),
      activity: this.activityService.getAll({ skip: 0, take: 100, sort: [{ field: 'createdUtc', dir: 'desc' }] })
    }).subscribe({
      next: ({ users, activity }) => {
        this.userMap = new Map(users.data.map((u: UserV1) => [u.id, u.userName]));
        this.userCount.set(String(users.total));
        this.lockedAccountCount.set(String(users.data.filter((u: UserV1) => u.isLockedOut).length));
        this.pendingConfirmationCount.set(String(users.data.filter((u: UserV1) => !u.emailConfirmed).length));

        this.activityCount.set(String(activity.total));
        this.failedLoginCount.set(String(activity.data.filter(a => a.loginOutcome !== 'Success').length));
        this.recentActivity.set(activity.data.slice(0, 10));

        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Failed to load dashboard data');
        this.isLoading.set(false);
      }
    });
  }
}
