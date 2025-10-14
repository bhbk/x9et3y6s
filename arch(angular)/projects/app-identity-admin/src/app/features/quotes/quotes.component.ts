import { Component, inject, OnInit, signal } from '@angular/core';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { SortDescriptor } from '@progress/kendo-data-query';
import { arrowRotateCwIcon, commentIcon } from '@progress/kendo-svg-icons';
import { QuoteService, AdminQuoteService, QuoteV1 } from 'lib-identity';

@Component({
  selector: 'app-quotes',
  standalone: true,
  imports: [
    KENDO_GRID,
    KENDO_BUTTONS,
    KENDO_INDICATORS,
    KENDO_ICONS
  ],
  template: `
    <div class="p-6">
      <div class="flex justify-between items-center mb-6">
        <div>
          <p class="text-gray-500">Manage quotes for users</p>
        </div>
        <button kendoButton themeColor="primary" (click)="fetchNewQuote()" [disabled]="isLoading()">
          <kendo-svg-icon [icon]="refreshIcon" size="small"></kendo-svg-icon>
          Get New Quote
        </button>
      </div>

      @if (error()) {
        <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="fetchNewQuote()">Retry</button>
        </div>
      }

      <!-- All Quotes Grid -->
      <div class="mb-8">
        @if (gridError()) {
          <div class="p-3 bg-red-50 border border-red-200 rounded text-red-700">
            {{ gridError() }}
            <button kendoButton fillMode="flat" themeColor="error" (click)="loadAllQuotes()">Retry</button>
          </div>
        } @else {
          <kendo-grid
            [data]="gridData()"
            [pageSize]="pageSize()"
            [skip]="skip()"
            [pageable]="pageableSettings"
            [sortable]="sortSettings"
            [sort]="sort()"
            [resizable]="true"
            [loading]="isGridLoading()"
            (pageChange)="onPageChange($event)"
            (sortChange)="onSortChange($event)"
          >
            <kendo-grid-column field="author" title="Author" [width]="180"></kendo-grid-column>
            <kendo-grid-column field="quote" title="Quote">
              <ng-template kendoGridCellTemplate let-dataItem>
                <span class="italic text-gray-700">"{{ dataItem.quote }}"</span>
              </ng-template>
            </kendo-grid-column>
            <kendo-grid-column field="category" title="Category" [width]="120"></kendo-grid-column>
            <kendo-grid-column field="tags" title="Tags" [width]="180">
              <ng-template kendoGridCellTemplate let-dataItem>
                {{ dataItem.tags?.join(', ') || '' }}
              </ng-template>
            </kendo-grid-column>
          </kendo-grid>
        }
      </div>

      <!-- Current Quote -->
      @if (isLoading()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else if (currentQuote()) {
        <div class="bg-white rounded-lg shadow-lg overflow-hidden">
          <div class="p-8 bg-gradient-to-br from-blue-50 to-indigo-50">
            <div class="flex items-start gap-4">
              <kendo-svg-icon [icon]="quoteIcon" size="xlarge" class="text-blue-200 flex-shrink-0"></kendo-svg-icon>
              <div>
                <blockquote class="text-xl text-gray-800 leading-relaxed italic">
                  "{{ currentQuote()?.quote }}"
                </blockquote>
                <p class="mt-4 text-gray-600 font-medium">
                  — {{ currentQuote()?.author }}
                </p>
              </div>
            </div>
          </div>
          <div class="p-4 border-t border-gray-100">
            <div class="flex flex-wrap gap-4 text-sm text-gray-500">
              @if (currentQuote()?.category) {
                <div>
                  <span class="font-medium">Category:</span>
                  <span class="ml-1 px-2 py-0.5 bg-gray-100 rounded">{{ currentQuote()?.category }}</span>
                </div>
              }
              @if (currentQuote()?.tags && currentQuote()!.tags!.length > 0) {
                <div class="flex items-center gap-2">
                  <span class="font-medium">Tags:</span>
                  @for (tag of currentQuote()?.tags; track tag) {
                    <span class="px-2 py-0.5 bg-blue-50 text-blue-600 rounded">{{ tag }}</span>
                  }
                </div>
              }
            </div>
          </div>
        </div>
      } @else {
        <div class="bg-white rounded-lg shadow p-12 text-center">
          <kendo-svg-icon [icon]="quoteIcon" size="xlarge" class="text-gray-200 mb-4"></kendo-svg-icon>
          <p class="text-gray-500">No quotes loaded yet. Click "Get New Quote" to fetch one.</p>
        </div>
      }
    </div>
  `
})
export class QuotesComponent implements OnInit {
  private readonly quoteService = inject(QuoteService);
  private readonly adminQuoteService = inject(AdminQuoteService);

  readonly refreshIcon = arrowRotateCwIcon;
  readonly quoteIcon = commentIcon;

  // Grid settings
  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };

  // State
  readonly currentQuote = signal<QuoteV1 | null>(null);
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'author', dir: 'asc' }]);
  readonly isLoading = signal(false);
  readonly error = signal<string | null>(null);
  readonly isGridLoading = signal(false);
  readonly gridError = signal<string | null>(null);

  ngOnInit(): void {
    this.fetchNewQuote();
    this.loadAllQuotes();
  }

  fetchNewQuote(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.quoteService.getQuote().subscribe({
      next: (quote) => {
        this.currentQuote.set(quote);
        this.isLoading.set(false);
      },
      error: () => {
        const fallbackQuote: QuoteV1 = {
          globalId: crypto.randomUUID(),
          author: 'System',
          quote: 'Welcome to the Identity Management System. Configure your quote service to display inspirational quotes.',
          category: 'system',
          tags: ['welcome']
        };
        this.currentQuote.set(fallbackQuote);
        this.isLoading.set(false);
      }
    });
  }

  loadAllQuotes(): void {
    this.isGridLoading.set(true);
    this.gridError.set(null);

    const sortDesc = this.sort()[0];
    const query = {
      skip: this.skip(),
      take: this.pageSize(),
      sort: sortDesc ? [{ field: sortDesc.field, dir: sortDesc.dir as 'asc' | 'desc' }] : undefined
    };

    this.adminQuoteService.getAll(query).subscribe({
      next: (result) => {
        this.gridData.set({
          data: result.data,
          total: result.total
        });
        this.isGridLoading.set(false);
      },
      error: () => {
        this.gridError.set('Failed to load quotes. The admin API may be unavailable.');
        this.isGridLoading.set(false);
      }
    });
  }

  onPageChange(event: PageChangeEvent): void {
    this.skip.set(event.skip);
    this.pageSize.set(event.take);
    this.loadAllQuotes();
  }

  onSortChange(sort: SortDescriptor[]): void {
    this.sort.set(sort);
    this.loadAllQuotes();
  }
}
