import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from './config.service';
import { UserJwtV2 } from '../models';

/**
 * Authentication service for STS API
 * Handles OAuth2 Resource Owner Password Grant flow with httpOnly cookie refresh tokens
 *
 * STS endpoints use [FromForm] binding, so requests must be sent as
 * application/x-www-form-urlencoded per OAuth2 spec (RFC 6749).
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private readonly formHeaders = new HttpHeaders({
    'Content-Type': 'application/x-www-form-urlencoded'
  });

  /**
   * Authenticate user with username and password
   * Returns access token in response body
   * Refresh token is set as httpOnly cookie by the server
   */
  login(issuer: string, user: string, password: string, client?: string): Observable<UserJwtV2> {
    let body = new HttpParams()
      .set('issuer', issuer)
      .set('user', user)
      .set('password', password)
      .set('grant_type', 'password');

    if (client) {
      body = body.set('client', client);
    }

    return this.http.post<UserJwtV2>(
      `${this.config.stsApiUrl}${this.config.pathBase}/oauth2/v2/ropg`,
      body.toString(),
      { headers: this.formHeaders, withCredentials: true }
    );
  }

  /**
   * Refresh the access token using httpOnly cookie
   * The refresh token is automatically sent via cookie
   */
  refreshToken(issuer: string, client?: string): Observable<UserJwtV2> {
    let body = new HttpParams()
      .set('issuer', issuer)
      .set('grant_type', 'refresh_token');

    if (client) {
      body = body.set('client', client);
    }

    return this.http.post<UserJwtV2>(
      `${this.config.stsApiUrl}${this.config.pathBase}/oauth2/v2/ropg-rt`,
      body.toString(),
      { headers: this.formHeaders, withCredentials: true }
    );
  }

  /**
   * Logout - clears refresh token cookie and invalidates session
   * This endpoint is on the User API (not STS)
   */
  logout(): Observable<void> {
    return this.http.post<void>(
      `${this.config.userApiUrl}${this.config.pathBase}/session/v1/logout`,
      {},
      { withCredentials: true }
    );
  }
}
