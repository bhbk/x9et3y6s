import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { QuoteV1 } from '../../models';
import { PagedResult } from '../../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class QuoteService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.userApiUrl}${this.config.pathBase}/quotes`;
  }

  /**
   * Get quote of the day
   */
  getQuote(): Observable<QuoteV1> {
    return this.http.get<QuoteV1>(`${this.baseUrl}/v1`);
  }

  /**
   * Get all quotes (paged)
   */
  getAll(): Observable<PagedResult<QuoteV1>> {
    return this.http.get<PagedResult<QuoteV1>>(`${this.baseUrl}/v1/page`);
  }
}
