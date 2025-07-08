import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import {
  UserService,
  AuthActivityService,
  AuthActivityV1
} from 'lib-identity';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [DatePipe],
  template: `
    <div class="p-6">
      <h1 class="text-2xl font-semibold text-gray-800 mb-6">Dashboard</h1>

      <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-5 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-sm font-medium text-gray-500 mb-1">Total Users</h3>
          <p class="text-3xl font-bold text-gray-900">{{ userCount() }}</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-sm font-medium text-gray-500 mb-1">Active Sessions</h3>
          <p class="text-3xl font-bold text-gray-900">{{ activityCount() }}</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-sm font-medium text-gray-500 mb-1">Failed Logins</h3>
          <p class="text-3xl font-bold text-red-600">{{ failedLoginCount() }}</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-sm font-medium text-gray-500 mb-1">Locked Accounts</h3>
          <p class="text-3xl font-bold text-amber-600">{{ lockedAccountCount() }}</p>
        </div>
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-sm font-medium text-gray-500 mb-1">Pending Confirmations</h3>
          <p class="text-3xl font-bold text-blue-600">{{ pendingConfirmationCount() }}</p>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow p-6">
        <h2 class="text-lg font-medium text-gray-800 mb-4">Recent Activity</h2>
        @if (recentActivity().length === 0) {
          <p class="text-gray-500">No recent activity found.</p>
        } @else {
          <div class="overflow-x-auto">
            <table class="min-w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200">
                  <th class="text-left py-2 pr-4 font-medium text-gray-500">Date</th>
                  <th class="text-left py-2 pr-4 font-medium text-gray-500">Type</th>
                  <th class="text-left py-2 pr-4 font-medium text-gray-500">Outcome</th>
                  <th class="text-left py-2 pr-4 font-medium text-gray-500">Remote Endpoint</th>
                </tr>
              </thead>
              <tbody>
                @for (item of recentActivity(); track item.id) {
                  <tr class="border-b border-gray-100">
                    <td class="py-2 pr-4 text-gray-700">{{ item.createdUtc | date:'short' }}</td>
                    <td class="py-2 pr-4 text-gray-700">{{ item.loginType }}</td>
                    <td class="py-2 pr-4">
                      <span [class]="item.loginOutcome === 'Success'
                        ? 'inline-flex px-2 py-0.5 text-xs font-medium rounded-full bg-green-100 text-green-800'
                        : 'inline-flex px-2 py-0.5 text-xs font-medium rounded-full bg-red-100 text-red-800'">
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
  `
})
export class DashboardComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly activityService = inject(AuthActivityService);

  readonly userCount = signal('--');
  readonly activityCount = signal('--');
  readonly failedLoginCount = signal('--');
  readonly lockedAccountCount = signal('--');
  readonly pendingConfirmationCount = signal('--');
  readonly recentActivity = signal<AuthActivityV1[]>([]);

  ngOnInit(): void {
    this.userService.getAll({ skip: 0, take: 100, sort: [{ field: 'userName', dir: 'asc' }] }).subscribe({
      next: (result) => {
        this.userCount.set(String(result.total));
        this.lockedAccountCount.set(String(result.data.filter(u => u.isLockedOut).length));
        this.pendingConfirmationCount.set(String(result.data.filter(u => !u.emailConfirmed).length));
      },
      error: () => {
        this.userCount.set('0');
        this.lockedAccountCount.set('0');
        this.pendingConfirmationCount.set('0');
      }
    });

    this.activityService.getAll({
      skip: 0,
      take: 100,
      sort: [{ field: 'createdUtc', dir: 'desc' }]
    }).subscribe({
      next: (result) => {
        this.activityCount.set(String(result.total));
        this.failedLoginCount.set(String(result.data.filter(a => a.loginOutcome !== 'Success').length));
        this.recentActivity.set(result.data.slice(0, 10));
      },
      error: () => {
        this.activityCount.set('0');
        this.failedLoginCount.set('0');
      }
    });
  }
}
