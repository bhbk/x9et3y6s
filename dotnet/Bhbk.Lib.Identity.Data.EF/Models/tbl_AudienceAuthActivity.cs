using System;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EF.Models
{
    public partial class tbl_AudienceAuthActivity
    {
        public Guid UserAuthActivityId { get; set; }
        public Guid AudienceId { get; set; }
        public DateTimeOffset Created { get; set; }

        public virtual tbl_UserAuthActivity UserAuthActivity { get; set; }
        public virtual tbl_Audience Audience { get; set; }
    }
}
