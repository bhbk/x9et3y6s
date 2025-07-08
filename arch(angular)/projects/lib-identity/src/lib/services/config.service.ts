import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppConfig } from '../models/app-config.model';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private readonly http = inject(HttpClient);
  private readonly platformId = inject(PLATFORM_ID);
  private config: AppConfig | null = null;

  private get isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  async loadConfig(): Promise<void> {
    // Only load config in browser
    if (!this.isBrowser) {
      return;
    }

    try {
      this.config = await firstValueFrom(
        this.http.get<AppConfig>('/config.json').pipe(
          catchError(() => of(null))
        )
      );
    } catch {
      console.warn('Failed to load config.json');
    }
  }

  get isLoaded(): boolean {
    return this.config !== null;
  }

  get defaultIssuer(): string {
    return this.config?.defaultIssuer ?? '';
  }

  get defaultClient(): string | undefined {
    return this.config?.defaultClient;
  }

  get pathBase(): string {
    return this.config?.pathBase ?? '';
  }

  get stsApiUrl(): string {
    return this.config?.stsApiUrl ?? '';
  }

  get adminApiUrl(): string {
    return this.config?.adminApiUrl ?? '';
  }

  get userApiUrl(): string {
    return this.config?.userApiUrl ?? '';
  }

  get alertApiUrl(): string {
    return this.config?.alertApiUrl ?? '';
  }

  get adminPortalUrl(): string | undefined {
    return this.config?.adminPortalUrl;
  }

  get userPortalUrl(): string | undefined {
    return this.config?.userPortalUrl;
  }
}
