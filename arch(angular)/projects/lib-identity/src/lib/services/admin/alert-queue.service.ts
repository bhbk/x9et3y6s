import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { EmailQueueV1, TextQueueV1, DataStateQuery, PagedResult } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AlertQueueService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}`;
  }

  getEmails(query?: DataStateQuery): Observable<PagedResult<EmailQueueV1>> {
    return this.http.post<PagedResult<EmailQueueV1>>(`${this.baseUrl}/enqueue/v1/email/page`, query ?? {});
  }

  deleteEmail(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/dequeue/v1/email/${id}`);
  }

  getTexts(query?: DataStateQuery): Observable<PagedResult<TextQueueV1>> {
    return this.http.post<PagedResult<TextQueueV1>>(`${this.baseUrl}/enqueue/v1/text/page`, query ?? {});
  }

  deleteText(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/dequeue/v1/text/${id}`);
  }
}
