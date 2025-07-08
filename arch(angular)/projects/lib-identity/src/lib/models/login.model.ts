/**
 * Login Provider Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Logins
 */

export interface LoginV1 {
  id: string;
  name: string;
  description?: string;
  loginKey?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  createdUtc: string;
}

export interface LoginCreate {
  name: string;
  description?: string;
  loginKey?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface LoginUpdate {
  id: string;
  name: string;
  description?: string;
  loginKey?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}
