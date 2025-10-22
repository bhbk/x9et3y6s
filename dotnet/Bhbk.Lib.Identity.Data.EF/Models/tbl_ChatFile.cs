using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_ChatFile
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid? MessageId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileContent { get; set; }
        public string Summary { get; set; }
        public DateTimeOffset Expires { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_ChatConversation Conversation { get; set; }
        public virtual tbl_ChatMessage Message { get; set; }
    }
}
