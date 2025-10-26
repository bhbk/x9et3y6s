/**
 * User Activity Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.UserActivity
 */

export interface UserActivityV1 {
  id: string;
  audienceIds: string[];
  userId?: string;
  loginType: string;
  loginOutcome: string;
  localEndpoint?: string;
  remoteEndpoint?: string;
  created: string;
}

// Login outcome types
export type LoginOutcome = 'Success' | 'Failure' | 'LockedOut' | 'NotAllowed' | 'RequiresTwoFactor';

// Login type constants
export type LoginType = 'ResourceOwner' | 'ClientCredential' | 'AuthorizationCode' | 'DeviceCode' | 'Implicit' | 'Refresh';
