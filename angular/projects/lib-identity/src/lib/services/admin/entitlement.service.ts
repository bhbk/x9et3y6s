import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { UserEntitlementV1, UserEntitlementCreate, UserEntitlementUpdate, EntitlementTypeV1, EntitlementScopeV1, DataStateQuery, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class EntitlementService extends BaseAdminService<UserEntitlementV1, UserEntitlementCreate, UserEntitlementUpdate> {
  protected readonly endpoint = 'entitlements';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }

  override getAll(query?: DataStateQuery): Observable<PagedResult<UserEntitlementV1>> {
    return this.http.post<PagedResult<UserEntitlementV1>>(`${this.baseUrl}/${this.endpoint}/v1/users/page`, query ?? {});
  }

  override getById(id: string): Observable<UserEntitlementV1> {
    return this.http.get<UserEntitlementV1>(`${this.baseUrl}/${this.endpoint}/v1/users/${id}`);
  }

  override create(item: UserEntitlementCreate): Observable<UserEntitlementV1> {
    return this.http.post<UserEntitlementV1>(`${this.baseUrl}/${this.endpoint}/v1/users`, item);
  }

  override update(item: UserEntitlementUpdate): Observable<UserEntitlementV1> {
    return this.http.put<UserEntitlementV1>(`${this.baseUrl}/${this.endpoint}/v1/users`, item);
  }

  override delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/users/${id}`);
  }

  getTypes(): Observable<EntitlementTypeV1[]> {
    return this.http.get<EntitlementTypeV1[]>(`${this.baseUrl}/${this.endpoint}/v1/types`);
  }

  getScopes(): Observable<EntitlementScopeV1[]> {
    return this.http.get<EntitlementScopeV1[]>(`${this.baseUrl}/${this.endpoint}/v1/scopes`);
  }

  getByUserId(userId: string): Observable<UserEntitlementV1[]> {
    return this.http.get<UserEntitlementV1[]>(`${this.baseUrl}/${this.endpoint}/v1/users/user/${userId}`);
  }

  getMyEntitlements(): Observable<UserEntitlementV1[]> {
    return this.http.get<UserEntitlementV1[]>(`${this.baseUrl}/${this.endpoint}/v1/users/me`);
  }
}
