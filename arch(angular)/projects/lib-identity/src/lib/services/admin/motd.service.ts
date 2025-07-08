import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { MOTDTssV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AdminMotdService extends BaseAdminService<MOTDTssV1, Partial<MOTDTssV1>, Partial<MOTDTssV1>> {
  protected readonly endpoint = 'motd';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }
}
