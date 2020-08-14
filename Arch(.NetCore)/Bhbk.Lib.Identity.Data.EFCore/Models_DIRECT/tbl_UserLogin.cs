using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_UserLogin
    {
        public Guid UserId { get; set; }
        public Guid LoginId { get; set; }
        public Guid? ActorId { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }

        public virtual tbl_Login Login { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
