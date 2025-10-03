import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { UserV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  /**
   * Get current user's profile
   */
  getProfile(): Observable<UserV1> {
    return this.http.get<UserV1>(`${this.config.userApiUrl}${this.config.pathBase}/profiles/v1`);
  }

  /**
   * Update current user's profile (FirstName, LastName, PhoneNumber)
   */
  updateProfile(profile: UserV1): Observable<UserV1> {
    return this.http.put<UserV1>(`${this.config.userApiUrl}${this.config.pathBase}/profiles/v1`, profile);
  }

  /**
   * Confirm email address with token
   */
  confirmEmail(token: string): Observable<void> {
    return this.http.post<void>(`${this.config.userApiUrl}${this.config.pathBase}/credentials/v1/email/confirm`, { token });
  }

  /**
   * Confirm phone number with code
   */
  confirmPhone(code: string): Observable<void> {
    return this.http.post<void>(`${this.config.userApiUrl}${this.config.pathBase}/credentials/v1/phone/confirm`, { code });
  }
}
