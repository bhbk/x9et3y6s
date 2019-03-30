using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class TUserClaims
    {
        public Guid UserId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual TClaims Claim { get; set; }
        public virtual TUsers User { get; set; }
    }
}
