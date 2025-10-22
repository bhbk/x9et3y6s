import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import {
  AudienceEntitlementService, AudienceEntitlementV1, AudienceEntitlementCreate, AudienceEntitlementUpdate,
  EntitlementTypeV1, EntitlementScopeV1, AudienceService, AudienceV1,
  IssuerService, IssuerV1
} from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, FilterableSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_DROPDOWNS } from '@progress/kendo-angular-dropdowns';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_DIALOGS } from '@progress/kendo-angular-dialog';
import { SortDescriptor } from '@progress/kendo-data-query';
import { plusIcon, pencilIcon, trashIcon, checkIcon, xIcon } from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-audience-entitlements',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    KENDO_GRID,
    KENDO_BUTTONS,
    KENDO_INPUTS,
    KENDO_LABELS,
    KENDO_DROPDOWNS,
    KENDO_INDICATORS,
    KENDO_ICONS,
    KENDO_DIALOGS
  ],
  template: `
    <div>
      @if (isInitialLoad()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else {
      <div class="flex justify-between items-center mb-6">
        <div>
          <p class="text-gray-500">Manage audience (service) entitlements for service-to-service access control</p>
        </div>
        <button kendoButton themeColor="primary" (click)="openCreate()">
          <kendo-svg-icon [icon]="plusIcon" size="small"></kendo-svg-icon>
          Add Entitlement
        </button>
      </div>

      @if (error()) {
        <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
        </div>
      }

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
          <kendo-grid-column field="audienceName" title="Audience" [width]="180" [sortable]="false">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="font-medium">{{ dataItem.audienceName }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="entitlementTypeName" title="Type" [width]="120" [sortable]="false">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="inline-flex items-center px-2 py-0.5 text-xs rounded-full border"
                [class]="getTypeBadgeClass(dataItem.entitlementTypeName)">
                {{ dataItem.entitlementTypeName }}
              </span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="entitlementScopeName" title="Scope" [width]="120" [sortable]="false">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm">{{ dataItem.entitlementScopeName }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="issuerName" title="Issuer" [width]="150" [sortable]="false">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm text-gray-500">{{ dataItem.issuerName || '-' }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="isEnabled" title="Status" [width]="100">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.isEnabled) {
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
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="created" title="Created" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm text-gray-500">{{ formatDate(dataItem.created) }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column title="Actions" [width]="120" [filterable]="false" [sortable]="false">
            <ng-template kendoGridCellTemplate let-dataItem>
              <div class="flex gap-1">
                <button kendoButton fillMode="flat" size="small" (click)="openEdit(dataItem)" title="Edit">
                  <kendo-svg-icon [icon]="pencilIcon" size="small"></kendo-svg-icon>
                </button>
                @if (dataItem.isDeletable) {
                  <button kendoButton fillMode="flat" themeColor="error" size="small" (click)="confirmDelete(dataItem)" title="Delete">
                    <kendo-svg-icon [icon]="trashIcon" size="small"></kendo-svg-icon>
                  </button>
                }
              </div>
            </ng-template>
          </kendo-grid-column>
        </kendo-grid>
      </div>
      }

      <!-- Create Dialog -->
      @if (showCreateDialog()) {
        <kendo-dialog
          title="Create Audience Entitlement"
          (close)="closeCreateDialog()"
          [minWidth]="400"
          [width]="500"
        >
          <form [formGroup]="createForm" class="p-2">
            <div class="space-y-4">
              <div>
                <kendo-label text="Audience" for="audienceId"></kendo-label>
                <kendo-dropdownlist
                  id="audienceId"
                  formControlName="audienceId"
                  [data]="audiences()"
                  textField="name"
                  valueField="id"
                  [style.width.%]="100"
                  [valuePrimitive]="true"
                  [filterable]="true"
                  (filterChange)="onAudienceFilterChange($event)"
                ></kendo-dropdownlist>
              </div>

              <div>
                <kendo-label text="Type" for="entitlementTypeId"></kendo-label>
                <kendo-dropdownlist
                  id="entitlementTypeId"
                  formControlName="entitlementTypeId"
                  [data]="entitlementTypes()"
                  textField="name"
                  valueField="id"
                  [style.width.%]="100"
                  [valuePrimitive]="true"
                ></kendo-dropdownlist>
              </div>

              <div>
                <kendo-label text="Scope" for="entitlementScopeId"></kendo-label>
                <kendo-dropdownlist
                  id="entitlementScopeId"
                  formControlName="entitlementScopeId"
                  [data]="entitlementScopes()"
                  textField="name"
                  valueField="id"
                  [style.width.%]="100"
                  [valuePrimitive]="true"
                  (valueChange)="onScopeChange($event)"
                ></kendo-dropdownlist>
              </div>

              @if (selectedScopeName() === 'Issuer') {
                <div>
                  <kendo-label text="Issuer" for="issuerId"></kendo-label>
                  <kendo-dropdownlist
                    id="issuerId"
                    formControlName="issuerId"
                    [data]="issuers()"
                    textField="name"
                    valueField="id"
                    [style.width.%]="100"
                    [valuePrimitive]="true"
                  ></kendo-dropdownlist>
                </div>
              }

              <div class="flex items-center gap-4">
                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" kendoCheckBox formControlName="isEnabled" />
                  <span>Enabled</span>
                </label>

                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" kendoCheckBox formControlName="isDeletable" />
                  <span>Deletable</span>
                </label>
              </div>
            </div>
          </form>

          <kendo-dialog-actions>
            <button kendoButton (click)="closeCreateDialog()">Cancel</button>
            <button
              kendoButton
              themeColor="primary"
              [disabled]="createForm.invalid || isSaving()"
              (click)="saveCreate()"
            >
              @if (isSaving()) {
                <kendo-loader size="small" themeColor="light"></kendo-loader>
              }
              Create
            </button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }

      <!-- Edit Dialog -->
      @if (showEditDialog()) {
        <kendo-dialog
          title="Edit Audience Entitlement"
          (close)="closeEditDialog()"
          [minWidth]="400"
          [width]="500"
        >
          <form [formGroup]="editForm" class="p-2">
            <div class="space-y-4">
              <div class="flex items-center gap-4">
                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" kendoCheckBox formControlName="isEnabled" />
                  <span>Enabled</span>
                </label>

                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" kendoCheckBox formControlName="isDeletable" />
                  <span>Deletable</span>
                </label>
              </div>
            </div>
          </form>

          <kendo-dialog-actions>
            <button kendoButton (click)="closeEditDialog()">Cancel</button>
            <button
              kendoButton
              themeColor="primary"
              [disabled]="editForm.invalid || isSaving()"
              (click)="saveEdit()"
            >
              @if (isSaving()) {
                <kendo-loader size="small" themeColor="light"></kendo-loader>
              }
              Update
            </button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }

      @if (showDeleteDialog()) {
        <kendo-dialog
          title="Delete Audience Entitlement"
          (close)="closeDeleteDialog()"
          [minWidth]="350"
          [width]="420"
        >
          <p>Are you sure you want to delete this entitlement for "{{ pendingDeleteAudienceName() }}"? This action cannot be undone.</p>
          <kendo-dialog-actions>
            <button kendoButton (click)="closeDeleteDialog()">Cancel</button>
            <button kendoButton themeColor="error" (click)="executeDelete()">Delete</button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }
    </div>
  `
})
export class AudienceEntitlementsComponent implements OnInit {
  private readonly entitlementService = inject(AudienceEntitlementService);
  private readonly audienceService = inject(AudienceService);
  private readonly issuerService = inject(IssuerService);
  private readonly fb = inject(FormBuilder);

  readonly plusIcon = plusIcon;
  readonly pencilIcon = pencilIcon;
  readonly trashIcon = trashIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;

  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };
  readonly filterSettings = 'menu' as FilterableSettings;

  readonly entitlementTypes = signal<EntitlementTypeV1[]>([]);
  readonly entitlementScopes = signal<EntitlementScopeV1[]>([]);
  readonly audiences = signal<AudienceV1[]>([]);
  readonly issuers = signal<IssuerV1[]>([]);
  readonly selectedScopeName = signal<string>('Global');
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'created', dir: 'desc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  readonly showCreateDialog = signal(false);
  readonly showEditDialog = signal(false);
  readonly isSaving = signal(false);
  private editingId: string | null = null;

  readonly showDeleteDialog = signal(false);
  readonly pendingDeleteAudienceName = signal('');
  private pendingDeleteId: string | null = null;

  createForm: FormGroup = this.fb.group({
    audienceId: ['', Validators.required],
    entitlementTypeId: ['', Validators.required],
    entitlementScopeId: ['', Validators.required],
    issuerId: [null],
    isEnabled: [true],
    isDeletable: [true]
  });

  editForm: FormGroup = this.fb.group({
    isEnabled: [true],
    isDeletable: [true]
  });

  ngOnInit(): void {
    this.loadLookups();
  }

  private loadLookups(): void {
    let loadedCount = 0;
    const checkReady = () => {
      loadedCount++;
      if (loadedCount >= 4) {
        this.loadData();
      }
    };

    this.entitlementService.getTypes().subscribe({
      next: (types) => { this.entitlementTypes.set(types); checkReady(); },
      error: (err) => { this.error.set(err.message || 'Failed to load entitlement types'); this.isInitialLoad.set(false); }
    });

    this.entitlementService.getScopes().subscribe({
      next: (scopes) => { this.entitlementScopes.set(scopes); checkReady(); },
      error: (err) => { this.error.set(err.message || 'Failed to load entitlement scopes'); this.isInitialLoad.set(false); }
    });

    this.audienceService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => { this.audiences.set(result.data); checkReady(); },
      error: (err) => { this.error.set(err.message || 'Failed to load audiences'); this.isInitialLoad.set(false); }
    });

    this.issuerService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => { this.issuers.set(result.data); checkReady(); },
      error: (err) => { this.error.set(err.message || 'Failed to load issuers'); this.isInitialLoad.set(false); }
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

    this.entitlementService.getAll(query).subscribe({
      next: (result) => {
        this.gridData.set({ data: result.data, total: result.total });
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load audience entitlements');
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

  onAudienceFilterChange(filter: string): void {
    if (!filter) {
      this.audienceService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
        next: (result) => this.audiences.set(result.data)
      });
    } else {
      this.audienceService.getAll({
        take: 50,
        filter: { logic: 'or', filters: [{ field: 'name', operator: 'contains', value: filter }] }
      }).subscribe({
        next: (result) => this.audiences.set(result.data)
      });
    }
  }

  onScopeChange(scopeId: string): void {
    const scope = this.entitlementScopes().find(s => s.id === scopeId);
    this.selectedScopeName.set(scope?.name ?? 'Global');

    if (scope?.name === 'Global') {
      this.createForm.patchValue({ issuerId: null });
    }
  }

  getTypeBadgeClass(typeName: string): string {
    switch (typeName) {
      case 'Admin': return 'bg-purple-100 text-purple-700 border-purple-200';
      case 'User': return 'bg-blue-100 text-blue-700 border-blue-200';
      case 'Viewer': return 'bg-gray-100 text-gray-700 border-gray-200';
      default: return 'bg-gray-100 text-gray-700 border-gray-200';
    }
  }

  formatDate(dateStr: string): string {
    try {
      return DateTime.fromISO(dateStr).toLocaleString(DateTime.DATETIME_MED);
    } catch {
      return dateStr;
    }
  }

  openCreate(): void {
    const defaultScope = this.entitlementScopes().length > 0 ? this.entitlementScopes()[0] : null;
    this.selectedScopeName.set(defaultScope?.name ?? 'Global');
    this.createForm.reset({
      audienceId: this.audiences().length > 0 ? this.audiences()[0].id : '',
      entitlementTypeId: this.entitlementTypes().length > 0 ? this.entitlementTypes()[0].id : '',
      entitlementScopeId: defaultScope?.id ?? '',
      issuerId: null,
      isEnabled: true,
      isDeletable: true
    });
    this.showCreateDialog.set(true);
  }

  closeCreateDialog(): void {
    this.showCreateDialog.set(false);
  }

  saveCreate(): void {
    if (this.createForm.invalid) return;

    this.isSaving.set(true);
    const formValue = this.createForm.value;

    const create: AudienceEntitlementCreate = {
      audienceId: formValue.audienceId,
      entitlementTypeId: formValue.entitlementTypeId,
      entitlementScopeId: formValue.entitlementScopeId,
      issuerId: formValue.issuerId || undefined,
      isEnabled: formValue.isEnabled,
      isDeletable: formValue.isDeletable
    };

    this.entitlementService.create(create).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closeCreateDialog();
        this.loadData();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to create entitlement');
      }
    });
  }

  openEdit(entitlement: AudienceEntitlementV1): void {
    this.editingId = entitlement.id;
    this.editForm.patchValue({
      isEnabled: entitlement.isEnabled,
      isDeletable: entitlement.isDeletable
    });
    this.showEditDialog.set(true);
  }

  closeEditDialog(): void {
    this.showEditDialog.set(false);
    this.editingId = null;
  }

  saveEdit(): void {
    if (this.editForm.invalid || !this.editingId) return;

    this.isSaving.set(true);
    const formValue = this.editForm.value;

    const update: AudienceEntitlementUpdate = {
      id: this.editingId,
      isEnabled: formValue.isEnabled,
      isDeletable: formValue.isDeletable
    };

    this.entitlementService.update(update).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closeEditDialog();
        this.loadData();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to update entitlement');
      }
    });
  }

  confirmDelete(entitlement: AudienceEntitlementV1): void {
    this.pendingDeleteId = entitlement.id;
    this.pendingDeleteAudienceName.set(entitlement.audienceName || 'Unknown');
    this.showDeleteDialog.set(true);
  }

  closeDeleteDialog(): void {
    this.showDeleteDialog.set(false);
    this.pendingDeleteId = null;
    this.pendingDeleteAudienceName.set('');
  }

  executeDelete(): void {
    if (!this.pendingDeleteId) return;
    const id = this.pendingDeleteId;
    this.closeDeleteDialog();

    this.isLoading.set(true);
    this.entitlementService.delete(id).subscribe({
      next: () => this.loadData(),
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to delete entitlement');
      }
    });
  }
}
