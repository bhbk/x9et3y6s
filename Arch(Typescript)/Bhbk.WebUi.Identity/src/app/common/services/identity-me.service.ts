import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators/map';
import { Observable } from 'rxjs';
import { IIdentityIssuerModel, IIdentityClientModel, IIdentityLoginModel, IIdentityRoleModel, IIdentityUserModel } from 'src/app/common/interfaces';

@Injectable({
    providedIn: 'root'
})
export class IdentityMeService {
    constructor(private http: HttpClient) { }

    getDetail(id: string): Observable<IIdentityUserModel> {
        return this.http
            .get<IIdentityUserModel>(`${environment.IdentityMeUrls.BaseApiUrl}${environment.IdentityMeUrls.BaseApiPath}/detail/v1`, {
                params: new HttpParams()
                    .set('id', id)
            });
    }
}
