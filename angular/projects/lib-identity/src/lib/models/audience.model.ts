/**
 * Audience Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Audiences
 */

import { RoleV1 } from './role.model';

export interface AudienceV1 {
  id: string;
  issuerId: string;
  name: string;
  description?: string;
  isLockedOut: boolean;
  isDeletable: boolean;
  lockoutEnd?: string;
  concurrencyStamp?: string;
  securityStamp?: string;
  created: string;
  roles?: RoleV1[];
}

export interface AudienceCreate {
  issuerId: string;
  name: string;
  description?: string;
  isLockedOut: boolean;
  isDeletable: boolean;
}

export interface AudienceUpdate {
  id: string;
  issuerId: string;
  name: string;
  description?: string;
  isLockedOut: boolean;
  isDeletable: boolean;
}

export interface AudiencePassword {
  entityId: string;
  newPassword: string;
  newPasswordConfirm: string;
}
