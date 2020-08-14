using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_Activity
    {
        public Guid Id { get; set; }
        public Guid? AudienceId { get; set; }
        public Guid? UserId { get; set; }
        public string ActivityType { get; set; }
        public string TableName { get; set; }
        public string KeyValues { get; set; }
        public string OriginalValues { get; set; }
        public string CurrentValues { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }

        public virtual tbl_Audience Audience { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
