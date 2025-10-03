export interface LLMProviderV1 {
  id: string;
  name: string;
  isEnabled: boolean;
  failoverOrder: number;
  isDeletable: boolean;
  createdUtc: string;
  modifiedUtc?: string;
  settings: LLMProviderSettingV1[];
}

export interface LLMProviderSettingV1 {
  id: string;
  providerId: string;
  configKey: string;
  configValue: string;
  isSecret: boolean;
  isDeletable: boolean;
  createdUtc: string;
}

export interface LLMProviderOrderUpdate {
  id: string;
  failoverOrder: number;
}
