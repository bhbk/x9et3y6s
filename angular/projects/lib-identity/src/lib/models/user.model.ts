/**
 * User Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Users
 */

import { RoleV1 } from './role.model';
import { ClaimV1 } from './claim.model';
import { LoginProviderV1 } from './login-provider.model';

export interface UserV1 {
  id: string;
  userName: string;
  email: string;
  phoneNumber?: string;
  firstName: string;
  lastName: string;
  isHumanBeing: boolean;
  isLockedOut: boolean;
  isDeletable: boolean;
  emailConfirmed: boolean;
  passwordConfirmed: boolean;
  phoneNumberConfirmed?: boolean;
  lockoutEndUtc?: string;
  concurrencyStamp?: string;
  securityStamp?: string;
  createdUtc: string;
  roles?: RoleV1[];
  claims?: ClaimV1[];
  loginProviders?: LoginProviderV1[];
}

export interface UserCreate {
  userName: string;
  email: string;
  phoneNumber?: string;
  firstName: string;
  lastName: string;
  isHumanBeing: boolean;
  isDeletable: boolean;
}

export interface UserUpdate {
  id: string;
  userName: string;
  email: string;
  phoneNumber?: string;
  firstName: string;
  lastName: string;
  isHumanBeing: boolean;
  isLockedOut: boolean;
  isDeletable: boolean;
}

export interface UserPassword {
  entityId: string;
  newPassword: string;
  newPasswordConfirm: string;
}
