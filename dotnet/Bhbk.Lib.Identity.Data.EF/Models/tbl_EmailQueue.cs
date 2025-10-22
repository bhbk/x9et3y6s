using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_EmailQueue
    {
        public tbl_EmailQueue()
        {
            tbl_EmailActivities = new HashSet<tbl_EmailActivity>();
        }

        public Guid Id { get; set; }
        public string FromEmail { get; set; }
        public string FromDisplay { get; set; }
        public string ToEmail { get; set; }
        public string ToDisplay { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsCancelled { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset SendAt { get; set; }
        public DateTimeOffset? Delivered { get; set; }

        public virtual ICollection<tbl_EmailActivity> tbl_EmailActivities { get; set; }
    }
}
