import { environment } from 'src/environments/environment';
import { Injectable, Injector } from '@angular/core';
import {
    HttpErrorResponse,
    HttpEvent,
    HttpInterceptor,
    HttpHandler,
    HttpRequest,
    HttpResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, first, map } from 'rxjs/operators';
import { IIdentityRefreshV2, IIdentityJwtV2 } from 'src/app/common/interfaces';
import { IdentityStsService } from 'src/app/common/services';

@Injectable({
    providedIn: 'root'
})
export class JwtInterceptor implements HttpInterceptor {
    currentUser: IIdentityJwtV2;
    stsService: IdentityStsService;

    constructor(private inject: Injector) {
        this.stsService = this.inject.get(IdentityStsService);
    }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any> | HttpResponse<any>> {
        this.currentUser = this.stsService.currentUserValue.getValue();

        if (this.currentUser == null
            || this.currentUser.access_token == null)
            return next.handle(req);

        if (!this.stsService.isExpiredAccess())
            return next.handle(this.authSet(req, this.currentUser.access_token));

        if (this.stsService.isExpiredAccess()
            && !this.stsService.isExpiredRefresh()) {
            let refresh: IIdentityRefreshV2 = {
                issuer: environment.issuer,
                client: this.currentUser.client,
                grant_type: 'refresh_token',
                refresh_token: this.currentUser.refresh_token
            };

            this.stsService.currentUserValue.next(null);

            this.stsService.postRefresh(refresh)
                .pipe(first())
                .subscribe(result => {
                    this.stsService.currentUserValue.next(result);
                    this.stsService.setStorage(result);
                });

            this.currentUser = this.stsService.currentUserValue.getValue();

            return next.handle(this.authSet(req, this.currentUser.access_token))
                .pipe(map((event: HttpEvent<any>) => {
                    // console.log(event);
                    return event;
                }),
                    catchError((error: HttpErrorResponse) => {
                        // console.error(event);
                        this.stsService.currentUserValue.next(null);
                        this.stsService.clearStorage();
                        return next.handle(this.authClear(req));
                    }));
        }

        return next.handle(req);

        if (this.stsService.isExpiredAccess()
            && !this.stsService.isExpiredRefresh()) {
            this.stsService.currentUserValue.next(null);

            let refresh: IIdentityRefreshV2 = {
                issuer: environment.issuer,
                client: this.currentUser.client,
                grant_type: 'refresh_token',
                refresh_token: this.currentUser.refresh_token
            };

            this.stsService.postRefresh(refresh)
                .pipe(first())
                .subscribe(result => {
                    this.stsService.currentUserValue.next(result);
                    this.stsService.setStorage(result);
                });

            return next.handle(this.authSet(req, this.currentUser.access_token));
        }

        return next.handle(req);
    }

    authClear(req: HttpRequest<any>): HttpRequest<any> {
        if (req.headers.has('Authorization'))
            return req.clone(
                {
                    headers: req.headers
                        .delete('Authorization')
                });

        return req;
    }

    authSet(req: HttpRequest<any>, token: string): HttpRequest<any> {
        return req.clone(
            {
                headers: req.headers
                    .set('Authorization', 'Bearer ' + token)
            });
    }
}