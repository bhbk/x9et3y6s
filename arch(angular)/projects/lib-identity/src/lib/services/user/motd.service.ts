import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { MOTDTssV1 } from '../../models';
import { PagedResult } from '../../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class MotdService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.userApiUrl}${this.config.pathBase}/motd`;
  }

  /**
   * Get message of the day
   */
  getMOTD(): Observable<MOTDTssV1> {
    return this.http.get<MOTDTssV1>(`${this.baseUrl}/v1`);
  }

  /**
   * Get all messages of the day (paged)
   */
  getAll(): Observable<PagedResult<MOTDTssV1>> {
    return this.http.get<PagedResult<MOTDTssV1>>(`${this.baseUrl}/v1/page`);
  }
}
