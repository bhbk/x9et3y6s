using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_TBL
{
    public partial class tbl_EmailQueue
    {
        public tbl_EmailQueue()
        {
            tbl_EmailActivities = new HashSet<tbl_EmailActivity>();
        }

        public Guid Id { get; set; }
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

        public virtual tbl_User From { get; set; }
        public virtual ICollection<tbl_EmailActivity> tbl_EmailActivities { get; set; }
    }
}
