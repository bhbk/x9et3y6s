using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_TBL
{
    public partial class tbl_RoleClaim
    {
        public Guid RoleId { get; set; }
        public Guid ClaimId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Claim Claim { get; set; }
        public virtual tbl_Role Role { get; set; }
    }
}
