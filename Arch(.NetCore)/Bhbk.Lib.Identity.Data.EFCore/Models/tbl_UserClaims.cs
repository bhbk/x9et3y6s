using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Data.EFCore.Models
{
    public partial class tbl_UserClaims
    {
        public Guid UserId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual tbl_Claims Claim { get; set; }
        public virtual tbl_Users User { get; set; }
    }
}
