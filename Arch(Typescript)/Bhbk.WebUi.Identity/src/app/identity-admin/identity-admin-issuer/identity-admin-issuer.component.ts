import { Component, OnInit } from '@angular/core';
import { debounceTime } from 'rxjs/operators';
import { IdentityAdminService } from 'src/app/common/services';
import { DataStateChangeEvent, GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NgForm } from '@angular/forms';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { IIdentityIssuerCreate, IIdentityIssuerModel } from 'src/app/common/interfaces'
import { NgGridBase } from 'src/app/common/misc'

@Component({
  selector: 'app-identity-admin-issuer',
  templateUrl: './identity-admin-issuer.component.html',
  styleUrls: ['./identity-admin-issuer.component.css']
})
export class IdentityAdminIssuerComponent 
  extends NgGridBase<IIdentityIssuerModel> implements OnInit {

  issuerLoading: boolean = false;
  issuerData: GridDataResult;
  issuerState: State = {
    skip: 0,
    take: 15,
    sort: [],
  };

  constructor(private adminService: IdentityAdminService) { 
    super();
  }

  ngOnInit() {
    this.fetchData();
  }

  pageChange(state: PageChangeEvent): void {
    this.issuerState = state;
    this.fetchData();
  }

  saveHandler(event, form: NgForm): void {

    // this.issuerLoading = true;

    if (event.isNew) {
      this.adminService.postIssuer(event.dataItem).subscribe(result => {
        this.issuerData.data.push(result);
        this.formatData();

        //update pager information...
        this.updateState(event.sender);

        // this.issuerLoading = false;
      });
    } else {
      this.adminService.putIssuer(event.dataItem).subscribe(result => {
        Object.assign(event.dataItem, result);
        this.formatData();

        // this.issuerLoading = false;
      });
    }

    event.sender.closeRow(event.rowIndex);
  }

  removeHandler(event): void {

    // this.issuerLoading = true;

    if (confirm(`Remove ${event.dataItem.name}?`) && !event.dataItem.immutable) {
      this.adminService.deleteIssuer(event.dataItem).subscribe(() => {
        this.issuerData.data.splice(this.issuerData.data.indexOf(event.dataItem), 1);
        this.updateState(event.sender);

        // this.issuerLoading = false;
      });
    }
  }

  private fetchData(): void {

    this.issuerLoading = true;

    this.adminService.getIssuers('',
      'asc',
      'name',
      this.issuerState.skip,
      this.issuerState.take).subscribe(result => {
        this.issuerData = { data: result.list, total: result.count };
        this.formatData();

        this.issuerLoading = false;
      });
    }  

  private formatData(): void {
    this.issuerData.data.forEach(issuer => {
      issuer.created = issuer.created ? new Date(issuer.created) : null;
      issuer.lastUpdated = issuer.lastUpdated ? new Date(issuer.lastUpdated) : null;
    });
  }  
}
