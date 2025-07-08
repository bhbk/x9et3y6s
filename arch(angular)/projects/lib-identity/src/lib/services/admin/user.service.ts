import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { UserV1, UserCreate, UserUpdate, UserPassword, RoleV1, ClaimV1, LoginV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class UserService extends BaseAdminService<UserV1, UserCreate, UserUpdate> {
  protected readonly endpoint = 'user';

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
   * Get logins for user
   */
  getLogins(id: string): Observable<LoginV1[]> {
    return this.http.get<LoginV1[]>(`${this.baseUrl}/${this.endpoint}/v1/${id}/logins`);
  }

  /**
   * Add login to user
   */
  addLogin(userId: string, loginId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/logins/${loginId}`, {});
  }

  /**
   * Remove login from user
   */
  removeLogin(userId: string, loginId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${this.endpoint}/v1/${userId}/logins/${loginId}`);
  }
}
