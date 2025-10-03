/**
 * Quote Models
 * Maps to: Bhbk.Lib.Identity.Models.Me.Quotes
 */

export interface QuoteV1 {
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
