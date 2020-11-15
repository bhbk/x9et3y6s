import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-identity-admin-role',
  templateUrl: './identity-admin-role.component.html',
  styleUrls: ['./identity-admin-role.component.css']
})
export class IdentityAdminRoleComponent implements OnInit {

  public roleData: GridDataResult;
  public roleLoading: boolean = false;
  public roleState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };

  constructor(private adminService: IdentityAdminService) { }

  ngOnInit() {
    this.loadData();
  }

  public pageChange(state: PageChangeEvent): void {
    this.roleState = state;
    this.loadData();
  }

  public dataStateChange(state: DataStateChangeEvent): void {
    this.roleState = state;
  }  

  private loadData(): void {
    this.roleLoading = true;
    this.adminService!.getRoles('',
      'asc',
      'name',
      this.roleState.skip,
      this.roleState.take)
        .pipe(debounceTime(1000))
        .subscribe(result => {
          this.roleData = { data: result.list, total: result.count };
          this.roleLoading = false;
        }, (failure: HttpErrorResponse) => {
          this.roleLoading = false;
        });
  }  
}
