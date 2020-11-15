import { environment } from 'src/environments/environment'
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { IIdentityJwtV2 } from 'src/app/common/interfaces';
import { IdentityStsService } from 'src/app/common/services';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  currentUser: IIdentityJwtV2;

  constructor(private stsService: IdentityStsService,
    private router: Router) { }

  ngOnInit() {
    this.stsService.currentUser.subscribe(x => this.currentUser = x);
  }

  logout() {
    this.stsService.currentUserValue.next(null);
    this.stsService.clearStorage();
  }
}
