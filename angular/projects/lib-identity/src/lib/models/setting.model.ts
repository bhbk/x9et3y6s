/**
 * Setting Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Settings
 */

export interface SettingV1 {
  id: string;
  issuerId?: string;
  audienceId?: string;
  userId?: string;
  configKey: string;
  configValue: string;
  isDeletable: boolean;
  created: string;
}

export interface SettingCreate {
  issuerId?: string;
  audienceId?: string;
  userId?: string;
  configKey: string;
  configValue: string;
  isDeletable: boolean;
}

export interface SettingUpdate {
  id: string;
  issuerId?: string;
  audienceId?: string;
  userId?: string;
  configKey: string;
  configValue: string;
  isDeletable: boolean;
}
