import { environment } from 'src/environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { catchError, delay, map, retry, tap, share, switchMap } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { IIdentityJwtV2, IIdentityLoginV2, IIdentityRefreshV2 } from 'src/app/common/interfaces';

@Injectable({
    providedIn: 'root'
})
export class IdentityStsService {
    private _jwtHelper = new JwtHelperService();
    public currentUserValue: BehaviorSubject<IIdentityJwtV2>;
    public currentUser: Observable<IIdentityJwtV2>;

    constructor(private http: HttpClient) {
        this.currentUserValue = new BehaviorSubject<IIdentityJwtV2>(JSON.parse(localStorage.getItem(environment.tokenName)));
        this.currentUser = this.currentUserValue.asObservable();
    }

    isExpiredAccess(): boolean {
        const date = this._jwtHelper.getTokenExpirationDate(this.currentUserValue.getValue().access_token);

        if (date === undefined)
            return false;

        return !(date.valueOf() > new Date().valueOf());
    }

    isExpiredRefresh(): boolean {
        const date = this._jwtHelper.getTokenExpirationDate(this.currentUserValue.getValue().refresh_token);

        if (date === undefined)
            return false;

        return !(date.valueOf() > new Date().valueOf());
    }

    setStorage(token: IIdentityJwtV2) {
        localStorage.setItem(environment.tokenName, JSON.stringify(token));
    }

    clearStorage() {
        localStorage.removeItem(environment.tokenName);
    }

    postLogin(access: IIdentityLoginV2): Observable<IIdentityJwtV2> {
        const content = `issuer=${encodeURIComponent(
            access.issuer
        )}&client=${encodeURIComponent(
            access.client.join(",")
        )}&grant_type=${encodeURIComponent(
            access.grant_type
        )}&user=${encodeURIComponent(
            access.user
        )}&password=${encodeURIComponent(
            access.password
        )}`;

        const headers = { headers: new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' }) };

        return this.http
            .post<IIdentityJwtV2>(`${environment.IdentityStsUrls.BaseApiUrl}${environment.IdentityStsUrls.BaseApiPath}/oauth2/v2/access`, content, headers)
            .pipe(map(result => {
                return result;
            }));
    }

    postRefresh(refresh: IIdentityRefreshV2): Observable<IIdentityJwtV2> {
        const content = `issuer=${encodeURIComponent(
            refresh.issuer
        )}&client=${encodeURIComponent(
            refresh.client.join(",")
        )}&grant_type=${encodeURIComponent(
            refresh.grant_type
        )}&refresh_token=${encodeURIComponent(
            refresh.refresh_token
        )}`;

        const headers = { headers: new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' }) };

        return this.http
            .post<IIdentityJwtV2>(`${environment.IdentityStsUrls.BaseApiUrl}${environment.IdentityStsUrls.BaseApiPath}/oauth2/v2/refresh`, content, headers)
            .pipe(map(result => {
                return result;
            }));
    }
}
