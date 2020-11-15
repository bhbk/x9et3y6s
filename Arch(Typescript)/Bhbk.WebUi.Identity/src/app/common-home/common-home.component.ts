import { environment } from 'src/environments/environment';
import { Component, OnInit } from '@angular/core';
import { first } from 'rxjs/operators';
import { IdentityMeService, IdentityStsService } from 'src/app/common/services';
import { IIdentityJwtV2, IIdentityUserModel } from 'src/app/common/interfaces'; 

@Component({
  selector: 'app-common-home',
  templateUrl: './common-home.component.html',
  styleUrls: ['./common-home.component.css']
})
export class CommonHomeComponent implements OnInit {

  currentUser: IIdentityJwtV2;
  currentUserDetail: IIdentityUserModel;

  constructor(private meService: IdentityMeService,
    private stsService: IdentityStsService) 
    {
      this.stsService.currentUser.subscribe(x => this.currentUser = x);
    }

    ngOnInit() 
    {
      this.meService.getDetail(this.currentUser.user)
      .pipe(first())
      .subscribe(result => 
          { 
            this.currentUserDetail = result; 
          });
    }
}
