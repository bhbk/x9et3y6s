import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { UserV1, UserCreate, UserUpdate, UserPassword, RoleV1, ClaimV1, LoginProviderV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseAdminService<UserV1, UserCreate, UserUpdate> {
  protected readonly endpoint = 'users';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }

  /**
   * Set user password
   */
  setPassword(password: UserPassword): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${this.endpoint}/v1/password`, password);
  }

  /**
   * Remove user password
   */
  removePassword(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${id}/password`);
  }

  /**
   * Get roles for user
   */
  getRoles(id: string): Observable<RoleV1[]> {
    return this.http.get<RoleV1[]>(`${this.baseUrl}/${this.endpoint}/v1/${id}/roles`);
  }

  /**
   * Add role to user
   */
  addRole(userId: string, roleId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/roles/${roleId}`, {});
  }

  /**
   * Remove role from user
   */
  removeRole(userId: string, roleId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/roles/${roleId}`);
  }

  /**
   * Get claims for user
   */
  getClaims(id: string): Observable<ClaimV1[]> {
    return this.http.get<ClaimV1[]>(`${this.baseUrl}/${this.endpoint}/v1/${id}/claims`);
  }

  /**
   * Add claim to user
   */
  addClaim(userId: string, claimId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/claims/${claimId}`, {});
  }

  /**
   * Remove claim from user
   */
  removeClaim(userId: string, claimId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/claims/${claimId}`);
  }

  /**
   * Get login providers for user
   */
  getLoginProviders(id: string): Observable<LoginProviderV1[]> {
    return this.http.get<LoginProviderV1[]>(`${this.baseUrl}/${this.endpoint}/v1/${id}/login-providers`);
  }

  /**
   * Add login provider to user
   */
  addLoginProvider(userId: string, loginProviderId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/add-to-login-provider/${loginProviderId}`, {});
  }

  /**
   * Remove login provider from user
   */
  removeLoginProvider(userId: string, loginProviderId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/remove-from-login-provider/${loginProviderId}`);
  }
}
