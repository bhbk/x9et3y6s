/**
 * Message of the Day Models
 * Maps to: Bhbk.Lib.Identity.Models.Me.MOTDs
 */

export interface MOTDTssV1 {
  globalId: string;
  id?: string;
  author: string;
  quote: string;
  length?: string;
  tags?: string[];
  category?: string;
  date?: string;
  title?: string;
  background?: string;
}
