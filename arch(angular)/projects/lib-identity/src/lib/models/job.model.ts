export interface JobV1 {
  id: string;
  name: string;
  isEnabled: boolean;
  isDeletable: boolean;
  createdUtc: string;
  modifiedUtc?: string;
  settings: JobSettingV1[];
}

export interface JobSettingV1 {
  id: string;
  jobId: string;
  configKey: string;
  configValue: string;
  isSecret: boolean;
  isDeletable: boolean;
  createdUtc: string;
}
