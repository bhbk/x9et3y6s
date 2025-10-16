using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_AuthActivityAudience
    {
        public Guid AuthActivityId { get; set; }
        public Guid AudienceId { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_AuthActivity AuthActivity { get; set; }
        public virtual tbl_Audience Audience { get; set; }
    }
}
