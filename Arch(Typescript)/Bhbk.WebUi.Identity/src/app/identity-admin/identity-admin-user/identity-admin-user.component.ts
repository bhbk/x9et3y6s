import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-identity-admin-user',
  templateUrl: './identity-admin-user.component.html',
  styleUrls: ['./identity-admin-user.component.css']
})
export class IdentityAdminUserComponent implements OnInit {

  public userData: GridDataResult;
  public userLoading: boolean = false;
  public userState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };

  constructor(private adminService: IdentityAdminService) { }

  ngOnInit() {
    this.loadData();
  }

  public pageChange(state: PageChangeEvent): void {
    this.userState = state;
    this.loadData();
  }

  public dataStateChange(state: DataStateChangeEvent): void {
    this.userState = state;
  }  

  private loadData(): void {
    this.userLoading = true;
    this.adminService!.getUsers('',
      'asc',
      'email',
      this.userState.skip,
      this.userState.take)
        .pipe(debounceTime(1000))
        .subscribe(result => {
          this.userData = { data: result.list, total: result.count };
          this.userLoading = false;
        }, (failure: HttpErrorResponse) => {
          this.userLoading = false;
        });
  }  
}
