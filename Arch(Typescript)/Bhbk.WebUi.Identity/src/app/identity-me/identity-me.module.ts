import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { 
  IdentityMeAuthorizeComponent,
  IdentityMeAuthorizeCallbackComponent,
} from 'src/app/identity-me';
import { IdentityMeRoutingModule } from 'src/app/identity-me/identity-me-routing.module';
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
    IdentityMeAuthorizeComponent,
    IdentityMeAuthorizeCallbackComponent,
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
    IdentityMeRoutingModule,
  ],
})
export class IdentityMeModule { }