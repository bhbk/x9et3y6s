import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { UserActivityV1, DataStateQuery, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class UserActivityService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}/activities`;
  }

  /**
   * Get all auth activity with optional filtering
   * .NET endpoint: POST activity/v1/page with [FromBody] DataStateV1
   */
  getAll(query?: DataStateQuery): Observable<PagedResult<UserActivityV1>> {
    return this.http.post<PagedResult<UserActivityV1>>(`${this.baseUrl}/v1/users/page`, query ?? {});
  }

  /**
   * Get auth activity by ID
   */
  getById(id: string): Observable<UserActivityV1> {
    return this.http.get<UserActivityV1>(`${this.baseUrl}/v1/users/${id}`);
  }

}
