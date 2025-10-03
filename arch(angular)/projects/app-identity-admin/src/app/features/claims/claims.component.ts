import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ClaimService, ClaimV1, ClaimCreate, ClaimUpdate, IssuerService, IssuerV1 } from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, FilterableSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_DROPDOWNS } from '@progress/kendo-angular-dropdowns';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_DIALOGS } from '@progress/kendo-angular-dialog';
import { SortDescriptor } from '@progress/kendo-data-query';
import { plusIcon, pencilIcon, trashIcon } from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-claims',
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
          <p class="text-gray-500">Manage claims for users</p>
        </div>
        <button kendoButton themeColor="primary" (click)="openCreate()" [disabled]="issuers().length === 0">
          <kendo-svg-icon [icon]="plusIcon" size="small"></kendo-svg-icon>
          Add Claim
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
          <kendo-grid-column field="type" title="Type" [width]="200">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="font-medium">{{ dataItem.type }}</span>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="value" title="Value" [width]="250">
            <ng-template kendoGridCellTemplate let-dataItem>
              <code class="text-sm bg-gray-100 px-2 py-0.5 rounded">{{ dataItem.value }}</code>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="subject" title="Subject" [width]="150">
          </kendo-grid-column>

          <kendo-grid-column field="valueType" title="Value Type" [width]="150">
          </kendo-grid-column>

          <kendo-grid-column field="issuerId" title="Issuer" [width]="140">
            <ng-template kendoGridCellTemplate let-dataItem>
              <span class="text-sm">{{ getIssuerName(dataItem.issuerId) }}</span>
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
          title="{{ isEditing() ? 'Edit Claim' : 'Create Claim' }}"
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
                  [disabled]="isEditing()"
                ></kendo-dropdownlist>
              </div>

              <div>
                <kendo-label text="Type" for="type" ></kendo-label>
                <kendo-textbox id="type" formControlName="type" [style.width.%]="100" placeholder="e.g., role, permission"></kendo-textbox>
              </div>

              <div>
                <kendo-label text="Value" for="value" ></kendo-label>
                <kendo-textbox id="value" formControlName="value" [style.width.%]="100"></kendo-textbox>
              </div>

              <div>
                <kendo-label text="Subject" for="subject"></kendo-label>
                <kendo-textbox id="subject" formControlName="subject" [style.width.%]="100"></kendo-textbox>
              </div>

              <div>
                <kendo-label text="Value Type" for="valueType"></kendo-label>
                <kendo-textbox id="valueType" formControlName="valueType" [style.width.%]="100" placeholder="e.g., string, boolean"></kendo-textbox>
              </div>

              <div>
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
              (click)="saveClaim()"
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
          title="Delete Claim"
          (close)="closeDeleteDialog()"
          [minWidth]="350"
          [width]="420"
        >
          <p>Are you sure you want to delete this claim? This action cannot be undone.</p>
          <kendo-dialog-actions>
            <button kendoButton (click)="closeDeleteDialog()">Cancel</button>
            <button kendoButton themeColor="error" (click)="executeDelete()">Delete</button>
          </kendo-dialog-actions>
        </kendo-dialog>
      }
    </div>
  `
})
export class ClaimsComponent implements OnInit {
  private readonly claimService = inject(ClaimService);
  private readonly issuerService = inject(IssuerService);
  private readonly fb = inject(FormBuilder);

  // Icons
  readonly plusIcon = plusIcon;
  readonly pencilIcon = pencilIcon;
  readonly trashIcon = trashIcon;

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
  readonly sort = signal<SortDescriptor[]>([{ field: 'type', dir: 'asc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  // Dialog state
  readonly showDialog = signal(false);
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  private editingId: string | null = null;
  readonly showDeleteDialog = signal(false);
  private pendingDeleteId: string | null = null;

  form: FormGroup = this.fb.group({
    issuerId: ['', Validators.required],
    type: ['', Validators.required],
    value: ['', Validators.required],
    subject: [''],
    valueType: [''],
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

    const issuerId = this.selectedIssuerId();

    if (issuerId) {
      this.claimService.getByIssuerId(issuerId).subscribe({
        next: (claims) => {
          this.gridData.set({
            data: claims,
            total: claims.length
          });
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load claims');
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

      this.claimService.getAll(query).subscribe({
        next: (result) => {
          this.gridData.set({
            data: result.data,
            total: result.total
          });
          this.isLoading.set(false);
          this.isInitialLoad.set(false);
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to load claims');
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
      type: '',
      value: '',
      subject: '',
      valueType: '',
      isDeletable: true
    });
    this.showDialog.set(true);
  }

  openEdit(claim: ClaimV1): void {
    this.isEditing.set(true);
    this.editingId = claim.id;
    this.form.patchValue({
      issuerId: claim.issuerId,
      type: claim.type,
      value: claim.value,
      subject: claim.subject || '',
      valueType: claim.valueType || '',
      isDeletable: claim.isDeletable
    });
    this.showDialog.set(true);
  }

  closeDialog(): void {
    this.showDialog.set(false);
    this.editingId = null;
  }

  saveClaim(): void {
    if (this.form.invalid) return;

    this.isSaving.set(true);
    const formValue = this.form.value;

    if (this.isEditing() && this.editingId) {
      const update: ClaimUpdate = {
        id: this.editingId,
        ...formValue
      };

      this.claimService.update(update).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to update claim');
        }
      });
    } else {
      const create: ClaimCreate = formValue;

      this.claimService.create(create).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to create claim');
        }
      });
    }
  }

  confirmDelete(claim: ClaimV1): void {
    this.pendingDeleteId = claim.id;
    this.showDeleteDialog.set(true);
  }

  closeDeleteDialog(): void {
    this.showDeleteDialog.set(false);
    this.pendingDeleteId = null;
  }

  executeDelete(): void {
    if (!this.pendingDeleteId) return;
    const id = this.pendingDeleteId;
    this.closeDeleteDialog();
    this.deleteClaim(id);
  }

  private deleteClaim(id: string): void {
    this.isLoading.set(true);

    this.claimService.delete(id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to delete claim');
      }
    });
  }
}
