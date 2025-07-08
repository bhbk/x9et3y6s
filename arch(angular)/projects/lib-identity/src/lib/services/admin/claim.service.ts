import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { ClaimV1, ClaimCreate, ClaimUpdate } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class ClaimService extends BaseAdminService<ClaimV1, ClaimCreate, ClaimUpdate> {
  protected readonly endpoint = 'claim';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }

  /**
   * Get claims by issuer ID
   */
  getByIssuerId(issuerId: string): Observable<ClaimV1[]> {
    return this.http.get<ClaimV1[]>(`${this.baseUrl}/${this.endpoint}/v1/issuer/${issuerId}`);
  }
}
