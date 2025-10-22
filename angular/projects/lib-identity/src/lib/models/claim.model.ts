/**
 * Claim Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Claims
 */

export interface ClaimV1 {
  id: string;
  issuerId: string;
  subject?: string;
  type: string;
  value: string;
  valueType?: string;
  isDeletable: boolean;
  created: string;
}

export interface ClaimCreate {
  issuerId: string;
  subject?: string;
  type: string;
  value: string;
  valueType?: string;
  isDeletable: boolean;
}

export interface ClaimUpdate {
  id: string;
  issuerId: string;
  subject?: string;
  type: string;
  value: string;
  valueType?: string;
  isDeletable: boolean;
}
