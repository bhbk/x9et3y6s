using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_TextQueue
    {
        public tbl_TextQueue()
        {
            tbl_TextActivities = new HashSet<tbl_TextActivity>();
        }

        public Guid Id { get; set; }
        public Guid? ActorId { get; set; }
        public Guid? FromId { get; set; }
        public string FromPhoneNumber { get; set; }
        public Guid ToId { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Body { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset SendAtUtc { get; set; }
        public DateTimeOffset? DeliveredUtc { get; set; }

        public virtual tbl_User From { get; set; }
        public virtual ICollection<tbl_TextActivity> tbl_TextActivities { get; set; }
    }
}
