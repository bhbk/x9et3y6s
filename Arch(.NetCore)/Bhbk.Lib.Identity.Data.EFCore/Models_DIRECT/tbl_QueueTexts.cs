using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_QueueTexts
    {
        public Guid Id { get; set; }
        public Guid ActorId { get; set; }
        public Guid FromId { get; set; }
        public string FromPhoneNumber { get; set; }
        public Guid ToId { get; set; }
        public string ToPhoneNumber { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime SendAt { get; set; }

        public virtual tbl_Users From { get; set; }
    }
}
