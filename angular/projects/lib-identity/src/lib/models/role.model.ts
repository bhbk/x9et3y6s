/**
 * Role Models
 * Maps to: Bhbk.Lib.Identity.Models.Admin.Roles
 */

export interface RoleV1 {
  id: string;
  audienceId: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  created: string;
}

export interface RoleCreate {
  audienceId: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}

export interface RoleUpdate {
  id: string;
  audienceId: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
}
