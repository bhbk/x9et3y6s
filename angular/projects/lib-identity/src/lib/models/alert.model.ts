export interface EmailQueueV1 {
  id: string;
  fromEmail?: string;
  fromDisplay?: string;
  toEmail: string;
  toDisplay?: string;
  subject: string;
  body: string;
  isCancelled: boolean;
  createdUtc: string;
  sendAtUtc: string;
  deliveredUtc?: string | null;
}

export interface TextQueueV1 {
  id: string;
  fromPhoneNumber?: string;
  toPhoneNumber: string;
  body: string;
  isCancelled: boolean;
  createdUtc: string;
  sendAtUtc: string;
  deliveredUtc?: string | null;
}

export type AlertDeliveryStatus = 'Pending' | 'Cancelled' | 'Delivered';
