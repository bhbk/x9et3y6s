import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { RefreshV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class SessionService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  /**
   * Get all active sessions (refresh tokens) for current user
   */
  getSessions(): Observable<RefreshV1[]> {
    return this.http.get<RefreshV1[]>(`${this.config.userApiUrl}${this.config.pathBase}/sessions/v1/refreshes`);
  }

  /**
   * Revoke a specific session
   */
  revokeSession(refreshId: string): Observable<void> {
    return this.http.delete<void>(`${this.config.userApiUrl}${this.config.pathBase}/sessions/v1/refreshes/${refreshId}`);
  }

  /**
   * Revoke all sessions for current user
   */
  revokeAllSessions(): Observable<void> {
    return this.http.delete<void>(`${this.config.userApiUrl}${this.config.pathBase}/sessions/v1/refreshes`);
  }

  /**
   * Logout - revokes current session and clears refresh token cookie
   */
  logout(): Observable<void> {
    return this.http.post<void>(
      `${this.config.userApiUrl}${this.config.pathBase}/sessions/v1/logout`,
      {},
      { withCredentials: true }
    );
  }
}
