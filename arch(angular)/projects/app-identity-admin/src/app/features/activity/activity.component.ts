import { Component, inject, OnInit, signal } from '@angular/core';
import { AuthActivityService, AuthActivityV1, UserService, AudienceService, UserV1, AudienceV1 } from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, FilterableSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_DROPDOWNS } from '@progress/kendo-angular-dropdowns';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { SortDescriptor } from '@progress/kendo-data-query';
import { checkCircleIcon, xCircleIcon, warningTriangleIcon, arrowRotateCwIcon } from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-activity',
  standalone: true,
  imports: [
    KENDO_GRID,
    KENDO_BUTTONS,
    KENDO_LABELS,
    KENDO_DROPDOWNS,
    KENDO_INDICATORS,
    KENDO_ICONS
  ],
  template: `
    <div class="p-6">
      @if (isInitialLoad()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else {
      <div class="flex justify-between items-center mb-6">
        <div>
          <p class="text-gray-500">Monitor authentication activity and login attempts</p>
        </div>
        <button kendoButton fillMode="outline" (click)="loadData()">
          <kendo-svg-icon [icon]="refreshIcon" size="small"></kendo-svg-icon>
          Refresh
        </button>
      </div>

      @if (error()) {
        <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
        </div>
      }

      <!-- Summary Statistics -->
      <div class="mb-6 grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Total Activities</div>
          <div class="text-2xl font-medium text-gray-900">{{ gridData().total }}</div>
        </div>
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Successful Logins</div>
          <div class="text-2xl font-medium text-green-600">{{ successCount() }}</div>
        </div>
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Failed Attempts</div>
          <div class="text-2xl font-medium text-red-600">{{ failureCount() }}</div>
        </div>
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Locked Out</div>
          <div class="text-2xl font-medium text-amber-600">{{ lockedCount() }}</div>
        </div>
      </div>

      <div class="bg-white rounded-lg border border-gray-200">
        <kendo-grid
          [data]="gridData()"
          [pageSize]="pageSize()"
          [skip]="skip()"
          [pageable]="pageableSettings"
          [sortable]="sortSettings"
          [sort]="sort()"
          [filterable]="filterSettings"
          [resizable]="true"
          [loading]="isLoading()"
          (pageChange)="onPageChange($event)"
          (sortChange)="onSortChange($event)"
        >
          <kendo-grid-column field="createdUtc" title="Time" [width]="180">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm">{{ formatDate(dataItem.createdUtc) }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="loginType" title="Type" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="inline-flex items-center px-2 py-0.5 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200">
                {{ formatLoginType(dataItem.loginType) }}
              </span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="loginOutcome" title="Outcome" [width]="120">
            <ng-template kendoGridCellTemplate let-dataItem>
              @switch (dataItem.loginOutcome) {
                @case ('Success') {
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full border border-green-200">
                    <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                    Success
                  </span>
                }
                @case ('Failure') {
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full border border-red-200">
                    <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                    Failed
                  </span>
                }
                @case ('LockedOut') {
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-amber-100 text-amber-700 text-xs rounded-full border border-amber-200">
                    <kendo-svg-icon [icon]="warningIcon" size="small"></kendo-svg-icon>
                    Locked
                  </span>
                }
                @default {
                  <span class="inline-flex items-center px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full border border-gray-200">
                    {{ dataItem.loginOutcome }}
                  </span>
                }
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="userId" title="User" [width]="180">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.userId) {
                <span class="text-sm">{{ getUserName(dataItem.userId) }}</span>
              } @else {
                <span class="text-gray-400">-</span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="audienceIds" title="Audience" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.audienceIds?.length) {
                <span class="text-sm">{{ getAudienceNames(dataItem.audienceIds) }}</span>
              } @else {
                <span class="text-gray-400">-</span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="localEndpoint" title="Local Address" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.localEndpoint) {
                <code class="text-xs bg-gray-100 px-1.5 py-0.5 rounded">{{ dataItem.localEndpoint }}</code>
              } @else {
                <span class="text-gray-400">-</span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="remoteEndpoint" title="Remote Address" [width]="140">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.remoteEndpoint) {
                <code class="text-xs bg-gray-100 px-1.5 py-0.5 rounded">{{ dataItem.remoteEndpoint }}</code>
              } @else {
                <span class="text-gray-400">-</span>
              }
            </ng-template>
          </kendo-grid-column>
        </kendo-grid>
      </div>
      }
    </div>
  `
})
export class ActivityComponent implements OnInit {
  private readonly activityService = inject(AuthActivityService);
  private readonly userService = inject(UserService);
  private readonly audienceService = inject(AudienceService);

  // Icons
  readonly checkIcon = checkCircleIcon;
  readonly xIcon = xCircleIcon;
  readonly warningIcon = warningTriangleIcon;
  readonly refreshIcon = arrowRotateCwIcon;

  // Grid settings
  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };
  readonly filterSettings = 'menu' as FilterableSettings;

  // State
  readonly users = signal<Map<string, UserV1>>(new Map());
  readonly audiences = signal<Map<string, AudienceV1>>(new Map());
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'createdUtc', dir: 'desc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  // Statistics
  readonly successCount = signal(0);
  readonly failureCount = signal(0);
  readonly lockedCount = signal(0);

  ngOnInit(): void {
    this.loadLookupData();
  }

  private loadLookupData(): void {
    // Load users and audiences for display names
    this.userService.getAll({ take: 1000, sort: [{ field: 'userName', dir: 'asc' }] }).subscribe({
      next: (result) => {
        const map = new Map<string, UserV1>();
        result.data.forEach(u => map.set(u.id, u));
        this.users.set(map);
      }
    });

    this.audienceService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => {
        const map = new Map<string, AudienceV1>();
        result.data.forEach(a => map.set(a.id, a));
        this.audiences.set(map);
        this.loadData();
      },
      error: () => {
        this.loadData();
      }
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const sortDesc = this.sort()[0];
    const query = {
      skip: this.skip(),
      take: this.pageSize(),
      sort: sortDesc ? [{ field: sortDesc.field, dir: sortDesc.dir as 'asc' | 'desc' }] : undefined
    };

    this.activityService.getAll(query).subscribe({
      next: (result) => {
        this.gridData.set({
          data: result.data,
          total: result.total
        });

        // Calculate statistics from visible data
        let success = 0, failure = 0, locked = 0;
        result.data.forEach(item => {
          if (item.loginOutcome === 'Success') success++;
          else if (item.loginOutcome === 'Failure') failure++;
          else if (item.loginOutcome === 'LockedOut') locked++;
        });
        this.successCount.set(success);
        this.failureCount.set(failure);
        this.lockedCount.set(locked);

        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load activity');
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      }
    });
  }

  onPageChange(event: PageChangeEvent): void {
    this.skip.set(event.skip);
    this.pageSize.set(event.take);
    this.loadData();
  }

  onSortChange(sort: SortDescriptor[]): void {
    this.sort.set(sort);
    this.loadData();
  }

  getUserName(userId: string): string {
    const user = this.users().get(userId);
    return user ? `${user.firstName} ${user.lastName}` : userId.substring(0, 8) + '...';
  }

  getAudienceNames(audienceIds: string[]): string {
    return audienceIds.map(id => {
      const audience = this.audiences().get(id);
      return audience ? audience.name : id.substring(0, 8) + '...';
    }).join(', ');
  }

  formatDate(dateStr: string): string {
    try {
      return DateTime.fromISO(dateStr).toLocaleString(DateTime.DATETIME_MED);
    } catch {
      return dateStr;
    }
  }

  formatLoginType(type: string): string {
    const typeMap: Record<string, string> = {
      'ResourceOwner': 'Password',
      'ClientCredential': 'Client Credential',
      'AuthorizationCode': 'Auth Code',
      'DeviceCode': 'Device Code',
      'Implicit': 'Implicit',
      'Refresh': 'Refresh Token'
    };
    return typeMap[type] || type;
  }
}
