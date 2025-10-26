export type LLMProviderContext = 'Admin' | 'User' | 'Public';

export interface LLMProviderV1 {
  id: string;
  name: string;
  context: LLMProviderContext;
  isEnabled: boolean;
  failoverOrder: number;
  isDeletable: boolean;
  created: string;
  modified?: string;
  settings: LLMProviderSettingV1[];
}

export interface LLMProviderSettingV1 {
  id: string;
  providerId: string;
  configKey: string;
  configValue: string | null;
  isSecret: boolean;
  isDeletable: boolean;
  created: string;
}

export interface LLMProviderOrderUpdate {
  id: string;
  failoverOrder: number;
}
