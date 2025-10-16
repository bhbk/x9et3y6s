/**
 * Password Models
 * Maps to: Bhbk.Lib.Identity.Models.Me.Passwords
 */

export interface PasswordAdd {
  entityId: string;
  newPassword: string;
  newPasswordConfirm: string;
}

export interface PasswordChange {
  entityId: string;
  currentPassword: string;
  newPassword: string;
  newPasswordConfirm: string;
}
