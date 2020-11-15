import { environment } from 'src/environments/environment'
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { IIdentityLoginV2, IIdentityJwtV2 } from 'src/app/common/interfaces';
import { IdentityStsService, NotifyService } from 'src/app/common/services';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-me-authorize',
    templateUrl: './me-authorize.component.html',
    styleUrls: ['./me-authorize.component.css']
})
export class IdentityMeAuthorizeComponent implements OnInit {
    currentLogin: IIdentityLoginV2 = {
        issuer: environment.issuer,
        client: [] = [],
        grant_type: 'password',
        user: '',
        password: ''
    };
    currentUser: IIdentityJwtV2;
    loginLoading: boolean = false;
    returnUrl: string;

    constructor(private route: ActivatedRoute,
        private notifyService: NotifyService,
        private stsService: IdentityStsService,
        private router: Router) { }

    ngOnInit() {
        this.stsService.currentUser.subscribe(x => this.currentUser = x);

        if (this.currentUser != null
            && this.currentUser.access_token != null) {
            this.router.navigate(['/me/home']);
        }
        else {
            this.stsService.currentUserValue.next(null);
            this.stsService.clearStorage();
            this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
        }
    }

    onSubmit() {
        this.loginLoading = true;

        this.stsService.postLogin(this.currentLogin)
            .subscribe(success => {
                this.stsService.currentUserValue.next(success);
                this.stsService.setStorage(success);
                this.router.navigate([this.returnUrl]);
                this.loginLoading = false;
            }, (failure: HttpErrorResponse) => {
                this.loginLoading = false;
            });
    }
}
