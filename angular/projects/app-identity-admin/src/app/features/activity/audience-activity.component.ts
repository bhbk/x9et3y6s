import { Component, inject, OnInit, signal } from '@angular/core';
import { AudienceActivityService, AudienceActivityV1, AudienceService, AudienceV1, UserService, UserV1 } from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, FilterableSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_DROPDOWNS } from '@progress/kendo-angular-dropdowns';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { SortDescriptor } from '@progress/kendo-data-query';
import { checkCircleIcon, xCircleIcon, warningTriangleIcon, arrowRotateCwIcon, filterIcon } from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-audience-activity',
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
          <p class="text-gray-500">Authentication activity grouped by audience — which applications are being accessed</p>
        </div>
        <div class="flex items-center gap-3">
          <kendo-dropdownlist
            [data]="audienceFilterItems()"
            [textField]="'name'"
            [valueField]="'id'"
            [value]="selectedAudience()"
            [valuePrimitive]="true"
            (valueChange)="onAudienceFilterChange($event)"
            [style.width.px]="220"
          ></kendo-dropdownlist>
          <button kendoButton fillMode="outline" (click)="loadData()">
            <kendo-svg-icon [icon]="refreshIcon" size="small"></kendo-svg-icon>
            Refresh
          </button>
        </div>
      </div>

      @if (error()) {
        <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
        </div>
      }

      <!-- Summary Statistics (audience-centric) -->
      <div class="mb-6 grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Total Access Events</div>
          <div class="text-2xl font-medium text-gray-900">{{ gridData().total }}</div>
        </div>
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Unique Audiences</div>
          <div class="text-2xl font-medium text-indigo-600">{{ uniqueAudienceCount() }}</div>
        </div>
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Unique Users</div>
          <div class="text-2xl font-medium text-blue-600">{{ uniqueUserCount() }}</div>
        </div>
        <div class="bg-white rounded-lg border border-gray-200 p-4">
          <div class="text-sm text-gray-500">Success Rate</div>
          <div class="text-2xl font-medium" [class]="successRate() >= 90 ? 'text-green-600' : successRate() >= 70 ? 'text-amber-600' : 'text-red-600'">
            {{ successRate() }}%
          </div>
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
          <kendo-grid-column field="audienceName" title="Audience" [width]="200">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.audienceName) {
                <span class="inline-flex items-center px-2 py-0.5 bg-indigo-50 text-indigo-700 text-sm font-medium rounded border border-indigo-200">
                  {{ dataItem.audienceName }}
                </span>
              } @else {
                <span class="text-gray-400">-</span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="userId" title="Accessed By" [width]="180">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.userId) {
                <span class="text-sm">{{ getUserName(dataItem.userId) }}</span>
              } @else {
                <span class="text-gray-400">-</span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="created" title="Time" [width]="180">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm">{{ formatDate(dataItem.created) }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="loginType" title="Grant Type" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="inline-flex items-center px-2 py-0.5 bg-blue-50 text-blue-700 text-xs rounded border border-blue-200">
                {{ formatLoginType(dataItem.loginType) }}
              </span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="loginOutcome" title="Result" [width]="120">
            <ng-template kendoGridCellTemplate let-dataItem>
              @switch (dataItem.loginOutcome) {
                @case ('Success') {
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full border border-green-200">
                    <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                    Granted
                  </span>
                }
                @case ('Failure') {
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full border border-red-200">
                    <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                    Denied
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

          <kendo-grid-column field="remoteEndpoint" title="Source" [width]="140">
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
export class AudienceActivityComponent implements OnInit {
  private readonly activityService = inject(AudienceActivityService);
  private readonly audienceService = inject(AudienceService);
  private readonly userService = inject(UserService);

  // Icons
  readonly checkIcon = checkCircleIcon;
  readonly xIcon = xCircleIcon;
  readonly warningIcon = warningTriangleIcon;
  readonly refreshIcon = arrowRotateCwIcon;
  readonly filterIcon = filterIcon;

  // Grid settings
  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };
  readonly filterSettings = 'menu' as FilterableSettings;

  // State
  readonly users = signal<Map<string, UserV1>>(new Map());
  readonly audiences = signal<AudienceV1[]>([]);
  readonly audienceFilterItems = signal<{ id: string; name: string }[]>([]);
  readonly selectedAudience = signal<string | null>(null);
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'audienceName', dir: 'asc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  // Statistics (audience-centric)
  readonly uniqueAudienceCount = signal(0);
  readonly uniqueUserCount = signal(0);
  readonly successRate = signal(0);

  ngOnInit(): void {
    this.loadLookupData();
  }

  private loadLookupData(): void {
    let lookupsDone = 0;
    const checkReady = () => { if (++lookupsDone >= 2) this.loadData(); };

    this.userService.getAll({ take: 1000, sort: [{ field: 'userName', dir: 'asc' }] }).subscribe({
      next: (result) => {
        const map = new Map<string, UserV1>();
        result.data.forEach(u => map.set(u.id, u));
        this.users.set(map);
        checkReady();
      },
      error: () => checkReady()
    });

    this.audienceService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => {
        this.audiences.set(result.data);
        this.audienceFilterItems.set([
          { id: '', name: 'All Audiences' },
          ...result.data.map(a => ({ id: a.id, name: a.name }))
        ]);
        checkReady();
      },
      error: () => checkReady()
    });
  }

  onAudienceFilterChange(audienceId: string): void {
    this.selectedAudience.set(audienceId || null);
    this.skip.set(0);
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const sortDesc = this.sort()[0];
    const query: Record<string, unknown> = {
      skip: this.skip(),
      take: this.pageSize(),
      sort: sortDesc ? [{ field: sortDesc.field, dir: sortDesc.dir as 'asc' | 'desc' }] : undefined
    };

    if (this.selectedAudience()) {
      query['filter'] = {
        logic: 'and',
        filters: [{ field: 'audienceId', operator: 'eq', value: this.selectedAudience() }]
      };
    }

    this.activityService.getAll(query).subscribe({
      next: (result) => {
        this.gridData.set({
          data: result.data,
          total: result.total
        });

        const audienceSet = new Set<string>();
        const userSet = new Set<string>();
        let successCount = 0;

        result.data.forEach(item => {
          if (item.audienceName) audienceSet.add(item.audienceName);
          if (item.userId) userSet.add(item.userId);
          if (item.loginOutcome === 'Success') successCount++;
        });

        this.uniqueAudienceCount.set(audienceSet.size);
        this.uniqueUserCount.set(userSet.size);
        this.successRate.set(result.data.length > 0 ? Math.round((successCount / result.data.length) * 100) : 0);

        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load audience activity');
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
