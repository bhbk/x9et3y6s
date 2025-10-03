import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { SettingV1, SettingCreate, SettingUpdate } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class SettingService extends BaseAdminService<SettingV1, SettingCreate, SettingUpdate> {
  protected readonly endpoint = 'settings';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }
}
