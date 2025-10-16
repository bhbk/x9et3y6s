import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { LLMProviderV1, LLMProviderOrderUpdate, LLMProviderSettingV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class LLMProviderService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}`;
  }

  getAll(): Observable<LLMProviderV1[]> {
    return this.http.get<LLMProviderV1[]>(`${this.baseUrl}/llm-providers/v1`);
  }

  update(provider: LLMProviderV1): Observable<LLMProviderV1> {
    return this.http.put<LLMProviderV1>(`${this.baseUrl}/llm-providers/v1`, provider);
  }

  updateOrder(order: LLMProviderOrderUpdate[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/llm-providers/v1/order`, order);
  }

  updateSettings(providerId: string, settings: LLMProviderSettingV1[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/llm-providers/v1/${providerId}/settings`, settings);
  }
}
