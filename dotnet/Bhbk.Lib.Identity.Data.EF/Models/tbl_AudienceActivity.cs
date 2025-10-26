using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_AudienceActivity
    {
        public Guid UserActivityId { get; set; }
        public Guid AudienceId { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_UserActivity UserActivity { get; set; }
        public virtual tbl_Audience Audience { get; set; }
    }
}
