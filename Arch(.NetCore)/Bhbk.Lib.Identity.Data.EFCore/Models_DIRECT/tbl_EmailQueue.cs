using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_EmailQueue
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public Guid? FromId { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplay { get; set; }
        public Guid ToId { get; set; }
        public string ToEmail { get; set; }
        public string ToDisplay { get; set; }
        public string Subject { get; set; }
        public string HtmlContent { get; set; }
        public string PlaintextContent { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset SendAtUtc { get; set; }

        public virtual tbl_User From { get; set; }
    }
}
