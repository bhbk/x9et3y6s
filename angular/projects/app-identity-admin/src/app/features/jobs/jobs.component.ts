import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { JobService, JobV1, JobSettingV1 } from 'lib-identity';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import {
  checkIcon,
  xIcon,
  chevronDownIcon,
  chevronRightIcon,
} from '@progress/kendo-svg-icons';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [
    FormsModule,
    KENDO_BUTTONS,
    KENDO_INPUTS,
    KENDO_LABELS,
    KENDO_INDICATORS,
    KENDO_ICONS,
  ],
  template: `
    <div class="p-6">
      @if (isInitialLoad()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else {
        <div class="mb-6">
          <p class="text-gray-500">Manage scheduled jobs and their settings</p>
        </div>

        @if (error()) {
          <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
            {{ error() }}
            <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
          </div>
        }

        <div class="space-y-3">
          @for (job of jobs(); track job.id) {
            <div class="bg-white rounded-lg border border-gray-200 shadow-sm">

              <!-- card header -->
              <div class="flex items-center gap-3 px-4 py-3">
                <div class="flex-1">
                  <span class="font-medium text-gray-900">{{ job.name }}</span>
                  @if (job.description) {
                    <p class="text-sm text-gray-500 mt-0.5">{{ job.description }}</p>
                  }
                </div>

                <span class="text-xs font-mono text-gray-400 mr-2" [title]="getScheduleTooltip(job)">
                  {{ getScheduleDisplay(job) }}
                </span>

                <label class="flex items-center gap-2 cursor-pointer mr-4">
                  <input type="checkbox" kendoCheckBox
                         [checked]="job.isEnabled"
                         (change)="toggleEnabled(job)" />
                  @if (job.isEnabled) {
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

                <button kendoButton fillMode="flat" size="small" (click)="toggleExpanded(job.id)">
                  <kendo-svg-icon
                    [icon]="isExpanded(job.id) ? chevronDownIcon : chevronRightIcon"
                    size="small"
                  ></kendo-svg-icon>
                  Settings ({{ job.settings.length }})
                </button>
              </div>

              <!-- expanded settings -->
              @if (isExpanded(job.id)) {
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
                      @for (setting of getEditingSettings(job.id); track setting.id) {
                        <tr class="border-b border-gray-50">
                          <td class="py-2 pr-4 font-mono text-gray-600">{{ setting.configKey }}</td>
                          <td class="py-2 pr-4">
                            @if (setting.isSecret) {
                              <input kendoTextBox
                                     type="password"
                                     [value]="setting.configValue"
                                     (input)="onSettingChange(job.id, setting.id, $event)"
                                     [style.width.%]="100" />
                            } @else {
                              <input kendoTextBox
                                     [value]="setting.configValue"
                                     (input)="onSettingChange(job.id, setting.id, $event)"
                                     [style.width.%]="100" />
                            }
                          </td>
                          <td class="py-2">
                            @if (isSettingDirty(job.id, setting.id)) {
                              <span class="w-2 h-2 bg-blue-500 rounded-full inline-block" title="Modified"></span>
                            }
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                  <div class="flex justify-end mt-3 gap-2">
                    <button kendoButton fillMode="flat" size="small" (click)="resetSettings(job.id)">
                      Reset
                    </button>
                    <button kendoButton themeColor="primary" size="small"
                            [disabled]="!hasSettingsChanged(job.id) || isSaving()"
                            (click)="saveSettings(job.id)">
                      @if (savingJobId() === job.id) {
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
  `
})
export class JobsComponent implements OnInit {
  private readonly jobService = inject(JobService);

  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;
  readonly chevronDownIcon = chevronDownIcon;
  readonly chevronRightIcon = chevronRightIcon;

  readonly jobs = signal<JobV1[]>([]);
  readonly expandedIds = signal<Set<string>>(new Set());
  readonly isInitialLoad = signal(true);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly savingJobId = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  private editingSettings = new Map<string, JobSettingV1[]>();
  private originalSettings = new Map<string, JobSettingV1[]>();

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.jobService.getAll().subscribe({
      next: (result) => {
        this.jobs.set(result);
        this.editingSettings.clear();
        this.originalSettings.clear();
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.error?.message || err.message || 'Failed to load jobs');
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      }
    });
  }

  getScheduleDisplay(job: JobV1): string {
    const schedules = job.settings.filter(s => s.configKey === 'Schedule');
    if (schedules.length === 0) return '';
    return schedules.map(s => s.configValue).join(', ');
  }

  getScheduleTooltip(job: JobV1): string {
    const schedules = job.settings.filter(s => s.configKey === 'Schedule');
    if (schedules.length === 0) return 'No schedule configured';
    return 'Cron: ' + schedules.map(s => s.configValue).join(', ');
  }

  toggleEnabled(job: JobV1): void {
    const updated = { ...job, isEnabled: !job.isEnabled };

    this.jobService.update(updated).subscribe({
      next: () => this.loadData(),
      error: (err) => {
        this.error.set(err.error?.message || err.message || 'Failed to update job');
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

  private initEditingSettings(jobId: string): void {
    if (this.editingSettings.has(jobId)) return;

    const job = this.jobs().find(j => j.id === jobId);
    if (!job) return;

    const copy = job.settings.map(s => ({ ...s }));
    this.editingSettings.set(jobId, copy);
    this.originalSettings.set(jobId, job.settings.map(s => ({ ...s })));
  }

  getEditingSettings(jobId: string): JobSettingV1[] {
    if (!this.editingSettings.has(jobId)) {
      this.initEditingSettings(jobId);
    }
    return this.editingSettings.get(jobId) ?? [];
  }

  onSettingChange(jobId: string, settingId: string, event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    const settings = this.editingSettings.get(jobId);
    if (!settings) return;

    const setting = settings.find(s => s.id === settingId);
    if (setting) {
      setting.configValue = value;
    }
  }

  isSettingDirty(jobId: string, settingId: string): boolean {
    const editing = this.editingSettings.get(jobId)?.find(s => s.id === settingId);
    const original = this.originalSettings.get(jobId)?.find(s => s.id === settingId);
    if (!editing || !original) return false;
    return editing.configValue !== original.configValue;
  }

  hasSettingsChanged(jobId: string): boolean {
    const editing = this.editingSettings.get(jobId);
    const original = this.originalSettings.get(jobId);
    if (!editing || !original) return false;

    return editing.some((s, i) => s.configValue !== original[i]?.configValue);
  }

  resetSettings(jobId: string): void {
    const original = this.originalSettings.get(jobId);
    if (!original) return;
    this.editingSettings.set(jobId, original.map(s => ({ ...s })));
  }

  saveSettings(jobId: string): void {
    const settings = this.editingSettings.get(jobId);
    if (!settings) return;

    this.isSaving.set(true);
    this.savingJobId.set(jobId);

    this.jobService.updateSettings(jobId, settings).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.savingJobId.set(null);
        this.editingSettings.delete(jobId);
        this.originalSettings.delete(jobId);
        this.loadData();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.savingJobId.set(null);
        this.error.set(err.error?.message || err.message || 'Failed to save settings');
      }
    });
  }
}
