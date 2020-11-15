import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-identity-admin-activity',
  templateUrl: './identity-admin-activity.component.html',
  styleUrls: ['./identity-admin-activity.component.css']
})
export class IdentityAdminActivityComponent implements OnInit {

  public activityData: GridDataResult;
  public activityLoading: boolean = false;
  public activityState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };
  
  constructor(private adminService: IdentityAdminService) { }

  ngOnInit() {
    this.loadData();
  }

  public pageChange(state: PageChangeEvent): void {
    this.activityState = state;
    this.loadData();
  }

  public dataStateChange(state: DataStateChangeEvent): void {
    this.activityState = state;
    this.pretty();
  }  

  private loadData(): void {
    this.activityLoading = true;
    this.adminService!.getActivity('',
      'desc',
      'created',
      this.activityState.skip,
      this.activityState.take)
        .pipe(debounceTime(1000))
        .subscribe(result => {
          this.activityData = { data: result.list, total: result.count };
          this.activityLoading = false;
          this.pretty();
        }, (failure: HttpErrorResponse) => {
          this.activityLoading = false;
        });
  }  

  private pretty() {
    this.activityData.data.forEach(issuer => {
      issuer.created = issuer.created ? new Date(issuer.created) : null;
      issuer.lastUpdated = issuer.lastUpdated ? new Date(issuer.lastUpdated) : null;
    });
  }  
}
