import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { AudienceActivityV1, DataStateQuery, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AudienceActivityService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}/activities`;
  }

  getAll(query?: DataStateQuery): Observable<PagedResult<AudienceActivityV1>> {
    return this.http.post<PagedResult<AudienceActivityV1>>(`${this.baseUrl}/v1/audiences/page`, query ?? {});
  }
}
