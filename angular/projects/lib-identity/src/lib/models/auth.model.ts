/**
 * STS Authentication Models
 * Maps to: Bhbk.Lib.Identity.Models.Sts
 */

// Login request (Resource Owner Password Grant)
export interface ResourceOwnerV2 {
  issuer: string;
  client?: string;
  user: string;
  password: string;
  grant_type: 'password';
}

// Refresh token request
export interface RefreshTokenV2 {
  issuer: string;
  client?: string;
  grant_type: 'refresh_token';
}

// JWT response (access token only - refresh token is in httpOnly cookie)
export interface UserJwtV2 {
  token_type: string;
  access_token: string;
  expires_in: number;
  issuer: string;
  client: string[];
  user: string;
}

// Decoded JWT payload
export interface JwtPayload {
  sub: string;           // user id
  iss: string;           // issuer
  aud: string | string[]; // audience(s)
  exp: number;           // expiration timestamp
  iat: number;           // issued at timestamp
  nbf?: number;          // not before timestamp
  jti?: string;          // JWT ID
  email?: string;
  name?: string;
  given_name?: string;
  family_name?: string;
  roles?: string[];
  [key: string]: unknown; // allow additional claims
}

// Authentication state for the store
export interface AuthState {
  isAuthenticated: boolean;
  accessToken: string | null;
  tokenExpiry: Date | null;
  user: AuthUser | null;
  isLoading: boolean;
  error: string | null;
}

// Authenticated user info (extracted from JWT + API data)
export interface AuthUser {
  id: string;
  issuer: string;
  clients: string[];
  email?: string;
  name?: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
  entitlements?: AuthEntitlement[];
}

// Entitlement info loaded from the API after authentication
export interface AuthEntitlement {
  entitlementTypeName: string;
  entitlementScopeName: string;
  issuerName?: string;
  audienceName?: string;
}
