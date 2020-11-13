using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_UserLogin
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public Guid? ActorId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset? CreatedUtc { get; set; }

        public virtual tbl_Login Login { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
