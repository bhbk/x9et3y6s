using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Internal.EntityModels
{
    public partial class AppRoleClaim
    {
        public Guid RoleId { get; set; }
        public Guid ClaimId { get; set; }
        public Guid? ActorId { get; set; }
        public DateTime Created { get; set; }
        public bool Immutable { get; set; }

        public virtual AppClaim Claim { get; set; }
        public virtual AppRole Role { get; set; }
    }
}
