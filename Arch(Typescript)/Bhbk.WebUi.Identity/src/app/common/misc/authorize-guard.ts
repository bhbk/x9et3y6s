import { environment } from 'src/environments/environment';
import { Injectable } from '@angular/core';
import { 
    ActivatedRouteSnapshot, 
    CanActivate, 
    CanActivateChild, 
    Router, 
    RouterStateSnapshot,
} from '@angular/router';
import { IIdentityJwtV2 } from 'src/app/common/interfaces';
import { IdentityStsService } from 'src/app/common/services';

@Injectable({
    providedIn: 'root'
})
export class AuthorizeGuard implements CanActivate, CanActivateChild {
    currentUser: IIdentityJwtV2;

    constructor(private router: Router,
        private stsService: IdentityStsService) {
        this.stsService.currentUser.subscribe(x => this.currentUser = x);
    }

    canActivate(route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot) {
            
        if (this.currentUser != null
            && this.currentUser.access_token != null) {
            return true;
        }

        this.router.navigate(['/me/authorize'],
            {
                queryParams: { returnUrl: state.url }
            });

        return false;
    }

    canActivateChild(route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot) {
            
        return this.canActivate(route, state);
    }
}