export interface AppConfig {
  /** Default issuer name for authentication (required) */
  defaultIssuer: string;
  /** Default client/audience for the application (optional) */
  defaultClient?: string;
  /** Path base prepended to all API routes (e.g. "/api") */
  pathBase?: string;
  /** STS API base URL for OAuth2 endpoints */
  stsApiUrl: string;
  /** Admin API base URL for management operations */
  adminApiUrl: string;
  /** User API base URL for self-service operations */
  userApiUrl: string;
  /** Alert API base URL for notifications */
  alertApiUrl: string;
  /** Admin portal URL for linking from user portal (optional) */
  adminPortalUrl?: string;
  /** User portal URL for linking from admin portal (optional) */
  userPortalUrl?: string;
}
