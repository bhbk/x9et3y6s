import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import {
  IIdentityActivityList,
  IIdentityClaimList,
  IIdentityLoginSet,
  IIdentityRoleList,
  IIdentityUserList,
} from 'src/app/common/interfaces';
import { IIdentityClientCreate, IIdentityClientModel, IIdentityClientList } from 'src/app/common/interfaces';
import { IIdentityIssuerCreate, IIdentityIssuerModel, IIdentityIssuerList } from 'src/app/common/interfaces';
import { NotifyService } from 'src/app/common/services/notify.service'

@Injectable({
  providedIn: 'root'
})
export class IdentityAdminService {
  constructor(private notifyService: NotifyService,
    private http: HttpClient) { }

  //activity stuff...
  getActivity(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityActivityList> {
    return this.http
      .get<IIdentityActivityList>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/activity/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }

  //claim stuff..
  getClaims(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityClaimList> {
    return this.http
      .get<IIdentityClaimList>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/claim/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }

  //client stuff...
  getClients(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityClientList> {
    return this.http
      .get<IIdentityClientList>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/client/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }

  //issuer stuff
  getIssuers(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityIssuerList> {
    return this.http
      .get<IIdentityIssuerList>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/issuer/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }

  putIssuer(issuer: IIdentityIssuerModel) {
    return this.http
      .put<IIdentityIssuerModel>(`${environment.IdentityAdminUrls.BaseApiUrl}/issuer/v1`, issuer)
      .pipe(tap(() => this.notifyService.showSuccess(`Updated ${issuer.name}`)));
  }

  postIssuer(issuer: IIdentityIssuerModel) {
    return this.http
      .post<IIdentityIssuerModel>(`${environment.IdentityAdminUrls.BaseApiUrl}/issuer/v1`, issuer)
      .pipe(tap(() => this.notifyService.showSuccess(`Added ${issuer.name}`)));
  }

  deleteIssuer(issuer: IIdentityIssuerModel) {
    return this.http
      .delete(`${environment.IdentityAdminUrls.BaseApiUrl}/issuer/v1/${issuer.id}`)
      .pipe(tap(() => this.notifyService.showSuccess(`Removed ${issuer.name}`)));
  }

  //login stuff...
  getLogins(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityLoginSet> {
    return this.http
      .get<IIdentityLoginSet>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/login/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }

  //role stuff...
  getRoles(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityRoleList> {
    return this.http
      .get<IIdentityRoleList>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/role/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }

  //user stuff...
  getUsers(filter: string, order: string, orderBy: string, skip: number, take: number): Observable<IIdentityUserList> {
    return this.http
      .get<IIdentityUserList>(`${environment.IdentityAdminUrls.BaseApiUrl}${environment.IdentityAdminUrls.BaseApiPath}/user/v1/page`, {
        params: new HttpParams()
          .set('filter', filter)
          .set('order', order)
          .set('orderBy', orderBy)
          .set('skip', skip.toString())
          .set('take', take.toString())
      });
  }
}
