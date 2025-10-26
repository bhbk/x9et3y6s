import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CdkDragDrop, CdkDrag, CdkDropList, CdkDragPlaceholder, moveItemInArray } from '@angular/cdk/drag-drop';
import { LLMProviderService, LLMProviderV1, LLMProviderSettingV1, LLMProviderContext } from 'lib-identity';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_LAYOUT } from '@progress/kendo-angular-layout';
import {
  handleDragIcon,
  checkIcon,
  xIcon,
  chevronDownIcon,
  chevronRightIcon,
  saveIcon,
} from '@progress/kendo-svg-icons';
import { SelectEvent } from '@progress/kendo-angular-layout';

@Component({
  selector: 'app-llm-providers',
  standalone: true,
  imports: [
    FormsModule,
    CdkDrag,
    CdkDropList,
    CdkDragPlaceholder,
    KENDO_BUTTONS,
    KENDO_INPUTS,
    KENDO_LABELS,
    KENDO_INDICATORS,
    KENDO_ICONS,
    KENDO_LAYOUT,
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
            <p class="text-gray-500">Manage LLM providers and failover priority per context</p>
          </div>
          @if (hasOrderChanged()) {
            <button kendoButton themeColor="primary" (click)="saveOrder()" [disabled]="isSaving()">
              @if (isSaving()) {
                <kendo-loader size="small" themeColor="light"></kendo-loader>
              }
              <kendo-svg-icon [icon]="saveIcon" size="small"></kendo-svg-icon>
              Save Order
            </button>
          }
        </div>

        @if (error()) {
          <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
            {{ error() }}
            <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
          </div>
        }

        <kendo-tabstrip (tabSelect)="onTabSelect($event)">
          @for (ctx of contexts; track ctx) {
            <kendo-tabstrip-tab [title]="ctx" [selected]="activeContext() === ctx">
              <ng-template kendoTabContent>
                <div class="pt-4">
                  @if (filteredProviders().length === 0) {
                    <div class="text-center py-8 text-gray-500">
                      No LLM providers configured for {{ activeContext() }} context.
                    </div>
                  } @else {
                    <div cdkDropList (cdkDropListDropped)="onDrop($event)" class="space-y-3">
                      @for (provider of filteredProviders(); track provider.id) {
                        <div cdkDrag class="bg-white rounded-lg border border-gray-200 shadow-sm"
                             [cdkDragData]="provider">

                          <div *cdkDragPlaceholder class="bg-blue-50 border-2 border-dashed border-blue-300 rounded-lg h-16"></div>

                          <div class="flex items-center gap-3 px-4 py-3">
                            <div cdkDragHandle class="cursor-grab text-gray-400 hover:text-gray-600">
                              <kendo-svg-icon [icon]="dragIcon" size="medium"></kendo-svg-icon>
                            </div>

                            <span class="text-sm font-mono text-gray-400 w-6">{{ provider.failoverOrder }}</span>

                            <span class="font-medium text-gray-900 flex-1">{{ provider.name }}</span>

                            <label class="flex items-center gap-2 cursor-pointer mr-4">
                              <input type="checkbox" kendoCheckBox
                                     [checked]="provider.isEnabled"
                                     (change)="toggleEnabled(provider)" />
                              @if (provider.isEnabled) {
                                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full border border-green-200">
                                  <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                                  Enabled
                                </span>
                              } @else {
                                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full border border-gray-200">
                                  <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                                  Disabled
                                </span>
                              }
                            </label>

                            <button kendoButton fillMode="flat" size="small" (click)="toggleExpanded(provider.id)">
                              <kendo-svg-icon
                                [icon]="isExpanded(provider.id) ? chevronDownIcon : chevronRightIcon"
                                size="small"
                              ></kendo-svg-icon>
                              Settings ({{ provider.settings.length }})
                            </button>
                          </div>

                          @if (isExpanded(provider.id)) {
                            <div class="border-t border-gray-200 px-4 py-3">
                              <table class="w-full text-sm">
                                <thead>
                                  <tr class="text-left text-gray-500 border-b border-gray-100">
                                    <th class="pb-2 pr-4 font-medium w-48">Key</th>
                                    <th class="pb-2 pr-4 font-medium">Value</th>
                                    <th class="pb-2 font-medium w-16"></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  @for (setting of getEditingSettings(provider.id); track setting.id) {
                                    <tr class="border-b border-gray-50">
                                      <td class="py-2 pr-4 font-mono text-gray-600">{{ setting.configKey }}</td>
                                      <td class="py-2 pr-4">
                                        @if (setting.isSecret) {
                                          <input kendoTextBox
                                                 type="password"
                                                 [value]="setting.configValue ?? ''"
                                                 (input)="onSettingChange(provider.id, setting.id, $event)"
                                                 [style.width.%]="100" />
                                        } @else {
                                          <input kendoTextBox
                                                 [value]="setting.configValue ?? ''"
                                                 (input)="onSettingChange(provider.id, setting.id, $event)"
                                                 [style.width.%]="100" />
                                        }
                                      </td>
                                      <td class="py-2">
                                        @if (isSettingDirty(provider.id, setting.id)) {
                                          <span class="w-2 h-2 bg-blue-500 rounded-full inline-block" title="Modified"></span>
                                        }
                                      </td>
                                    </tr>
                                  }
                                </tbody>
                              </table>
                              <div class="flex justify-end mt-3 gap-2">
                                <button kendoButton fillMode="flat" size="small" (click)="resetSettings(provider.id)">
                                  Reset
                                </button>
                                <button kendoButton themeColor="primary" size="small"
                                        [disabled]="!hasSettingsChanged(provider.id) || isSaving()"
                                        (click)="saveSettings(provider.id)">
                                  @if (savingProviderId() === provider.id) {
                                    <kendo-loader size="small" themeColor="light"></kendo-loader>
                                  }
                                  Save Settings
                                </button>
                              </div>
                            </div>
                          }
                        </div>
                      }
                    </div>
                  }
                </div>
              </ng-template>
            </kendo-tabstrip-tab>
          }
        </kendo-tabstrip>
      }
    </div>
  `
})
export class LlmProvidersComponent implements OnInit {
  private readonly llmProviderService = inject(LLMProviderService);

  readonly dragIcon = handleDragIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;
  readonly chevronDownIcon = chevronDownIcon;
  readonly chevronRightIcon = chevronRightIcon;
  readonly saveIcon = saveIcon;

  readonly contexts: LLMProviderContext[] = ['Admin', 'User', 'Public'];
  readonly activeContext = signal<LLMProviderContext>('Admin');

  readonly allProviders = signal<LLMProviderV1[]>([]);
  readonly filteredProviders = computed(() =>
    this.allProviders().filter(p => p.context === this.activeContext())
  );
  readonly expandedIds = signal<Set<string>>(new Set());
  readonly isInitialLoad = signal(true);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly savingProviderId = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly hasOrderChanged = signal(false);

  /*
   * working copies of settings per provider, keyed by provider id.
   * original values are kept to detect dirty state.
   */
  private editingSettings = new Map<string, LLMProviderSettingV1[]>();
  private originalSettings = new Map<string, LLMProviderSettingV1[]>();

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.llmProviderService.getAll().subscribe({
      next: (result) => {
        this.allProviders.set(result);
        this.hasOrderChanged.set(false);
        this.editingSettings.clear();
        this.originalSettings.clear();
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || err.message || 'Failed to load providers');
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      }
    });
  }

  onTabSelect(event: SelectEvent): void {
    this.activeContext.set(this.contexts[event.index]);
    this.hasOrderChanged.set(false);
  }

  onDrop(event: CdkDragDrop<LLMProviderV1[]>): void {
    const contextProviders = [...this.filteredProviders()];
    moveItemInArray(contextProviders, event.previousIndex, event.currentIndex);
    contextProviders.forEach((p, i) => p.failoverOrder = i + 1);

    // Merge back into allProviders
    const all = this.allProviders().map(p =>
      p.context === this.activeContext()
        ? contextProviders.find(cp => cp.id === p.id) ?? p
        : p
    );
    this.allProviders.set(all);
    this.hasOrderChanged.set(true);
  }

  saveOrder(): void {
    this.isSaving.set(true);
    const order = this.filteredProviders().map(p => ({ id: p.id, failoverOrder: p.failoverOrder }));

    this.llmProviderService.updateOrder(order).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.hasOrderChanged.set(false);
        this.loadData();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to save order');
      }
    });
  }

  toggleEnabled(provider: LLMProviderV1): void {
    const updated = { ...provider, isEnabled: !provider.isEnabled };

    this.llmProviderService.update(updated).subscribe({
      next: () => this.loadData(),
      error: (err) => {
        this.error.set(err.error?.message || err.message || 'Failed to update provider');
      }
    });
  }

  toggleExpanded(id: string): void {
    const current = new Set(this.expandedIds());
    if (current.has(id)) {
      current.delete(id);
    } else {
      current.add(id);
      this.initEditingSettings(id);
    }
    this.expandedIds.set(current);
  }

  isExpanded(id: string): boolean {
    return this.expandedIds().has(id);
  }

  private initEditingSettings(providerId: string): void {
    if (this.editingSettings.has(providerId)) return;

    const provider = this.allProviders().find(p => p.id === providerId);
    if (!provider) return;

    const copy = provider.settings.map(s => ({ ...s }));
    this.editingSettings.set(providerId, copy);
    this.originalSettings.set(providerId, provider.settings.map(s => ({ ...s })));
  }

  getEditingSettings(providerId: string): LLMProviderSettingV1[] {
    if (!this.editingSettings.has(providerId)) {
      this.initEditingSettings(providerId);
    }
    return this.editingSettings.get(providerId) ?? [];
  }

  onSettingChange(providerId: string, settingId: string, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    const settings = this.editingSettings.get(providerId);
    if (!settings) return;

    const setting = settings.find(s => s.id === settingId);
    if (setting) {
      setting.configValue = value;
    }
  }

  isSettingDirty(providerId: string, settingId: string): boolean {
    const editing = this.editingSettings.get(providerId)?.find(s => s.id === settingId);
    const original = this.originalSettings.get(providerId)?.find(s => s.id === settingId);
    if (!editing || !original) return false;
    return editing.configValue !== original.configValue;
  }

  hasSettingsChanged(providerId: string): boolean {
    const editing = this.editingSettings.get(providerId);
    const original = this.originalSettings.get(providerId);
    if (!editing || !original) return false;

    return editing.some((s, i) => s.configValue !== original[i]?.configValue);
  }

  resetSettings(providerId: string): void {
    const original = this.originalSettings.get(providerId);
    if (!original) return;
    this.editingSettings.set(providerId, original.map(s => ({ ...s })));
  }

  saveSettings(providerId: string): void {
    const settings = this.editingSettings.get(providerId);
    if (!settings) return;

    this.isSaving.set(true);
    this.savingProviderId.set(providerId);

    this.llmProviderService.updateSettings(providerId, settings).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.savingProviderId.set(null);
        this.editingSettings.delete(providerId);
        this.originalSettings.delete(providerId);
        this.loadData();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.savingProviderId.set(null);
        this.error.set(err.error?.message || err.message || 'Failed to save settings');
      }
    });
  }
}
