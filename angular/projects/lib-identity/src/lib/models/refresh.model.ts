/**
 * Refresh/Session Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Refreshes
 */

export interface RefreshV1 {
  id: string;
  issuerId: string;
  audienceId?: string;
  userId?: string;
  refreshValue?: string;
  refreshType: string;
  validFrom: string;
  validTo: string;
  issued: string;
  // Session metadata
  ipAddress?: string;
  userAgent?: string;
  deviceName?: string;
  location?: string;
}

// For displaying sessions to user
export interface SessionInfo {
  id: string;
  deviceName: string;
  ipAddress: string;
  location?: string;
  userAgent: string;
  issuedAt: Date;
  expiresAt: Date;
  isCurrent: boolean;
}
