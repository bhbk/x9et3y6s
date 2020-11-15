import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-identity-admin-claim',
  templateUrl: './identity-admin-claim.component.html',
  styleUrls: ['./identity-admin-claim.component.css']
})
export class IdentityAdminClaimComponent implements OnInit {

  public claimData: GridDataResult;
  public claimLoading: boolean = false;
  public claimState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };
  
  constructor(private adminService: IdentityAdminService) { }

  ngOnInit() {
    this.loadData();
  }

  public pageChange(state: PageChangeEvent): void {
    this.claimState = state;
    this.loadData();
  }

  public dataStateChange(state: DataStateChangeEvent): void {
    this.claimState = state;
  }  

  private loadData(): void {
    // this.claimLoading = true;
    // this.adminService!.getClaims('',
    //   'desc',
    //   'created',
    //   this.claimState.skip,
    //   this.claimState.take)
    //     .pipe(debounceTime(1000))
    //     .subscribe(result => {
    //       this.claimData = { data: result.list, total: result.count };
    //       this.claimLoading = false;
    //     }, (failure: HttpErrorResponse) => {
    //       this.claimLoading = false;
    //     });
  }  
}
