import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { PasswordChange } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class PasswordService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  /**
   * Change password (sends confirmation email with token)
   */
  changePassword(data: PasswordChange): Observable<void> {
    return this.http.put<void>(`${this.config.userApiUrl}${this.config.pathBase}/credentials/v1/password`, data);
  }

  /**
   * Set password directly (for immediate change without email confirmation)
   */
  setPassword(data: PasswordChange): Observable<void> {
    return this.http.put<void>(`${this.config.userApiUrl}${this.config.pathBase}/credentials/v1/password/set`, data);
  }

  /**
   * Confirm password change with token
   */
  confirmPassword(userId: string, password: string, token: string): Observable<void> {
    return this.http.put<void>(
      `${this.config.userApiUrl}${this.config.pathBase}/credentials/v1/password/confirm/${userId}`,
      { password, token }
    );
  }

  /**
   * Request password reset email (public endpoint)
   */
  requestPasswordReset(email: string): Observable<void> {
    return this.http.post<void>(`${this.config.stsApiUrl}${this.config.pathBase}/oauth2/v1/password/reset`, { email });
  }

  /**
   * Reset password with token (public endpoint)
   */
  resetPassword(token: string, newPassword: string, confirmPassword: string): Observable<void> {
    return this.http.post<void>(`${this.config.stsApiUrl}${this.config.pathBase}/oauth2/v1/password/reset/confirm`, {
      token,
      newPassword,
      confirmPassword
    });
  }
}
