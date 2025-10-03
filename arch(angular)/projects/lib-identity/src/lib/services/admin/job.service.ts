import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { JobV1, JobSettingV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(ConfigService);

  private get baseUrl(): string {
    return `${this.config.adminApiUrl}${this.config.pathBase}`;
  }

  getAll(): Observable<JobV1[]> {
    return this.http.get<JobV1[]>(`${this.baseUrl}/jobs/v1`);
  }

  update(job: JobV1): Observable<JobV1> {
    return this.http.put<JobV1>(`${this.baseUrl}/jobs/v1`, job);
  }

  updateSettings(jobId: string, settings: JobSettingV1[]): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/jobs/v1/${jobId}/settings`, settings);
  }
}
