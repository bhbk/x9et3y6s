/**
 * Login Provider Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.LoginProviders
 */

export interface LoginProviderV1 {
  id: string;
  name: string;
  description?: string;
  providerKey?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  createdUtc: string;
}

export interface LoginProviderCreate {
  name: string;
  description?: string;
  providerKey?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface LoginProviderUpdate {
  id: string;
  name: string;
  description?: string;
  providerKey?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}
