import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { AudienceEntitlementV1, AudienceEntitlementCreate, AudienceEntitlementUpdate, EntitlementTypeV1, EntitlementScopeV1, DataStateQuery, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AudienceEntitlementService extends BaseAdminService<AudienceEntitlementV1, AudienceEntitlementCreate, AudienceEntitlementUpdate> {
  protected readonly endpoint = 'entitlements';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }

  override getAll(query?: DataStateQuery): Observable<PagedResult<AudienceEntitlementV1>> {
    return this.http.post<PagedResult<AudienceEntitlementV1>>(`${this.baseUrl}/${this.endpoint}/v1/audiences/page`, query ?? {});
  }

  override getById(id: string): Observable<AudienceEntitlementV1> {
    return this.http.get<AudienceEntitlementV1>(`${this.baseUrl}/${this.endpoint}/v1/audiences/${id}`);
  }

  override create(item: AudienceEntitlementCreate): Observable<AudienceEntitlementV1> {
    return this.http.post<AudienceEntitlementV1>(`${this.baseUrl}/${this.endpoint}/v1/audiences`, item);
  }

  override update(item: AudienceEntitlementUpdate): Observable<AudienceEntitlementV1> {
    return this.http.put<AudienceEntitlementV1>(`${this.baseUrl}/${this.endpoint}/v1/audiences`, item);
  }

  override delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/audiences/${id}`);
  }

  getTypes(): Observable<EntitlementTypeV1[]> {
    return this.http.get<EntitlementTypeV1[]>(`${this.baseUrl}/${this.endpoint}/v1/types`);
  }

  getScopes(): Observable<EntitlementScopeV1[]> {
    return this.http.get<EntitlementScopeV1[]>(`${this.baseUrl}/${this.endpoint}/v1/scopes`);
  }

  getByAudienceId(audienceId: string): Observable<AudienceEntitlementV1[]> {
    return this.http.get<AudienceEntitlementV1[]>(`${this.baseUrl}/${this.endpoint}/v1/audiences/audience/${audienceId}`);
  }
}
