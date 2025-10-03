import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AudienceService, AudienceV1, AudienceCreate, AudienceUpdate, IssuerService, IssuerV1 } from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, FilterableSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_DROPDOWNS } from '@progress/kendo-angular-dropdowns';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_DIALOGS } from '@progress/kendo-angular-dialog';
import { SortDescriptor } from '@progress/kendo-data-query';
import { plusIcon, pencilIcon, trashIcon, checkIcon, xIcon, lockIcon } from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-audiences',
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
          <p class="text-gray-500">Manage client audiences and their settings</p>
        </div>
        <button kendoButton themeColor="primary" (click)="openCreate()" [disabled]="issuers().length === 0">
          <kendo-svg-icon [icon]="plusIcon" size="small"></kendo-svg-icon>
          Add Audience
        </button>
      </div>

      <!-- Issuer Filter -->
      <div class="mb-4">
        <kendo-label text="Filter by Issuer" for="issuerFilter"></kendo-label>
        <kendo-dropdownlist
          id="issuerFilter"
          [data]="issuerFilterOptions()"
          textField="name"
          valueField="id"
          [value]="selectedIssuerId()"
          (valueChange)="onIssuerFilterChange($event)"
          [style.width.px]="300"
        ></kendo-dropdownlist>
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
          [loading]="isLoading()"
          (pageChange)="onPageChange($event)"
          (sortChange)="onSortChange($event)"
        >
          <kendo-grid-column field="name" title="Name" [width]="200">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="font-medium">{{ dataItem.name }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="description" title="Description" [width]="250">
          </kendo-grid-column>

          <kendo-grid-column field="issuerId" title="Issuer" [width]="150">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm">{{ getIssuerName(dataItem.issuerId) }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="isLockedOut" title="Status" [width]="120">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.isLockedOut) {
                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full border border-red-200">
                  <kendo-svg-icon [icon]="lockIcon" size="small"></kendo-svg-icon>
                  Locked
                </span>
              } @else {
                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full border border-green-200">
                  <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                  Active
                </span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="createdUtc" title="Created" [width]="160">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm text-gray-500">{{ formatDate(dataItem.createdUtc) }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column title="Actions" [width]="160" [filterable]="false" [sortable]="false">
            <ng-template kendoGridCellTemplate let-dataItem>
              <div class="flex gap-1">
                <button kendoButton fillMode="flat" size="small" (click)="openEdit(dataItem)" title="Edit">
                  <kendo-svg-icon [icon]="pencilIcon" size="small"></kendo-svg-icon>
                </button>
                <button kendoButton fillMode="flat" size="small" (click)="openPasswordDialog(dataItem)" title="Set Password">
                  <kendo-svg-icon [icon]="lockIcon" size="small"></kendo-svg-icon>
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
          title="{{ isEditing() ? 'Edit Audience' : 'Create Audience' }}"
          (close)="closeDialog()"
          [minWidth]="400"
          [width]="500"
        >
          <form [formGroup]="form" class="p-2">
            <div class="space-y-4">
              <div>
                <kendo-label text="Issuer" for="issuerId" ></kendo-label>
                <kendo-dropdownlist
                  id="issuerId"
                  formControlName="issuerId"
                  [data]="issuers()"
                  textField="name"
                  valueField="id"
                  [style.width.%]="100"
                  [valuePrimitive]="true"
                ></kendo-dropdownlist>
                @if (form.get('issuerId')?.invalid && form.get('issuerId')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Issuer is required</div>
                }
              </div>

              <div>
                <kendo-label text="Name" for="name" ></kendo-label>
                <kendo-textbox id="name" formControlName="name" [style.width.%]="100"></kendo-textbox>
                @if (form.get('name')?.invalid && form.get('name')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Name is required</div>
                }
              </div>

              <div>
                <kendo-label text="Description" for="description"></kendo-label>
                <kendo-textarea id="description" formControlName="description" [style.width.%]="100" [rows]="3"></kendo-textarea>
              </div>

              <div class="flex items-center gap-4">
                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" kendoCheckBox formControlName="isLockedOut" />
                  <span>Locked Out</span>
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
              (click)="saveAudience()"
            >
              @if (isSaving()) {
                <kendo-loader size="small" themeColor="light"></kendo-loader>
              }
              {{ isEditing() ? 'Update' : 'Create' }}
            </button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }

      <!-- Password Dialog -->
      @if (showPasswordDialog()) {
        <kendo-dialog
          title="Set Audience Password"
          (close)="closePasswordDialog()"
          [minWidth]="400"
          [width]="450"
        >
          <form [formGroup]="passwordForm" class="p-2">
            <div class="space-y-4">
              <div>
                <kendo-label text="New Password" for="newPassword" ></kendo-label>
                <kendo-textbox
                  id="newPassword"
                  formControlName="newPassword"
                  type="password"
                  [style.width.%]="100"
                ></kendo-textbox>
                @if (passwordForm.get('newPassword')?.invalid && passwordForm.get('newPassword')?.touched) {
                  <div class="text-red-500 text-sm mt-1">Password is required (min 8 characters)</div>
                }
              </div>

              <div>
                <kendo-label text="Confirm Password" for="confirmPassword" ></kendo-label>
                <kendo-textbox
                  id="confirmPassword"
                  formControlName="newPasswordConfirm"
                  type="password"
                  [style.width.%]="100"
                ></kendo-textbox>
                @if (passwordForm.hasError('passwordMismatch')) {
                  <div class="text-red-500 text-sm mt-1">Passwords do not match</div>
                }
              </div>
            </div>
          </form>

          <kendo-dialog-actions>
            <button kendoButton (click)="closePasswordDialog()">Cancel</button>
            <button
              kendoButton
              themeColor="primary"
              [disabled]="passwordForm.invalid || isSaving()"
              (click)="savePassword()"
            >
              @if (isSaving()) {
                <kendo-loader size="small" themeColor="light"></kendo-loader>
              }
              Set Password
            </button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }

      @if (showDeleteDialog()) {
        <kendo-dialog
          title="Delete Audience"
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
export class AudiencesComponent implements OnInit {
  private readonly audienceService = inject(AudienceService);
  private readonly issuerService = inject(IssuerService);
  private readonly fb = inject(FormBuilder);

  // Icons
  readonly plusIcon = plusIcon;
  readonly pencilIcon = pencilIcon;
  readonly trashIcon = trashIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;
  readonly lockIcon = lockIcon;

  // Grid settings
  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };
  readonly filterSettings = 'menu' as FilterableSettings;

  // State
  readonly issuers = signal<IssuerV1[]>([]);
  readonly selectedIssuerId = signal<string | null>(null);
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'name', dir: 'asc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  // Dialog state
  readonly showDialog = signal(false);
  readonly showPasswordDialog = signal(false);
  readonly showDeleteDialog = signal(false);
  readonly pendingDeleteName = signal('');
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  private editingId: string | null = null;
  private passwordEntityId: string | null = null;
  private pendingDeleteId: string | null = null;

  form: FormGroup = this.fb.group({
    issuerId: ['', Validators.required],
    name: ['', Validators.required],
    description: [''],
    isLockedOut: [false],
    isDeletable: [true]
  });

  passwordForm: FormGroup = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    newPasswordConfirm: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  ngOnInit(): void {
    this.loadIssuers();
  }

  issuerFilterOptions(): Array<{id: string | null, name: string}> {
    return [
      { id: null, name: 'All Issuers' },
      ...this.issuers()
    ];
  }

  private loadIssuers(): void {
    this.issuerService.getAll({ take: 1000, sort: [{ field: 'name', dir: 'asc' }] }).subscribe({
      next: (result) => {
        this.issuers.set(result.data);
        this.loadData();
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load issuers');
        this.isInitialLoad.set(false);
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

    const issuerId = this.selectedIssuerId();

    if (issuerId) {
      // getByIssuerId returns array, not paged result
      this.audienceService.getByIssuerId(issuerId).subscribe({
        next: (audiences) => {
          this.gridData.set({
            data: audiences,
            total: audiences.length
          });
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load audiences');
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        }
      });
    } else {
      this.audienceService.getAll(query).subscribe({
        next: (result) => {
          this.gridData.set({
            data: result.data,
            total: result.total
          });
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load audiences');
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        }
      });
    }
  }

  onIssuerFilterChange(issuerId: string | null): void {
    this.selectedIssuerId.set(issuerId);
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

  getIssuerName(issuerId: string): string {
    return this.issuers().find(i => i.id === issuerId)?.name || 'Unknown';
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
    const defaultIssuerId = this.selectedIssuerId() || (this.issuers().length > 0 ? this.issuers()[0].id : '');
    this.form.reset({
      issuerId: defaultIssuerId,
      name: '',
      description: '',
      isLockedOut: false,
      isDeletable: true
    });
    this.showDialog.set(true);
  }

  openEdit(audience: AudienceV1): void {
    this.isEditing.set(true);
    this.editingId = audience.id;
    this.form.patchValue({
      issuerId: audience.issuerId,
      name: audience.name,
      description: audience.description || '',
      isLockedOut: audience.isLockedOut,
      isDeletable: audience.isDeletable
    });
    this.showDialog.set(true);
  }

  closeDialog(): void {
    this.showDialog.set(false);
    this.editingId = null;
  }

  saveAudience(): void {
    if (this.form.invalid) return;

    this.isSaving.set(true);
    const formValue = this.form.value;

    if (this.isEditing() && this.editingId) {
      const update: AudienceUpdate = {
        id: this.editingId,
        ...formValue
      };

      this.audienceService.update(update).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to update audience');
        }
      });
    } else {
      const create: AudienceCreate = formValue;

      this.audienceService.create(create).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to create audience');
        }
      });
    }
  }

  openPasswordDialog(audience: AudienceV1): void {
    this.passwordEntityId = audience.id;
    this.passwordForm.reset();
    this.showPasswordDialog.set(true);
  }

  closePasswordDialog(): void {
    this.showPasswordDialog.set(false);
    this.passwordEntityId = null;
  }

  savePassword(): void {
    if (this.passwordForm.invalid || !this.passwordEntityId) return;

    this.isSaving.set(true);
    const { newPassword, newPasswordConfirm } = this.passwordForm.value;

    this.audienceService.setPassword({
      entityId: this.passwordEntityId,
      newPassword,
      newPasswordConfirm
    }).subscribe({
      next: () => {
        this.isSaving.set(false);
        this.closePasswordDialog();
      },
      error: (err) => {
        this.isSaving.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to set password');
      }
    });
  }

  private passwordMatchValidator(control: FormGroup): { passwordMismatch: true } | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmPassword = control.get('newPasswordConfirm')?.value;
    return newPassword === confirmPassword ? null : { passwordMismatch: true };
  }

  confirmDelete(audience: AudienceV1): void {
    this.pendingDeleteId = audience.id;
    this.pendingDeleteName.set(audience.name);
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
    this.deleteAudience(id);
  }

  private deleteAudience(id: string): void {
    this.isLoading.set(true);

    this.audienceService.delete(id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to delete audience');
      }
    });
  }
}
