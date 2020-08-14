using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT
{
    public partial class tbl_RoleClaim
    {
        public Guid RoleId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid? ActorId { get; set; }
        public bool Immutable { get; set; }
        public DateTime Created { get; set; }

        public virtual tbl_Claim Claim { get; set; }
        public virtual tbl_Role Role { get; set; }
    }
}
