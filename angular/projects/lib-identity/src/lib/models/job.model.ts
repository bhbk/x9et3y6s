export interface JobV1 {
  id: string;
  name: string;
  description?: string;
  isEnabled: boolean;
  isDeletable: boolean;
  created: string;
  modified?: string;
  settings: JobSettingV1[];
}

export interface JobSettingV1 {
  id: string;
  jobId: string;
  configKey: string;
  configValue: string;
  isSecret: boolean;
  isDeletable: boolean;
  created: string;
}
