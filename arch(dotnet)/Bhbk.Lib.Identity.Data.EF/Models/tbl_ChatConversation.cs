using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_ChatConversation
    {
        public tbl_ChatConversation()
        {
            tbl_ChatFiles = new HashSet<tbl_ChatFile>();
            tbl_ChatMessages = new HashSet<tbl_ChatMessage>();
        }

        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public DateTimeOffset StartedUtc { get; set; }
        public DateTimeOffset? EndedUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset? ModifiedUtc { get; set; }

        public virtual tbl_User User { get; set; }
        public virtual ICollection<tbl_ChatFile> tbl_ChatFiles { get; set; }
        public virtual ICollection<tbl_ChatMessage> tbl_ChatMessages { get; set; }
    }
}
