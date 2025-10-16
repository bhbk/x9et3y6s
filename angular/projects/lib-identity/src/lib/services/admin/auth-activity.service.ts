import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { AuthActivityV1, DataStateQuery, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AuthActivityService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}/activities`;
  }

  /**
   * Get all auth activity with optional filtering
   * .NET endpoint: POST activity/v1/page with [FromBody] DataStateV1
   */
  getAll(query?: DataStateQuery): Observable<PagedResult<AuthActivityV1>> {
    return this.http.post<PagedResult<AuthActivityV1>>(`${this.baseUrl}/v1/page`, query ?? {});
  }

  /**
   * Get auth activity by ID
   */
  getById(id: string): Observable<AuthActivityV1> {
    return this.http.get<AuthActivityV1>(`${this.baseUrl}/v1/${id}`);
  }

}
