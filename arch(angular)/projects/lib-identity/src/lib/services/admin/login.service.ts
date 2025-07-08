import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { LoginV1, LoginCreate, LoginUpdate } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class LoginService extends BaseAdminService<LoginV1, LoginCreate, LoginUpdate> {
  protected readonly endpoint = 'login';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }
}
