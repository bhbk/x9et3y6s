using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class uvw_EmailQueue
    {
        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public Guid? FromId { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplay { get; set; }
        public Guid? ToId { get; set; }
        public string ToEmail { get; set; }
        public string ToDisplay { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset SendAtUtc { get; set; }
        public DateTimeOffset? DeliveredUtc { get; set; }
    }
}
