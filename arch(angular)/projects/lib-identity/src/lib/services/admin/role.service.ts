import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { RoleV1, RoleCreate, RoleUpdate } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class RoleService extends BaseAdminService<RoleV1, RoleCreate, RoleUpdate> {
  protected readonly endpoint = 'role';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }

  /**
   * Get roles by audience ID
   */
  getByAudienceId(audienceId: string): Observable<RoleV1[]> {
    return this.http.get<RoleV1[]>(`${this.baseUrl}/${this.endpoint}/v1/audience/${audienceId}`);
  }
}
