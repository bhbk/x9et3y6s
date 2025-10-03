import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { LoginProviderV1, LoginProviderCreate, LoginProviderUpdate } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class LoginProviderService extends BaseAdminService<LoginProviderV1, LoginProviderCreate, LoginProviderUpdate> {
  protected readonly endpoint = 'login-providers';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }
}
