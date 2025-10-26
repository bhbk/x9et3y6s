/**
 * Audience Activity Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.AudienceActivity
 */

export interface AudienceActivityV1 {
  userActivityId: string;
  audienceId: string;
  audienceName: string;
  userId?: string;
  loginType: string;
  loginOutcome: string;
  localEndpoint?: string;
  remoteEndpoint?: string;
  created: string;
}
