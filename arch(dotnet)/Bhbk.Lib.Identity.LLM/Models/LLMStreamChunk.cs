namespace Bhbk.Lib.Identity.LLM.Models
{
    public class LLMStreamChunk
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public bool IsComplete { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public long? FileSize { get; set; }
    }
}
