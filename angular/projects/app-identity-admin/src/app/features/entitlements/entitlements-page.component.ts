import { Component } from '@angular/core';
import { KENDO_LAYOUT } from '@progress/kendo-angular-layout';
import { UserEntitlementsComponent } from './user-entitlements.component';
import { AudienceEntitlementsComponent } from './audience-entitlements.component';

@Component({
  selector: 'app-entitlements-page',
  standalone: true,
  imports: [KENDO_LAYOUT, UserEntitlementsComponent, AudienceEntitlementsComponent],
  template: `
    <div class="p-6">
      <kendo-tabstrip>
        <kendo-tabstrip-tab [title]="'User Entitlements'" [selected]="true">
          <ng-template kendoTabContent>
            <app-user-entitlements></app-user-entitlements>
          </ng-template>
        </kendo-tabstrip-tab>
        <kendo-tabstrip-tab [title]="'Audience Entitlements'">
          <ng-template kendoTabContent>
            <app-audience-entitlements></app-audience-entitlements>
          </ng-template>
        </kendo-tabstrip-tab>
      </kendo-tabstrip>
    </div>
  `
})
export class EntitlementsPageComponent {}
