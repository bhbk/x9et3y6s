import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { IssuerV1, IssuerCreate, IssuerUpdate } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class IssuerService extends BaseAdminService<IssuerV1, IssuerCreate, IssuerUpdate> {
  protected readonly endpoint = 'issuer';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }
}
