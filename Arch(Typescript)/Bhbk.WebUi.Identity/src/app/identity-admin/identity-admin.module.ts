import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { 
  IdentityAdminActivityComponent,
  IdentityAdminClientComponent,
  IdentityAdminClaimComponent,
  IdentityAdminIssuerComponent,
  IdentityAdminLoginComponent,
  IdentityAdminRoleComponent,
  IdentityAdminUserComponent,
} from 'src/app/identity-admin';
import { IdentityAdminRoutingModule } from 'src/app/identity-admin/identity-admin-routing.module';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { DialogModule, DialogsModule } from '@progress/kendo-angular-dialog';
import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule, TextBoxModule } from '@progress/kendo-angular-inputs';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ComboBoxModule, DropDownsModule } from '@progress/kendo-angular-dropdowns'
import { ExcelExportModule } from '@progress/kendo-angular-excel-export';
import { NotificationModule } from '@progress/kendo-angular-notification';

@NgModule({
  declarations: [
    IdentityAdminActivityComponent,
    IdentityAdminClaimComponent,
    IdentityAdminClientComponent,
    IdentityAdminIssuerComponent,
    IdentityAdminLoginComponent,
    IdentityAdminRoleComponent,
    IdentityAdminUserComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    GridModule,
    ButtonsModule,
    ComboBoxModule,
    DialogModule, 
    DialogsModule,
    DropDownsModule,
    ExcelExportModule,
    InputsModule, 
    NotificationModule,
    PopupModule,
    TextBoxModule, 
    IdentityAdminRoutingModule,
  ],
})
export class IdentityAdminModule { }