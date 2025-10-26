import { Component } from '@angular/core';
import { KENDO_LAYOUT } from '@progress/kendo-angular-layout';
import { UserActivityComponent } from './user-activity.component';
import { AudienceActivityComponent } from './audience-activity.component';

@Component({
  selector: 'app-activity-page',
  standalone: true,
  imports: [KENDO_LAYOUT, UserActivityComponent, AudienceActivityComponent],
  template: `
    <div class="p-6">
      <kendo-tabstrip>
        <kendo-tabstrip-tab [title]="'User Activity'" [selected]="true">
          <ng-template kendoTabContent>
            <app-user-activity></app-user-activity>
          </ng-template>
        </kendo-tabstrip-tab>
        <kendo-tabstrip-tab [title]="'Audience Activity'">
          <ng-template kendoTabContent>
            <app-audience-activity></app-audience-activity>
          </ng-template>
        </kendo-tabstrip-tab>
      </kendo-tabstrip>
    </div>
  `
})
export class ActivityPageComponent {}
