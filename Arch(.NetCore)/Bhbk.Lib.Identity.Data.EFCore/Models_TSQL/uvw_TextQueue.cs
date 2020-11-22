using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_TSQL
{
    public partial class uvw_TextQueue
    {
        public Guid Id { get; set; }
        public Guid? FromId { get; set; }
        public string FromPhoneNumber { get; set; }
        public Guid? ToId { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset SendAtUtc { get; set; }
        public DateTimeOffset? DeliveredUtc { get; set; }
    }
}
