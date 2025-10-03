using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_ChatMessage
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public string ToolCalls { get; set; }
        public string ToolResults { get; set; }
        public int? InputTokens { get; set; }
        public int? OutputTokens { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_ChatConversation Conversation { get; set; }
    }
}
