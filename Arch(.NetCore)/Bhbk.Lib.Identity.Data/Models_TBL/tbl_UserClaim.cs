using System;
using System.Collections.Generic;

#nullable disable

namespace Bhbk.Lib.Identity.Data.Models_Tbl
{
    public partial class tbl_UserClaim
    {
        public Guid UserId { get; set; }
        public Guid ClaimId { get; set; }
        public bool IsDeletable { get; set; }
        public DateTimeOffset CreatedUtc { get; set; }

        public virtual tbl_Claim Claim { get; set; }
        public virtual tbl_User User { get; set; }
    }
}
