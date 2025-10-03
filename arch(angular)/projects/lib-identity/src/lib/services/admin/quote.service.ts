import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ConfigService } from '../config.service';
import { BaseAdminService } from './base-admin.service';
import { QuoteV1 } from '../../models';

@Injectable({
  providedIn: 'root'
})
export class AdminQuoteService extends BaseAdminService<QuoteV1, Partial<QuoteV1>, Partial<QuoteV1>> {
  protected readonly endpoint = 'quotes';

  constructor() {
    super(inject(HttpClient), inject(ConfigService));
  }
}
