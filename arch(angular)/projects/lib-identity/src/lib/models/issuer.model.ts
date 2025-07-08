/**
 * Issuer Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Issuers
 */

import { AudienceV1 } from './audience.model';

export interface IssuerV1 {
  id: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  createdUtc: string;
  audiences?: AudienceV1[];
}

export interface IssuerCreate {
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface IssuerUpdate {
  id: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}
