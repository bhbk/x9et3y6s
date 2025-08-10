import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RoleService, RoleV1, RoleCreate, RoleUpdate, AudienceService, AudienceV1, IssuerService, IssuerV1 } from 'lib-identity';
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
  selector: 'app-roles',
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
    <div class="p-6">
      @if (isInitialLoad()) {
        <div class="flex items-center justify-center p-12">
          <kendo-loader size="large"></kendo-loader>
        </div>
      } @else {
      <div class="flex justify-between items-center mb-6">
        <div>
          <h1 class="text-2xl font-semibold text-gray-800">Roles</h1>
          <p class="text-gray-500">Manage authorization roles for audiences</p>
        </div>
        <button kendoButton themeColor="primary" (click)="openCreate()" [disabled]="audiences().length === 0">
          <kendo-svg-icon [icon]="plusIcon" size="small"></kendo-svg-icon>
          Add Role
        </button>
      </div>

      <!-- Filters -->
      <div class="mb-4 flex gap-4 items-end">
        <div>
          <kendo-label text="Filter by Issuer" for="issuerFilter"></kendo-label>
          <kendo-dropdownlist
            id="issuerFilter"
            [data]="issuerFilterOptions()"
            textField="name"
            valueField="id"
            [value]="selectedIssuerId()"
            (valueChange)="onIssuerFilterChange($event)"
            [style.width.px]="250"
          ></kendo-dropdownlist>
        </div>
        <div>
          <kendo-label text="Filter by Audience" for="audienceFilter"></kendo-label>
          <kendo-dropdownlist
            id="audienceFilter"
            [data]="audienceFilterOptions()"
            textField="name"
            valueField="id"
            [value]="selectedAudienceId()"
            (valueChange)="onAudienceFilterChange($event)"
            [style.width.px]="250"
          ></kendo-dropdownlist>
        </div>
      </div>

      @if (error()) {
        <div class="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
          {{ error() }}
          <button kendoButton fillMode="flat" themeColor="error" (click)="loadData()">Retry</button>
        </div>
      }

      <div class="bg-white rounded-lg shadow">
        <kendo-grid
          [data]="gridData()"
          [pageSize]="pageSize()"
          [skip]="skip()"
          [pageable]="pageableSettings"
          [sortable]="sortSettings"
          [sort]="sort()"
          [filterable]="filterSettings"
          [loading]="isLoading()"
          (pageChange)="onPageChange($event)"
          (sortChange)="onSortChange($event)"
        >
          <kendo-grid-column field="name" title="Name" [width]="200">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="font-medium">{{ dataItem.name }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="description" title="Description" [width]="300">
          </kendo-grid-column>

          <kendo-grid-column field="audienceId" title="Audience" [width]="180">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm">{{ getAudienceName(dataItem.audienceId) }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="isEnabled" title="Status" [width]="100">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.isEnabled) {
                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                  <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                  Enabled
                </span>
              } @else {
                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-gray-100 text-gray-600 text-xs rounded-full">
                  <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                  Disabled
                </span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="createdUtc" title="Created" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm text-gray-500">{{ formatDate(dataItem.createdUtc) }}</span>
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

      <!-- Create/Edit Dialog -->
      @if (showDialog()) {
        <kendo-dialog
          title="{{ isEditing() ? 'Edit Role' : 'Create Role' }}"
          (close)="closeDialog()"
          [minWidth]="400"
          [width]="500"
        >
          <form [formGroup]="form" class="p-2">
            <div class="space-y-4">
              <div>
                <kendo-label text="Audience" for="audienceId" ></kendo-label>
                <kendo-dropdownlist
                  id="audienceId"
                  formControlName="audienceId"
                  [data]="audiences()"
                  textField="name"
                  valueField="id"
                  [style.width.%]="100"
                  [valuePrimitive]="true"
                  [disabled]="isEditing()"
                ></kendo-dropdownlist>
              </div>

              <div>
                <kendo-label text="Name" for="name" ></kendo-label>
                <kendo-textbox id="name" formControlName="name" [style.width.%]="100"></kendo-textbox>
              </div>

              <div>
                <kendo-label text="Description" for="description"></kendo-label>
                <kendo-textarea id="description" formControlName="description" [style.width.%]="100" [rows]="3"></kendo-textarea>
              </div>

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
            <button kendoButton (click)="closeDialog()">Cancel</button>
            <button
              kendoButton
              themeColor="primary"
              [disabled]="form.invalid || isSaving()"
              (click)="saveRole()"
            >
              @if (isSaving()) {
                <kendo-loader size="small" themeColor="light"></kendo-loader>
              }
              {{ isEditing() ? 'Update' : 'Create' }}
            </button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }

      @if (showDeleteDialog()) {
        <kendo-dialog
          title="Delete Role"
          (close)="closeDeleteDialog()"
          [minWidth]="350"
          [width]="420"
        >
          <p>Are you sure you want to delete "{{ pendingDeleteName() }}"? This action cannot be undone.</p>
          <kendo-dialog-actions>
            <button kendoButton (click)="closeDeleteDialog()">Cancel</button>
            <button kendoButton themeColor="error" (click)="executeDelete()">Delete</button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }
    </div>
  `
})
export class RolesComponent implements OnInit {
  private readonly roleService = inject(RoleService);
  private readonly audienceService = inject(AudienceService);
  private readonly issuerService = inject(IssuerService);
  private readonly fb = inject(FormBuilder);

  // Icons
  readonly plusIcon = plusIcon;
  readonly pencilIcon = pencilIcon;
  readonly trashIcon = trashIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;

  // Grid settings
  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };
  readonly filterSettings = 'menu' as FilterableSettings;

  // State
  readonly issuers = signal<IssuerV1[]>([]);
  readonly audiences = signal<AudienceV1[]>([]);
  readonly filteredAudiences = signal<AudienceV1[]>([]);
  readonly selectedIssuerId = signal<string | null>(null);
  readonly selectedAudienceId = signal<string | null>(null);
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'name', dir: 'asc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  // Dialog state
  readonly showDialog = signal(false);
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  private editingId: string | null = null;

  // Delete dialog state
  readonly showDeleteDialog = signal(false);
  readonly pendingDeleteName = signal('');
  private pendingDeleteId: string | null = null;

  form: FormGroup = this.fb.group({
    audienceId: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    isEnabled: [true],
    isDeletable: [true]
  });

  ngOnInit(): void {
    this.loadIssuers();
  }

  issuerFilterOptions(): Array<{id: string | null, name: string}> {
    return [
      { id: null, name: 'All Issuers' },
      ...this.issuers()
    ];
  }

  audienceFilterOptions(): Array<{id: string | null, name: string}> {
    return [
      { id: null, name: 'All Audiences' },
      ...this.filteredAudiences()
    ];
  }

  private loadIssuers(): void {
    this.issuerService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => {
        this.issuers.set(result.data);
        this.loadAudiences();
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load issuers');
        this.isInitialLoad.set(false);
      }
    });
  }

  private loadAudiences(): void {
    this.audienceService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => {
        this.audiences.set(result.data);
        this.filteredAudiences.set(result.data);
        this.loadData();
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load audiences');
        this.isInitialLoad.set(false);
      }
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const audienceId = this.selectedAudienceId();

    if (audienceId) {
      this.roleService.getByAudienceId(audienceId).subscribe({
        next: (roles) => {
          this.gridData.set({
            data: roles,
            total: roles.length
          });
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load roles');
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        }
      });
    } else {
      const sortDesc = this.sort()[0];
      const query = {
        skip: this.skip(),
        take: this.pageSize(),
        sort: sortDesc ? [{ field: sortDesc.field, dir: sortDesc.dir as 'asc' | 'desc' }] : undefined
      };

      this.roleService.getAll(query).subscribe({
        next: (result) => {
          this.gridData.set({
            data: result.data,
            total: result.total
          });
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load roles');
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        }
      });
    }
  }

  onIssuerFilterChange(issuerId: string | null): void {
    this.selectedIssuerId.set(issuerId);
    this.selectedAudienceId.set(null);

    if (issuerId) {
      this.filteredAudiences.set(this.audiences().filter(a => a.issuerId === issuerId));
    } else {
      this.filteredAudiences.set(this.audiences());
    }

    this.skip.set(0);
    this.loadData();
  }

  onAudienceFilterChange(audienceId: string | null): void {
    this.selectedAudienceId.set(audienceId);
    this.skip.set(0);
    this.loadData();
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

  getAudienceName(audienceId: string): string {
    return this.audiences().find(a => a.id === audienceId)?.name || 'Unknown';
  }

  formatDate(dateStr: string): string {
    try {
      return DateTime.fromISO(dateStr).toLocaleString(DateTime.DATETIME_MED);
    } catch {
      return dateStr;
    }
  }

  openCreate(): void {
    this.isEditing.set(false);
    this.editingId = null;
    const defaultAudienceId = this.selectedAudienceId() || (this.audiences().length > 0 ? this.audiences()[0].id : '');
    this.form.reset({
      audienceId: defaultAudienceId,
      name: '',
      description: '',
      isEnabled: true,
      isDeletable: true
    });
    this.showDialog.set(true);
  }

  openEdit(role: RoleV1): void {
    this.isEditing.set(true);
    this.editingId = role.id;
    this.form.patchValue({
      audienceId: role.audienceId,
      name: role.name,
      description: role.description || '',
      isEnabled: role.isEnabled,
      isDeletable: role.isDeletable
    });
    this.showDialog.set(true);
  }

  closeDialog(): void {
    this.showDialog.set(false);
    this.editingId = null;
  }

  saveRole(): void {
    if (this.form.invalid) return;

    this.isSaving.set(true);
    const formValue = this.form.value;

    if (this.isEditing() && this.editingId) {
      const update: RoleUpdate = {
        id: this.editingId,
        ...formValue
      };

      this.roleService.update(update).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to update role');
        }
      });
    } else {
      const create: RoleCreate = formValue;

      this.roleService.create(create).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to create role');
        }
      });
    }
  }

  confirmDelete(role: RoleV1): void {
    this.pendingDeleteId = role.id;
    this.pendingDeleteName.set(role.name);
    this.showDeleteDialog.set(true);
  }

  closeDeleteDialog(): void {
    this.showDeleteDialog.set(false);
    this.pendingDeleteId = null;
    this.pendingDeleteName.set('');
  }

  executeDelete(): void {
    if (!this.pendingDeleteId) return;
    const id = this.pendingDeleteId;
    this.closeDeleteDialog();
    this.deleteRole(id);
  }

  private deleteRole(id: string): void {
    this.isLoading.set(true);

    this.roleService.delete(id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to delete role');
      }
    });
  }
}
