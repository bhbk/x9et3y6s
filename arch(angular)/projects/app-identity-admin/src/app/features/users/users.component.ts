import { Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { UserService, UserV1, UserCreate, UserUpdate } from 'lib-identity';
import { KENDO_GRID, GridDataResult, PageChangeEvent, SortSettings, FilterableSettings, PagerSettings } from '@progress/kendo-angular-grid';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';
import { KENDO_INPUTS } from '@progress/kendo-angular-inputs';
import { KENDO_LABELS } from '@progress/kendo-angular-label';
import { KENDO_INDICATORS } from '@progress/kendo-angular-indicators';
import { KENDO_ICONS } from '@progress/kendo-angular-icons';
import { KENDO_DIALOGS } from '@progress/kendo-angular-dialog';
import { SortDescriptor } from '@progress/kendo-data-query';
import { plusIcon, pencilIcon, trashIcon, checkIcon, xIcon, lockIcon, userIcon } from '@progress/kendo-svg-icons';
import { DateTime } from 'luxon';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    KENDO_GRID,
    KENDO_BUTTONS,
    KENDO_INPUTS,
    KENDO_LABELS,
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
          <h1 class="text-2xl font-semibold text-gray-800">Users</h1>
          <p class="text-gray-500">Manage user accounts</p>
        </div>
        <button kendoButton themeColor="primary" (click)="openCreate()">
          <kendo-svg-icon [icon]="plusIcon" size="small"></kendo-svg-icon>
          Add User
        </button>
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
          <kendo-grid-column field="userName" title="Username" [width]="150">
            <ng-template kendoGridCellTemplate let-dataItem>
              <div class="flex items-center gap-2">
                <kendo-svg-icon [icon]="userIcon" size="small" class="text-gray-400"></kendo-svg-icon>
                <span class="font-medium">{{ dataItem.userName }}</span>
              </div>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="email" title="Email" [width]="200">
          </kendo-grid-column>

          <kendo-grid-column title="Name" [width]="180">
            <ng-template kendoGridCellTemplate let-dataItem>
              {{ dataItem.firstName }} {{ dataItem.lastName }}
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column title="Verified" [width]="100">
            <ng-template kendoGridCellTemplate let-dataItem>
              <div class="flex gap-1">
                @if (dataItem.emailConfirmed) {
                  <span class="text-green-500" title="Email verified">
                    <kendo-svg-icon [icon]="checkIcon" size="small"></kendo-svg-icon>
                  </span>
                } @else {
                  <span class="text-gray-300" title="Email not verified">
                    <kendo-svg-icon [icon]="xIcon" size="small"></kendo-svg-icon>
                  </span>
                }
              </div>
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="isLockedOut" title="Status" [width]="100">
            <ng-template kendoGridCellTemplate let-dataItem>
              @if (dataItem.isLockedOut) {
                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-red-100 text-red-700 text-xs rounded-full">
                  <kendo-svg-icon [icon]="lockIcon" size="small"></kendo-svg-icon>
                  Locked
                </span>
              } @else {
                <span class="inline-flex items-center gap-1 px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">
                  Active
                </span>
              }
            </ng-template>
          </kendo-grid-column>

          <kendo-grid-column field="createdUtc" title="Created" [width]="150">
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
          title="{{ isEditing() ? 'Edit User' : 'Create User' }}"
          (close)="closeDialog()"
          [minWidth]="450"
          [width]="550"
        >
          <form [formGroup]="form" class="p-2">
            <div class="space-y-4">
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <kendo-label text="First Name" for="firstName" ></kendo-label>
                  <kendo-textbox id="firstName" formControlName="firstName" [style.width.%]="100"></kendo-textbox>
                </div>
                <div>
                  <kendo-label text="Last Name" for="lastName" ></kendo-label>
                  <kendo-textbox id="lastName" formControlName="lastName" [style.width.%]="100"></kendo-textbox>
                </div>
              </div>

              <div>
                <kendo-label text="Username" for="userName" ></kendo-label>
                <kendo-textbox id="userName" formControlName="userName" [style.width.%]="100"></kendo-textbox>
              </div>

              <div>
                <kendo-label text="Email" for="email" ></kendo-label>
                <kendo-textbox id="email" formControlName="email" [style.width.%]="100"></kendo-textbox>
              </div>

              <div>
                <kendo-label text="Phone Number" for="phoneNumber"></kendo-label>
                <kendo-textbox id="phoneNumber" formControlName="phoneNumber" [style.width.%]="100"></kendo-textbox>
              </div>

              <div class="flex items-center gap-4 flex-wrap">
                <label class="flex items-center gap-2 cursor-pointer">
                  <input type="checkbox" kendoCheckBox formControlName="isHumanBeing" />
                  <span>Human Being</span>
                </label>

                @if (isEditing()) {
                  <label class="flex items-center gap-2 cursor-pointer">
                    <input type="checkbox" kendoCheckBox formControlName="isLockedOut" />
                    <span>Locked Out</span>
                  </label>
                }

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
              (click)="saveUser()"
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
          title="Set User Password"
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
          title="Delete User"
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
export class UsersComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly fb = inject(FormBuilder);

  // Icons
  readonly plusIcon = plusIcon;
  readonly pencilIcon = pencilIcon;
  readonly trashIcon = trashIcon;
  readonly checkIcon = checkIcon;
  readonly xIcon = xIcon;
  readonly lockIcon = lockIcon;
  readonly userIcon = userIcon;

  // Grid settings
  readonly pageSize = signal(10);
  readonly pageableSettings: PagerSettings = { pageSizes: [10, 20, 30] };
  readonly sortSettings: SortSettings = { mode: 'single', allowUnsort: true };
  readonly filterSettings = 'menu' as FilterableSettings;

  // State
  readonly gridData = signal<GridDataResult>({ data: [], total: 0 });
  readonly skip = signal(0);
  readonly sort = signal<SortDescriptor[]>([{ field: 'userName', dir: 'asc' }]);
  readonly isLoading = signal(false);
  readonly isInitialLoad = signal(true);
  readonly error = signal<string | null>(null);

  // Dialog state
  readonly showDialog = signal(false);
  readonly showPasswordDialog = signal(false);
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  readonly showDeleteDialog = signal(false);
  readonly pendingDeleteName = signal('');
  private editingId: string | null = null;
  private passwordEntityId: string | null = null;
  private pendingDeleteId: string | null = null;

  form: FormGroup = this.fb.group({
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phoneNumber: [''],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    isHumanBeing: [true],
    isLockedOut: [false],
    isDeletable: [true]
  });

  passwordForm: FormGroup = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    newPasswordConfirm: ['', Validators.required]
  }, { validators: this.passwordMatchValidator });

  ngOnInit(): void {
    this.loadData();
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

    this.userService.getAll(query).subscribe({
      next: (result) => {
        this.gridData.set({
          data: result.data,
          total: result.total
        });
        this.isLoading.set(false);
        this.isInitialLoad.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load users');
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
    this.form.reset({
      userName: '',
      email: '',
      phoneNumber: '',
      firstName: '',
      lastName: '',
      isHumanBeing: true,
      isLockedOut: false,
      isDeletable: true
    });
    this.showDialog.set(true);
  }

  openEdit(user: UserV1): void {
    this.isEditing.set(true);
    this.editingId = user.id;
    this.form.patchValue({
      userName: user.userName,
      email: user.email,
      phoneNumber: user.phoneNumber || '',
      firstName: user.firstName,
      lastName: user.lastName,
      isHumanBeing: user.isHumanBeing,
      isLockedOut: user.isLockedOut,
      isDeletable: user.isDeletable
    });
    this.showDialog.set(true);
  }

  closeDialog(): void {
    this.showDialog.set(false);
    this.editingId = null;
  }

  saveUser(): void {
    if (this.form.invalid) return;

    this.isSaving.set(true);
    const formValue = this.form.value;

    if (this.isEditing() && this.editingId) {
      const update: UserUpdate = {
        id: this.editingId,
        userName: formValue.userName,
        email: formValue.email,
        phoneNumber: formValue.phoneNumber || undefined,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        isHumanBeing: formValue.isHumanBeing,
        isLockedOut: formValue.isLockedOut,
        isDeletable: formValue.isDeletable
      };

      this.userService.update(update).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to update user');
        }
      });
    } else {
      const create: UserCreate = {
        userName: formValue.userName,
        email: formValue.email,
        phoneNumber: formValue.phoneNumber || undefined,
        firstName: formValue.firstName,
        lastName: formValue.lastName,
        isHumanBeing: formValue.isHumanBeing,
        isDeletable: formValue.isDeletable
      };

      this.userService.create(create).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeDialog();
          this.loadData();
        },
        error: (err) => {
          this.isSaving.set(false);
          this.error.set(err.error?.message || err.message || 'Failed to create user');
        }
      });
    }
  }

  openPasswordDialog(user: UserV1): void {
    this.passwordEntityId = user.id;
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

    this.userService.setPassword({
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

  confirmDelete(user: UserV1): void {
    this.pendingDeleteId = user.id;
    this.pendingDeleteName.set(user.userName);
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
    this.deleteUser(id);
  }

  private deleteUser(id: string): void {
    this.isLoading.set(true);

    this.userService.delete(id).subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        this.isLoading.set(false);
        this.error.set(err.error?.message || err.message || 'Failed to delete user');
      }
    });
  }
}
