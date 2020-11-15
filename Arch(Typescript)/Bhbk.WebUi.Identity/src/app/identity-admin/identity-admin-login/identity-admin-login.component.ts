import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-identity-admin-login',
  templateUrl: './identity-admin-login.component.html',
  styleUrls: ['./identity-admin-login.component.css']
})
export class IdentityAdminLoginComponent implements OnInit {

  public loginData: GridDataResult;
  public loginLoading: boolean = false;
  public loginState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };  

  constructor(private adminService: IdentityAdminService) { }

  ngOnInit() {
    this.loadData();
  }

  public pageChange(state: PageChangeEvent): void {
    this.loginState = state;
    this.loadData();
  }

  public dataStateChange(state: DataStateChangeEvent): void {
    this.loginState = state;
  }  

  private loadData(): void {
    this.loginLoading = true;
    this.adminService!.getLogins('',
      'asc',
      'name',
      this.loginState.skip,
      this.loginState.take)
        .pipe(debounceTime(1000))
        .subscribe(result => {
          this.loginData = { data: result.list, total: result.count };
          this.loginLoading = false;
        }, (failure: HttpErrorResponse) => {
          this.loginLoading = false;
        });
  }  
}
