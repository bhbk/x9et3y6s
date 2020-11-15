import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-identity-admin-client',
  templateUrl: './identity-admin-client.component.html',
  styleUrls: ['./identity-admin-client.component.css']
})
export class IdentityAdminClientComponent implements OnInit {

  public clientData: GridDataResult;
  public clientLoading: boolean = false;
  public clientState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };

  constructor(private adminService: IdentityAdminService) { }

  ngOnInit() {
    this.loadData();
  }

  public pageChange(state: PageChangeEvent): void {
    this.clientState = state;
    this.loadData();
  }

  public dataStateChange(state: DataStateChangeEvent): void {
    this.clientState = state;
  }  

  private loadData(): void {
    this.clientLoading = true;
    this.adminService!.getClients('',
      'asc',
      'name',
      this.clientState.skip,
      this.clientState.take)
        .pipe(debounceTime(1000))
        .subscribe(result => {
          this.clientData = { data: result.list, total: result.count };
          this.clientLoading = false;
        }, (failure: HttpErrorResponse) => {
          this.clientLoading = false;
        });
  }  
}
