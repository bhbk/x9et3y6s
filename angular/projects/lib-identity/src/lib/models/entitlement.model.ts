/**
 * Entitlement Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Entitlements
 */

export interface EntitlementTypeV1 {
  id: string;
  name: string;
  description?: string;
  sortOrder: number;
  isEnabled: boolean;
  isDeletable: boolean;
  created: string;
}

export interface EntitlementScopeV1 {
  id: string;
  name: string;
  description?: string;
  sortOrder: number;
  isEnabled: boolean;
  created: string;
}

export interface UserEntitlementV1 {
  id: string;
  userId: string;
  entitlementTypeId: string;
  entitlementScopeId: string;
  issuerId?: string;
  audienceId?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  created: string;
  userName?: string;
  entitlementTypeName?: string;
  entitlementScopeName?: string;
  issuerName?: string;
  audienceName?: string;
}

export interface UserEntitlementCreate {
  userId: string;
  entitlementTypeId: string;
  entitlementScopeId: string;
  issuerId?: string;
  audienceId?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface UserEntitlementUpdate {
  id: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface AudienceEntitlementV1 {
  id: string;
  audienceId: string;
  entitlementTypeId: string;
  entitlementScopeId: string;
  issuerId?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  created: string;
  audienceName?: string;
  entitlementTypeName?: string;
  entitlementScopeName?: string;
  issuerName?: string;
}

export interface AudienceEntitlementCreate {
  audienceId: string;
  entitlementTypeId: string;
  entitlementScopeId: string;
  issuerId?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface AudienceEntitlementUpdate {
  id: string;
  isEnabled: boolean;
  isDeletable: boolean;
}
