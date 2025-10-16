import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore, QuoteService, QuoteV1 } from 'lib-identity';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { arrowRotateCwIcon, commentIcon } from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, KENDO_BUTTONS, KENDO_INDICATORS, KENDO_ICONS],
  template: `
    <div class="p-6">
      <div class="mb-6">
        <p class="text-gray-500">Manage your account settings and security</p>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
        <a routerLink="/profile" class="bg-white rounded-lg border border-gray-200 p-6 hover:bg-gray-100 hover:shadow-md transition-all">
          <h3 class="text-lg font-medium text-gray-800 mb-2">Profile</h3>
          <p class="text-gray-600 text-sm">Update your personal information</p>
        </a>
        <a routerLink="/security" class="bg-white rounded-lg border border-gray-200 p-6 hover:bg-gray-100 hover:shadow-md transition-all">
          <h3 class="text-lg font-medium text-gray-800 mb-2">Security</h3>
          <p class="text-gray-600 text-sm">Manage your password and security settings</p>
        </a>
        <a routerLink="/sessions" class="bg-white rounded-lg border border-gray-200 p-6 hover:bg-gray-100 hover:shadow-md transition-all">
          <h3 class="text-lg font-medium text-gray-800 mb-2">Sessions</h3>
          <p class="text-gray-600 text-sm">View and manage your active sessions</p>
        </a>
      </div>

      <!-- Quote Card -->
      <div class="bg-white rounded-lg border border-gray-200 overflow-hidden">
        <div class="p-6 bg-gradient-to-br from-blue-50 to-indigo-50">
          <div class="flex justify-between items-start mb-4">
            <h2 class="text-lg font-medium text-gray-800">Quote of the Day</h2>
            <button kendoButton themeColor="primary" (click)="fetchNewQuote()" [disabled]="isLoadingQuote()">
              <kendo-svg-icon [icon]="refreshIcon" size="small"></kendo-svg-icon>
              Get New Quote
            </button>
          </div>
          @if (isLoadingQuote()) {
            <div class="flex items-center justify-center p-8">
              <kendo-loader size="large"></kendo-loader>
            </div>
          } @else if (currentQuote()) {
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
          } @else {
            <div class="text-center p-4">
              <kendo-svg-icon [icon]="quoteIcon" size="xlarge" class="text-gray-200 mb-2"></kendo-svg-icon>
              <p class="text-gray-500">No quote available. Click "Get New Quote" to fetch one.</p>
            </div>
          }
        </div>
        @if (currentQuote()?.category || (currentQuote()?.tags && currentQuote()!.tags!.length > 0)) {
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
        }
      </div>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  readonly authStore = inject(AuthStore);
  private readonly quoteService = inject(QuoteService);

  readonly refreshIcon = arrowRotateCwIcon;
  readonly quoteIcon = commentIcon;

  readonly currentQuote = signal<QuoteV1 | null>(null);
  readonly isLoadingQuote = signal(false);

  ngOnInit(): void {
    this.fetchNewQuote();
  }

  fetchNewQuote(): void {
    this.isLoadingQuote.set(true);

    this.quoteService.getQuote().subscribe({
      next: (quote) => {
        this.currentQuote.set(quote);
        this.isLoadingQuote.set(false);
      },
      error: () => {
        const fallbackQuote: QuoteV1 = {
          globalId: crypto.randomUUID(),
          author: 'System',
          quote: 'Welcome to the Identity Portal. Have a productive day!',
          category: 'system',
          tags: ['welcome']
        };
        this.currentQuote.set(fallbackQuote);
        this.isLoadingQuote.set(false);
      }
    });
  }
}
