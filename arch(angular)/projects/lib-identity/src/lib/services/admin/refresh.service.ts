import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { RefreshV1 } from '../../models';

/**
 * Refresh token management via Admin API.
 *
 * There is no standalone RefreshController in the .NET Admin API.
 * Refresh tokens are managed as sub-resources of Users and Audiences:
 *   - GET    user/v1/{userID}/refreshes
 *   - DELETE user/v1/{userID}/refresh
 *   - DELETE user/v1/{userID}/refresh/{refreshID}
 *   - GET    audience/v1/{audienceID}/refreshes
 *   - DELETE audience/v1/{audienceID}/refresh
 *   - DELETE audience/v1/{audienceID}/refresh/{refreshID}
 */
@Injectable({
  providedIn: 'root'
})
export class RefreshService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}`;
  }

  /**
   * Get refresh tokens for a specific user
   */
  getByUserId(userId: string): Observable<RefreshV1[]> {
    return this.http.get<RefreshV1[]>(`${this.baseUrl}/user/v1/${userId}/refreshes`);
  }

  /**
   * Get refresh tokens for a specific audience
   */
  getByAudienceId(audienceId: string): Observable<RefreshV1[]> {
    return this.http.get<RefreshV1[]>(`${this.baseUrl}/audience/v1/${audienceId}/refreshes`);
  }

  /**
   * Revoke a specific refresh token for a user
   */
  revokeForUser(userId: string, refreshId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/user/v1/${userId}/refresh/${refreshId}`);
  }

  /**
   * Revoke all refresh tokens for a user
   */
  revokeAllForUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/user/v1/${userId}/refresh`);
  }

  /**
   * Revoke a specific refresh token for an audience
   */
  revokeForAudience(audienceId: string, refreshId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/audience/v1/${audienceId}/refresh/${refreshId}`);
  }

  /**
   * Revoke all refresh tokens for an audience
   */
  revokeAllForAudience(audienceId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/audience/v1/${audienceId}/refresh`);
  }
}
