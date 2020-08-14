using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_QueueEmail
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
        public DateTime Created { get; set; }
        public DateTime SendAt { get; set; }
    }
}
