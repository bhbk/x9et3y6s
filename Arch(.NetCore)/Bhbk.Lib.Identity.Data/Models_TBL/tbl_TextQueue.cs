using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_Tbl
{
    public partial class tbl_TextQueue
    {
        public tbl_TextQueue()
        {
            tbl_TextActivities = new HashSet<tbl_TextActivity>();
        }

        public Guid Id { get; set; }
        public string FromPhoneNumber { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Body { get; set; }
        public bool IsCancelled { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }
        public DateTimeOffset SendAtUtc { get; set; }
        public DateTimeOffset? DeliveredUtc { get; set; }

        public virtual ICollection<tbl_TextActivity> tbl_TextActivities { get; set; }
    }
}
