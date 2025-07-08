import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { AudienceV1, AudienceCreate, AudienceUpdate, AudiencePassword, RoleV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AudienceService extends BaseAdminService<AudienceV1, AudienceCreate, AudienceUpdate> {
  protected readonly endpoint = 'audience';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }

  /**
   * Get audiences by issuer ID
   */
  getByIssuerId(issuerId: string): Observable<AudienceV1[]> {
    return this.http.get<AudienceV1[]>(`${this.baseUrl}/${this.endpoint}/v1/issuer/${issuerId}`);
  }

  /**
   * Set audience password
   */
  setPassword(password: AudiencePassword): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${this.endpoint}/v1/password`, password);
  }

  /**
   * Remove audience password
   */
  removePassword(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${id}/password`);
  }

  /**
   * Get roles for audience
   */
  getRoles(id: string): Observable<RoleV1[]> {
    return this.http.get<RoleV1[]>(`${this.baseUrl}/${this.endpoint}/v1/${id}/roles`);
  }

  /**
   * Add role to audience
   */
  addRole(audienceId: string, roleId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${this.endpoint}/v1/${audienceId}/roles/${roleId}`, {});
  }

  /**
   * Remove role from audience
   */
  removeRole(audienceId: string, roleId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${audienceId}/roles/${roleId}`);
  }
}
