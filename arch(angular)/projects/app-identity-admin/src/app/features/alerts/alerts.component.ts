import { Component, inject, OnInit, signal } from '@angular/core';
import { AlertQueueService, EmailQueueV1, TextQueueV1, AlertDeliveryStatus } from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_LAYOUT } from '@progress/kendo-angular-layout';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_DIALOGS } from '@progress/kendo-angular-dialog';
import { SortDescriptor } from '@progress/kendo-data-query';
import { trashIcon, clockIcon, checkCircleIcon, xCircleIcon, arrowRotateCwIcon } from '@progress/kendo-svg-icons';
import { SelectEvent } from '@progress/kendo-angular-layout';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [
    KENDO_GRID,
    KENDO_LAYOUT,
    KENDO_BUTTONS,
    KENDO_INDICATORS,
    KENDO_ICONS,
    KENDO_DIALOGS
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
            <p class="text-gray-500">Monitor queued email and text notifications</p>
          </div>
          <button kendoButton fillMode="outline" (click)="refreshActiveTab()">
            <kendo-svg-icon [icon]="refreshIcon" size="small"></kendo-svg-icon>
            Refresh
          </button>
        </div>

        @if (error()) {
          <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
            {{ error() }}
            <button kendoButton fillMode="flat" themeColor="error" (click)="refreshActiveTab()">Retry</button>
          </div>
        }

        <div class="mb-6 grid grid-cols-1 md:grid-cols-4 gap-4">
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <div class="text-sm text-gray-500">Emails Queued</div>
            <div class="text-2xl font-medium text-gray-900">{{ emailGridData().total }}</div>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <div class="text-sm text-gray-500">Texts Queued</div>
            <div class="text-2xl font-medium text-gray-900">{{ textGridData().total }}</div>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <div class="text-sm text-gray-500">Pending Delivery</div>
            <div class="text-2xl font-medium text-amber-600">{{ pendingCount() }}</div>
          </div>
          <div class="bg-white rounded-lg border border-gray-200 p-4">
            <div class="text-sm text-gray-500">Cancelled</div>
            <div class="text-2xl font-medium text-red-600">{{ cancelledCount() }}</div>
          </div>
        </div>

        <kendo-tabstrip (tabSelect)="onTabSelect($event)">
          <kendo-tabstrip-tab [title]="'Email Queue'" [selected]="true">
            <ng-template kendoTabContent>
              <div class="bg-white rounded-lg border border-gray-200 mt-4">
                <kendo-grid
                  [data]="emailGridData()"
                  [pageSize]="emailPageSize()"
                  [skip]="emailSkip()"
                  [pageable]="pageableSettings"
                  [sortable]="sortSettings"
                  [sort]="emailSort()"
                  [resizable]="true"
                  [loading]="isLoading()"
                  (pageChange)="onEmailPageChange($event)"
                  (sortChange)="onEmailSortChange($event)"
                >
                  <kendo-grid-column field="fromEmail" title="From" [width]="180">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ dataItem.fromDisplay || dataItem.fromEmail || '-' }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="toEmail" title="To" [width]="180">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ dataItem.toDisplay || dataItem.toEmail }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="subject" title="Subject">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ dataItem.subject }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column title="Status" [width]="120">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      @switch (getStatus(dataItem)) {
                        @case ('Pending') {
                          <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-amber-100 text-amber-700 text-xs rounded-full border border-amber-200">
                            <kendo-svg-icon [icon]="clockIcon" size="small"></kendo-svg-icon>
                            Pending
                          </span>
                        }
                        @case ('Cancelled') {
                          <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full border border-red-200">
                            <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                            Cancelled
                          </span>
                        }
                        @case ('Delivered') {
                          <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full border border-green-200">
                            <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                            Delivered
                          </span>
                        }
                      }
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="createdUtc" title="Created" [width]="170">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ formatDate(dataItem.createdUtc) }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="sendAtUtc" title="Send At" [width]="170">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ formatDate(dataItem.sendAtUtc) }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column title="Actions" [width]="80">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <button kendoButton fillMode="flat" themeColor="error" size="small"
                              (click)="confirmDelete(dataItem.id, 'email')" title="Delete">
                        <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
                      </button>
                    </ng-template>
                  </kendo-grid-column>
                </kendo-grid>
              </div>
            </ng-template>
          </kendo-tabstrip-tab>

          <kendo-tabstrip-tab [title]="'Text Queue'">
            <ng-template kendoTabContent>
              <div class="bg-white rounded-lg border border-gray-200 mt-4">
                <kendo-grid
                  [data]="textGridData()"
                  [pageSize]="textPageSize()"
                  [skip]="textSkip()"
                  [pageable]="pageableSettings"
                  [sortable]="sortSettings"
                  [sort]="textSort()"
                  [resizable]="true"
                  [loading]="isLoading()"
                  (pageChange)="onTextPageChange($event)"
                  (sortChange)="onTextSortChange($event)"
                >
                  <kendo-grid-column field="fromPhoneNumber" title="From" [width]="150">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      @if (dataItem.fromPhoneNumber) {
                        <code class="text-xs bg-gray-100 px-1.5 py-0.5 rounded">{{ dataItem.fromPhoneNumber }}</code>
                      } @else {
                        <span class="text-gray-400">-</span>
                      }
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="toPhoneNumber" title="To" [width]="150">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <code class="text-xs bg-gray-100 px-1.5 py-0.5 rounded">{{ dataItem.toPhoneNumber }}</code>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="body" title="Body">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ truncate(dataItem.body, 80) }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column title="Status" [width]="120">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      @switch (getStatus(dataItem)) {
                        @case ('Pending') {
                          <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-amber-100 text-amber-700 text-xs rounded-full border border-amber-200">
                            <kendo-svg-icon [icon]="clockIcon" size="small"></kendo-svg-icon>
                            Pending
                          </span>
                        }
                        @case ('Cancelled') {
                          <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full border border-red-200">
                            <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                            Cancelled
                          </span>
                        }
                        @case ('Delivered') {
                          <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full border border-green-200">
                            <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                            Delivered
                          </span>
                        }
                      }
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="createdUtc" title="Created" [width]="170">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ formatDate(dataItem.createdUtc) }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column field="sendAtUtc" title="Send At" [width]="170">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <span class="text-sm">{{ formatDate(dataItem.sendAtUtc) }}</span>
                    </ng-template>
                  </kendo-grid-column>

                  <kendo-grid-column title="Actions" [width]="80">
                    <ng-template kendoGridCellTemplate let-dataItem>
                      <button kendoButton fillMode="flat" themeColor="error" size="small"
                              (click)="confirmDelete(dataItem.id, 'text')" title="Delete">
                        <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
                      </button>
                    </ng-template>
                  </kendo-grid-column>
                </kendo-grid>
              </div>
            </ng-template>
          </kendo-tabstrip-tab>
        </kendo-tabstrip>

        @if (showDeleteDialog()) {
          <kendo-dialog
            title="Delete Queue Item"
            (close)="closeDeleteDialog()"
            [minWidth]="350"
            [width]="420"
          >
            <p>Are you sure you want to remove this {{ deleteItemType() }} from the queue? This action cannot be undone.</p>
            <kendo-dialog-actions>
              <button kendoButton (click)="closeDeleteDialog()">Cancel</button>
              <button kendoButton themeColor="error" (click)="executeDelete()">Delete</button>
            </kendo-dialog-actions>
          </kendo-dialog>
        }
      }
    </div>
  `
})
export class AlertsComponent implements OnInit {
  private readonly alertQueueService = inject(AlertQueueService);

  readonly trashIcon = trashIcon;
  readonly clockIcon = clockIcon;
  readonly checkIcon = checkCircleIcon;
  readonly xIcon = xCircleIcon;
  readonly refreshIcon = arrowRotateCwIcon;

  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };

  // Email grid state
  readonly emailGridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly emailSkip = signal(0);
  readonly emailSort = signal<SortDescriptor[]>([{ field: 'createdUtc', dir: 'desc' }]);
  readonly emailPageSize = signal(10);

  // Text grid state
  readonly textGridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly textSkip = signal(0);
  readonly textSort = signal<SortDescriptor[]>([{ field: 'createdUtc', dir: 'desc' }]);
  readonly textPageSize = signal(10);

  // Shared state
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);
  readonly activeTab = signal(0);
  private textLoaded = false;

  // Statistics
  readonly pendingCount = signal(0);
  readonly cancelledCount = signal(0);

  // Delete dialog
  readonly showDeleteDialog = signal(false);
  readonly deleteItemType = signal<'email' | 'text'>('email');
  private pendingDeleteId: string | null = null;

  ngOnInit(): void {
    this.loadEmails();
  }

  loadEmails(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const sortDesc = this.emailSort()[0];
    const query = {
      skip: this.emailSkip(),
      take: this.emailPageSize(),
      sort: sortDesc ? [{ field: sortDesc.field, dir: sortDesc.dir as 'asc' | 'desc' }] : undefined
    };

    this.alertQueueService.getEmails(query).subscribe({
      next: (result) => {
        this.emailGridData.set({ data: result.data, total: result.total });
        this.updateStats();
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load email queue');
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      }
    });
  }

  loadTexts(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const sortDesc = this.textSort()[0];
    const query = {
      skip: this.textSkip(),
      take: this.textPageSize(),
      sort: sortDesc ? [{ field: sortDesc.field, dir: sortDesc.dir as 'asc' | 'desc' }] : undefined
    };

    this.alertQueueService.getTexts(query).subscribe({
      next: (result) => {
        this.textGridData.set({ data: result.data, total: result.total });
        this.textLoaded = true;
        this.updateStats();
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load text queue');
        this.isLoading.set(false);
      }
    });
  }

  refreshActiveTab(): void {
    if (this.activeTab() === 0) {
      this.loadEmails();
    } else {
      this.loadTexts();
    }
  }

  onTabSelect(event: SelectEvent): void {
    this.activeTab.set(event.index);
    if (event.index === 1 && !this.textLoaded) {
      this.loadTexts();
    }
  }

  onEmailPageChange(event: PageChangeEvent): void {
    this.emailSkip.set(event.skip);
    this.emailPageSize.set(event.take);
    this.loadEmails();
  }

  onEmailSortChange(sort: SortDescriptor[]): void {
    this.emailSort.set(sort);
    this.loadEmails();
  }

  onTextPageChange(event: PageChangeEvent): void {
    this.textSkip.set(event.skip);
    this.textPageSize.set(event.take);
    this.loadTexts();
  }

  onTextSortChange(sort: SortDescriptor[]): void {
    this.textSort.set(sort);
    this.loadTexts();
  }

  getStatus(item: EmailQueueV1 | TextQueueV1): AlertDeliveryStatus {
    if (item.isCancelled) return 'Cancelled';
    if (item.deliveredUtc) return 'Delivered';
    return 'Pending';
  }

  confirmDelete(id: string, type: 'email' | 'text'): void {
    this.pendingDeleteId = id;
    this.deleteItemType.set(type);
    this.showDeleteDialog.set(true);
  }

  closeDeleteDialog(): void {
    this.showDeleteDialog.set(false);
    this.pendingDeleteId = null;
  }

  executeDelete(): void {
    if (!this.pendingDeleteId) return;

    const id = this.pendingDeleteId;
    const type = this.deleteItemType();
    this.closeDeleteDialog();

    const obs = type === 'email'
      ? this.alertQueueService.deleteEmail(id)
      : this.alertQueueService.deleteText(id);

    obs.subscribe({
      next: () => {
        if (type === 'email') {
          this.loadEmails();
        } else {
          this.loadTexts();
        }
      },
      error: (err) => {
        this.error.set(err.message || `Failed to delete ${type}`);
      }
    });
  }

  formatDate(dateStr: string): string {
    try {
      return DateTime.fromISO(dateStr).toLocaleString(DateTime.DATETIME_MED);
    } catch {
      return dateStr;
    }
  }

  truncate(text: string, max: number): string {
    return text.length > max ? text.substring(0, max) + '...' : text;
  }

  private updateStats(): void {
    let pending = 0;
    let cancelled = 0;

    for (const item of this.emailGridData().data as EmailQueueV1[]) {
      if (item.isCancelled) cancelled++;
      else if (!item.deliveredUtc) pending++;
    }

    for (const item of this.textGridData().data as TextQueueV1[]) {
      if (item.isCancelled) cancelled++;
      else if (!item.deliveredUtc) pending++;
    }

    this.pendingCount.set(pending);
    this.cancelledCount.set(cancelled);
  }
}
